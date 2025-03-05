﻿# YoutubeDownloaderCLI

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

## How do I build the source code?

1. Clone the master (or dev if u want the newest maybe still buggy features) branch of this repo
2. With the newest version of dotnet installed, go to the directory where you saved the cloned .sln file in a terminal of your choice
3. type 'dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:EnableTrim=true -o ./Release' then press enter -- replace win-x64 with linux-x64 for linux.
4. your exe's or binaries will be in ./Release

## Libraries used

- Newtonsoft.JSON
- Cli.Wrap
- YoutubeExplode
