# Unofficial MicroSculpture Downloader
This program assists in downloading the brilliant insect photographs from [MicroSculpture](http://microsculpture.net/).

## Please respect the artist
Obviously you do not gain any copyright on the pictures by using this software. So do not distribute or sell them!  
If you enjoy the pictures consider supporting Levon Biss by visiting an exhibition or purchasing his book. You can find more information on [his website](https://www.levonbiss.com/blog/).

## Usage
- Download pictures
  - Compile the project for [.Net Framework 4.6.2.](https://www.microsoft.com/en-us/download/details.aspx?id=53344)
  - Execute to download all insect images listed in Program.cs
- Create wallpapers (Linux scripts only, sorry)
  - Install [ImageMagick](https://www.imagemagick.org)
  - Navigate into "MicroSculptureDownloader" folder
  - Trim pictures using "trim_pictures.sh"
  - Create wallpaper using "make_wallpapers.sh"
    - Supply wallpaper size as first argument like "1920x1080"

## Roadmap
Even though its sufficient for getting some nice wallpapers, there is some room for improvement.

### Wallpaper creation
Creating wallpapers with ImageMagick and scripts works but is not a nice experience.  
Move trimming and resizing into the program.

### CLI/GUI
Changes in functionality have to be hard coded right now. Some core functionality should be accessible by CLI/GUI:
- Download with insect name and resolution option
- Batch download with list of insects/resolutions
- Options to download all insects/resolutions
- Option to specify output image format
- Allow trimming
- Allow resizing & wallpaper creation
