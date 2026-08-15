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

interface Engine {
  id: string;
  name: string;
  description?: string;
}

const ENGINE_EVAL_INFO: Record<string, { scale: string; description: string }> = {
  minimax: {
    scale: "-1 to +1",
    description:
      "Returns exact game-theoretic value. +1 = X wins, -1 = O wins, 0 = draw.",
  },
  "alpha-beta": {
    scale: "-1 to +1",
    description: "Same as Minimax but computed faster via pruning.",
  },
  mcts: {
    scale: "-1 to +1",
    description:
      "Based on win rate from random simulations. Closer to 0 = uncertain.",
  },
  neural: {
    scale: "-1 to +1",
    description:
      "Neural network confidence score. Learned positional understanding.",
  },
};

interface EvalBarProps {
  evaluation: number;
  engineName?: string;
  engines?: Engine[];
  evalEngine?: string;
  onEvalEngineChange?: (value: string) => void;
}

const EvalBar = ({ evaluation, engineName, engines, evalEngine, onEvalEngineChange }: EvalBarProps) => {
  const percentage = Math.max(0, Math.min(100, (evaluation + 1) * 50));
  const evalDisplay = evaluation > 0 ? `+${evaluation.toFixed(2)}` : evaluation.toFixed(2);
  const evalInfo = evalEngine ? ENGINE_EVAL_INFO[evalEngine] : null;

  const xColor = tokens.colorPaletteBlueForeground2;
  const oColor = tokens.colorPaletteBerryForeground1;
  const valueColor =
    evaluation > 0.05 ? xColor : evaluation < -0.05 ? oColor : tokens.colorNeutralForeground3;

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

        {evalInfo && (
          <Tooltip
            relationship="description"
            withArrow
            content={
              <div style={{ maxWidth: 220 }}>
                <Text weight="semibold" size={200}>Scale: {evalInfo.scale}</Text>
                <div>
                  <Text size={100} style={{ color: tokens.colorNeutralForeground2 }}>
                    {evalInfo.description}
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
