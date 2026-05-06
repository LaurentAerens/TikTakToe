namespace TikTakToe.Engines.Exceptions;

public class NoMoveAvailableException : InvalidOperationException
{
    public NoMoveAvailableException()
        : base("No valid moves available. The board is full or no empty squares exist.")
    {
    }
}
