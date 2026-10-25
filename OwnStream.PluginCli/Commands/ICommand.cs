using System.CommandLine;

namespace OwnStream.PluginCli.Commands;

public interface ICommand
{
	public Task<CommandResult> Run(CommandContext context);
	public void Configure(Command command);
}