namespace TikTakToe.Engines.Exceptions;

public class BoardSizeNotSupportedException : InvalidOperationException
{
    public BoardSizeNotSupportedException(string engineName, int rows, int cols)
        : base($"The {engineName} engine does not support board size {rows}x{cols}.")
    {
    }
}
