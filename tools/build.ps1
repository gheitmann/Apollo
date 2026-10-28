param([string]$MediaPortalDirectory=(Join-Path $PSScriptRoot '..\..\Apollo-runtime\client'),[string]$Python='python')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$client=(Get-Item -LiteralPath $MediaPortalDirectory).FullName
$compiler='C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe'
$cecil=(Get-Item -LiteralPath (Join-Path $root '..\Mono.Cecil.dll')).FullName
$build=Join-Path $root 'build'
New-Item -ItemType Directory -Path $build -Force | Out-Null
function Compile([string[]]$Arguments){& $compiler @Arguments;if($LASTEXITCODE -ne 0){throw 'Compilation failed.'}}
Compile @('/nologo','/target:library','/platform:anycpu','/r:System.Xml.dll','/r:System.Core.dll',('/r:'+(Join-Path $client 'Core.dll')),('/r:'+(Join-Path $client 'Common.Utils.dll')),('/r:'+(Join-Path $client 'Utils.dll')),('/out:'+(Join-Path $root 'plugins\process\ApolloPlugin.dll')),(Join-Path $root 'Source\ApolloPlugin.cs'))
$source=Join-Path $root 'Source\InstallerUi'
Compile @('/nologo','/target:library','/platform:anycpu','/r:System.Windows.Forms.dll','/r:System.Drawing.dll',('/out:'+(Join-Path $build 'Apollo.InstallerPresentation.dll')),(Join-Path $source 'InstallerPresentation.cs'))
Compile @('/nologo','/target:exe','/platform:x86','/r:System.Core.dll',('/r:'+$cecil),('/out:'+(Join-Path $build 'PatchInstaller.exe')),(Join-Path $source 'PatchInstaller.cs'))
Compile @('/nologo','/target:winexe','/platform:x86','/r:System.Windows.Forms.dll','/r:System.Xml.dll','/r:System.IO.Compression.dll','/r:System.IO.Compression.FileSystem.dll',('/out:'+(Join-Path $build 'ApolloInstaller.x86.exe')),(Join-Path $source 'ApolloInstaller.cs'),(Join-Path $source 'InstallerCompatibility.cs'))
Compile @('/nologo','/target:exe','/platform:x86','/r:System.Windows.Forms.dll','/r:System.Drawing.dll',('/r:'+(Join-Path $build 'Apollo.InstallerPresentation.dll')),('/out:'+(Join-Path $build 'PresentationProbe.x86.exe')),(Join-Path $source 'PresentationProbe.cs'))
Compile @('/nologo','/target:winexe','/platform:x64','/r:System.Windows.Forms.dll','/r:System.Xml.dll','/r:System.IO.Compression.dll','/r:System.IO.Compression.FileSystem.dll',('/out:'+(Join-Path $build 'ApolloInstaller.x64.exe')),(Join-Path $source 'ApolloInstaller.cs'),(Join-Path $source 'InstallerCompatibility.cs'))
Compile @('/nologo','/target:exe','/platform:x64','/r:System.Windows.Forms.dll','/r:System.Drawing.dll',('/r:'+(Join-Path $build 'Apollo.InstallerPresentation.dll')),('/out:'+(Join-Path $build 'PresentationProbe.x64.exe')),(Join-Path $source 'PresentationProbe.cs'))
Copy-Item -LiteralPath $cecil -Destination $build
Write-Output 'Compiled Apollo plugin and x86 installer components.'
Compile @('/nologo','/target:exe','/platform:x86','/r:System.Xml.dll','/r:System.IO.Compression.dll','/r:System.IO.Compression.FileSystem.dll',('/out:'+(Join-Path $build 'DependencyTool.x86.exe')),(Join-Path $PSScriptRoot 'DependencyTool.cs'),(Join-Path $source 'InstallerCompatibility.cs'))
& (Join-Path $build 'DependencyTool.x86.exe') $client 'export' (Join-Path $root 'plugins/process/ApolloPlugin.dll') (Join-Path $build 'ApolloPlugin-dependency.xml')
if($LASTEXITCODE -ne 0){throw 'Compiled plugin dependency scan failed.'}
& $Python (Join-Path $PSScriptRoot 'migrate.py')
if($LASTEXITCODE -ne 0){throw 'Migration failed.'}
& $Python (Join-Path $PSScriptRoot 'audit.py')
if($LASTEXITCODE -ne 0){throw 'Static audit failed.'}
& $Python (Join-Path $PSScriptRoot 'package.py')
if($LASTEXITCODE -ne 0){throw 'Package verification failed.'}
& (Join-Path $build 'DependencyTool.x86.exe') $client 'regression' (Join-Path $root 'dist/Apollo-MP139-x86-ALPHA3.mpe1')
if($LASTEXITCODE -ne 0){throw 'Native dependency regression failed.'}
$bundle=Join-Path $root 'dist\Apollo-private-ALPHA3.zip'
$setup=Join-Path $root 'dist\Apollo-Setup-ALPHA3.exe'
Compile @('/nologo','/target:winexe','/platform:x86','/r:System.Windows.Forms.dll','/r:System.IO.Compression.dll','/r:System.IO.Compression.FileSystem.dll',('/resource:'+$bundle+',Apollo.Bundle'),('/out:'+$setup),(Join-Path $source 'Setup.cs'))
$hash=(Get-FileHash -LiteralPath $setup -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText($setup+'.sha256',$hash+'  '+[IO.Path]::GetFileName($setup)+[Environment]::NewLine)
Write-Output ('Built single-file setup: '+$setup)
