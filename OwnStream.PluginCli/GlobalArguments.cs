using System.CommandLine;

namespace OwnStream.PluginCli;

public static class GlobalArguments
{
	public const string ProjectDirectoryEnvironmentVariable = "OSPCLI_PROJECT_DIR";

	public static Option<DirectoryInfo> ProjectOption = new("--project", "-p")
	{
		Description = $"Path to the project directory. Defaults to ${ProjectDirectoryEnvironmentVariable}, then the current directory.",
		Required = false,
		DefaultValueFactory = _ =>
		{
			string? projectDirectory = Environment.GetEnvironmentVariable(ProjectDirectoryEnvironmentVariable);

			return new DirectoryInfo(string.IsNullOrWhiteSpace(projectDirectory)
				? Directory.GetCurrentDirectory()
				: projectDirectory);
		}
	};

	public static Option<bool> JsonOption = new("--json", "-j")
	{
		Description = "Output result in JSON",
		Required = false
	};
}