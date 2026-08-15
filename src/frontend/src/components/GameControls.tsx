import { Button, Card, Text, tokens } from "@fluentui/react-components";
import { Previous24Regular, ArrowClockwise24Regular, Next24Regular } from "@fluentui/react-icons";

interface GameControlsProps {
  onNewGame: () => void;
  onUndo?: () => void;
  onRedo?: () => void;
  canUndo?: boolean;
  canRedo?: boolean;
  currentTurn: "X" | "O";
  status: string;
}

const GameControls = ({ onNewGame, onUndo, onRedo, canUndo, canRedo, currentTurn, status }: GameControlsProps) => {
  const xColor = tokens.colorPaletteBlueForeground2;
  const oColor = tokens.colorPaletteBerryForeground1;

  return (
    <div className="flex flex-col gap-3 animate-slide-in">
      <Card style={{ padding: 12, textAlign: "center" }}>
        <Text size={100} style={{ color: tokens.colorNeutralForeground3, textTransform: "uppercase", letterSpacing: 1 }}>
          Status
        </Text>
        <div>
          <Text weight="semibold" size={300}>{status}</Text>
        </div>
      </Card>

      <Card style={{ padding: 12, textAlign: "center" }}>
        <Text size={100} style={{ color: tokens.colorNeutralForeground3, textTransform: "uppercase", letterSpacing: 1 }}>
          Turn
        </Text>
        <div>
          <Text
            size={700}
            weight="bold"
            style={{
              fontFamily: "ui-monospace, monospace",
              color: currentTurn === "X" ? xColor : oColor,
            }}
          >
            {currentTurn}
          </Text>
        </div>
      </Card>

      <div className="flex gap-2">
        <Button
          appearance="outline"
          icon={<Previous24Regular />}
          onClick={onUndo}
          disabled={!canUndo}
          style={{ flex: 1 }}
        />
        <Button
          appearance="primary"
          icon={<ArrowClockwise24Regular />}
          onClick={onNewGame}
          style={{ flex: 1 }}
        />
        <Button
          appearance="outline"
          icon={<Next24Regular />}
          onClick={onRedo}
          disabled={!canRedo}
          style={{ flex: 1 }}
        />
      </div>
    </div>
  );
};

export default GameControls;
