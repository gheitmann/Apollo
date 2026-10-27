function Test-MediaPortalClient([string]$Directory) {
    try {
        foreach($name in @('MediaPortal.exe','MpeCore.dll','MpeInstaller.exe')) {if(-not(Test-Path -LiteralPath (Join-Path $Directory $name))){return $false}}
        $bytes=[IO.File]::ReadAllBytes((Join-Path $Directory 'MediaPortal.exe'))
        $pe=[BitConverter]::ToInt32($bytes,60)
        return [BitConverter]::ToUInt16($bytes,$pe+4) -in @(0x14c,0x8664)
    } catch {return $false}
}
function Resolve-MediaPortalDirectory {
    param([string]$Directory,[switch]$SelectDirectory)
    if($Directory){
        $resolved=[IO.Path]::GetFullPath($Directory)
        if(-not(Test-MediaPortalClient $resolved)){throw "Select a MediaPortal client folder containing MediaPortal.exe, MpeCore.dll and MpeInstaller.exe: $resolved"}
        return $resolved
    }
    $candidates=@()
    foreach($root in @(${env:ProgramFiles(x86)},$env:ProgramFiles)) {if($root){$candidates+=Join-Path $root 'Team MediaPortal\MediaPortal'}}
    foreach($key in @('HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*')){
        foreach($item in @(Get-ItemProperty -Path $key -ErrorAction SilentlyContinue | Where-Object {$_.DisplayName -match '^MediaPortal'})) {
            foreach($field in @('InstallLocation','InstallPath','ApplicationDir')) {if($item.$field){$candidates+=$item.$field}}
        }
    }
    $valid=@($candidates | Select-Object -Unique | Where-Object {Test-MediaPortalClient $_})
    if($valid.Count -eq 1){return [IO.Path]::GetFullPath($valid[0])}
    if($SelectDirectory){
        Add-Type -AssemblyName System.Windows.Forms
        $dialog=New-Object System.Windows.Forms.FolderBrowserDialog
        try {
            $dialog.Description='Select the MediaPortal client folder'
            $dialog.ShowNewFolderButton=$false
            if($dialog.ShowDialog() -ne [Windows.Forms.DialogResult]::OK){throw 'Folder selection cancelled.'}
            return Resolve-MediaPortalDirectory -Directory $dialog.SelectedPath
        } finally {$dialog.Dispose()}
    }
    throw 'No unique MediaPortal client was found. Use --mediaportal <client folder>.'
}
