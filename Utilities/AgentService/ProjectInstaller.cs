using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;


namespace iRINGAgentService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            // Remove the default Event Log Installer
            EventLogInstaller DefaultInstaller = null;
            foreach (Installer installer in serviceInstaller1.Installers)
            {
                if (installer is EventLogInstaller)
                {
                    DefaultInstaller = (EventLogInstaller)installer;
                    break;
                }
            }

            if (DefaultInstaller != null)
            {
                serviceInstaller1.Installers.Remove(DefaultInstaller);
            }
            
            if ( System.Diagnostics.EventLog.SourceExists("iRINGAgentService") )
            {
                System.Diagnostics.EventLog.DeleteEventSource("iRINGAgentService");
            }
            if (!System.Diagnostics.EventLog.SourceExists("iRINGAgentService"))
            {
                System.Diagnostics.EventLog.CreateEventSource("iRINGAgentService", "iRINGAgentServiceLog");
            }
           
        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
