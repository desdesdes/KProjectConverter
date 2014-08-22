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
  public class CsProjectParser
  {
    private static XNamespace _msbuildNs = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");

    CSProject _project;
    XElement _projectXml;

    private CsProjectParser(CSProject project)
    {
      _project = project;
      _projectXml = XElement.Load(project.ProjectFilePath);

      var projectInfo = new CSProjectInfo();
      projectInfo.AsmName = _projectXml.Elements(_msbuildNs + "PropertyGroup").Elements(_msbuildNs + "AssemblyName").Single().Value;
      projectInfo.Version = _projectXml.Elements(_msbuildNs + "PropertyGroup").Elements(_msbuildNs + "TargetFrameworkVersion").Single().Value;
      projectInfo.RootNamespace = _projectXml.Elements(_msbuildNs + "PropertyGroup").Elements(_msbuildNs + "RootNamespace").Single().Value;
      projectInfo.ProjectGuid = Guid.Parse(_projectXml.Elements(_msbuildNs + "PropertyGroup").Elements(_msbuildNs + "ProjectGuid").Single().Value);

      _project.Info = projectInfo;
    }

    public static CsProjectParser LoadProject(CSProject project)
    {
      return new CsProjectParser(project);
    }

    public void CheckAssemblyNameAndDirectoryName()
    {
      var directoryName = Path.GetFileName(Path.GetDirectoryName(_project.ProjectFilePath));
      if (!directoryName.Equals(_project.Info.AsmName, StringComparison.OrdinalIgnoreCase))
      {
        _project.Errors.Add(string.Format("Assemblyname '{0}' does not match directory name '{1}'.", _project.Info.AsmName, directoryName));
      }
    }

    public void LoadReferences()
    {
      var referenceGroups = _projectXml.Elements(_msbuildNs + "ItemGroup").Elements(_msbuildNs + "Reference");
      _project.Info.References.AddRange(referenceGroups.Attributes("Include").Select(i => i.Value));

      var projectReferenceGroups = _projectXml.Elements(_msbuildNs + "ItemGroup").Elements(_msbuildNs + "ProjectReference");
      _project.Info.References.AddRange(projectReferenceGroups.Elements(_msbuildNs + "Name").Select(i => i.Value));
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
        var dep = new KDependency();
        dep.Package = package.Attribute("id").Value;
        dep.Version = package.Attribute("version").Value;

        if (_project.Info.Packages.Any(p => dep.Package.Equals(p.Package, StringComparison.OrdinalIgnoreCase)))
        {
          _project.Errors.Add(string.Format("Assemblyname '{0}' has references to multiple versions of the same package '{1}'.", _project.Info.AsmName, dep.Package));
        }
        else
        {
          _project.Info.Packages.Add(dep);
        }
      }
    }

    public void CheckCompileFiles()
    {
      var projectRoot = Path.GetDirectoryName(_project.ProjectFilePath);

      var compileElements = _projectXml.Elements(_msbuildNs + "ItemGroup").Elements(_msbuildNs + "Compile");
      foreach (var compileElement in compileElements)
      {
        var relativeCompileFilePath = compileElement.Attribute("Include")?.Value;
        if(relativeCompileFilePath == null)
        {
          continue;
        }

        if (relativeCompileFilePath.StartsWith(".."))
        {
          _project.Warnings.Add(string.Format("File cannot be included because it is outside of project, review this file '{0}'.", relativeCompileFilePath));
        }
      }
    }
  }
}