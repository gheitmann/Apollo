function Get-MediaPortalArchitecture {
    param([Parameter(Mandatory=$true)][string]$Directory)
    # Read metadata only: never load a foreign-bitness DLL into PowerShell.
    $required=@()
    foreach($name in @('MpeCore.dll','MpeInstaller.exe','Utils.dll')){
        $file=Join-Path $Directory $name
        $identity=[Reflection.AssemblyName]::GetAssemblyName($file)
        switch($identity.ProcessorArchitecture.ToString()){
            'X86'   {$required+='x86'}
            'Amd64' {$required+='x64'}
            'MSIL'  { }
            default {throw "Unsupported MediaPortal architecture in ${name}: $($identity.ProcessorArchitecture)"}
        }
    }
    $required=@($required | Select-Object -Unique)
    if($required.Count -gt 1){throw 'The selected MediaPortal folder contains mixed x86/x64 installer dependencies.'}
    if($required.Count -eq 1){return $required[0]}
    # All dependencies are IL-only; either process is valid.
    if([Environment]::Is64BitOperatingSystem){return 'x64'}
    return 'x86'
}
