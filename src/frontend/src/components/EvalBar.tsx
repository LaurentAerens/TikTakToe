import {
  Popover,
  PopoverTrigger,
  PopoverSurface,
  Button,
  Tooltip,
  Card,
  Text,
  tokens,
} from "@fluentui/react-components";
import { MoreVertical20Regular, Info20Regular } from "@fluentui/react-icons";
import EngineSelector from "@/components/EngineSelector";
import type { EngineOption } from "@/types/engine";

interface EvalBarProps {
  /** Normalised to -1..+1 from the backend's -1000..1000 score. */
  evaluation: number;
  engineName?: string;
  engines?: EngineOption[];
  evalEngine?: string;
  onEvalEngineChange?: (value: string) => void;
  /** Set when the eval request failed or no game is running; renders a neutral bar. */
  unavailable?: boolean;
}

const EvalBar = ({
  evaluation,
  engineName,
  engines,
  evalEngine,
  onEvalEngineChange,
  unavailable,
}: EvalBarProps) => {
  const value = unavailable ? 0 : evaluation;
  const percentage = Math.max(0, Math.min(100, (value + 1) * 50));
  const evalDisplay = unavailable ? "--" : value > 0 ? `+${value.toFixed(2)}` : value.toFixed(2);
  const selectedEngine = engines?.find((engine) => engine.id === evalEngine);

  const xColor = tokens.colorPaletteBlueForeground2;
  const oColor = tokens.colorPaletteBerryForeground1;
  const valueColor =
    unavailable || Math.abs(value) <= 0.05
      ? tokens.colorNeutralForeground3
      : value > 0
      ? xColor
      : oColor;

  return (
    <Card style={{ padding: 8, height: "100%" }}>
      <div className="flex flex-col items-center gap-2 h-full">
        <div className="flex items-center gap-0.5">
          {engineName && (
            <Text
              size={100}
              style={{ color: tokens.colorNeutralForeground3, maxWidth: 70, textAlign: "center" }}
              truncate
            >
              {engineName}
            </Text>
          )}
          {engines && evalEngine && onEvalEngineChange && (
            <Popover positioning="after">
              <PopoverTrigger disableButtonEnhancement>
                <Button appearance="subtle" size="small" icon={<MoreVertical20Regular />} />
              </PopoverTrigger>
              <PopoverSurface style={{ width: 240 }}>
                <EngineSelector
                  label="Eval Engine"
                  engines={engines}
                  value={evalEngine}
                  onChange={onEvalEngineChange}
                />
              </PopoverSurface>
            </Popover>
          )}
        </div>

        <div
          className="relative w-8 flex-1 overflow-hidden"
          style={{
            borderRadius: tokens.borderRadiusCircular,
            backgroundColor: tokens.colorNeutralBackground3,
            border: `1px solid ${tokens.colorNeutralStroke2}`,
          }}
        >
          <div
            style={{
              position: "absolute",
              bottom: 0,
              width: "100%",
              height: `${percentage}%`,
              backgroundColor: tokens.colorBrandBackground2,
              transition: "height 0.7s ease-out",
            }}
          />
          <div
            style={{
              position: "absolute",
              top: "50%",
              left: 0,
              width: "100%",
              height: 1,
              backgroundColor: tokens.colorNeutralBackground1,
              opacity: 0.4,
            }}
          />
        </div>

        <Text
          weight="semibold"
          size={200}
          style={{ color: valueColor, fontFamily: "ui-monospace, monospace" }}
        >
          {evalDisplay}
        </Text>

        {selectedEngine && (
          <Tooltip
            relationship="description"
            withArrow
            content={
              <div style={{ maxWidth: 220 }}>
                <Text weight="semibold" size={200}>Scale: -1 to +1</Text>
                <div>
                  <Text size={100} style={{ color: tokens.colorNeutralForeground2 }}>
                    {selectedEngine.name} scores the position from X's perspective
                    (+1 = X winning, -1 = O winning, 0 = level)
                    {selectedEngine.description ? ` - ${selectedEngine.description}.` : "."}
                  </Text>
                </div>
              </div>
            }
          >
            <Button appearance="subtle" size="small" icon={<Info20Regular />} />
          </Tooltip>
        )}
      </div>
    </Card>
  );
};

export default EvalBar;
