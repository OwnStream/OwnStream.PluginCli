namespace OwnStream.PluginCli.Commands;

public class CommandResult
{
	/**
	 * Used for JSON outputs
	 */
	public required object RawResult { get; set; }

	/**
	 * To printing directly into stdout
	 */
	public required string Result { get; set; }
	
	public bool Success { get; set; }
}