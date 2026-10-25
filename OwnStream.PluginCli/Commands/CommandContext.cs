using System.CommandLine;

namespace OwnStream.PluginCli.Commands;

public class CommandContext(ParseResult pr)
{
	public ParseResult ParseResult { get; } = pr;

	public Dictionary<string, string> Arguments { get; set; } = [];

	public DirectoryInfo ProjectPath { get; } =
		new(pr.GetRequiredValue<DirectoryInfo>(GlobalArguments.ProjectOption).FullName);
}