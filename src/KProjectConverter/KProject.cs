using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace KProjectConverter
{
  /// <summary>
  /// Summary description for KProject
  /// </summary>
  public class KProject
  {
    private static XNamespace _msbuildNs = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");
    CSProject _project;
    ISet<string> _projectReferences;

    public KProject(CSProject project, ISet<string> projectReferences)
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

    public void DeleteOldProjectFiles()
    {
      File.Delete(_project.ProjectFilePath);
      File.Delete(string.Format("{0}.vspscc", _project.ProjectFilePath));
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

    public void BuildKproj()
    {
      var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("embed/base.kproj");
      var kprojXml = XElement.Load(stream);
      var globalXml = kprojXml.Elements(_msbuildNs + "PropertyGroup").Single(e => e.Attribute("Label")?.Value?.Equals("Globals") == true);
      globalXml.Element(_msbuildNs + "ProjectGuid").Value = _project.Info.ProjectGuid.ToString("D");
      globalXml.Element(_msbuildNs + "RootNamespace").Value = _project.Info.RootNamespace;

      var kprojectFilePath = Path.ChangeExtension(_project.ProjectFilePath, "kproj");
      kprojXml.Save(kprojectFilePath);
    }

    public void BuildProjectJson(IEnumerable<KDependency> additionalDependencies)
    {
      var projectJson = new JObject();

      var packageSet = new HashSet<string>();
      var generalDependencies = new List<JProperty>();

      foreach (var additionalDependency in additionalDependencies)
      {
        generalDependencies.Add(new JProperty(additionalDependency.Package, additionalDependency.Version));
      }

      foreach (var package in _project.Info.Packages)
      {
        generalDependencies.Add(new JProperty(package.Package, package.Version));
        packageSet.Add(package.Package);
      }

      var netDependencies = new List<JProperty>();
      foreach (var reference in _project.Info.References)
      {
        if (FrameworkReferenceResolver.IsFrameworkReference(reference))
        {
          if (!FrameworkReferenceResolver.IsStandardKReference(reference))
          {
            netDependencies.Add(new JProperty(FrameworkReferenceResolver.GetFrameworkReferenceCorrectCase(reference), ""));
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
            _project.Warnings.Add(string.Format("Possible resolvable reference to '{0}', check if this is part of a package", projectName));
          }
        }
      }

      projectJson.Add(new JProperty("dependencies", new JObject(generalDependencies)));

      // Build the project.json version specifier from csproj version specifier, example "v4.5.1" to "net451"
      var version = _project.Info.Version.Replace(".", "").Replace("v", "net");
      projectJson.Add(new JProperty("frameworks", new JObject(new JProperty(version, new JObject(new JProperty("dependencies", new JObject(netDependencies)))))));

      var projectPath = Path.Combine(Path.GetDirectoryName(_project.ProjectFilePath), "project.json");
      File.WriteAllText(projectPath, projectJson.ToString());
    }
  }
}