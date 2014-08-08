using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KProjectConverter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var dirPath = args.Length == 1 ? args[0] : @"C:\Development Next\Profit\ran01355-owin-vnext-support - Copy\runtime";

            var projects = new List<ProjectInfo>();
            FindConvertableProjects(dirPath, projects);

            if(projects.Any(p => p.Errors.Count > 0))
            {
                PrintProjectInfo(projects);
                return;
            }

            var foundProjectReferences = new HashSet<string>(projects.Select(p => p.AsmName), StringComparer.OrdinalIgnoreCase);

            KGlobal.BuildGlobalJson(projects, dirPath);
            KGlobal.BuildNuGetConfig(dirPath);

            foreach (var project in projects)
            {
                //TODO Get and write version, description and authors to ProjectJson

                var kproj = new KProject(project, foundProjectReferences);
                kproj.BuildProjectJson();
                kproj.BackupOldProjectFiles();
            }

            PrintProjectInfo(projects);
        }

        private static void PrintProjectInfo(List<ProjectInfo> projects)
        {
            var standardColor = Console.ForegroundColor;
            foreach (var project in projects)
            {
                if(project.Errors.Count == 0)
                {
                    continue;
                }

                Console.WriteLine(string.Format(". {0}", project.ProjectFilePath));
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var error in project.Errors)
                {
                    Console.WriteLine(string.Format("! {0}", error));
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (var warning in project.Warnings)
                {
                    Console.WriteLine(string.Format("? {0}", warning));
                }
                Console.WriteLine();
            }
            Console.ForegroundColor = standardColor;

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
            }
        }
    }
}
