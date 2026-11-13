# OwnStream.PluginCli

A copy of OwnStream's plugin runtime, working in the CLI. Made to quickly test plugins during development.
Designed to be used with the VSCode Plugin.

## Installation

CI builds are self-contained executables. Simply download the latest version, and call it through the CLI.

## Building

Since OwnStream.PluginCore is published in GitHub Packages, you have to provide an access token with packages.read
permission to download it. Before building, create a `nuget.config` file next to `OwnStream.PluginCli.sln` with the
following contents:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <clear/>
        <add key="github" value="https://nuget.pkg.github.com/OwnStream/index.json"/>
    </packageSources>
    <packageSourceCredentials>
        <github>
            <add key="Username" value="YOUR_GITHUB_USERNAME"/>
            <add key="ClearTextPassword" value="YOUR_PERSONAL_ACCESS_TOKEN"/>
        </github>
    </packageSourceCredentials>
</configuration>
```

## Usage

`OwnStream.PluginCli [global arguments] <command> [arguments]`

### Global arguments

- **`-p`/`--project`**: Path to the project directory. Defaults to the `OSPCLI_PROJECT_DIR` environment variable, then
  the current directory.
- **`-j`/`--json`**: Output the result in JSON instead of human-readable text.

### `testMeta`

Validates the meta.json file, and outputs the values in a human-readable format.

### `run`

Runs the specified feature of the plugin. Available features are:

| Name                     | Argument              | Required arguments                                                                                                                                            |
|--------------------------|-----------------------|:--------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Search                   | `metadata.search`     | `-t/--type`: `Movie` or `Tv`, `-q/--query`: Search query, `-y/--year`: Search year                                                                            |
| Get Movie                | `metadata.getMovie`   | `-id/--id`: ID of the movie to get                                                                                                                            |
| Get TV show              | `metadata.getTv`      | `-id/--id`: ID of the TV Series to get                                                                                                                        |
| Get TV show seasons      | `metadata.getTvSeasons` | `-id/--id`: ID of the TV Series to get the seasons of                                                                                                         |
| Get TV show season info  | `metadata.getTvSeason`  | `-id/--id`: ID of the TV Series to get the metadata of, `-s/--season`: Season index of the requested item                                                     |
| Get TV show episode info | `metadata.getTvEpisode` | `-id/--id`: ID of the TV Series to get the metadata of, `-s/--season`: Season index of the requested item, `-e/--episode` Episode index of the requested item |

### `build`

Builds the plugin into a package that can be installed in OwnStream.

- **`-a/--all`**: Instead of building the plugin at the project path, builds all plugins in the subdirectories.
- **`-o/--out <path>`**: Puts the output files in the specified directory (default: current working directory)

## TODO:

- [x] `build` command to package a plugin
- [ ] `runTests` to run tests for the plugin - defined by a `_tests.json` file in the project root