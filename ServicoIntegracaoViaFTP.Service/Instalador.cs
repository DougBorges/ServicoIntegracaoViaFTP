using System.ComponentModel;
using System.Configuration.Install;

namespace ServicoIntegracaoViaFtp.Service {
    [RunInstaller(true)]
    public partial class Instalador : Installer {
        public Instalador() {
            InitializeComponent();
        }
    }
}