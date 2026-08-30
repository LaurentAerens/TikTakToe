import { DRAW_WINNER_VALUE, type GameDto, type MoveDto, type PlayerDto } from '@/api';

/** The backend returns moves in unspecified order, so sort before rendering or replaying. */
export function sortMoves(moves: MoveDto[]): MoveDto[] {
  return [...moves].sort((a, b) => a.moveNumber - b.moveNumber);
}

/** The player whose turn it is, or undefined once the game is over. */
export function waitingPlayer(game: GameDto | undefined): PlayerDto | undefined {
  if (game === undefined || game.waitingForPlayerId === null) {
    return undefined;
  }

  return game.players.find((player) => player.id === game.waitingForPlayerId);
}

/** Drives the polling interval: true while the background worker owes us an engine move. */
export function isEngineTurn(game: GameDto | undefined): boolean {
  return waitingPlayer(game)?.isEngine ?? false;
}

/** The board marker owned by a player. Turn order defines it: index i owns marker i + 1. */
export function markerFor(game: GameDto, playerId: string): number {
  const index = game.players.findIndex((player) => player.id === playerId);
  return index < 0 ? 0 : index + 1;
}

/** The engine capability id of the opponent, taken from its player record. */
export function opponentEngineId(game: GameDto | undefined): string | undefined {
  return game?.players.find((player) => player.isEngine)?.externalId ?? undefined;
}

export function describeStatus(
  game: GameDto | undefined,
  humanPlayerId: string | undefined,
  opponentName: string,
): string {
  if (game === undefined) {
    return 'Press New Game to start';
  }

  if (game.state.isGameOver) {
    if (game.state.winnerValue === DRAW_WINNER_VALUE) {
      return 'Draw!';
    }

    return game.state.winnerPlayerId === humanPlayerId ? 'You win!' : `${opponentName} wins!`;
  }

  return game.waitingForPlayerId === humanPlayerId ? 'Your turn' : `${opponentName} is thinking...`;
}
