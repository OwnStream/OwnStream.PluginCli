# OwnStream.PluginCli

A copy of OwnStream's plugin runtime, working in the CLI. Made to quickly test plugins during development.
Designed to be used with the VSCode Plugin.

## Installation

CI builds are self-contained executables. Simply download the latest version, and call it through the CLI.

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
| Get TV show seasons      | `metadata.getSeasons` | `-id/--id`: ID of the TV Series to get the seasons of                                                                                                         |
| Get TV show season info  | `metadata.getSeason`  | `-id/--id`: ID of the TV Series to get the metadata of, `-s/--season`: Season index of the requested item                                                     |
| Get TV show episode info | `metadata.getEpisode` | `-id/--id`: ID of the TV Series to get the metadata of, `-s/--season`: Season index of the requested item, `-e/--episode` Episode index of the requested item |

## TODO:
- [ ] `build` command to package a plugin
- [ ] `runTests` to run tests for the plugin - defined by a `_tests.json` file in the project root