using System;
using System.Linq;

namespace KProjectConverter
{
    /// <summary>
    /// Summary description for Options
    /// </summary>
    public class Options
    {
        public string RootDirectory { get; set; }
        public bool Convert { get; set; }
        public bool ShowHelp { get; set; }

        public void LoadFromCommandLineArgs(string[] args)
        {
            if(CheckArg("?", args) || CheckArg("h", args))
            {
                ShowHelp = true;
                return;
            }

            if(args.Length == 0)
            {
                throw new Exception("First command line arg must be the path to process.");
            }

            RootDirectory = args[0];

            Convert = CheckArg("c", args);
        }

        private static bool CheckArg(string argToCheck, string[] args)
        {
            var argOptions = new[] {"/" + argToCheck, "-"+ argToCheck, "--" + argToCheck };
            if (args.Any(i => argOptions.Contains(i, StringComparer.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }
    }
}