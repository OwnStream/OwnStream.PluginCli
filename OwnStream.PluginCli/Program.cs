using System.CommandLine;
using System.Reflection;
using System.Text.Json;
using OwnStream.PluginCli;
using OwnStream.PluginCli.Attributes;
using OwnStream.PluginCli.Commands;

RootCommand rootCommand = new("OwnStream Plugin CLI")
{
	GlobalArguments.ProjectOption,
	GlobalArguments.JsonOption
};

foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()
	         .Where(x => x.IsClass && x.IsAssignableTo(typeof(ICommand))))
{
	CommandAttribute? attr = type.GetCustomAttribute<CommandAttribute>();
	if (attr == null) continue;

	if (Activator.CreateInstance(type) is not ICommand obj)
	{
		Console.WriteLine($"Failed to create command instance for {type.Name}");
		continue;
	}

	Command command = new(attr.Name, attr.Description);

	obj.Configure(command);

	command.SetAction(async pr =>
	{
		CommandContext context = new(pr);

		CommandResult result = await obj.Run(context);
		bool outputJson = pr.GetValue(GlobalArguments.JsonOption);
		if (outputJson)
		{
			JsonSerializerOptions options = new()
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
			Console.WriteLine(JsonSerializer.Serialize(result.RawResult, options));
			return 0;
		}

		Console.WriteLine(result.Result);
		return 0;
	});

	rootCommand.Subcommands.Add(command);
}

return await rootCommand.Parse(args).InvokeAsync();