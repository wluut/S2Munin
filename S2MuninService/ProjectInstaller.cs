using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using S2.Munin.Plugin;


namespace S2.Munin.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            this.S2MuninServiceProcessInstaller.BeforeUninstall += new InstallEventHandler(serviceInstallerService1_Uninstall);
            this.S2MuninServiceProcessInstaller.Committed += new InstallEventHandler(serviceInstallerService1_Committed);
        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
            S2.Munin.Plugin.Logger.RegisterApplication();
        }

        private void serviceInstallerService1_Committed(object sender, InstallEventArgs e)
        {
            ServiceController serviceController = new ServiceController(this.S2MuninServiceInstaller.ServiceName);
            serviceController.Start();
        }

        private void serviceInstallerService1_Uninstall(object sender, InstallEventArgs e)
        {
            S2.Munin.Plugin.Logger.UnRegisterApplication();
        }
    }
}
