using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;


namespace S2.Munin.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            this.S2MuninServiceProcessInstaller.BeforeUninstall += new InstallEventHandler(serviceInstallerService1_Uninstall);
        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
            S2.Munin.Plugin.Logger.RegisterApplication();

            var serviceInstaller = sender as ServiceInstaller;
            // Start the service after it is installed.
            if (serviceInstaller != null)
            {
                var serviceController = new ServiceController(serviceInstaller.ServiceName);
                serviceController.Start();
            }
        }

        private void serviceInstallerService1_Uninstall(object sender, InstallEventArgs e)
        {
            S2.Munin.Plugin.Logger.UnRegisterApplication();
        }
    }
}
