import { useState } from "react";
import { Button, tokens } from "@fluentui/react-components";

export type CellValue = "X" | "O" | null;
export type BoardState = CellValue[];

interface TicTacToeBoardProps {
  board: BoardState;
  onCellClick: (index: number) => void;
  disabled?: boolean;
  winningCells?: number[];
}

const TicTacToeBoard = ({ board, onCellClick, disabled, winningCells = [] }: TicTacToeBoardProps) => {
  const [hoveredCell, setHoveredCell] = useState<number | null>(null);

  const renderCell = (index: number) => {
    const value = board[index];
    const isWinning = winningCells.includes(index);
    const isEmpty = value === null;

    const xColor = tokens.colorPaletteBlueForeground2;
    const oColor = tokens.colorPaletteBerryForeground1;

    return (
      <Button
        key={index}
        appearance="subtle"
        shape="rounded"
        onClick={() => isEmpty && !disabled && onCellClick(index)}
        onMouseEnter={() => setHoveredCell(index)}
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
          color: value === "X" ? xColor : value === "O" ? oColor : tokens.colorNeutralForeground3,
          transition: "transform 0.15s ease",
        }}
      >
        {value ? (
          <span className="animate-cell-pop">{value}</span>
        ) : hoveredCell === index && !disabled ? (
          <span style={{ fontSize: "1.5rem", opacity: 0.25 }}>·</span>
        ) : null}
      </Button>
    );
  };

  return (
    <div className="grid grid-cols-3 gap-2 w-[360px] max-w-full aspect-square">
      {Array.from({ length: 9 }, (_, i) => renderCell(i))}
    </div>
  );
};

export default TicTacToeBoard;
