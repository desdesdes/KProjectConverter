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

            var projects = new List<ProjectInfo>();
            FindConvertableProjects(options.RootDirectory, projects);

            // Quit now if conversion is not wanted, just display info
            if(!options.Convert)
            {
                PrintProjectInfo(projects);
                WaitWhenRunningInDebugger();
                return;
            }

            // If there are any errors quit conversion and display info
            if (projects.Any(p => p.Errors.Count > 0))
            {
                PrintProjectInfo(projects);

                Console.WriteLine("Conversion cancelled because of errors!");
                WaitWhenRunningInDebugger();
                return;
            }

            var foundProjectReferences = new HashSet<string>(projects.Select(p => p.AsmName), StringComparer.OrdinalIgnoreCase);

            KGlobal.WriteStandardKFiles(options.RootDirectory);
            KGlobal.BuildGlobalJson(projects, options.RootDirectory);

            foreach (var project in projects)
            {
                var kproj = new KProject(project, foundProjectReferences);
                kproj.BuildProjectJson();
                kproj.DeleteOldProjectFiles();
            }

            PrintProjectInfo(projects);
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

        private static void PrintProjectInfo(List<ProjectInfo> projects)
        {
            var standardColor = Console.ForegroundColor;
            foreach (var project in projects)
            {
                Console.WriteLine(string.Format(". {0}", project.ProjectFilePath));
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

        private static void FindConvertableProjects(string dirPath, List<ProjectInfo> projects)
        {
            // process subdirs, add the sound projects to the list
            var projectsInSubDirs = new List<ProjectInfo>();
            foreach (var subDirPath in Directory.GetDirectories(dirPath))
            {
                FindConvertableProjects(subDirPath, projectsInSubDirs);
            }
            projects.AddRange(projectsInSubDirs);

            // Find the project files in current directory
            var projectInCurrentDirectory = Directory.GetFiles(dirPath, "*.csproj").Select(i => new ProjectInfo() { ProjectFilePath = i }).ToArray();
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
            if(projectInCurrentDirectory.Length > 1)
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
                projectChecker.CheckAssemblyInfoFile();
            }
        }
    }
}
