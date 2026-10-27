"""Build and byte-verify Apollo's native MPE1 and private single-EXE bundle."""
from pathlib import Path
import hashlib,json,zipfile,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[1]
DIST=ROOT/'dist';DIST.mkdir(exist_ok=True)
root=ET.parse(ROOT/'packaging/MediaPortalExtension.template.xml').getroot()
entries=root.findall('./Groups/Items/GroupItem/Files/Items/FileItem')
paths={e.findtext('LocalFileName'):ROOT/e.findtext('LocalFileName') for e in entries}
for rel,path in paths.items():
 assert path.resolve().is_relative_to(ROOT.resolve()) and path.is_file(),rel
 if path.suffix.lower()=='.xml':ET.parse(path)
for entry in entries:
 dst=entry.findtext('DestinationFilename')
 assert dst.startswith('%Skin%\\Apollo\\') or dst=='%Plugins%\\process\\ApolloPlugin.dll',dst
 if dst.replace('\\','/').split('/')[-1].lower()=='skinsettings.xml':assert entry.findtext('UpdateOption')=='NeverOverwrite'
assert len(paths)==len(entries)
original=json.loads((ROOT/'packaging/files.json').read_text())
artwork=[r for r in original if r['source'].startswith('skin/') and Path(r['source']).suffix.lower() in ['.png','.jpg','.jpeg','.bmp','.gif','.ttf']]
with zipfile.ZipFile(ROOT.parent/'Apollo-assessment/Apollo.mpe1') as z:
 for r in artwork:assert (ROOT/r['source']).read_bytes()==z.read(r['zip']),r['source']
baseline={rel:hashlib.sha256(p.read_bytes()).hexdigest() for rel,p in paths.items()}
(ROOT/'packaging/baseline-sha256.json').write_text(json.dumps(baseline,indent=2),encoding='utf-8')
def archive(path,files):
 with zipfile.ZipFile(path,'w',compression=zipfile.ZIP_DEFLATED) as z:
  for name,value in files.items():z.writestr(name,value)
 with zipfile.ZipFile(path) as z:
  assert len(z.namelist())==len(files)
  for name,value in files.items():assert z.read(name)==value,name
 digest=hashlib.sha256(path.read_bytes()).hexdigest()
 path.with_suffix(path.suffix+'.sha256').write_text(digest+'  '+path.name+'\n',encoding='utf-8')
 return digest
payload={e.findtext('ZipFileName').replace('\\','/'):(ROOT/e.findtext('LocalFileName')).read_bytes() for e in entries}
payload['MediaPortalExtension.xml']=ET.tostring(root,encoding='utf-8',xml_declaration=True)
mpe=DIST/'Apollo-MP139-x86-ALPHA1.mpe1';digest=archive(mpe,payload)
bundle={n:(ROOT/'build'/n).read_bytes() for n in ['ApolloInstaller.x86.exe','ApolloInstaller.x64.exe','Apollo.InstallerPresentation.dll','PresentationProbe.x86.exe','PresentationProbe.x64.exe','PatchInstaller.exe','Mono.Cecil.dll']}
for name in ['Initialize.ps1','Find-MediaPortal.ps1','Get-MediaPortalArchitecture.ps1','Start-ApolloInstaller.ps1']:bundle[name]=(ROOT/'Source/InstallerUi'/name).read_bytes()
bundle[mpe.name]=mpe.read_bytes();bundle[mpe.name+'.sha256']=mpe.with_suffix('.mpe1.sha256').read_bytes()
bundle['Mono.Cecil-LICENSE.txt']=(ROOT.parent/'Mono.Cecil-LICENSE.txt').read_bytes()
archive(DIST/'Apollo-private-ALPHA1.zip',bundle)
report={'payload_files':len(entries),'xml_files':sum(p.suffix.lower()=='.xml' for p in paths.values()),'original_artwork_and_fonts_unchanged':len(artwork),'bundle_entries':len(bundle),'mpe_sha256':digest}
(ROOT/'docs/package-verification.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
