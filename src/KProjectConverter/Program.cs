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

      var projects = new List<CSProject>();
      FindConvertableProjects(options.RootDirectory, projects);

      // Print info if error are found or conversion not needed
      var hasErrors = projects.Any(p => p.Errors.Count > 0);
      if (hasErrors || !options.Convert)
      {
        PrintProjectInfo(options, projects);

        if(hasErrors && options.Convert)
        {
          Console.WriteLine("Conversion cancelled because of errors!");
        }

        WaitWhenRunningInDebugger();
        return;
      }

      // Start conversion


      KGlobal.WriteStandardKFiles(options.RootDirectory);
      KGlobal.BuildGlobalJson(projects, options.RootDirectory);



      var foundProjectReferences = new HashSet<string>(projects.Select(p => p.AsmName), StringComparer.OrdinalIgnoreCase);
      foreach (var project in projects)
      {
        var kproj = new KProject(project, foundProjectReferences);

        kproj.AddProjectReference(options.AddProjectReference);
        kproj.BuildProjectJson();
        kproj.DeleteOldProjectFiles();
      }

      PrintProjectInfo(options, projects);
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

    private static void PrintProjectInfo(Options options, List<CSProject> projects)
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

    private static void FindConvertableProjects(string dirPath, List<CSProject> projects)
    {
      // process subdirs, add the sound projects to the list
      var projectsInSubDirs = new List<CSProject>();
      foreach (var subDirPath in Directory.GetDirectories(dirPath))
      {
        FindConvertableProjects(subDirPath, projectsInSubDirs);
      }
      projects.AddRange(projectsInSubDirs);

      // Find the project files in current directory
      var projectInCurrentDirectory = Directory.GetFiles(dirPath, "*.csproj").Select(i => new CSProject() { ProjectFilePath = i }).ToArray();
      projects.AddRange(projectInCurrentDirectory);

      // If subdir also contains projects then conversion will fail
      if (projectsInSubDirs.Count > 0)
      {
        foreach (var project in projectInCurrentDirectory)
        {
          project.Errors.Add(string.Format("Project contains nested '{0}' projects, move nested projects out of this folder.", projectsInSubDirs.Count));
        }
      }

      // If multiple projects in one folder then report error
      if (projectInCurrentDirectory.Length > 1)
      {
        foreach (var project in projectInCurrentDirectory)
        {
          project.Errors.Add(string.Format("Project directory contains '{0}' projects, split projects in project per directory.", projectInCurrentDirectory.Length));
        }
      }

      foreach (var project in projectInCurrentDirectory)
      {
        var projectChecker = ProjectChecker.LoadProject(project);
        projectChecker.CheckAssemblyNameAndDirectoryName();
        projectChecker.LoadReferences();
        projectChecker.LoadPackages();
        projectChecker.CheckCompileFiles();
      }
    }
  }
}
