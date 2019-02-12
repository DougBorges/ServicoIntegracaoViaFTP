using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ServicoIntegracaoViaFtp.Service {
    partial class Instalador {
        private IContainer components = null;
        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller serviceProcessInstaller;

        protected override void Dispose(Boolean disposing) {
            if (disposing && components != null) {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            serviceInstaller = new ServiceInstaller {
                Description = "Gerar arquivos para integração de dados e disponibilizar via FTP",
                DisplayName = "Serviço de Integração de Dados Via FTP",
                ServiceName = "ServicoIntegracaoViaFtp",
                StartType = ServiceStartMode.Manual
            };

            serviceProcessInstaller = new ServiceProcessInstaller {
                Account = ServiceAccount.LocalSystem,
                Password = null,
                Username = null
            };

            Installers.AddRange(new Installer[] { serviceProcessInstaller, serviceInstaller });
        }
    }
}