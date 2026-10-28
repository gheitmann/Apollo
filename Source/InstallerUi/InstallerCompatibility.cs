using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Xml.Serialization;
using System.Collections;

// All changes are process-local. Never write client binaries or registry values.
static class InstallerCompatibility
{
    public static Version SetClientVersion(string installed)
    {
        Version version = AssemblyName.GetAssemblyName(Path.Combine(installed, "MediaPortal.exe")).Version;
        if (version == null || version.Major == 0) throw new InvalidOperationException("Cannot identify the selected MediaPortal.exe version.");
        Type manager = Assembly.LoadFrom(Path.Combine(installed, "Common.Utils.dll")).GetType("MediaPortal.Common.Utils.CompatibilityManager", true);
        FieldInfo field = manager.GetField("AppVersion", BindingFlags.Public | BindingFlags.Static);
        if (field == null || field.FieldType != typeof(Version)) throw new MissingFieldException("Unsupported MediaPortal compatibility version field.");
        // .NET Framework permits this process-local readonly field initialization.
        // The native manager otherwise reads our helper EXE's 0.0.0.0 version.
        field.GetValue(null); // Complete its native static constructor first.
        field.SetValue(null, version);
        if (!version.Equals(field.GetValue(null))) throw new InvalidOperationException("MediaPortal compatibility context initialization failed.");
        return version;
    }

    public static object Scan(Assembly mpe, string plugin)
    {
        Type type = mpe.GetType("MpeCore.Classes.PluginDependencyItem", true);
        object item = Activator.CreateInstance(type);
        type.GetProperty("AssemblyName").SetValue(item, Path.GetFileName(plugin), null);
        if (!(bool)type.GetMethod("ScanVersionInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(item, new object[] { plugin }))
            throw new InvalidOperationException("Cannot read compiled Apollo plugin compatibility attributes.");
        return item;
    }

    public static string Serialize(object item)
    {
        using (StringWriter writer = new StringWriter()) { new XmlSerializer(item.GetType()).Serialize(writer, item); return writer.ToString(); }
    }

    public static string Verify(Assembly mpe, string packagePath)
    {
        Type type = mpe.GetType("MpeCore.PackageClass", true);
        object package;
        using (ZipArchive zip = ZipFile.OpenRead(packagePath))
        using (Stream stream = zip.GetEntry("MediaPortalExtension.xml").Open())
            package = new XmlSerializer(type).Deserialize(stream);
        object dependencies = type.GetProperty("Dependencies").GetValue(package, null);
        int count = 0;
        foreach (object dependency in (IEnumerable)dependencies.GetType().GetProperty("Items").GetValue(dependencies, null))
        {
            string name = (string)dependency.GetType().GetProperty("Type").GetValue(dependency, null);
            string providerName = name == "Skin" ? "SkinVersion" : name == "MediaPortal" ? "MediaPortalVersion" : null;
            if (providerName == null) throw new InvalidOperationException("Unverified dependency provider: " + name);
            Type providerType = mpe.GetType("MpeCore.Classes.VersionProvider." + providerName, true);
            object provider = Activator.CreateInstance(providerType);
            if (!(bool)providerType.GetMethod("Validate").Invoke(provider, new object[] { dependency }))
                throw new InvalidOperationException("Native dependency rejected: " + name);
            count++;
        }
        if (count != 2 || !(bool)type.GetMethod("CheckPluginsDependencies").Invoke(package, null))
            throw new InvalidOperationException("Native Apollo plugin compatibility validation failed.");
        return "PASS: native MediaPortal, Skin and Apollo plugin dependency checks; no installation performed.";
    }
}
