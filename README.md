# Unofficial MicroSculpture Downloader
This program assists in downloading photographs from [MicroSculpture](http://microsculpture.net/).

## Usage
- Compile the project for .Net Framework 4.6.2.
- Execute to download all insect images listed in Program.cs

## Roadmap
Even though its sufficient for getting some nice wallpapers, there is some room for improvement.

### Switch to .Net Core 2
Once ImageSharp is released and reliably parses JPGs it can be used exclusively. System.Drawing references can be dropped and the project can be switched back to .Net Core 2.

### CLI/GUI
Changes in functionality have to be hard coded right now. Some core functionality can be made accessible by CLI/GUI:
- Download with insect name and resolution option
- Batch download with list of insects/resolutions
- Options to download all insects/resolutions
- Option to specify output image format
