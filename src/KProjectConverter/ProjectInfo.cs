using System;
using System.Collections.Generic;

namespace KProjectConverter
{
    /// <summary>
    /// Summary description for ProjectToConvert
    /// </summary>
    public class ProjectInfo
    {
        public ProjectInfo()
        {
            Warnings = new List<string>();
            Errors = new List<string>();
            References = new List<string>();
            Packages = new List<PackageDependency>();
        }

        public string AsmName { get; set; }
        public string ProjectFilePath { get; set; }
        public List<string> Errors { get; private set; }
        public List<string> References { get; private set; }
        public List<PackageDependency> Packages { get; private set; }
        public List<string> Warnings { get; private set; }
    }
}