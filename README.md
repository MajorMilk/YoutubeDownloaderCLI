# YoutubeDownloaderCLI

Designed as a CLI alternative for my other project 'YouTube -To-MPX'

## Installation Steps

1. Download FFMPEG and add it to your PATH variable
2. Build the source code with the latest version of dotnet
3. add the dir containing resulting exe files to your PATH variable
4. Done, simply type **"YoutubeDownloader {YouTube link}"** or **"PlaylistDownloader {playlist link}"**

Complete MP4's can be downloaded by adding any additional argument to the YoutubeDownloader

Example: YoutubeDownloader {link} **v**

All files are downloaded to your downloads folder in a folder called 'YoutubeDownloads'

Files are first downloaded as webm's directly from YouTube, then converted into m4a format.

A JSON file containing metadata is also written to the drive.

**TODO**:
Auto Metadata entry via ffmpeg


## Libraries used

- Newtonsoft.JSON
- Cli.Wrap
- YoutubeExplode