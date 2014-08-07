using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace KProjectConverter
{
    /// <summary>
    /// Summary description for KProject
    /// </summary>
    public class KProject
    {
        ProjectInfo _project;
        ISet<string> _projectReferences;

        public KProject(ProjectInfo project, ISet<string> projectReferences)
        {
            _project = project;
            _projectReferences = projectReferences;
        }

        private static void MakeBackup(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Move(filePath, string.Format("{0}.backup", filePath));
            }
        }

        public void BackupOldProjectFiles()
        {
            MakeBackup(_project.ProjectFilePath);
            MakeBackup(string.Format("{0}.vspscc", _project.ProjectFilePath));
        }

        private static string GetProjectNameFromReference(string reference)
        {
            var pos = reference.IndexOf(',');

            if (pos == -1)
            {
                return reference;
            }

            return reference.Substring(0, pos);
        }

        public void BuildProjectJson()
        {
            var projectJson = new JObject();

            var packageSet = new HashSet<string>();
            var generalDependencies = new List<JProperty>();
            foreach (var package in _project.Packages)
            {
                generalDependencies.Add(new JProperty(package.Package, package.Version));
                packageSet.Add(package.Package);
            }

            var netDependencies = new List<JProperty>();
            foreach (var reference in _project.References)
            {
                if(FrameworkReferenceResolver.IsFrameworkReference(reference))
                {
                    if(!FrameworkReferenceResolver.IsStandardKReference(reference))
                    {
                        netDependencies.Add(new JProperty(reference, ""));
                    }
                }
                else
                {
                    // Build project name from reference
                    var projectName = GetProjectNameFromReference(reference);

                    if (_projectReferences.Contains(projectName))
                    {
                        generalDependencies.Add(new JProperty(reference, ""));
                    }
                    else if (!packageSet.Contains(projectName))
                    {
                        _project.Warnings.Add(string.Format("Possible resolvable reference to '{0}'", projectName));
                    }
                }

            }

            if (netDependencies.Count > 0)
            {
                projectJson.Add(new JProperty("frameworks", new JObject(new JProperty("net45", new JObject(new JProperty("dependencies", new JObject(netDependencies)))))));
            }

            if (generalDependencies.Count > 0)
            {
                projectJson.Add(new JProperty("dependencies", new JObject(generalDependencies)));
            }

            var projectPath = Path.Combine(Path.GetDirectoryName(_project.ProjectFilePath), "project.json");
            //File.WriteAllText(projectPath, projectJson.ToString());
        }
    }
}