param([string]$MediaPortalDirectory,[switch]$VerifyOnly,[switch]$Preview)
$ErrorActionPreference='Stop'
try {
    & (Join-Path $PSScriptRoot 'Initialize.ps1') -MediaPortalDirectory $MediaPortalDirectory -SelectDirectory:(-not $VerifyOnly)
    $runtime=Join-Path $PSScriptRoot 'Runtime'
    $nativeFallback=Test-Path -LiteralPath (Join-Path $runtime 'native-fallback.txt')
    $launcher=Join-Path $runtime 'ApolloInstaller.exe'
    if($VerifyOnly){if($nativeFallback){Write-Output 'UNVERIFIED: private presentation unavailable; original installer fallback requires user consent. No installation performed.';exit 2};Write-Output 'PASS: initialized and verified. No installation performed.';exit 0}
    if($nativeFallback){
        Add-Type -AssemblyName System.Windows.Forms
        if($Preview){[Windows.Forms.MessageBox]::Show('No private presentation preview is available for this client. No installation performed.','Apollo preview') | Out-Null;exit 0}
        $reason=[IO.File]::ReadAllText((Join-Path $runtime 'native-fallback.txt'))
        $answer=[Windows.Forms.MessageBox]::Show("$reason`r`n`r`nYou can continue at your own risk with the original MediaPortal Extension Installer. Back up your Apollo settings first. Presentation fixes and automatic settings protection are unavailable on this fallback path. Continue?",'Apollo installer compatibility',[Windows.Forms.MessageBoxButtons]::YesNo,[Windows.Forms.MessageBoxIcon]::Warning,[Windows.Forms.MessageBoxDefaultButton]::Button2)
        if($answer -ne [Windows.Forms.DialogResult]::Yes){Write-Output 'Installation cancelled.';exit 0}
        $installed=[IO.File]::ReadAllText((Join-Path $runtime 'MediaPortalDirectory.txt')).Trim()
        $launcher=Join-Path $installed 'MpeInstaller.exe'
    }
    if($Preview){$arguments='--preview'} else {
        if(Get-Process MediaPortal -ErrorAction SilentlyContinue){throw 'Close MediaPortal before installing Apollo.'}
        if(Test-Path -LiteralPath (Join-Path $runtime 'compatibility-warning.txt')){
            Add-Type -AssemblyName System.Windows.Forms
            $message=[IO.File]::ReadAllText((Join-Path $runtime 'compatibility-warning.txt'))
            $answer=[Windows.Forms.MessageBox]::Show("$message`r`n`r`nThe required installer interfaces were detected. This client version has not been fully tested with Apollo. Continue at your own risk?",'Apollo compatibility',[Windows.Forms.MessageBoxButtons]::YesNo,[Windows.Forms.MessageBoxIcon]::Warning,[Windows.Forms.MessageBoxDefaultButton]::Button2)
            if($answer -ne [Windows.Forms.DialogResult]::Yes){Write-Output 'Installation cancelled.';exit 0}
        }
        $packages=@(Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.mpe1' -File)
        if($packages.Count -ne 1){throw 'The embedded skin package is missing or ambiguous.'}
        $package=$packages[0].FullName
        $expected=([IO.File]::ReadAllText($package+'.sha256').Trim() -split '\s+')[0]
        if((Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash -ne $expected){throw 'The skin package checksum does not match.'}
        $arguments='"'+$package+'"'
    }
    $working=if($nativeFallback){$installed}else{$runtime}
    $process=Start-Process -FilePath $launcher -ArgumentList $arguments -WorkingDirectory $working -Wait -PassThru
    if($process.ExitCode -ne 0){throw "Apollo installer exited with code $($process.ExitCode). See the retained diagnostics."}
} catch {Write-Error $_;exit 1}
