# Take all pictures from current directory
# Resize them to given size (Preserving ratio and not cutting anything off)
# Add black border to fill given size
# Save to "wallpaper/<resolution>"

# Make sure we get wallpaper size
if [[ $# -eq 0 ]]; then
	echo "Pass walpaper size like 1920x1080"
	exit 1
fi

# Create directory for wallpapers
mkdir -p "wallpaper/$1"
for file in trimmed/*.png ; do
	# Resize picture and add black borders
	convert "$file" -resize "$1"\> -background black -gravity center -extent "$1" "wallpaper/$1/${file#trimmed/}" ;
done
