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
        public static void BuildGlobalJson(List<ProjectInfo> projects, string rootDirPath)
        {
            var projectSubDir = FindProjectPaths(projects, rootDirPath);

            var jsonPaths = new JArray();
            foreach (var subDir in projectSubDir)
            {
                jsonPaths.Add(subDir);
            }

            var globalJson = new JObject();
            globalJson.Add(new JProperty("sources", jsonPaths));

            File.WriteAllText(Path.Combine(rootDirPath, "global.json"), globalJson.ToString());
        }

        private static string[] FindProjectPaths(List<ProjectInfo> projects, string rootDirPath)
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

        public static void BuildNuGetConfig(string dirPath)
        {
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("embed/NuGet.Config");
            using (var fileStream = new FileStream(Path.Combine(dirPath, "NuGet.Config"), FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }
    }
}