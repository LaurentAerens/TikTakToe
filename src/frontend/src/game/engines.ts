import type { EngineCapabilityDto } from '@/api';
import type { EngineOption } from '@/types/engine';

export function describeEngine(capability: EngineCapabilityDto): string {
  const size = `up to ${capability.maxBoardSizeX}x${capability.maxBoardSizeY}`;
  return capability.depth ? `${size} - depth-aware` : size;
}

export function toEngineOption(capability: EngineCapabilityDto): EngineOption {
  return {
    id: capability.id,
    name: capability.displayName,
    description: describeEngine(capability),
  };
}

export function findCapability(
  capabilities: EngineCapabilityDto[],
  engineId: string | undefined,
): EngineCapabilityDto | undefined {
  return engineId === undefined ? undefined : capabilities.find((capability) => capability.id === engineId);
}

/** True when the engine can actually play this board size; only Random handles non-3x3. */
export function supportsBoard(capability: EngineCapabilityDto, rows: number, cols: number): boolean {
  return rows <= capability.maxBoardSizeX && cols <= capability.maxBoardSizeY;
}
