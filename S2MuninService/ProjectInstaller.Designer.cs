namespace S2.Munin.Service
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.S2MuninServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.S2MuninServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // S2MuninServiceProcessInstaller
            // 
            this.S2MuninServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalService;
            this.S2MuninServiceProcessInstaller.Password = null;
            this.S2MuninServiceProcessInstaller.Username = null;
            this.S2MuninServiceProcessInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller1_AfterInstall);
            // 
            // S2MuninServiceInstaller
            // 
            this.S2MuninServiceInstaller.Description = "Munin Client for .NET Plugins";
            this.S2MuninServiceInstaller.DisplayName = "S2 Munin";
            this.S2MuninServiceInstaller.ServiceName = "S2MuninService";
            this.S2MuninServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.S2MuninServiceProcessInstaller,
            this.S2MuninServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller S2MuninServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller S2MuninServiceInstaller;
    }
}