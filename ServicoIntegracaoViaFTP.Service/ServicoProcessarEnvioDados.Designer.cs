using System;
using System.ComponentModel;

namespace ServicoIntegracaoViaFtp.Service {
    partial class ProcessarEnvioDadosUsuarios {
        private IContainer components = null;

        protected override void Dispose(Boolean disposing) {
            if (disposing && components != null) {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            components = new Container();
            ServiceName = "ServicoIntegracaoViaFtp";
        }
    }
}