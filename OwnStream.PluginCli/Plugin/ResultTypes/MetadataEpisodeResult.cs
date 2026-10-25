namespace OwnStream.PluginCli.Plugin.ResultTypes;

public class MetadataEpisodeResult
{
	public string Title { get; set; }
	public Dictionary<string, string>? TitleTranslations { get; set; }
	public string? Summary { get; set; }
	public Dictionary<string, string>? SummaryTranslations { get; set; }
	public string? ReleaseDate { get; set; }
	public MetadataImage[]? Thumbnails { get; set; }
}