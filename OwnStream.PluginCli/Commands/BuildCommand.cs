using System.CommandLine;
using System.IO.Compression;
using System.Text.Json;
using OwnStream.PluginCli.Attributes;
using OwnStream.PluginCore;

namespace OwnStream.PluginCli.Commands;

[Command("build", "Compiles the plugin in a single .zip file, ready to be installed (or served)")]
public class BuildCommand : ICommand
{
	public void Configure(Command command)
	{
	}

	public async Task<CommandResult> Run(CommandContext context)
	{
		BuildResult br;

		try
		{
			List<string> warnings = [];
			PluginMeta? pluginMeta =
				JsonSerializer.Deserialize<PluginMeta>(await File.ReadAllTextAsync(Path.Join(
					context.ProjectPath.FullName,
					"meta.json")));
			if (pluginMeta == null)
				throw new Exception("Parsing returned null");

			await using FileStream zipStream =
				new(
					$"{pluginMeta.Id}-{pluginMeta.Version.Code}_{pluginMeta.Version.Name}.zip",
					FileMode.Create);
			await using ZipArchive zip = new(zipStream, ZipArchiveMode.Create, true);

			zip.Comment = JsonSerializer.Serialize(pluginMeta);
			if (!AddFile(zip, context.ProjectPath.FullName, pluginMeta.Index))
				throw new Exception($"File '{pluginMeta.Index}' not found. Are you sure you referenced the correct file in meta.json?");
			if (string.IsNullOrWhiteSpace(pluginMeta.Image) && !AddFile(zip, context.ProjectPath.FullName, "icon.png"))
				warnings.Add("icon.png not found");

			br = new BuildResult(true, "Build successful.", warnings);
		}
		catch (Exception e)
		{
			br = new BuildResult(false, "Failed to build plugin.\n" + e.Message);
		}

		return new CommandResult
		{
			RawResult = br,
			Result = (string.Join("\n", br.Warnings.Select(x => $"[WARN] {x}")) + "\n\n" + br.Message).Trim(),
			Success = br.Success
		};
	}

	private static bool AddFile(ZipArchive archive, string projectPath, string fileName)
	{
		if (!File.Exists(fileName)) return false;
		ZipArchiveEntry entry = archive.CreateEntry(fileName);
		using Stream stream = entry.Open();
		using FileStream fileStream = File.OpenRead(Path.Join(projectPath, fileName));
		fileStream.CopyTo(stream);
		return true;
	}

	class BuildResult(bool success, string message, List<string>? warnings = null)
	{
		public bool Success { get; set; } = success;
		public string Message { get; set; } = message;
		public List<string> Warnings { get; set; } = warnings ?? [];
	}
}