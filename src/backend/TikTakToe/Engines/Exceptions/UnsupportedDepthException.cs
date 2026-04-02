namespace TikTakToe.Engines.Exceptions;

public class UnsupportedDepthException : InvalidOperationException
{
	public UnsupportedDepthException(string engineName)
		: base($"The {engineName} engine does not support depth-based analysis.")
	{
	}
}
