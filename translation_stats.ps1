
$topdir = $PSScriptRoot

# need to use utf-8 for git-blame to work rigth
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8


$ALL_PERSONALTIES = -100;

$ALL_PERSONALTY_FILES = $null

$resourceRoot = $topdir + "\translation\en\RedirectedResources\assets\abdata"

$personalityFilePaths = @(
    "adv\scenario\c{0}"
    "communication\info_*\*_{0}"
    "etcetra\list\nickname\*\c{0}"
    "h\list\*\personality_voice_c{0}_* "
)

$additionalPaths = @{
    'Event Titles' = @('action\list\event');
    'Maker Pose Text' = @('custom\customscenelist');
    'Positions' = @('h\list\*\animationinfo_*', 'h\list\*\hpointtoggle');
    'Maker Items' = @('list\characustom');
    'Random Names' = @('list\random_name');
    'Map Names' = @('map\list\mapinfo');
    'Studio Lists' = @('studio\info');
}



function Personality-Files () {
    param (
        [int]$id
    )

    if ($id -ge 0) {
        $key = '{0:d2}' -f $id
    } elseif ($id -eq $ALL_PERSONALTIES) {
        $key = '*'
    } else {
        $key = '{0:d}' -f $id
    }
    $files = @()

    foreach ($path in $personalityFilePaths) {
        $searchPath =  $resourceRoot + "\" + ($path -f $key)
        $files +=  Get-ChildItem -Path $searchPath -Recurse -Include translation.txt
    }
    return $files
}


function Other-Files () {
    param (
        [string]$assetPath
    )

    if ($ALL_PERSONALTY_FILES -eq $null) {
        $ALL_PERSONALITY_FILES = @{};
        Personality-Files -id $ALL_PERSONALTIES | ForEach-Object -Process {
            $ALL_PERSONALITY_FILES.Add($_.FullName, $true)
        }
    }

    $searchPath =  $resourceRoot + "\" + $assetPath

    $tmpFiles = Get-ChildItem -Path $searchPath -Recurse -Include translation.txt
    $files = @()

    foreach ($file in $tmpFiles) {
        if ($ALL_PERSONALITY_FILES.Contains($file.FullName)) {continue}
        $files+=$file
    }

    return $files
}

function File-Stats() {
    param (
        $files
    )


    $result = @{
        lines = 0;
        translated = 0;
        authors =  '';
    }

    $authors = @{}

    foreach ($file in $files) {
        $cmd = 'git annotate HEAD -- {0}' -f $file.FullName
        $newlines = Invoke-Expression -Command $cmd 2>($tmpErr=New-TemporaryFile)
        Remove-Item $tmpErr
        if ($LASTEXITCODE -ne 0) { continue }

        $translationLines = $newlines |  Select-String -Pattern '.+=.*'
        if ($translationLines.Matches.Count -eq 0) { continue }
        $result['lines'] += $translationLines.Matches.Count

        $translatedLines = $translationLines.Matches | ForEach-Object -Process { $_.Value} | Select-String -Pattern '(?<!//).+=.+'
        if ($translatedLines.Matches.Count -eq 0) { continue }
        $result['translated'] += $translatedLines.Matches.Count

        $authorLines = $translatedLines.Matches | ForEach-Object -Process { $_.Value} | Select-String -Pattern '^[0-9a-f]+\s+\((.*)\s+[0-9]{4}-[0-9]{2}-[0-9]{2}\s+[^\)]+\d\)'

        if ($authorLines.Matches.Count -eq 0) {continue}
        $names = $authorLines.Matches | ForEach-Object -Process {$_.Groups[1].Value.Trim()};

        foreach ($name in $names) {
            if ($authors.ContainsKey($name)) {
                $authors[$name]++
            } else {
                $authors[$name] = 1
            }
        }
    }

    $entry = (($authors.GetEnumerator() | Sort -Descending Value) | ForEach-Object {$_.Name} ) -join ", "
    $result['authors'] = $entry
    return $result
}

function Personality-Stats () {
    param (
        [int]$id
    )

    $files = Personality-Files -id $id

    return File-Stats -files $files

}

function Other-Stats () {
    param (
        [string[]]$assetPaths
    )

    $files = @()
    foreach ($pth in $assetPaths) {
        $files += Other-Files -assetPath $pth
    }

    return File-Stats -files $files

}


function Table-Format-Entry () {
    param (
        [string]$fmt,
        [int]$len
    )
    $prefix = ''
    $suffix = ''
    $mylen = $len + 2
    if ($fmt.StartsWith(':')) {
        $prefix = ':'
        $mylen-=1;
    }
    if ($fmt.EndsWith(':')) {
        $suffix = ':'
        $mylen-=1;
    }
    $result = $prefix + ("-" * $mylen) + $suffix
    return $result
}



function Translation-Status-Table () {

    $ids = @('Area', '--:')
    $percents = @('Status', '--:')
    $authors = @('Current translation contributors', ':--')


    foreach ($id in @(-1, -2, -4, -5, -8, -9, -10)) {
        Write-Host  "Processing NPC personality ${id}"
        $stats = Personality-Stats -id $id
        $ids += ('NPC {0}' -f $id)
        $percents += ('{0:00.00}%' -f (($stats['translated'] * 100) / $stats['lines']))
        $authors += $stats['authors']
    }

    foreach ($id in 0..38) {
        Write-Host  "Processing Character personality ${id}"
        $stats = Personality-Stats -id $id
        $ids += ('Pers. {0:d2}' -f $id)
        $percents += ('{0:00.00}%' -f (($stats['translated'] * 100) / $stats['lines']))
        $authors += $stats['authors']
    }

    foreach ($entry in ($additionalPaths.Keys | Sort)) {
        Write-Host  "Processing ${entry}"
        $stats = Other-Stats -assetPaths $additionalPaths[$entry]
        $ids += $entry
        $percents += ('{0:00.00}%' -f (($stats['translated'] * 100) / $stats['lines']))
        $authors += $stats['authors']
    }

    $sizes = @(
        ($ids | Measure-Object -Maximum -Property Length).Maximum,
        ($percents | Measure-Object -Maximum -Property Length).Maximum,
        ($authors | Measure-Object -Maximum -Property Length).Maximum)

    $fmt = '| {0} | {1} | {2} |'
    Write-Host  "Preparing results"

    $i = 0
    while ($i -lt $ids.Length) {
        $idStr =  $ids.Get($i)
        $percentStr = $percents.Get($i)
        $authorStr = $authors.Get($i)

       if ($i -eq 1) {
            # format line
            echo ("|" +
                  (Table-Format-Entry $idStr $sizes[0]) + '|' +
                  (Table-Format-Entry $percentStr $sizes[1]) + '|' +
                  (Table-Format-Entry $authorStr $sizes[2]) + '|')

        } else {

            $idStr = $idStr.PadLeft($sizes[0]/2).PadRight($sizes[0])
            $percentStr =  $percentStr.PadLeft($sizes[1])
            $authorStr = $authorStr.PadRight($sizes[2])

            echo ($fmt -f $idStr, $percentStr, $authorStr)
        }
        $i++;
    }
}


Write-Warning 'collecting stats, this takes a very long time...'

Translation-Status-Table

pause