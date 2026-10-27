param([string]$MediaPortalDirectory, [switch]$SelectDirectory)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Find-MediaPortal.ps1')
$installed=Resolve-MediaPortalDirectory -Directory $MediaPortalDirectory -SelectDirectory:$SelectDirectory
. (Join-Path $PSScriptRoot 'Get-MediaPortalArchitecture.ps1')
$architecture=Get-MediaPortalArchitecture -Directory $installed
if($architecture -eq 'x64' -and -not [Environment]::Is64BitOperatingSystem){throw 'This client requires 64-bit Windows.'}
Write-Output "Selected MediaPortal installer architecture: $architecture"
$runtime=Join-Path $PSScriptRoot 'Runtime'
if(Test-Path -LiteralPath $runtime){throw 'Runtime already exists. Extract a fresh copy before initializing again.'}
foreach($name in @('MpeCore.dll','MpeInstaller.exe')){
    if(-not(Test-Path -LiteralPath (Join-Path $installed $name))){throw "Missing installed MediaPortal file: $name"}
}
New-Item -ItemType Directory -Path $runtime | Out-Null
if(Test-Path -LiteralPath (Join-Path $installed 'MediaPortalDirs.xml')){Copy-Item -LiteralPath (Join-Path $installed 'MediaPortalDirs.xml') -Destination $runtime}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Apollo.InstallerPresentation.dll') -Destination $runtime
foreach($name in @('ApolloInstaller','PresentationProbe')){
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot ($name+'.'+$architecture+'.exe')) -Destination (Join-Path $runtime ($name+'.exe'))
}
[IO.File]::WriteAllText((Join-Path $runtime 'MediaPortalDirectory.txt'),$installed)
[IO.File]::WriteAllText((Join-Path $runtime 'architecture.txt'),$architecture)
$version=[Reflection.AssemblyName]::GetAssemblyName((Join-Path $installed 'MpeCore.dll')).Version
if($version.Major -ne 1 -or $version.Minor -ne 39){
    [IO.File]::WriteAllText((Join-Path $runtime 'compatibility-warning.txt'),"MpeCore $version is outside the tested MediaPortal 1.39 family.")
}
try {
    $patchArguments=@((Join-Path $installed 'MpeCore.dll'),(Join-Path $runtime 'Apollo.InstallerPresentation.dll'),(Join-Path $runtime 'MpeCore.dll'))
    $patch=Start-Process -FilePath (Join-Path $PSScriptRoot 'PatchInstaller.exe') -ArgumentList (($patchArguments | ForEach-Object {'"'+$_+'"'}) -join ' ') -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput (Join-Path $runtime 'patch-output.txt') -RedirectStandardError (Join-Path $runtime 'patch-error.txt')
    if($patch.ExitCode -ne 0){throw ([IO.File]::ReadAllText((Join-Path $runtime 'patch-error.txt')).Trim())}
& (Join-Path $runtime 'PresentationProbe.exe')
if($LASTEXITCODE -ne 0){throw 'Presentation probe failed. Inspect Runtime/presentation-probe.txt.'}
$probe=Start-Process -FilePath (Join-Path $runtime 'ApolloInstaller.exe') -ArgumentList '--verify' -WindowStyle Hidden -Wait -PassThru
if($probe.ExitCode -ne 0){throw 'Assembly binding verification failed.'}
} catch {
    $version=[Reflection.AssemblyName]::GetAssemblyName((Join-Path $installed 'MpeCore.dll')).Version
    $reason="Installer presentation unavailable for MpeCore $version. " + $_.Exception.Message
    [IO.File]::WriteAllText((Join-Path $runtime 'native-fallback.txt'),$reason)
    Write-Output $reason
    Write-Output 'The original MediaPortal installer can be offered with explicit user consent. No installed files were changed.'
    return
}
Write-Output 'Ready. Open Runtime/ApolloInstaller.exe for a harmless preview, or drag an MPE1 package onto it to install using the native installer.'
Write-Output 'Installed MediaPortal binaries were not changed.'
