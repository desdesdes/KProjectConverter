using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace KProjectConverter
{
    /// <summary>
    /// Summary description for ProjectChecker
    /// </summary>
    public class ProjectChecker
    {
        private static XNamespace _msbuildNs = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");

        ProjectInfo _project;
        XElement _projectXml;

        private ProjectChecker(ProjectInfo project)
	    {
            _project = project;
            _projectXml = XElement.Load(project.ProjectFilePath);
        }

        public static ProjectChecker LoadProject(ProjectInfo project)
        {
            return new ProjectChecker(project);
        }

        public void CheckAssemblyNameAndDirectoryName()
        {
            _project.AsmName = _projectXml.Elements(_msbuildNs + "PropertyGroup").Elements(_msbuildNs + "AssemblyName").Single().Value;

            var directoryName = Path.GetFileName(Path.GetDirectoryName(_project.ProjectFilePath));
            if (!directoryName.Equals(_project.AsmName, StringComparison.OrdinalIgnoreCase))
            {
                _project.Errors.Add(string.Format("Assemblyname '{0}' does not match directory name '{1}'.", _project.AsmName, directoryName));
            }
        }

        public void LoadReferences()
        {
            var referenceGroups = _projectXml.Elements(_msbuildNs + "ItemGroup").Elements(_msbuildNs + "Reference");
            _project.References.AddRange(referenceGroups.Attributes("Include").Select(i => i.Value));
        }

        public void LoadPackages()
        {
            var packagesConfigFilePath = Path.Combine(Path.GetDirectoryName(_project.ProjectFilePath), "packages.config");
            if (!File.Exists(packagesConfigFilePath))
            {
                return;
            }

            var packagesConfigXml = XElement.Load(packagesConfigFilePath);

            foreach (var package in packagesConfigXml.Elements("package"))
            {
                var dep = new PackageDependency();
                dep.Package = package.Attribute("id").Value;
                dep.Version = package.Attribute("version").Value;
                _project.Packages.Add(dep);
            }
        }

        public void CheckAssemblyInfoFile()
        {
            var projectRootDirPath = Path.GetDirectoryName(_project.ProjectFilePath);
            var assemblyInfoFiles = Directory.GetFiles(projectRootDirPath, "AssemblyInfo.cs", SearchOption.AllDirectories);

            if(assemblyInfoFiles.Length > 0)
            {
                _project.Warnings.Add(string.Format("{0} AssemblyInfo.cs files found, review this file.", assemblyInfoFiles.Length));
            }

            foreach (var assemblyInfoFilePath in assemblyInfoFiles)
            {
                var version = GetAssemblyFileVersion(assemblyInfoFilePath);
                if(version != null)
                {
                    _project.Warnings.Add(string.Format("AssemblyFileVersion '{0}' is defined in '{1}', remove and set this in project.json", version, assemblyInfoFilePath.Substring(projectRootDirPath.Length)));
                }
            }
        }

        private static string GetAssemblyFileVersion(string assemblyInfoFilePath)
        {
            var streamreaderAssemblyInfo = new StreamReader(assemblyInfoFilePath);
            string strLine;
            while ((strLine = streamreaderAssemblyInfo.ReadLine()) != null)
            {
                var matchVersion = Regex.Match(strLine, @"(?:AssemblyFileVersion\("")(?<ver>(\d*)\.(\d*)(\.(\d*)(\.(\d*))?)?)(?:""\))", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
                if (matchVersion.Success)
                {
                    var groupVersion = matchVersion.Groups["ver"];
                    if ((groupVersion.Success) && (!String.IsNullOrEmpty(groupVersion.Value)))
                    {
                        return groupVersion.Value;
                    }
                }
            }

            return null;
        }
    }
}