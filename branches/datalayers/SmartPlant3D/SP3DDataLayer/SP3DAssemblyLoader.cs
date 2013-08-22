using System;
using System.Reflection;
using Microsoft.Win32;
using System.Threading;
using System.Security.Principal;

//[assembly: WebActivator.PreApplicationStartMethod(typeof(iringtools.sdk.sp3ddatalayer.App_Start.SP3DAssemblyLoader), "RegisterAssembly", Order =2)]
[assembly: WebActivator.PreApplicationStartMethod(typeof(iringtools.sdk.sp3ddatalayer.App_Start.SP3DAssemblyLoader), "RegisterAssembly")]
//[assembly: PreApplicationStartMethod(typeof(iringtools.sdk.sp3ddatalayer.App_Start.SP3DAssemblyLoader),"RegisterAssembly", )]
namespace iringtools.sdk.sp3ddatalayer.App_Start
{
    public static class SP3DAssemblyLoader
    {
        public static void RegisterAssembly()
        {
            System.Diagnostics.EventLog.WriteEntry("Application", "Register Assemlby called. HAndler" + AppDomain.CurrentDomain.FriendlyName);

            //Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            //var strCurPath = Environment.GetEnvironmentVariable("PATH");
            //string sExtraPath = "";
            //sExtraPath = Registry.GetValue(Registry.LocalMachine.ToString() + "\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\S3DHost.exe", "Path", "").ToString();
            //if ((string.IsNullOrEmpty(sExtraPath)))
            //    sExtraPath = Registry.GetValue(Registry.LocalMachine.ToString() + "\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\App Paths\\S3DHost.exe", "Path", "").ToString();
            //if ((string.IsNullOrEmpty(sExtraPath)))
            //    sExtraPath = Registry.GetValue(Registry.LocalMachine.ToString() + "\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\TaskHost.exe", "Path", "").ToString();
            //if ((string.IsNullOrEmpty(sExtraPath)))
            //    sExtraPath = Registry.GetValue(Registry.LocalMachine.ToString() + "\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\App Paths\\TaskHost.exe", "Path", "").ToString();

            //Environment.SetEnvironmentVariable("PATH", strCurPath + ";" + sExtraPath);
            System.Diagnostics.EventLog.WriteEntry("AppUserName", "Application User Name: " + Environment.UserName);
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }
        
        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            System.Diagnostics.EventLog.WriteEntry("Application", "CurrentDomain_AssemblyResolve called." + AppDomain.CurrentDomain.FriendlyName);
            Assembly objExecutingAssemblies = Assembly.GetExecutingAssembly();
            AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();
            //Loop through the array of referenced assembly names.
            foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
            {
                //Look for the assembly names that have raised the "AssemblyResolve" event.
                if ((strAssmbName.Name.EndsWith("CommonMiddle") && strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",")) == "CommonMiddle"))
                {
                    //We only have this handler to deal with loading of CommonMiddle. Rest everything we dont bother.
                    //AppDomain.CurrentDomain.AssemblyResolve -= MyResolveEventHandler;
                    //Build the path of the assembly from where it has to be loaded.
                    //Check CurrentUser,LocalMachine, 64bit CurrentUser,LocalMachine
                    AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                    const string ProductInstallationKey = "\\Software\\Intergraph\\SP3D\\Installation";
                    const string Wow6432Path = "\\Software\\Wow6432Node\\Intergraph\\SP3D\\Installation";
                    const string InstallDirKey = "INSTALLDIR";

                    string sInstallPath = Convert.ToString(Registry.GetValue(Registry.CurrentUser.ToString() + ProductInstallationKey, InstallDirKey, ""));
                    //try Local Machine
                    if (string.IsNullOrEmpty(sInstallPath))
                    {
                        sInstallPath = Convert.ToString(Registry.GetValue(Registry.LocalMachine.ToString() + ProductInstallationKey, InstallDirKey, ""));
                    }
                    //try 64-bit CurrentUser
                    if (string.IsNullOrEmpty(sInstallPath))
                    {
                        sInstallPath = Convert.ToString(Registry.GetValue(Registry.CurrentUser.ToString() + Wow6432Path, InstallDirKey, ""));
                    }
                    //try 64-bit LocalMachine
                    if (string.IsNullOrEmpty(sInstallPath))
                    {
                        sInstallPath = Convert.ToString(Registry.GetValue(Registry.LocalMachine.ToString() + Wow6432Path, InstallDirKey, ""));
                    }
                    if ((!string.IsNullOrEmpty((sInstallPath.Trim()))))
                    {
                        if ((sInstallPath.EndsWith("\\") == false))
                            sInstallPath += "\\";
                        sInstallPath += "Core\\Container\\Bin\\Assemblies\\Release\\";
                        //sInstallPath += ".\\Bin\\";
                    }
                    else
                    {
                        throw new Exception("Error Reading SmartPlant 3D Installation Directory from Registry !!! Exiting");
                    }
                    //Load the assembly from the specified path and return it.
                    string strTempAssmbPath = sInstallPath + "CommonMiddle" + ".dll";
                    System.Diagnostics.EventLog.WriteEntry("Application", "CurrentDomain_AssemblyResolve called successfully." + AppDomain.CurrentDomain.FriendlyName);
                    return Assembly.LoadFrom(strTempAssmbPath);
                }
            }
            return null;
        }
    }
}
