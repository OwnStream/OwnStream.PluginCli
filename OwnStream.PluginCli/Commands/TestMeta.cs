using System.CommandLine;
using System.Text;
using System.Text.Json;
using OwnStream.PluginCli.Attributes;
using OwnStream.PluginCli;
using OwnStream.PluginCore;

namespace OwnStream.PluginCli.Commands;

[Command("testMeta", "Tests if the metadata check passes")]
public class TestMeta : ICommand
{
	public void Configure(Command command)
	{
	}

	public Task<CommandResult> Run(CommandContext context)
	{
		bool hasMetaFile = context.ProjectPath.GetFiles("meta.json").Any();
		MetaFileResult mfr;
		if (!hasMetaFile)
			mfr = new MetaFileResult(false, "meta.json file does not exist");

		try
		{
			PluginMeta? pluginMeta =
				JsonSerializer.Deserialize<PluginMeta>(File.ReadAllText(Path.Join(context.ProjectPath.FullName,
					"meta.json")));
			if (pluginMeta == null)
			{
				throw new Exception("meta.json file is invalid");
			}

			mfr = new MetaFileResult(true, "meta.json file is valid")
			{
				Meta = pluginMeta
			};
		}
		catch (Exception e)
		{
			mfr = new MetaFileResult(false, "Failed to parse the meta.json file.\n" + e.Message);
		}

		return Task.FromResult(new CommandResult
		{
			RawResult = mfr,
			Result = mfr.Success ? SerializePluginMeta(mfr.Meta!) : mfr.Message
		});
	}

	private string SerializePluginMeta(PluginMeta meta)
	{
		StringBuilder sb = new();
		sb
			.AppendLine("meta.json file is valid!")
			.AppendLine("========================")
			.AppendLine(meta.Name + $" ({meta.Version.Name} / {meta.Version.Code})")
			.AppendLine($"Icon Path: {meta.Image ?? "Doesn't have an icon"}")
			.AppendLine($"Permissions: ({meta.Permissions.Length})");
		foreach (string perm in meta.Permissions)
		{
			if (perm.StartsWith("https:") || perm.StartsWith("http:"))
				sb.AppendLine($"- Web access to: {perm}");
			else
				sb.AppendLine($"- {perm}");
		}

		sb.AppendLine($"Features: ({meta.Features.Length})");
		foreach (string feature in meta.Features)
			sb.AppendLine($"- {feature}");

		if (meta.AutoUpdater != null)
		{
			sb.AppendLine($"Auto Updater: {meta.AutoUpdater.Source}");
			foreach ((string key, string value) in meta.AutoUpdater.Params)
			{
				sb.AppendLine($"- {key}: {value}");
			}
		}
		
		if (meta.Homepage != null) sb.AppendLine($"Homepage: {meta.Homepage}");
		if (meta.Source != null) sb.AppendLine($"Source Code: {meta.Source}");
		if (meta.Issues != null) sb.AppendLine($"Issues: {meta.Issues}");

		return sb.ToString();
	}

	public class MetaFileResult(bool success, string message)
	{
		public bool Success { get; } = success;
		public string Message { get; } = message;
		public PluginMeta? Meta { get; set; }
	}
}