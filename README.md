# YoutubeDownloaderCLI

A command-line tool for downloading YouTube videos and playlists. This tool serves as a CLI alternative to my other project, **YouTube-To-MPX**.

## Installation

### Prerequisites

1. **FFmpeg**: Download and install FFmpeg, then add it to your system's PATH
2. **.NET SDK**: Install the latest version of .NET SDK.

### Steps

1. Clone this repository.
2. Build the source code using the latest .NET version.
3. Add the directory containing the compiled executable files to your system's PATH.
4. Done! You can now use the tool from the command line.

## Adding things to your PATH variable

You can learn how to do this [here](https://stackoverflow.com/questions/44272416/how-to-add-a-folder-to-path-environment-variable-in-windows-10-with-screensho) on Windows and [here](https://linuxize.com/post/how-to-add-directory-to-path-in-linux/) on Linux.

## How do I use the new Spotify Metadata features?
1. Clone the dev branch of this repo and build the code 
2. Create an app on the spotify developers portal [here](https://developer.spotify.com)
3. Run the Spotify binary at least once, it will generate a json file in the same dir as the binary with places for you to put your client id and client secret
4. After placing your client id and secret into that json, simply run the exe, and the program will try and scrape metadata for all your m4a files

## Usage

To download a single YouTube video:

```sh
YoutubeDownloader "<YouTube Video URL>"
```

To download a full playlist:

```sh
PlaylistDownloader "<Playlist URL>"
```

To download a complete MP4 version, add an extra argument:

```sh
YoutubeDownloader "<YouTube Video URL>" v
```

### Output Details

- All downloaded files are saved in `~/Downloads/YoutubeDownloads`.
- Files are initially downloaded as **WebM** and converted to **M4A**.
- A JSON file containing metadata is also generated.

### Notes

- On **Linux**, you may need to manually create the `thumbnails` directory under the `json` directory.
- If using **Linux**, surround URLs with double quotes (`"`) to avoid shell-related issues.

## Building from Source

1. Clone the repository (use the `dev` branch for the latest features, though it may contain bugs):
   ```sh
   git clone https://github.com/your-repo/YoutubeDownloaderCLI.git
   ```
2. Navigate to the directory containing the solution file (`.sln`).
3. Run the following command to build the project:
   ```sh
   dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:EnableTrim=true -o ./Release
   ```
   - For Linux, replace `win-x64` with `linux-x64`.
4. The compiled executables will be available in the `./Release` folder.

## Dependencies

This project uses the following libraries:

- **Newtonsoft.JSON** (for JSON handling)
- **Cli.Wrap** (for executing external processes)
- **YoutubeExplode** (for downloading videos)

## Feedback & Contributions

Your feedback is greatly appreciated! I am particularly interested in:

- Preferred output formats
- Whether WebM files should be deleted after conversion
- Whether JSON metadata files should be deleted after processing
- Whether converted files should be saved in the Music folder instead

Pull requests are welcome! If you know of an API that provides better metadata retrieval based on title and author, please share it.
