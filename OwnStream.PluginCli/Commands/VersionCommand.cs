using System.CommandLine;
using System.Reflection;
using OwnStream.PluginCli.Attributes;
using OwnStream.PluginCore;

namespace OwnStream.PluginCli.Commands;

[Command("version", "Get CLI & PluginCore version")]
public class VersionCommand : ICommand
{
	public void Configure(Command command)
	{
	}

	public Task<CommandResult> Run(CommandContext context)
	{
		string cliVersion = Assembly.GetAssembly(GetType())!.GetName().Version!.ToString(3);
		string coreVersion = Assembly.GetAssembly(typeof(PluginInstance))!.GetName().Version!.ToString(3);

		Dictionary<string, string> versions = new()
		{
			["cli"] = cliVersion,
			["core"] = coreVersion
		};
		
		return Task.FromResult(new CommandResult
		{
			RawResult = versions,
			Result = $"CLI Version: {cliVersion}\nPluginCore Version: {coreVersion}"
		});
	}

	public class MetaFileResult(bool success, string message)
	{
		public bool Success { get; } = success;
		public string Message { get; } = message;
		public PluginMeta? Meta { get; set; }
	}
}