using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KProjectConverter
{
  /// <summary>
  /// Summary description for KGlobal
  /// </summary>
  public static class KGlobal
  {
    public static void BuildGlobalJson(IEnumerable<CSProject> projects, string rootDirPath, string addSources = null)
    {
      var projectSubDir = FindProjectPaths(projects, rootDirPath);

      var jsonPaths = new JArray();
      foreach (var subDir in projectSubDir)
      {
        if (!string.IsNullOrEmpty(subDir))
        {
          jsonPaths.Add(subDir);
        }
      }

      if(!string.IsNullOrEmpty(addSources))
      {
        jsonPaths.Add(addSources);
      }

      var globalJson = new JObject();
      globalJson.Add(new JProperty("sources", jsonPaths));

      File.WriteAllText(Path.Combine(rootDirPath, "global.json"), globalJson.ToString());
    }

    private static string[] FindProjectPaths(IEnumerable<CSProject> projects, string rootDirPath)
    {
      var paths = new HashSet<string>();
      foreach (var project in projects)
      {
        var subPath = Path.GetDirectoryName(Path.GetDirectoryName(project.ProjectFilePath));
        var formatted = subPath.Substring(rootDirPath.Length).Trim(Path.DirectorySeparatorChar);

        paths.Add(formatted);
      }

      return paths.ToArray();
    }

    public static void WriteStandardKFiles(string dirPath)
    {
      WriteResourceFile("embed/NuGet.Config", Path.Combine(dirPath, "NuGet.Config"));
      WriteResourceFile("embed/build.cmd", Path.Combine(dirPath, "build.cmd"));
      WriteResourceFile("embed/build.sh", Path.Combine(dirPath, "build.sh"));
      WriteResourceFile("embed/makefile.shade", Path.Combine(dirPath, "makefile.shade"));
    }

    private static void WriteResourceFile(string resourceName, string filePath)
    {
      var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
      using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
      {
        stream.CopyTo(fileStream);
      }
    }
  }
}