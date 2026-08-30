import { useState } from "react";
import { Button, tokens } from "@fluentui/react-components";
import { EMPTY_CELL, boardSize, glyphForValue, type Cell } from "@/game/board";

interface TicTacToeBoardProps {
  /** Server-native board: 0 is empty, other values are the owning player's marker. */
  board: number[][];
  onCellClick: (x: number, y: number) => void;
  disabled?: boolean;
  winningCells?: Cell[];
}

const TicTacToeBoard = ({ board, onCellClick, disabled, winningCells = [] }: TicTacToeBoardProps) => {
  const [hoveredCell, setHoveredCell] = useState<string | null>(null);
  const { cols } = boardSize(board);

  const renderCell = (x: number, y: number) => {
    const key = `${x}-${y}`;
    const value = board[x][y];
    const isWinning = winningCells.some((cell) => cell.x === x && cell.y === y);
    const isEmpty = value === EMPTY_CELL;
    const glyph = glyphForValue(value);

    const xColor = tokens.colorPaletteBlueForeground2;
    const oColor = tokens.colorPaletteBerryForeground1;

    return (
      <Button
        key={key}
        appearance="subtle"
        shape="rounded"
        onClick={() => isEmpty && !disabled && onCellClick(x, y)}
        onMouseEnter={() => setHoveredCell(key)}
        onMouseLeave={() => setHoveredCell(null)}
        disabled={disabled || !isEmpty}
        style={{
          aspectRatio: "1 / 1",
          height: "auto",
          minWidth: 0,
          fontSize: "2.5rem",
          fontFamily: "ui-monospace, SFMono-Regular, monospace",
          fontWeight: 700,
          backgroundColor: isWinning ? tokens.colorBrandBackground2 : tokens.colorNeutralBackground3,
          border: `1px solid ${isWinning ? tokens.colorBrandStroke1 : tokens.colorNeutralStroke2}`,
          boxShadow: isWinning
            ? `0 0 24px ${tokens.colorBrandBackground2Hover}`
            : undefined,
          color: glyph === "X" ? xColor : glyph === "O" ? oColor : tokens.colorNeutralForeground3,
          transition: "transform 0.15s ease",
        }}
      >
        {!isEmpty ? (
          <span className="animate-cell-pop">{glyph}</span>
        ) : hoveredCell === key && !disabled ? (
          <span style={{ fontSize: "1.5rem", opacity: 0.25 }}>·</span>
        ) : null}
      </Button>
    );
  };

  return (
    // utilities.css only defines grid-cols-3, so the column count is set inline to
    // stay correct for whatever board size the server hands back.
    <div
      className="gap-2 w-[360px] max-w-full aspect-square"
      style={{ display: "grid", gridTemplateColumns: `repeat(${cols}, 1fr)` }}
    >
      {board.map((row, x) => row.map((_, y) => renderCell(x, y)))}
    </div>
  );
};

export default TicTacToeBoard;
