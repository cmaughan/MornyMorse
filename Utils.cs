using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MornyMorse;
internal class Utils
{
    public static string GetFilePath(string file)
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MornyMorse");
        Directory.CreateDirectory(folderPath);
        return Path.Combine(folderPath, file);
    }
    private static string GetDropboxPathFromRegistry()
    {
        var infoPath = @"Dropbox\info.json";
        var localAppData = Environment.GetEnvironmentVariable("LocalAppData");
        if (string.IsNullOrEmpty(localAppData)) return string.Empty;

        var jsonPath = Path.Combine(localAppData, infoPath);
        if (!File.Exists(jsonPath))
        {
            var appData = Environment.GetEnvironmentVariable("AppData");
            if (string.IsNullOrEmpty(appData)) return string.Empty;
            jsonPath = Path.Combine(appData, infoPath);
        }

        return !File.Exists(jsonPath) ? string.Empty : File.ReadAllText(jsonPath).Split('\"')[5].Replace(@"\\", @"\");
    }
    public static string GetDropBoxPath(string file)
    {
        string? folderPath = Utils.GetDropboxPathFromRegistry();
        if (string.IsNullOrEmpty(folderPath)) return string.Empty;
        Directory.CreateDirectory(folderPath);
        return Path.Combine(folderPath, file);
    }

}
