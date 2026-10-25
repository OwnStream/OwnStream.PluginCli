using System.CommandLine;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using OwnStream.PluginCli.Attributes;
using OwnStream.PluginCore.ResultTypes;
using OwnStream.PluginCli;
using OwnStream.PluginCore;

namespace OwnStream.PluginCli.Commands;

[Command("run", "Run a feature of the plugin")]
public class RunFeature : ICommand
{
	private readonly Argument<string> featureArgument = new("feature")
	{
		Description = "The feature to run"
	};

	private static JsonSerializerOptions opts = new()
	{
		WriteIndented = true,
		IndentCharacter = '\t',
		IndentSize = 1,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true
	};

	public void Configure(Command command)
	{
		command.Arguments.Add(featureArgument);
		command.TreatUnmatchedTokensAsErrors = false;
	}

	public async Task<CommandResult> Run(CommandContext context)
	{
		using PluginInstance plugin = new(Path.Join(context.ProjectPath.FullName, "code.js"),
			JsonSerializer.Deserialize<PluginMeta>(
				await File.ReadAllTextAsync(Path.Join(context.ProjectPath.FullName, "meta.json")))!);

		string feature = context.ParseResult.GetRequiredValue(featureArgument);

		object? result;
		try
		{
			string[] rawFeatureArgs = context.ParseResult.UnmatchedTokens.ToArray();
			object[] convertedArgs = ConvertFeatureArgs(feature, rawFeatureArgs);
			result = await plugin.InvokeFeature(feature, convertedArgs);
		}
		catch (Exception e)
		{
			Console.Error.WriteLine(e);
			result = null;
		}

		if (result == null)
		{
			return new CommandResult
			{
				RawResult = new { },
				Result = "Code returned null result.",
				Success = true
			};
		}

		string resultString = JsonSerializer.Serialize(result, opts);

		switch (feature)
		{
			case "metadata.search":
			{
				StringBuilder sb = new();
				MetadataSearchResult[] results =
					JsonSerializer.Deserialize<MetadataSearchResult[]>(resultString, opts)!;
				sb.AppendLine($"Search results: ({results.Length})");
				sb.AppendLine("============================");
				foreach (MetadataSearchResult r in results)
				{
					sb.AppendLine($"[{r.Id}] {r.Titles[0]}")
						.AppendLine($"Year: {r.Year}")
						.AppendLine($"Poster URL: {r.Poster}");
					if (r.Titles.Length <= 1)
					{
						sb.AppendLine();
						continue;
					}

					sb.AppendLine($"Alternative Titles: ({r.Titles.Length - 1})");
					foreach (string t in r.Titles.Skip(1))
					{
						sb.AppendLine($"- {t}");
					}

					sb.AppendLine();
				}

				resultString = sb.ToString();
				break;
			}

			case "metadata.getMovie":
			{
				StringBuilder sb = new();
				MetadataMovieResult r = JsonSerializer.Deserialize<MetadataMovieResult>(resultString, opts)!;

				sb.AppendLine($"{r.Title}");
				if (!string.IsNullOrEmpty(r.Tagline))
					sb.AppendLine($"{r.Tagline}");
				if (!string.IsNullOrEmpty(r.Description))
					sb.AppendLine("Description:\n" + r.Description);
				if (!string.IsNullOrEmpty(r.ReleaseDate))
					sb.AppendLine($"Released: {r.ReleaseDate}");
				if (r.AgeRatings?.Count > 0)
				{
					sb.AppendLine($"Age ratings: ({r.AgeRatings.Count})");
					foreach ((string region, string rating) in r.AgeRatings)
						sb.AppendLine($"- [{region}] {rating}");
				}

				if (r.TitleTranslations?.Count > 0)
				{
					sb.AppendLine($"Title Translations: ({r.TitleTranslations.Count})");
					foreach ((string key, string value) in r.TitleTranslations!.Where(x =>
						         !string.IsNullOrEmpty(x.Value)))
						sb.AppendLine($"- [{key}] {value}");
				}

				if (r.TaglineTranslations?.Count > 0)
				{
					sb.AppendLine($"Tagline Translations: ({r.TaglineTranslations.Count})");
					foreach ((string key, string value) in r.TaglineTranslations!.Where(x =>
						         !string.IsNullOrEmpty(x.Value)))
						sb.AppendLine($"- [{key}] {value}");
				}

				if (r.DescriptionTranslations?.Count > 0)
				{
					sb.AppendLine($"Description Translations: ({r.DescriptionTranslations.Count})");
					foreach ((string key, string value) in r.DescriptionTranslations!.Where(x =>
						         !string.IsNullOrEmpty(x.Value)))
						sb.AppendLine($"- [{key}] {value}");
				}

				if (r.Images?.Count > 0)
				{
					sb.AppendLine("Images:");
					foreach ((string type, MetadataImage[] images) in r.Images)
					{
						if (images.Length == 0) continue;
						sb.AppendLine($"- {type}: ({images.Length})");
						foreach (MetadataImage image in images)
						{
							sb.AppendLine($"  - {image.Url}");
							if (!string.IsNullOrWhiteSpace(image.Locale))
								sb.AppendLine($"    Locale: {image.Locale}");
						}
					}
				}

				resultString = sb.ToString();
				break;
			}

			case "metadata.getTv":
			{
				StringBuilder sb = new();
				MetadataTvResult r = JsonSerializer.Deserialize<MetadataTvResult>(resultString, opts)!;

				sb.AppendLine($"{r.Title}");
				if (!string.IsNullOrEmpty(r.Tagline))
					sb.AppendLine($"{r.Tagline}");
				if (!string.IsNullOrEmpty(r.Description))
					sb.AppendLine("Description:\n" + r.Description);
				if (!string.IsNullOrEmpty(r.ReleaseDate))
					sb.AppendLine($"Released: {r.ReleaseDate}");
				if (!string.IsNullOrEmpty(r.FinishDate))
					sb.AppendLine($"Finished Airing: {r.FinishDate}");
				if (r.AgeRatings?.Count > 0)
				{
					sb.AppendLine($"Age ratings: ({r.AgeRatings.Count})");
					foreach ((string region, string rating) in r.AgeRatings)
						sb.AppendLine($"- [{region}] {rating}");
				}

				if (r.TitleTranslations?.Count > 0)
				{
					sb.AppendLine($"Title Translations: ({r.TitleTranslations.Count})");
					foreach ((string key, string value) in r.TitleTranslations!.Where(x =>
						         !string.IsNullOrEmpty(x.Value)))
						sb.AppendLine($"- [{key}] {value}");
				}

				if (r.TaglineTranslations?.Count > 0)
				{
					sb.AppendLine($"Tagline Translations: ({r.TaglineTranslations.Count})");
					foreach ((string key, string value) in r.TaglineTranslations!.Where(x =>
						         !string.IsNullOrEmpty(x.Value)))
						sb.AppendLine($"- [{key}] {value}");
				}

				if (r.DescriptionTranslations?.Count > 0)
				{
					sb.AppendLine($"Description Translations: ({r.DescriptionTranslations.Count})");
					foreach ((string key, string value) in r.DescriptionTranslations!.Where(x =>
						         !string.IsNullOrEmpty(x.Value)))
						sb.AppendLine($"- [{key}] {value}");
				}

				if (r.Images?.Count > 0)
				{
					sb.AppendLine("Images:");
					foreach ((string type, MetadataImage[] images) in r.Images)
					{
						if (images.Length == 0) continue;
						sb.AppendLine($"- {type}: ({images.Length})");
						foreach (MetadataImage image in images)
						{
							sb.AppendLine($"  - {image.Url}");
							if (!string.IsNullOrWhiteSpace(image.Locale))
								sb.AppendLine($"    Locale: {image.Locale}");
						}
					}
				}

				resultString = sb.ToString();
				break;
			}

			case "metadata.getSeasons":
			{
				StringBuilder sb = new();
				MetadataSeasonListResult[] results =
					JsonSerializer.Deserialize<MetadataSeasonListResult[]>(resultString, opts)!;

				sb.AppendLine($"Seasons list: ({results.Length})");
				sb.AppendLine("============================");
				foreach (MetadataSeasonListResult r in results)
				{
					sb.AppendLine($"[{r.Id}] S{r.Index:-01} {r.Name}");
					if (r.Posters != null)
					{
						sb.AppendLine($"Poster URLs: ({r.Posters.Length})");
						foreach (MetadataImage image in r.Posters)
						{
							sb.AppendLine($"  - {image.Url}");
							if (!string.IsNullOrWhiteSpace(image.Locale))
								sb.AppendLine($"    Locale: {image.Locale}");
						}
					}

					sb.AppendLine();
				}

				resultString = sb.ToString();
				break;
			}

			case "metadata.getSeason":
			{
				StringBuilder sb = new();
				MetadataSeasonResult r = JsonSerializer.Deserialize<MetadataSeasonResult>(resultString, opts)!;

				sb.AppendLine($"{r.Title}");
				if (!string.IsNullOrEmpty(r.Description))
					sb.AppendLine("Description:\n" + r.Description);
				if (r.Posters != null)
				{
					sb.AppendLine($"Posters: ({r.Posters.Length})");
					foreach (MetadataImage image in r.Posters)
					{
						sb.AppendLine($"  - {image.Url}");
						if (!string.IsNullOrWhiteSpace(image.Locale))
							sb.AppendLine($"    Locale: {image.Locale}");
					}
				}

				resultString = sb.ToString();
				break;
			}

			case "metadata.getEpisode":
			{
				StringBuilder sb = new();
				MetadataEpisodeResult r = JsonSerializer.Deserialize<MetadataEpisodeResult>(resultString, opts)!;

				sb.AppendLine($"{r.Title}");
				if (!string.IsNullOrEmpty(r.Summary))
					sb.AppendLine("Summary:\n" + r.Summary);
				if (!string.IsNullOrEmpty(r.ReleaseDate))
					sb.AppendLine($"Released: {r.ReleaseDate}");
				if (r.Thumbnails != null)
				{
					sb.AppendLine($"Thumbnails: ({r.Thumbnails.Length})");
					foreach (MetadataImage image in r.Thumbnails)
					{
						sb.AppendLine($"  - {image.Url}");
						if (!string.IsNullOrWhiteSpace(image.Locale))
							sb.AppendLine($"    Locale: {image.Locale}");
					}
				}
				
				if (r.TitleTranslations?.Count > 0)
				{
					sb.AppendLine($"Title Translations: ({r.TitleTranslations.Count})");
					foreach ((string key, string value) in r.TitleTranslations!.Where(x =>
						         !string.IsNullOrEmpty(x.Value)))
						sb.AppendLine($"- [{key}] {value}");
				}

				if (r.SummaryTranslations?.Count > 0)
				{
					sb.AppendLine($"Tagline Translations: ({r.SummaryTranslations.Count})");
					foreach ((string key, string value) in r.SummaryTranslations!.Where(x =>
						         !string.IsNullOrEmpty(x.Value)))
						sb.AppendLine($"- [{key}] {value}");
				}

				resultString = sb.ToString();
				break;
			}
		}

		return new CommandResult
		{
			RawResult = new
			{
				success = true,
				result = result,
			},
			Result = resultString,
			Success = true
		};
	}

	private static object[] ConvertFeatureArgs(string feature, string[] tokens)
	{
		return feature switch
		{
			"metadata.search" => ConvertMetadataSearchArgs(tokens),
			"metadata.getMovie" or "metadata.getTv" or "metadata.getSeason" or "metadata.getSeasons"
				or "metadata.getEpisode" =>
				ConvertGetMetadataArgs(tokens, feature),
			_ => tokens
		};
	}

	private static object[] ConvertMetadataSearchArgs(string[] tokens)
	{
		Dictionary<string, string?> args = ParseOptions(tokens);

		string query = Require(args, "-q", "--query");
		string type = Require(args, "-t", "--type");
		string yearText = Require(args, "-y", "--year");

		if (type is not "Movie" and not "Tv")
			throw new ArgumentException("metadata.search requires -t/--type to be either 'movie' or 'tv'.");

		if (!int.TryParse(yearText, out int parsedYear))
			throw new ArgumentException("metadata.search requires -y/--year to be a valid year.");

		return
		[
			type,
			query,
			parsedYear
		];
	}

	private static object[] ConvertGetMetadataArgs(string[] tokens, string feature)
	{
		Dictionary<string, string?> args = ParseOptions(tokens);

		string id = Require(args, "-id", "--id");
		string? season = RequireIf(feature is "metadata.get.season" or "metadata.get.episode", args, "-s", "--season");
		string? episode = RequireIf(feature == "metadata.get.episode", args, "-e", "--episode");

		return new[]
		{
			id,
			season,
			episode
		}.Cast<object>().ToArray();
	}

	private static Dictionary<string, string?> ParseOptions(string[] tokens)
	{
		Dictionary<string, string?> result = new(StringComparer.OrdinalIgnoreCase);

		for (int i = 0; i < tokens.Length; i++)
		{
			string token = tokens[i];

			if (!token.StartsWith('-'))
				throw new ArgumentException($"Unexpected argument '{token}'.");

			string key;
			string? value;

			int equalsIndex = token.IndexOf('=');
			if (equalsIndex >= 0)
			{
				key = token[..equalsIndex];
				value = token[(equalsIndex + 1)..];
			}
			else
			{
				key = token;

				if (i + 1 >= tokens.Length || tokens[i + 1].StartsWith('-'))
					throw new ArgumentException($"Missing value for option '{key}'.");

				value = tokens[++i];
			}

			result[key] = value;
		}

		return result;
	}

	private static string? RequireIf(bool condition, Dictionary<string, string?> args, params string[] names) =>
		condition ? Require(args, names) : Optional(args, names);

	private static string Require(Dictionary<string, string?> args, params string[] names)
	{
		string? value = Optional(args, names);

		return string.IsNullOrWhiteSpace(value)
			? throw new ArgumentException($"Missing required option: {string.Join("/", names)}.")
			: value;
	}

	private static string? Optional(Dictionary<string, string?> args, params string[] names)
	{
		foreach (string name in names)
		{
			if (args.TryGetValue(name, out string? value))
				return value;
		}

		return null;
	}
}