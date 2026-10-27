"""Prepare the isolated official MP 1.39 x86 runtime, preserving the installed client."""
from pathlib import Path
import shutil,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[1];BASE=ROOT.parent/'Apollo-runtime';CLIENT=BASE/'client';DATA=BASE/'data'
UPSTREAM=ROOT.parent/'Apollo-runtime-original'
CLIENT.mkdir(parents=True,exist_ok=True);DATA.mkdir(exist_ok=True)
for source,target in [(UPSTREAM/'$_33_',CLIENT),(UPSTREAM/'$_34_',DATA),(UPSTREAM/'$_35_',CLIENT/'Plugins'),(UPSTREAM/'$_37_',DATA/'skin'),(UPSTREAM/'$_38_',DATA/'language'),(UPSTREAM/'$_40_',DATA/'thumbs')]:
 shutil.copytree(source,target,dirs_exist_ok=True)
for folder in ['log','database','cache','thumbs','Playlists']:(DATA/folder).mkdir(exist_ok=True)
dirs=ET.parse(CLIENT/'MediaPortalDirs.xml').getroot()
for node in dirs.findall('Dir'):
 key=node.get('id');node.find('Path').text=str(CLIENT/'Plugins' if key=='Plugins' else DATA if key=='Config' else DATA/key.lower())+'\\'
ET.ElementTree(dirs).write(CLIENT/'MediaPortalDirs.xml',encoding='utf-8',xml_declaration=True)
profile=ET.Element('profile')
def section(name,values):
 s=ET.SubElement(profile,'section',{'name':name})
 for k,v in values.items():ET.SubElement(s,'entry',{'name':k}).text=str(v)
section('skin',{'name':'Apollo','theme':'default','language':'English','usePrefix':False})
section('general',{'startbasichome':True,'startfullscreen':False,'startscreen':35,'splashscreen':False,'hideextensions':True,'minimizeonexit':False,'loglevel':3,'screenselector':0,'autostart':False,'useoverlays':True})
section('plugins',{'ApolloPlugin':True,'ApolloPlugin.dll':True,'Fanart Handler':True,'FanartHandler.dll':True,'Latest Media Handler':True,'LatestMediaHandler.dll':True,'TV-Series':True,'MP-TVSeries.dll':True,'Moving Pictures':True,'MovingPictures.dll':True,'PowerScheduler':False,'PowerSchedulerClientPlugin.dll':False,'MiniDisplay':False})
section('musicfiles',{'view':0});section('debug',{'useExtendedCharsWithStandardCulture':True})
ET.ElementTree(profile).write(DATA/'MediaPortal.xml',encoding='utf-8',xml_declaration=True)
shutil.copytree(ROOT/'skin',DATA/'skin/Apollo',dirs_exist_ok=True)
shutil.copy2(ROOT/'plugins/process/ApolloPlugin.dll',CLIENT/'Plugins/process/ApolloPlugin.dll')
installed=Path('C:/Program Files/Team MediaPortal/MediaPortal')
for relative in ['process/FanartHandler.dll','process/LatestMediaHandler.dll','windows/MovingPictures.dll','windows/Cornerstone.dll','windows/Cornerstone.MP.dll','windows/MP-TVSeries.dll']:
 shutil.copy2(installed/'Plugins'/relative,CLIENT/'Plugins'/relative)
# Test-only current managed dependencies, never included in the skin package.
for name in ['FanartHandler.xml','LatestMediaHandler.xml']:
 source=Path('C:/ProgramData/Team MediaPortal/MediaPortal')/name
 if source.is_file():shutil.copy2(source,DATA/name)
for name in ['Newtonsoft.Json.dll','NLog.dll','SQLite.NET.dll','MediaInfo.Wrapper.dll','TraktAPI.dll']:
 source=installed/name
 if source.is_file() and not (CLIENT/name).exists():shutil.copy2(source,CLIENT/name)
print('Prepared isolated x86 client:',CLIENT)
