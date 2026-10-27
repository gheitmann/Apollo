// Apollo compatibility maintenance. Original skin/plugin: Wizard123 and YSMP.
// Distributed under the GNU GPL, as the original Apollo skin.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using MediaPortal.Common.Utils;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

[assembly: AssemblyTitle("Apollo skin support")]
[assembly: AssemblyVersion("1.1.0.1")]
[assembly: CompatibleVersion("1.39.0.0")]
namespace ApolloPlugin {
 public sealed class ImagePlugin : IPlugin, ISetupForm {
  private bool active;
  private string SkinRoot { get {return Path.Combine(Config.GetFolder(Config.Dir.Skin),"Apollo");} }
  public string Author() {return "Wizard123, YSMP; compatibility maintenance";}
  public bool CanEnable() {return true;}
  public bool DefaultEnabled() {return true;}
  public string Description() {return "Apollo tile selection and skin settings backup";}
  public bool GetHome(out string name,out string image,out string focus,out string picture) {name=PluginName();image=focus=picture="";return false;}
  public bool HasSetup() {return false;}
  public string PluginName() {return "ApolloPlugin";}
  public void ShowPlugin() {}
  public int GetWindowId() {return 0;}
  public void Start() {
   try {
    using(var settings=new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config,"MediaPortal.xml")))
     active=string.Equals(settings.GetValueAsString("skin","name",""),"Apollo",StringComparison.OrdinalIgnoreCase);
    if(!active || !Directory.Exists(SkinRoot)) return;
    GUIPropertyManager.SetProperty("#media.user",Environment.UserName);
    UpdateTileLists(SkinRoot);
    Log.Info("Apollo: tile lists initialized using configured skin directory.");
   } catch(Exception e) {Log.Error("Apollo: initialization failed: {0}",e.Message);}
  }
  public static void UpdateTileLists(string root) {
   var path=Path.Combine(root,"PluginDefines.xml");
   if(!File.Exists(path)) return;
   var doc=new XmlDocument();doc.XmlResolver=null;doc.PreserveWhitespace=true;doc.Load(path);
   bool changed=false;
   foreach(var spec in new[]{new[]{"#PluginTileList",Path.Combine(root,"Media","Tiles")},new[]{"#PluginTileListDark",Path.Combine(root,"Themes","Darkness","Media","Tiles")}}) {
    if(!Directory.Exists(spec[1])) continue;
    var items=Directory.GetFiles(spec[1]).Where(p=>new[]{".png",".jpg",".jpeg",".bmp"}.Contains(Path.GetExtension(p).ToLowerInvariant()))
      .Select(Path.GetFileNameWithoutExtension).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s=>s,StringComparer.OrdinalIgnoreCase).ToArray();
    if(items.Length==0) continue;
    var node=doc.SelectSingleNode("/window/define[starts-with(text(),'"+spec[0]+":')]");
    if(node==null) continue;
    string value=spec[0]+":"+string.Join(",",items);
    if(node.InnerText!=value) {node.InnerText=value;changed=true;}
   }
   if(changed) {
    string temp=path+".apollo-tmp";
    try {doc.Save(temp);File.Replace(temp,path,null);} finally {if(File.Exists(temp)) File.Delete(temp);}
   }
  }
  public void Stop() {
   if(!active) return;
   try {
    string source=Path.Combine(SkinRoot,"SkinSettings.xml");
    if(File.Exists(source)) File.Copy(source,Config.GetFile(Config.Dir.Config,"ApolloSettingsBak.xml"),true);
   } catch(Exception e) {Log.Warn("Apollo: settings backup could not be saved: {0}",e.Message);}
   active=false;
  }
 }
}
