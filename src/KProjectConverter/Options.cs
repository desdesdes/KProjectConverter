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
    public string AddProjectReference { get; set; }
    public string AddSources { get; set; }

    public void LoadFromCommandLineArgs(string[] args)
    {
      if (CheckArg("?", args) || CheckArg("h", args))
      {
        ShowHelp = true;
        return;
      }

      if (args.Length == 0)
      {
        throw new Exception("First command line arg must be the path to process.");
      }

      RootDirectory = args[0];

      Convert = CheckArg("c", args);
      AddProjectReference = GetArg("addref", args);
      AddSources = GetArg("addsources", args);
    }

    private static bool CheckArg(string argToCheck, string[] args)
    {
      var argOptions = new[] {
        string.Format("/{0}", argToCheck),
        string.Format("-{0}", argToCheck),
        string.Format("--{0}", argToCheck)
      }; 

      if (args.Any(i => argOptions.Contains(i, StringComparer.OrdinalIgnoreCase)))
      {
        return true;
      }

      return false;
    }

    private static string GetArg(string argToCheck, string[] args)
    {
      var argOptions = new[] {
        string.Format("/{0}:", argToCheck),
        string.Format("-{0}:", argToCheck),
        string.Format("--{0}:", argToCheck),
        string.Format("/{0}=", argToCheck),
        string.Format("-{0}=", argToCheck),
        string.Format("--{0}=", argToCheck)
      };

      foreach (var arg in args)
      {
        foreach (var opt in argOptions)
        {
          if(arg.StartsWith(opt, StringComparison.OrdinalIgnoreCase))
          {
            return arg.Substring(opt.Length);
          }
        }
      }

      return null;
    }
  }
}