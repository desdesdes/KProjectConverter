using System;
using System.Collections.Generic;

namespace KProjectConverter
{
    /// <summary>
    /// Summary description for StandardNetDeps
    /// </summary>
    public static class FrameworkReferenceResolver
    {
        private static HashSet<string> _standardKReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Microsoft.CSharp",
            "mscorlib",
            "System",
            "System.Core"
        };

        // Found in C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1
        // Use dir *.dll /B > c:\test.log to get simple list
        private static HashSet<string> _framework451References = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Accessibility",
            "CustomMarshalers",
            "ISymWrapper",
            "Microsoft.Activities.Build",
            "Microsoft.Build.Conversion.v4.0",
            "Microsoft.Build",
            "Microsoft.Build.Engine",
            "Microsoft.Build.Framework",
            "Microsoft.Build.Tasks.v4.0",
            "Microsoft.Build.Utilities.v4.0",
            "Microsoft.CSharp",
            "Microsoft.JScript",
            "Microsoft.VisualBasic.Compatibility.Data",
            "Microsoft.VisualBasic.Compatibility",
            "Microsoft.VisualBasic",
            "Microsoft.VisualC",
            "Microsoft.VisualC.STLCLR",
            "mscorlib",
            "PresentationBuildTasks",
            "PresentationCore",
            "PresentationFramework.Aero",
            "presentationframework.aero2",
            "presentationframework.aerolite",
            "PresentationFramework.Classic",
            "PresentationFramework",
            "PresentationFramework.Luna",
            "PresentationFramework.Royale",
            "ReachFramework",
            "sysglobl",
            "System.Activities.Core.Presentation",
            "System.Activities",
            "System.Activities.DurableInstancing",
            "System.Activities.Presentation",
            "System.AddIn.Contract",
            "System.AddIn",
            "System.ComponentModel.Composition",
            "System.ComponentModel.Composition.registration",
            "System.ComponentModel.DataAnnotations",
            "System.configuration",
            "System.Configuration.Install",
            "System.Core",
            "System.Data.DataSetExtensions",
            "System.Data",
            "System.Data.Entity.Design",
            "System.Data.Entity",
            "System.Data.Linq",
            "System.Data.OracleClient",
            "System.Data.Services.Client",
            "System.Data.Services.Design",
            "System.Data.Services",
            "System.Data.SqlXml",
            "System.Deployment",
            "System.Design",
            "System.Device",
            "System.DirectoryServices.AccountManagement",
            "System.DirectoryServices",
            "System.DirectoryServices.Protocols",
            "System",
            "System.Drawing.Design",
            "System.Drawing",
            "System.EnterpriseServices",
            "System.EnterpriseServices.Thunk",
            "System.EnterpriseServices.Wrapper",
            "System.IdentityModel",
            "System.IdentityModel.Selectors",
            "System.identitymodel.services",
            "System.IO.Compression",
            "System.IO.Compression.FileSystem",
            "System.IO.Log",
            "System.Management",
            "System.Management.Instrumentation",
            "System.Messaging",
            "System.Net",
            "System.Net.Http",
            "System.Net.Http.WebRequest",
            "System.Numerics",
            "System.Printing",
            "System.Reflection.Context",
            "System.Runtime.Caching",
            "System.Runtime.DurableInstancing",
            "System.Runtime.Remoting",
            "System.Runtime.Serialization",
            "System.Runtime.Serialization.Formatters.Soap",
            "System.Security",
            "System.ServiceModel.Activation",
            "System.ServiceModel.Activities",
            "System.ServiceModel.Channels",
            "System.ServiceModel.Discovery",
            "System.ServiceModel",
            "System.ServiceModel.Routing",
            "System.ServiceModel.Web",
            "System.ServiceProcess",
            "System.Speech",
            "System.Transactions",
            "System.Web.Abstractions",
            "System.Web.ApplicationServices",
            "System.Web.DataVisualization.Design",
            "System.Web.DataVisualization",
            "System.Web",
            "System.Web.DynamicData.Design",
            "System.Web.DynamicData",
            "System.Web.Entity.Design",
            "System.Web.Entity",
            "System.Web.Extensions.Design",
            "System.Web.Extensions",
            "System.Web.Mobile",
            "System.Web.RegularExpressions",
            "System.Web.Routing",
            "System.Web.Services",
            "System.Windows.Controls.Ribbon",
            "System.Windows",
            "System.Windows.Forms.DataVisualization.Design",
            "System.Windows.Forms.DataVisualization",
            "System.Windows.Forms",
            "System.Windows.Input.Manipulations",
            "System.Windows.Presentation",
            "System.Workflow.Activities",
            "System.Workflow.ComponentModel",
            "System.Workflow.Runtime",
            "System.WorkflowServices",
            "System.Xaml",
            "System.XML",
            "System.Xml.Linq",
            "System.Xml.Serialization",
            "UIAutomationClient",
            "UIAutomationClientsideProviders",
            "UIAutomationProvider",
            "UIAutomationTypes",
            "WindowsBase",
            "WindowsFormsIntegration",
            "XamlBuildTask"
        };

        public static bool IsFrameworkReference(string package)
        {
            return _framework451References.Contains(package) ;
        }

        public static bool IsStandardKReference(string package)
        {
            return _standardKReferences.Contains(package);
        }
    }
}