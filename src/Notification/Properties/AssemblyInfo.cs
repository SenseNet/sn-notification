using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("SenseNet.Tests")]

#if DEBUG
[assembly: AssemblyTitle("SenseNet.Notification (Debug)")]
#else
[assembly: AssemblyTitle("SenseNet.Notification (Release)")]
#endif

[assembly: AssemblyDescription("Notification component for the sensenet ECM platform.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Sense/Net Inc.")]
[assembly: AssemblyCopyright("Copyright © Sense/Net Inc.")]
[assembly: AssemblyProduct("sensenet ECM")]
[assembly: AssemblyTrademark("Sense/Net Inc.")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("7.1.0.0")]
[assembly: AssemblyFileVersion("7.1.0.0")]
[assembly: AssemblyInformationalVersion("7.1.0")]

[assembly: ComVisible(false)]
[assembly: Guid("fcc6395c-7fd0-400e-a01c-274e2a54bdcb")]