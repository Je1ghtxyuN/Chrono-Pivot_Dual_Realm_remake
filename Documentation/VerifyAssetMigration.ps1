$ErrorActionPreference='Stop'
$vrProject=Split-Path -Parent $PSScriptRoot
$authorAssets='D:\_work\_SRTP\output\lowpoly-garden\AuthoringProject\Assets'
$checked=0
foreach($folder in @('ReportScenesLowPoly','ReportGardenLowPoly')) {
    $source=Join-Path $authorAssets $folder
    $destination=Join-Path $vrProject ('Assets\'+$folder)
    foreach($file in Get-ChildItem -LiteralPath $source -File -Recurse | Where-Object Extension -ne '.unity') {
        $target=Join-Path $destination $file.FullName.Substring($source.Length+1)
        if((Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath $target).Hash){throw ('Original asset changed: '+$target)}
        $checked++
    }
    if((Get-FileHash -LiteralPath ($source+'.meta')).Hash -ne (Get-FileHash -LiteralPath ($destination+'.meta')).Hash){throw 'Folder GUID changed'}
}
$known=[Collections.Generic.HashSet[string]]::new()
foreach($folder in @((Join-Path $vrProject 'Assets'),(Join-Path $vrProject 'Library\PackageCache'))) {
    foreach($meta in Get-ChildItem -LiteralPath $folder -Recurse -Filter '*.meta') {
        $match=[regex]::Match([IO.File]::ReadAllText($meta.FullName),'(?m)^guid: ([0-9a-f]{32})')
        if($match.Success){[void]$known.Add($match.Groups[1].Value)}
    }
}
$unknown=[Collections.Generic.HashSet[string]]::new()
foreach($file in Get-ChildItem -LiteralPath (Join-Path $vrProject 'Assets') -Recurse -File | Where-Object Extension -ne '.meta') {
    foreach($match in [regex]::Matches([IO.File]::ReadAllText($file.FullName),'guid: ([0-9a-f]{32})')) {
        $guid=$match.Groups[1].Value
        if(!$known.Contains($guid) -and $guid -notmatch '^0000000000000000[0def]000000000000000$'){[void]$unknown.Add($guid)}
    }
}
if($unknown.Count){throw ('Unresolved GUIDs: '+($unknown -join ', '))}
$scenes=@(foreach($folder in @('ReportScenesLowPoly','ReportGardenLowPoly')) {Get-ChildItem -LiteralPath (Join-Path $vrProject ('Assets\'+$folder)) -Recurse -Filter '*.unity'})
if($scenes.Count -ne 15){throw 'Scene count mismatch'}
$buildRoot=Join-Path $vrProject 'Builds\Windows'
foreach($file in @('ChronoPivot_ReportVR.exe','UnityPlayer.dll','ChronoPivot_ReportVR_Data\Plugins\x86_64\UnityOpenXR.dll','ChronoPivot_ReportVR_Data\Plugins\x86_64\openxr_loader.dll')) {
    if(!(Test-Path -LiteralPath (Join-Path $buildRoot $file))){throw ('Missing Windows XR build file: '+$file)}
}
Add-Type -AssemblyName System.Drawing
$photoCount=0
foreach($folder in @('ReportPhotos','GardenPhotos')) {
    foreach($photo in Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot $folder) -Filter '*.png') {
        $bitmap=[Drawing.Image]::FromFile($photo.FullName)
        try{if($bitmap.Width -ne 2560 -or $bitmap.Height -ne 1440){throw ('Photo size mismatch '+$photo.Name)}}finally{$bitmap.Dispose()}
        $photoCount++
    }
}
if($photoCount -ne 23){throw 'Photo count mismatch'}
$message='PASS: preserved self-modeled assets='+$checked+'; scenes=15; photos=23; unresolved GUIDs=0; Windows executable and OpenXR native plugins present.'
$message | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'Verification\asset-migration.txt') -Encoding utf8
Write-Output $message
