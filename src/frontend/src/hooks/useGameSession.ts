import { useCallback, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { gameApi, type GameDto, type MoveInput } from '@/api';
import { isEngineTurn } from '@/game/state';
import { useApiErrorToast } from './useApiErrorToast';

/**
 * Every engine except Random rejects boards that are not 3x3, and it fails inside the
 * background worker where the client never sees the error - the game just stalls.
 */
export const BOARD_ROWS = 3;
export const BOARD_COLS = 3;

/** Sits just under the worker's 500ms queue tick so engine replies land promptly. */
const ENGINE_POLL_INTERVAL_MS = 400;

export interface GameSession {
  gameId: string;
  /** Registered fresh for each game and sent as X-Player-Id on every move. */
  humanPlayerId: string;
}

export function gameQueryKey(gameId: string | null) {
  return ['game', gameId] as const;
}

export function useGameSession() {
  const queryClient = useQueryClient();
  const showError = useApiErrorToast();
  const [session, setSession] = useState<GameSession | null>(null);

  const gameQuery = useQuery<GameDto>({
    queryKey: gameQueryKey(session?.gameId ?? null),
    queryFn: async ({ signal }) => {
      if (session === null) {
        throw new Error('No active game session.');
      }

      return gameApi.getGame(session.gameId, signal);
    },
    enabled: session !== null,
    // Polls only while the engine owes us a move, then stops by itself.
    refetchInterval: (query) => (isEngineTurn(query.state.data) ? ENGINE_POLL_INTERVAL_MS : false),
  });

  const startGame = useMutation({
    mutationFn: async ({ enginePlayerId }: { enginePlayerId: string }) => {
      const humanPlayerId = await gameApi.createHumanPlayer();
      const game = await gameApi.createGame({
        rows: BOARD_ROWS,
        cols: BOARD_COLS,
        // Order decides turn order: the human moves first and owns board marker 1.
        playerIds: [humanPlayerId, enginePlayerId],
      });

      return { humanPlayerId, game };
    },
    onSuccess: ({ humanPlayerId, game }) => {
      setSession({ gameId: game.id, humanPlayerId });
      queryClient.setQueryData(gameQueryKey(game.id), game);
    },
    onError: (error) => showError('Could not start the game', error),
  });

  const playMove = useMutation({
    mutationFn: async (move: MoveInput) => {
      if (session === null) {
        throw new Error('No active game session.');
      }

      return gameApi.makeMove(session.gameId, session.humanPlayerId, move);
    },
    // Seeding the cache is what flips refetchInterval on for the engine's reply.
    onSuccess: (game) => queryClient.setQueryData(gameQueryKey(game.id), game),
    onError: (error) => showError('Move rejected', error),
  });

  const startGameWith = useCallback(
    (enginePlayerId: string) => startGame.mutate({ enginePlayerId }),
    [startGame],
  );

  const submitMove = useCallback((move: MoveInput) => playMove.mutate(move), [playMove]);

  return {
    session,
    game: gameQuery.data,
    startGame: startGameWith,
    isStarting: startGame.isPending,
    submitMove,
    isMoving: playMove.isPending,
  };
}
