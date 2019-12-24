$dir = $PSScriptRoot + "\"
$copy = $dir + "\copy\" 

$textures = $copy + "\BepInEx\Translation\en\Texture"
$out = $dir + "\out"
$outTextures = $dir + "\out\BepInEx\Translation\en\Texture"

New-Item -ItemType Directory -Force -Path ($outTextures)
Remove-Item -Force -Path ($copy) -Recurse -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Force -Path $copy
Copy-Item -Path ($dir + "\BepInEx") -Destination ($copy + "\BepInEx") -Recurse -Force 
Remove-Item -Force -Path ($textures + "\CharaStudio") -Recurse
Remove-Item -Force -Path ($textures + "\Expansions") -Recurse
Remove-Item -Force -Path ($textures + "\Expansions Tutorial") -Recurse
Compress-Archive -Path $textures -Force -CompressionLevel "Optimal" -DestinationPath ($outTextures + "\Koikatu.zip")
Remove-Item -Force -Path ($copy) -Recurse


New-Item -ItemType Directory -Force -Path $copy
Copy-Item -Path ($dir + "\BepInEx") -Destination ($copy + "\BepInEx") -Recurse -Force 
Remove-Item -Force -Path ($textures + "\Cursors") -Recurse
Remove-Item -Force -Path ($textures + "\Expansions") -Recurse
Remove-Item -Force -Path ($textures + "\Expansions Tutorial") -Recurse
Remove-Item -Force -Path ($textures + "\Koikatu") -Recurse
Remove-Item -Force -Path ($textures + "\KoikatuVR") -Recurse
Remove-Item -Force -Path ($textures + "\Tutorial") -Recurse
Compress-Archive -Path $textures -Force -CompressionLevel "Optimal" -DestinationPath ($outTextures + "\CharaStudio.zip")
Remove-Item -Force -Path ($copy) -Recurse


New-Item -ItemType Directory -Force -Path $copy
Copy-Item -Path ($dir + "\BepInEx") -Destination ($copy + "\BepInEx") -Recurse -Force 
Remove-Item -Force -Path ($textures + "\CharaStudio") -Recurse
Remove-Item -Force -Path ($textures + "\Cursors") -Recurse
Remove-Item -Force -Path ($textures + "\Koikatu") -Recurse
Remove-Item -Force -Path ($textures + "\KoikatuVR") -Recurse
Remove-Item -Force -Path ($textures + "\Tutorial") -Recurse
Compress-Archive -Path $textures -Force -CompressionLevel "Optimal" -DestinationPath ($outTextures + "\Expansions.zip")
Remove-Item -Force -Path ($copy) -Recurse


Compress-Archive -Path ($out + "\BepInEx") -Force -CompressionLevel "Optimal" -DestinationPath ($dir + "\KoikatsuImageTranslation_"+$(get-date -f yyyy-MM-dd)+".zip")
Remove-Item -Force -Path ($out) -Recurse
