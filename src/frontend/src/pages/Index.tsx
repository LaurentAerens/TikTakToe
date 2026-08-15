import { useState, useCallback } from "react";
import TicTacToeBoard, { type BoardState, type CellValue } from "@/components/TicTacToeBoard";
import EvalBar from "@/components/EvalBar";
import GameControls from "@/components/GameControls";
import RightPanel from "@/components/RightPanel";
import AppLayout from "@/components/AppLayout";
import {
  Button,
  Drawer,
  DrawerHeader,
  DrawerHeaderTitle,
  DrawerBody,
  Card,
  Badge,
  Text,
  tokens,
} from "@fluentui/react-components";
import { PanelRight24Regular, Dismiss24Regular } from "@fluentui/react-icons";

const DEMO_ENGINES = [
  { id: "minimax", name: "Minimax", description: "Classic optimal play" },
  { id: "alpha-beta", name: "Alpha-Beta", description: "Pruned search" },
  { id: "mcts", name: "MCTS", description: "Monte Carlo Tree Search" },
  { id: "neural", name: "NeuralTTT", description: "Neural network based" },
];

const WINNING_LINES = [
  [0, 1, 2], [3, 4, 5], [6, 7, 8],
  [0, 3, 6], [1, 4, 7], [2, 5, 8],
  [0, 4, 8], [2, 4, 6],
];

function checkWinner(board: BoardState): { winner: CellValue; line: number[] } | null {
  for (const line of WINNING_LINES) {
    const [a, b, c] = line;
    if (board[a] && board[a] === board[b] && board[a] === board[c]) {
      return { winner: board[a], line };
    }
  }
  return null;
}

const Index = () => {
  const [board, setBoard] = useState<BoardState>(Array(9).fill(null));
  const [history, setHistory] = useState<BoardState[]>([Array(9).fill(null)]);
  const [historyIndex, setHistoryIndex] = useState(0);
  const [playEngine, setPlayEngine] = useState("minimax");
  const [evalEngine, setEvalEngine] = useState("alpha-beta");
  const [evaluation, setEvaluation] = useState(0);
  const [drawerOpen, setDrawerOpen] = useState(false);

  const result = checkWinner(board);
  const isDraw = !result && board.every((c) => c !== null);
  const currentTurn: "X" | "O" = board.filter((c) => c !== null).length % 2 === 0 ? "X" : "O";

  const status = result
    ? `${result.winner} wins!`
    : isDraw
    ? "Draw!"
    : `${currentTurn}'s turn`;

  const gameOver = !!result || isDraw;

  const handleCellClick = useCallback((index: number) => {
    if (gameOver) return;
    const newBoard = [...board];
    newBoard[index] = currentTurn;
    const newHistory = history.slice(0, historyIndex + 1);
    newHistory.push(newBoard);
    setBoard(newBoard);
    setHistory(newHistory);
    setHistoryIndex(newHistory.length - 1);

    const xCount = newBoard.filter((c) => c === "X").length;
    const oCount = newBoard.filter((c) => c === "O").length;
    const winResult = checkWinner(newBoard);
    if (winResult) {
      setEvaluation(winResult.winner === "X" ? 1 : -1);
    } else {
      setEvaluation((xCount - oCount) * 0.1 + (Math.random() - 0.5) * 0.3);
    }
  }, [board, currentTurn, gameOver, history, historyIndex]);

  const handleNewGame = () => {
    const empty = Array(9).fill(null);
    setBoard(empty);
    setHistory([empty]);
    setHistoryIndex(0);
    setEvaluation(0);
  };

  const handleUndo = () => {
    if (historyIndex > 0) {
      setHistoryIndex(historyIndex - 1);
      setBoard(history[historyIndex - 1]);
    }
  };

  const handleRedo = () => {
    if (historyIndex < history.length - 1) {
      setHistoryIndex(historyIndex + 1);
      setBoard(history[historyIndex + 1]);
    }
  };

  return (
    <AppLayout>
      <div className="flex flex-col gap-6 max-w-5xl mx-auto animate-slide-in">
        <div className="flex gap-4 md:gap-6 items-stretch justify-center">
          <div className="hidden sm:flex" style={{ minHeight: 360 }}>
            <EvalBar
              evaluation={evaluation}
              engineName={DEMO_ENGINES.find((e) => e.id === evalEngine)?.name}
              engines={DEMO_ENGINES}
              evalEngine={evalEngine}
              onEvalEngineChange={setEvalEngine}
            />
          </div>

          <div className="flex-shrink-0">
            <TicTacToeBoard
              board={board}
              onCellClick={handleCellClick}
              disabled={gameOver}
              winningCells={result?.line}
            />
          </div>

          <div className="w-32 md:w-40 flex-shrink-0 flex flex-col gap-3">
            <GameControls
              onNewGame={handleNewGame}
              onUndo={handleUndo}
              onRedo={handleRedo}
              canUndo={historyIndex > 0}
              canRedo={historyIndex < history.length - 1}
              currentTurn={currentTurn}
              status={status}
            />
            <Button
              appearance="outline"
              icon={<PanelRight24Regular />}
              onClick={() => setDrawerOpen(true)}
            >
              Panel
            </Button>
          </div>
        </div>

        {/* Mobile eval */}
        <div className="sm:hidden">
          <Card style={{ padding: 8 }}>
            <div
              className="relative overflow-hidden"
              style={{
                height: 24,
                borderRadius: tokens.borderRadiusCircular,
                backgroundColor: tokens.colorNeutralBackground3,
                border: `1px solid ${tokens.colorNeutralStroke2}`,
              }}
            >
              <div
                style={{
                  position: "absolute",
                  left: 0,
                  top: 0,
                  height: "100%",
                  width: `${(evaluation + 1) * 50}%`,
                  backgroundColor: tokens.colorBrandBackground2,
                  transition: "width 0.7s ease-out",
                }}
              />
              <div className="absolute inset-0 flex items-center justify-center">
                <Text size={100} weight="bold">
                  {evaluation > 0 ? `+${evaluation.toFixed(2)}` : evaluation.toFixed(2)}
                </Text>
              </div>
            </div>
          </Card>
        </div>

        {/* Move log */}
        <Card style={{ padding: 12 }}>
          <Text
            size={100}
            style={{
              color: tokens.colorNeutralForeground3,
              textTransform: "uppercase",
              letterSpacing: 1,
            }}
          >
            Move Log
          </Text>
          <div className="flex flex-wrap gap-1.5 mt-2">
            {history.slice(1).map((_, i) => {
              const movePlayer = i % 2 === 0 ? "X" : "O";
              const prevBoard = history[i];
              const currBoard = history[i + 1];
              const moveIndex = currBoard.findIndex((c, idx) => c !== prevBoard[idx]);
              const row = Math.floor(moveIndex / 3) + 1;
              const col = (moveIndex % 3) + 1;
              const isCurrent = i === historyIndex - 1;
              return (
                <Badge
                  key={i}
                  appearance={isCurrent ? "filled" : "outline"}
                  color={isCurrent ? "brand" : movePlayer === "X" ? "informative" : "danger"}
                  size="medium"
                  style={{ fontFamily: "ui-monospace, monospace" }}
                >
                  {movePlayer}({row},{col})
                </Badge>
              );
            })}
            {history.length === 1 && (
              <Text size={100} style={{ color: tokens.colorNeutralForeground3 }}>
                No moves yet
              </Text>
            )}
          </div>
        </Card>
      </div>

      <Drawer
        type="overlay"
        position="end"
        open={drawerOpen}
        onOpenChange={(_, { open }) => setDrawerOpen(open)}
        size="medium"
      >
        <DrawerHeader>
          <DrawerHeaderTitle
            action={
              <Button
                appearance="subtle"
                aria-label="Close"
                icon={<Dismiss24Regular />}
                onClick={() => setDrawerOpen(false)}
              />
            }
          >
            Game Panel
          </DrawerHeaderTitle>
        </DrawerHeader>
        <DrawerBody>
          <RightPanel
            engines={DEMO_ENGINES}
            playEngine={playEngine}
            onPlayEngineChange={setPlayEngine}
          />
        </DrawerBody>
      </Drawer>
    </AppLayout>
  );
};

export default Index;
