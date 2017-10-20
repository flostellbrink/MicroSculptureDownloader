# Take all level 5 (-5.png) pictures from "download"
# Trim empty space
# Save to trimmed

# Remark: There may be higher resolutions than level 5
# Remark: There may also be insects without level 5 resolution
# TODO: Find highest resolution per insect automatically

# Create directory for wallpapers
mkdir -p trimmed
for file in download/*-5.png ; do
	# Trim empty space and resize
	convert "$file" -trim +repage "trimmed/${file#download/}" ;
done
