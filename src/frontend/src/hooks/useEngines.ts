import { useQuery } from '@tanstack/react-query';
import { gameApi, type EngineCapabilityDto } from '@/api';

/**
 * Engine ids are regenerated whenever the dev database resets on backend restart,
 * so they are cached briefly rather than indefinitely.
 */
const ENGINE_STALE_TIME_MS = 5 * 60 * 1000;

export function useEngines() {
  return useQuery<EngineCapabilityDto[]>({
    queryKey: ['engines'],
    queryFn: ({ signal }) => gameApi.listEngines(signal),
    staleTime: ENGINE_STALE_TIME_MS,
  });
}
