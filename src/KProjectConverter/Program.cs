using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace KProjectConverter
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var options = new Options();
      options.LoadFromCommandLineArgs(args);

      var csProjects = FindConvertableProjects(options.RootDirectory);

      foreach (var project in csProjects)
      {
        var csProjectParser = CsProjectParser.LoadProject(project);
        csProjectParser.CheckAssemblyNameAndDirectoryName();
        csProjectParser.LoadReferences();
        csProjectParser.LoadPackages();
        csProjectParser.CheckCompileFiles();
      }

      // Print info if error are found or conversion not needed
      var hasErrors = csProjects.Any(p => p.Errors.Count > 0);
      if (hasErrors || !options.Convert)
      {
        PrintProjectInfo(options, csProjects);

        if(hasErrors && options.Convert)
        {
          Console.WriteLine("Conversion cancelled because of errors!");
        }

        WaitWhenRunningInDebugger();
        return;
      }

      // Start conversion
      KGlobal.WriteStandardKFiles(options.RootDirectory);
      KGlobal.BuildGlobalJson(csProjects, options.RootDirectory, options.AddSources);

      // Add aditional Dependencies
      var additonalDependencies = new List<KDependency>();
      if(!string.IsNullOrEmpty(options.AddProjectReference))
      {
        additonalDependencies.Add(new KDependency() { Package = options.AddProjectReference, Version = "" });
      }

      var foundProjectReferences = new HashSet<string>(csProjects.Select(p => p.Info.AsmName), StringComparer.OrdinalIgnoreCase);
      foreach (var csProject in csProjects)
      {
        var kproj = new KProject(csProject, foundProjectReferences);
        kproj.BuildKproj();
        kproj.BuildProjectJson(additonalDependencies);
        kproj.DeleteOldProjectFiles();
      }

      PrintProjectInfo(options, csProjects);
      WaitWhenRunningInDebugger();
    }

    private static void WaitWhenRunningInDebugger()
    {
      if (Debugger.IsAttached)
      {
        Console.WriteLine("Press a key to close.");
        Console.ReadKey();
      }
    }

    private static void PrintProjectInfo(Options options, IEnumerable<CSProject> projects)
    {
      var standardColor = Console.ForegroundColor;
      foreach (var project in projects)
      {
        Console.WriteLine(string.Format(". {0}", project.ProjectFilePath.Substring(options.RootDirectory.Length)));
        Console.ForegroundColor = ConsoleColor.Red;
        foreach (var error in project.Errors)
        {
          Console.WriteLine(string.Format("! {0}", error));
        }
        Console.ForegroundColor = standardColor;

        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (var warning in project.Warnings)
        {
          Console.WriteLine(string.Format("? {0}", warning));
        }
        Console.ForegroundColor = standardColor;

        Console.WriteLine();
      }

      Console.WriteLine();
    }

    private static IEnumerable<CSProject> FindConvertableProjects(string dirPath)
    {
      // process subdirs, add the sound projects to the list
      var projectsInSubDirs = new List<CSProject>();
      foreach (var subDirPath in Directory.GetDirectories(dirPath))
      {
        projectsInSubDirs.AddRange(FindConvertableProjects(subDirPath));
      }

      // Find the project files in current directory
      var projectInCurrentDirectory = Directory.GetFiles(dirPath, "*.csproj").Select(i => new CSProject() { ProjectFilePath = i }).ToList();

      // If subdir also contains projects then conversion will fail
      if (projectInCurrentDirectory.Count > 0 && projectsInSubDirs.Count > 0)
      {
        foreach (var project in projectInCurrentDirectory)
        {
          project.Errors.Add(string.Format("Project contains nested '{0}' projects, move nested projects out of this folder.", projectsInSubDirs.Count));
        }
      }

      // If multiple projects in one folder then report error
      if (projectInCurrentDirectory.Count > 1)
      {
        foreach (var project in projectInCurrentDirectory)
        {
          project.Errors.Add(string.Format("Project directory contains '{0}' projects, split projects in project per directory.", projectInCurrentDirectory.Count));
        }
      }

      projectsInSubDirs.AddRange(projectInCurrentDirectory);
      return projectsInSubDirs;
    }
  }
}
