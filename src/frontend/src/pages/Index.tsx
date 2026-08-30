import { useCallback, useMemo, useState } from "react";
import TicTacToeBoard from "@/components/TicTacToeBoard";
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
import { boardAfter, createEmptyBoard, findWinningLine, glyphForValue } from "@/game/board";
import { findCapability, toEngineOption } from "@/game/engines";
import { describeStatus, markerFor, opponentEngineId, sortMoves } from "@/game/state";
import { useEngines } from "@/hooks/useEngines";
import { useEvaluation } from "@/hooks/useEvaluation";
import { BOARD_COLS, BOARD_ROWS, useGameSession } from "@/hooks/useGameSession";

const Index = () => {
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [chosenPlayEngineId, setChosenPlayEngineId] = useState("");
  const [chosenEvalEngineId, setChosenEvalEngineId] = useState("");

  const enginesQuery = useEngines();
  const capabilities = useMemo(() => enginesQuery.data ?? [], [enginesQuery.data]);
  const engineOptions = useMemo(() => capabilities.map(toEngineOption), [capabilities]);

  // Engine ids only exist once /engines resolves, so the default is derived rather than
  // synced into state - the useState above holds an explicit user choice only.
  const defaultEngineId = capabilities[0]?.id ?? "";
  const playEngineId = chosenPlayEngineId || defaultEngineId;
  const evalEngineId = chosenEvalEngineId || defaultEngineId;

  const { session, game, startGame, isStarting, submitMove, isMoving } = useGameSession();

  const moves = useMemo(() => (game ? sortMoves(game.moves) : []), [game]);

  // null means "follow the live position", so new moves appear without syncing state.
  const [pinnedIndex, setPinnedIndex] = useState<number | null>(null);
  const isLatest = pinnedIndex === null;
  const viewIndex = pinnedIndex === null ? moves.length : Math.min(pinnedIndex, moves.length);

  const board = useMemo(
    () => (game ? boardAfter(game, viewIndex) : createEmptyBoard(BOARD_ROWS, BOARD_COLS)),
    [game, viewIndex],
  );

  const opponentName = useMemo(() => {
    const capability = findCapability(capabilities, opponentEngineId(game));
    return capability?.displayName ?? "Engine";
  }, [capabilities, game]);

  const winningCells = useMemo(
    () => (game?.state.isGameOver && isLatest ? findWinningLine(board, game.state.winnerValue) : []),
    [board, game, isLatest],
  );

  const evaluationQuery = useEvaluation(game ? board : undefined, evalEngineId || undefined);

  const status = describeStatus(game, session?.humanPlayerId, opponentName);
  const isMyTurn = game !== undefined && game.waitingForPlayerId === session?.humanPlayerId;
  const boardDisabled =
    game === undefined || game.state.isGameOver || !isMyTurn || isMoving || !isLatest;

  const currentTurn =
    game && game.waitingForPlayerId
      ? glyphForValue(markerFor(game, game.waitingForPlayerId))
      : "X";

  const handleCellClick = useCallback(
    (x: number, y: number) => {
      if (boardDisabled) return;
      submitMove({ x, y });
    },
    [boardDisabled, submitMove],
  );

  const handleNewGame = useCallback(() => {
    const capability = findCapability(capabilities, playEngineId || undefined);
    if (capability === undefined) return;
    setPinnedIndex(null);
    startGame(capability.playerId);
  }, [capabilities, playEngineId, startGame]);

  const handleUndo = useCallback(
    () => setPinnedIndex((index) => Math.max(0, (index ?? moves.length) - 1)),
    [moves.length],
  );

  // Stepping back onto the newest move re-attaches to the live position.
  const handleRedo = useCallback(
    () => setPinnedIndex((index) => (index === null || index + 1 >= moves.length ? null : index + 1)),
    [moves.length],
  );

  const evaluation = evaluationQuery.data ?? 0;
  const evaluationUnavailable = game === undefined || evaluationQuery.isError || evaluationQuery.data === undefined;

  return (
    <AppLayout>
      <div className="flex flex-col gap-6 max-w-5xl mx-auto animate-slide-in">
        <div className="flex gap-4 md:gap-6 items-stretch justify-center">
          <div className="hidden sm:flex" style={{ minHeight: 360 }}>
            <EvalBar
              evaluation={evaluation}
              engineName={engineOptions.find((e) => e.id === evalEngineId)?.name}
              engines={engineOptions}
              evalEngine={evalEngineId}
              onEvalEngineChange={setChosenEvalEngineId}
              unavailable={evaluationUnavailable}
            />
          </div>

          <div className="flex-shrink-0">
            <TicTacToeBoard
              board={board}
              onCellClick={handleCellClick}
              disabled={boardDisabled}
              winningCells={winningCells}
            />
          </div>

          <div className="w-32 md:w-40 flex-shrink-0 flex flex-col gap-3">
            <GameControls
              onNewGame={handleNewGame}
              onUndo={handleUndo}
              onRedo={handleRedo}
              canUndo={viewIndex > 0}
              canRedo={viewIndex < moves.length}
              currentTurn={currentTurn}
              status={isStarting ? "Starting..." : status}
              newGameDisabled={isStarting || playEngineId === ""}
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
                  width: `${((evaluationUnavailable ? 0 : evaluation) + 1) * 50}%`,
                  backgroundColor: tokens.colorBrandBackground2,
                  transition: "width 0.7s ease-out",
                }}
              />
              <div className="absolute inset-0 flex items-center justify-center">
                <Text size={100} weight="bold">
                  {evaluationUnavailable
                    ? "--"
                    : evaluation > 0
                    ? `+${evaluation.toFixed(2)}`
                    : evaluation.toFixed(2)}
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
            {moves.map((move, index) => {
              const glyph = glyphForValue(move.value);
              const isCurrent = index === viewIndex - 1;
              return (
                <Badge
                  key={move.id}
                  appearance={isCurrent ? "filled" : "outline"}
                  color={isCurrent ? "brand" : glyph === "X" ? "informative" : "danger"}
                  size="medium"
                  style={{ fontFamily: "ui-monospace, monospace" }}
                >
                  {glyph}({move.x + 1},{move.y + 1})
                </Badge>
              );
            })}
            {moves.length === 0 && (
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
            engines={engineOptions}
            playEngine={playEngineId}
            onPlayEngineChange={setChosenPlayEngineId}
            playerId={session?.humanPlayerId}
          />
        </DrawerBody>
      </Drawer>
    </AppLayout>
  );
};

export default Index;
