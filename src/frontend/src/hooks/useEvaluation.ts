import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { EVAL_WIN_SCORE, gameApi } from '@/api';

/** Board marker whose perspective the eval bar shows - the human always moves first. */
const EVAL_PERSPECTIVE_PLAYER = 1;

function clampToUnit(value: number): number {
  return Math.max(-1, Math.min(1, value));
}

/**
 * Scores the displayed position. Failures are not retried and must not block play:
 * callers fall back to a neutral bar when `isError` is set.
 */
export function useEvaluation(board: number[][] | undefined, engineId: string | undefined) {
  return useQuery<number>({
    queryKey: ['eval', engineId, board],
    queryFn: async ({ signal }) => {
      if (engineId === undefined || board === undefined) {
        throw new Error('Evaluation needs both an engine and a board.');
      }

      const score = await gameApi.evaluate(
        { engineId, board, player: EVAL_PERSPECTIVE_PLAYER },
        signal,
      );

      return clampToUnit(score / EVAL_WIN_SCORE);
    },
    enabled: engineId !== undefined && board !== undefined,
    // Hold the previous score while the next one loads instead of flashing "--" every move.
    placeholderData: keepPreviousData,
    retry: false,
  });
}
