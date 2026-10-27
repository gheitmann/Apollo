"""Reuse the reviewed Backdrop installer presentation in the Apollo x86 setup."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
ORIGINAL=ROOT.parent/'Backdrop/Source/InstallerUi'
OUT=ROOT/'Source/InstallerUi';OUT.mkdir(parents=True,exist_ok=True)
for name in ['Setup.cs','InstallerPresentation.cs','PatchInstaller.cs','ApolloInstaller.cs','PresentationProbe.cs','Initialize.ps1','Get-MediaPortalArchitecture.ps1']:
 source=name.replace('Apollo','Backdrop')
 s=(ORIGINAL/source).read_text(encoding='utf-8-sig').replace('Backdrop','Apollo')
 s=s.replace('1.39.0.27','1.1.0.2')
 if name=='Initialize.ps1':
  s=s.replace("New-Item -ItemType Directory -Path $runtime | Out-Null", "New-Item -ItemType Directory -Path $runtime | Out-Null\nif(Test-Path -LiteralPath (Join-Path $installed 'MediaPortalDirs.xml')){Copy-Item -LiteralPath (Join-Path $installed 'MediaPortalDirs.xml') -Destination $runtime}")
  # Retain architecture selection from the maintained launcher.
 if name=='ApolloInstaller.cs':
  s=s.replace('"PASS: native installer binds to isolated MpeCore; native plugin destination=" + plugins', '"PASS: native installer binds to isolated MpeCore; native config destination=" + config.GetMethod("GetFolder").Invoke(null, new object[] { Enum.Parse(dirs, "Config") }) + "; native skin destination=" + config.GetMethod("GetFolder").Invoke(null, new object[] { Enum.Parse(dirs, "Skin") }) + "; native plugin destination=" + plugins')
  old='entry.Invoke(null, entry.GetParameters().Length == 0 ? null : new object[] { args });'
  new='''string skin = (string)config.GetMethod("GetFolder").Invoke(null, new object[] { Enum.Parse(dirs, "Skin") });
            string settingsPath = Path.Combine(skin, "Apollo", "SkinSettings.xml");
            byte[] settingsBackup = File.Exists(settingsPath) ? File.ReadAllBytes(settingsPath) : null;
            if(settingsBackup != null) {
                string data = (string)config.GetMethod("GetFolder").Invoke(null, new object[] { Enum.Parse(dirs, "Config") });
                string backups = Path.Combine(data,"ApolloBackups"); Directory.CreateDirectory(backups);
                File.WriteAllBytes(Path.Combine(backups,"SkinSettings-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+".xml"),settingsBackup);
            }
            try { entry.Invoke(null, entry.GetParameters().Length == 0 ? null : new object[] { args }); }
            finally { if(settingsBackup != null) {Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)); File.WriteAllBytes(settingsPath,settingsBackup);} }'''
  assert old in s;s=s.replace(old,new)
 (OUT/name).write_text(s,encoding='utf-8')
print('Prepared reused installer sources for Apollo x86.')
