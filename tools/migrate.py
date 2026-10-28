"""Apply the reviewed Apollo x86 compatibility changes to the extracted baseline."""
from pathlib import Path
import copy, hashlib, json, re, xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[1]
SKIN=ROOT/'skin'
REFERENCE=ROOT.parent/'Apollo-runtime-original'/'$_37_'/'DefaultWideHD'
def save(path,root):
 ET.indent(root,space='  ');path.write_bytes(ET.tostring(root,encoding='utf-8',xml_declaration=True))
def put(control,key,value):
 e=control.find(key)
 if e is None:e=ET.SubElement(control,key)
 e.text=str(value)
for path in [SKIN/'BasicHomeEdits.xml',SKIN/'Themes/Darkness/BasicHomeEdits.xml']:
 s=path.read_text(encoding='utf-8-sig')
 try:ET.fromstring(s)
 except ET.ParseError:s=s.replace('</controls>','</control>\n  </controls>');ET.fromstring(s);path.write_text(s,encoding='utf-8')
path=SKIN/'MusicInfoHandler.xml';s=path.read_text(encoding='utf-8-sig');s=s.replace('<import>common.overlay.xml</import>','<!-- Apollo uses the native media overlays; the old common.overlay.xml was not supplied. -->');path.write_text(s,encoding='utf-8')

# Drop a dangling, offscreen-only legacy decoration without changing OSD controls.
p=SKIN/'mytvFullScreen.xml'
s=p.read_text(encoding='utf-8-sig')
s=s.replace('<texture>video-overlay.png</texture>','<!-- Legacy offscreen decoration: no texture was supplied by Apollo. -->\n      <texture></texture>')
p.write_text(s,encoding='utf-8')

# Add missing native settings to the existing Apollo layout, using current IDs/labels.
# The broad settings templates and artwork are deliberately retained.
specs={
 'settings_MyVideos.xml':(['36','43','47','48'],(1370,240), '46'),
 'settings_MyVideos_Database.xml':(['19'],None,'18'),
 'settings_GUI_OnScreenDisplay.xml':(['5'],(500,600),'4'),
 'settings_MyPictures_SlideShow.xml':(['50','51'],(1370,240),'45')}
for filename,(ids,position,lastid) in specs.items():
 path=SKIN/filename;root=ET.parse(path).getroot();native=ET.parse(REFERENCE/filename).getroot()
 present={c.findtext('id') for c in root.findall('.//control')};needed=[id for id in ids if id not in present]
 if not needed:continue
 last=next(c for c in root.findall('.//control') if c.findtext('id')==lastid)
 parent=next(p for p in root.iter() if last in list(p))
 original_default=root.findtext('defaultcontrol')
 for index,id in enumerate(needed):
  source=next(c for c in native.findall('.//control') if c.findtext('id')==id)
  node=ET.Element('control')
  for key in ['description','type','id','label']:
   child=source.find(key)
   if child is not None:node.append(copy.deepcopy(child))
  for key,value in [('width',480 if position and position[0]>1300 else 820),('height',50),('font','font10'),('textXOff',22),('textYOff',7),('onup',needed[index-1] if index else lastid),('ondown',needed[index+1] if index+1<len(needed) else original_default),('onleft',lastid),('onright',original_default)]:put(node,key,value)
  if node.findtext('type')=='button':put(node,'textureFocus','btn_list_blue2.png');put(node,'textureNoFocus','')
  if position:
   put(node,'posX',position[0]);put(node,'posY',position[1]+index*60);root.find('controls').append(node)
  else:parent.append(node)
 put(last,'onright',needed[0])
 if not position:put(last,'ondown',needed[0])
 save(path,root)

# The original plugin writes this list at startup. Produce a correct first-run list too.
root=ET.parse(SKIN/'PluginDefines.xml').getroot()
for name,directory in [('#PluginTileList',SKIN/'Media/Tiles'),('#PluginTileListDark',SKIN/'Themes/Darkness/Media/Tiles')]:
 values=sorted({p.stem for p in directory.iterdir() if p.suffix.lower() in ['.png','.jpg','.jpeg','.bmp']},key=str.casefold)
 for node in root.findall('define'):
  if (node.text or '').startswith(name+':'):node.text=name+':'+','.join(values)
save(SKIN/'PluginDefines.xml',root)

# Hide an unresolved user caption when the optional support plugin is disabled.
leaf=re.compile(r'<control\b[^>]*>(?:(?!</?control\b).)*</control>',re.S)
def user_caption(match):
 block=match.group(0)
 if '<label>#media.user</label>' not in block or 'string.isset(#media.user)' in block:return block
 if '<visible>' in block:return re.sub(r'<visible>(.*?)</visible>',lambda m:'<visible>['+m.group(1)+']+string.isset(#media.user)</visible>',block,flags=re.S)
 return block.replace('</control>','<visible>string.isset(#media.user)</visible>\n</control>')
for path in SKIN.rglob('*.xml'):
 s=path.read_text(encoding='utf-8-sig')
 if '<label>#media.user</label>' in s:
  changed=leaf.sub(user_caption,s)
  if changed!=s:path.write_text(changed,encoding='utf-8')

original=ET.parse(ROOT/'packaging/original.xml').getroot()
mapping=json.loads((ROOT/'packaging/files.json').read_text())
original_files={f.findtext('DestinationFilename'):f for f in original.findall('.//FileItem')}
entries=[]
for record in mapping:
 dst=record['destination'];source=record['source']
 if source.startswith('legacy/') or source in ['plugins/process/FanartHandler.dll','plugins/process/LatestMediaHandler.dll']:continue
 entry=copy.deepcopy(original_files[dst]);entry.find('LocalFileName').text=source
 entry.find('ZipFileName').text=record['zip'].replace('/','\\')
 entry.find('UpdateOption').text='NeverOverwrite' if Path(source).name.lower()=='skinsettings.xml' else 'AlwaysOverwrite'
 entries.append(entry)
package=copy.deepcopy(original)
groups=package.find('Groups/Items');groups.clear()
group=ET.SubElement(groups,'GroupItem',{'Name':'Apollo payload'})
for tag,value in [('ParentGroup',''),('DisplayName','Apollo skin and support plugin'),('DefaulChecked','true'),('Description','Original Apollo artwork and layouts, updated for MediaPortal x86.')]:ET.SubElement(group,tag).text=value
items=ET.SubElement(ET.SubElement(group,'Files'),'Items');items.extend(entries)
unique=package.find('UniqueFileList');unique.clear();ET.SubElement(unique,'Items').extend(copy.deepcopy(entries))
for section in package.findall('./Sections/Items/SectionItem'):
 for action in list(section.findall('./Actions/Items/ActionItem')):
  if action.get('ActionType')=='RunApplication':section.find('Actions/Items').remove(action)
  elif action.get('ActionType')=='InstallFiles':action.set('ConditionGroup','Apollo payload')
plugin_items=package.find('PluginDependencies/Items');plugin_items.clear()
plugin_items.append(ET.parse(ROOT/'build/ApolloPlugin-dependency.xml').getroot())
for d in package.findall('./Dependencies/Items/DependencyItem'):
 if d.findtext('Type')=='MediaPortal':
  for tag in ['MinVersion','MaxVersion']:
   for part,value in zip(['Major','Minor','Build','Revision'],[1,9 if tag=='MinVersion' else 99,0,0]):d.find(tag+'/'+part).text=str(value)
  d.find('WarnOnly').text='true';d.find('Message').text='Tested target: MediaPortal 1.39 x86. Other client versions are unverified.'
info=package.find('GeneralInfo');info.find('Version/Revision').text='3';info.find('DevelopmentStatus').text='Testing'
info.find('VersionDescription').text='ALPHA3: fix helper-host version detection; regenerate plugin dependency metadata and verify native dependencies before installation.'
info.find('UpdateUrl').text='';info.find('OnlineLocation').text='';info.find('ReleaseDate').text='2026-10-28T00:00:00'
for node in package.findall('.//LocalFileName'):
 if ':' in (node.text or ''):node.text=''
project=package.find('ProjectSettings')
if project is not None:project.clear()
save(ROOT/'packaging/MediaPortalExtension.template.xml',package)
actual={f.findtext('LocalFileName'):hashlib.sha256((ROOT/f.findtext('LocalFileName')).read_bytes()).hexdigest() for f in entries}
(ROOT/'packaging/baseline-sha256.json').write_text(json.dumps(actual,indent=2),encoding='utf-8')
print('Prepared',len(entries),'payload entries. No global language resources, legacy restore EXE or third-party plugin replacements.')
