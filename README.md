# Unofficial MicroSculpture Downloader

This program assists in downloading the brilliant insect photographs from [MicroSculpture](http://microsculpture.net/).

## Please respect the artist

Obviously you do not gain any copyright on the pictures by using this software. So do not distribute or sell them!
If you enjoy the pictures consider supporting Levon Biss by visiting an exhibition or purchasing his book. You can find more information on [his website](https://www.levonbiss.com/).

## Usage

- Install [.Net Core](https://www.microsoft.com/net/learn/get-started/)
- Navigate into `MicroSculptureDownloader` folder
- Start the project with `dotnet run`

### Behaviour

By default the program will generate evenly cropped 4k wallpaper with a 100px border.

Images are only downloaded once, and then loaded from disk. To override this behaviour use the `--force` switch.

If you don't supply a resolution level and let the program generate a wallpaper, it will automatically find an appropriate resolution to download.

### CLI

These are the complete options:

```
Unofficial downloader for the brilliant insect photographs on http://microsculpture.net/

Usage: MicroSculptureDownloader [arguments] [options]

Arguments:
  insect            Insect to download

Options:
  -?|--help         Show help information
  -f|--force        Skip cache and force downloads.
  -l|--level        Request specific image resolution.
  -L|--list         List all insects.
  -r|--resolutions  List all resolutions.
  -g|--generate     Whether to generate wallpapers. (Defaults to true)
  -w|--width        Width of generated wallpapers. (Defaults to 3840)
  -h|--height       Height of generated wallpapers. (Defaults to 2160)
  -t|--trim         Whether to trim wallpapers, i.e. remove borders. (Defaults to true)
  -b|--border       Uniform border around wallpaper. (Defaults to 100)
```
