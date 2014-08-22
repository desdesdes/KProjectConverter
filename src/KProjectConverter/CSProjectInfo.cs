using System;
using System.Collections.Generic;

namespace KProjectConverter
{
  public class CSProjectInfo
  {
    public string AsmName { get; set; }
    public string Version { get; set; }
    public Guid ProjectGuid { get; set; }
    public string RootNamespace { get; set; }
    public HashSet<string> References { get; } = new HashSet<string>();
    public List<KDependency> Packages { get; } = new List<KDependency>();
  }
}