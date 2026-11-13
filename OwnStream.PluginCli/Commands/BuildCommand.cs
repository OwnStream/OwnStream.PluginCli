using System.CommandLine;
using System.IO.Compression;
using System.Text.Json;
using OwnStream.PluginCli.Attributes;
using OwnStream.PluginCore;

namespace OwnStream.PluginCli.Commands;

[Command("build", "Compiles the plugin in a single .zip file, ready to be installed (or served)")]
public class BuildCommand : ICommand
{
	private readonly Option<bool> allArgument = new("--all", "-a")
	{
		Description = "Build all packages in the subdirectories of current directory",
		Required = false
	};

	private readonly Option<DirectoryInfo> outputArgument = new("--output", "-o")
	{
		Description = "Output build files to the following directory",
		Required = false,
		DefaultValueFactory = _ => new DirectoryInfo(Directory.GetCurrentDirectory())
	};

	public void Configure(Command command)
	{
		command.Add(allArgument);
		command.Add(outputArgument);
	}

	public async Task<CommandResult> Run(CommandContext context)
	{
		BuildResult br;

		List<string> warnings = [];
		DirectoryInfo outDir = context.ParseResult.GetRequiredValue(outputArgument);
		if (context.ParseResult.GetRequiredValue(allArgument))
		{
			DirectoryInfo[] dirs = context.ProjectPath.GetDirectories().Where(x =>
				!x.Name.StartsWith('.') && !x.Attributes.HasFlag(FileAttributes.Hidden) &&
				x.FullName != outDir.FullName).ToArray();
			foreach (DirectoryInfo dir in dirs)
			{
				try
				{
					string[] warns = await BuildPlugin(dir.Name, outDir);
					warnings.AddRange(warns.Select(x => $".[{dir.Name}] {x}"));
				}
				catch (Exception e)
				{
					warnings.Add($"![{dir.Name}] Build failed: {e.Message}");
				}
			}

			int failed = warnings.Count(x => x.StartsWith('!'));
			int success = dirs.Length - failed;

			br = new BuildResult(true, $"Finished building plugins: {success} successful, {failed} failed.",
				warnings.Select(x => x[1..]));
		}
		else
		{
			try
			{
				string[] warns = await BuildPlugin(context.ProjectPath.FullName, outDir);
				br = new BuildResult(true, "Build successful", warns);
			}
			catch (Exception e)
			{
				br = new BuildResult(false, "Failed to build plugin.\n" + e.Message);
			}
		}

		return new CommandResult
		{
			RawResult = br,
			Result = (string.Join("\n", br.Warnings.Select(x => $"[WARN] {x}")) + "\n\n" + br.Message).Trim(),
			Success = br.Success
		};
	}

	private async Task<string[]> BuildPlugin(string dir, DirectoryInfo outDir)
	{
		List<string> warnings = [];
		PluginMeta? pluginMeta =
			JsonSerializer.Deserialize<PluginMeta>(await File.ReadAllTextAsync(Path.Join(dir, "meta.json")));
		if (pluginMeta == null)
			throw new Exception("Parsing returned null");

		await using FileStream zipStream =
			new(Path.Join(outDir.FullName, $"{pluginMeta.Id}-{pluginMeta.Version.Code}_{pluginMeta.Version.Name}.zip"),
				FileMode.Create);
		await using ZipArchive zip = new(zipStream, ZipArchiveMode.Create, true);

		zip.Comment = JsonSerializer.Serialize(pluginMeta);
		if (!AddFile(zip, dir, pluginMeta.Index))
			throw new Exception(
				$"File '{pluginMeta.Index}' not found. Are you sure you referenced the correct file in meta.json?");
		if (string.IsNullOrWhiteSpace(pluginMeta.Image) && !AddFile(zip, dir, "icon.png"))
			warnings.Add("icon.png not found");

		return warnings.ToArray();
	}

	private static bool AddFile(ZipArchive archive, string projectPath, string fileName)
	{
		string filePath = Path.Join(projectPath, fileName);
		if (!File.Exists(filePath)) return false;
		ZipArchiveEntry entry = archive.CreateEntry(fileName);
		using Stream stream = entry.Open();
		using FileStream fileStream = File.OpenRead(filePath);
		fileStream.CopyTo(stream);
		return true;
	}

	class BuildResult(bool success, string message, IEnumerable<string>? warnings = null)
	{
		public bool Success { get; set; } = success;
		public string Message { get; set; } = message;
		public IEnumerable<string> Warnings { get; set; } = warnings ?? [];
	}
}