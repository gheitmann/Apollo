"""Read-only native control contract and resource audit for the Apollo migration."""
from pathlib import Path
from collections import defaultdict
import json, xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[1]
SKIN=ROOT/'skin'
types={'GUIButtonControl':'button','GUIToggleButtonControl':'togglebutton','GUICheckButton':'checkbutton','GUICheckButtonControl':'checkbutton','GUIButton3PartControl':'button3part','GUIButton3Part':'button3part','GUIImage':'image','GUILabelControl':'label','GUIFadeLabel':'fadelabel','GUITextControl':'textbox','GUITextScrollUpControl':'textboxscrollup','GUIListControl':'listcontrol','GUIFacadeControl':'facadeview','GUISpinControl':'spincontrol','GUIMenuButton':'menubutton','GUIMenuButtonControl':'menubutton','GUISliderControl':'slider','GUIProgressControl':'progress','GUIVideoControl':'videowindow','GUIVolumeBar':'volumebar','GUICoverFlow':'coverflow','GUIThumbnailPanel':'thumbnailpanel','GUISliderButton':'sliderbutton'}
names={p.relative_to(SKIN).as_posix().lower():p for p in SKIN.rglob('*.xml')}
trees={};errors=[]
for name,p in names.items():
 try:trees[name]=ET.parse(p).getroot()
 except ET.ParseError as ex:errors.append((name,str(ex)))
styles={e.get('Name'):e for e in trees.get('references.xml',[]) if e.tag=='style'}
def resolve(name,parent):
 for key in [(Path(parent).parent/name).as_posix().lower(),name.lower()]:
  if key in trees:return key
 return None
missing_resources=set()
def controls(name,seen=None):
 seen=set() if seen is None else seen
 if name in seen or name not in trees:return []
 seen.add(name);out=list(trees[name].iter('control'))
 for e in list(trees[name].iter('import'))+list(trees[name].iter('include')):
  value=(e.text or '').strip().replace('\\','/')
  if not value or '#' in value:continue
  target=resolve(value,name)
  if target:out.extend(controls(target,seen))
  else:missing_resources.add((name,value))
 return out
def ctype(c):
 result=c.findtext('type')
 if not result and c.get('Style') in styles:result=styles[c.get('Style')].findtext('type')
 return (result or '').lower()
fields=defaultdict(list);windows={}
for line in (ROOT/'docs/native-bindings.tsv').read_text(encoding='utf-8-sig').splitlines():
 a=line.split('\t')
 if a[0]=='BIND':fields[a[2]].append((a[3],a[4],a[5]))
 elif a[0]=='WINDOW':windows[a[1]]=a[2].replace('\\','/').lstrip('/').lower()
mismatches=[];missing=[];checked=0
for cls,filename in windows.items():
 if filename not in trees:continue
 cc=controls(filename);byid=defaultdict(set)
 for c in cc:
  try:byid[str(int(c.findtext('id','-1')))].add(ctype(c))
  except ValueError:pass
 for field,expected,id in fields[cls]:
  actual=byid.get(id,set());want=types.get(expected)
  if not actual:missing.append({'file':filename,'id':id,'expected':expected,'field':field});continue
  checked+=1
  if want and want not in actual and expected!='GUIControl':mismatches.append({'file':filename,'id':id,'actual':sorted(actual),'expected':want,'field':field})
report={'xml_files':len(names),'xml_errors':errors,'native_windows':sum(n in trees for n in windows.values()),'existing_bindings_checked':checked,'type_mismatches':mismatches,'missing_fields_review':missing,'missing_includes':sorted(missing_resources)}
(ROOT/'docs/static-audit.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in report.items() if k!='missing_fields_review'},indent=2));print('Missing bound fields for review:',len(missing))
if errors or mismatches or missing_resources:raise SystemExit('Static audit failed; inspect docs/static-audit.json.')
