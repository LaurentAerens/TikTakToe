/** View model shared by every engine dropdown. Keyed on the engine capability id. */
export interface EngineOption {
  id: string;
  name: string;
  description?: string;
}
