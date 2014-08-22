using System;
using System.Collections.Generic;

namespace KProjectConverter
{
  /// <summary>
  /// Summary description for ProjectToConvert
  /// </summary>
  public class CSProject
  {
    public string ProjectFilePath { get; set; }
    public CSProjectInfo Info { get; set; }
    public List<string> Errors { get; } = new List<string>();
    public List<string> Warnings { get; } = new List<string>();
  }
}