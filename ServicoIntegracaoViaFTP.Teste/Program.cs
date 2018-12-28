using System;
using System.Windows.Forms;
using ServicoIntegracaoViaFtp.Executor;

namespace ServicoIntegracaoViaFtp.Teste {
    public class Program {
        [STAThread]
        public static void Main(String[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args == null || args.Length == 0 || String.IsNullOrEmpty(args[0])) {
                Application.Run(new FormIntegracaoViaFtp());
            } else {
                Application.Run(new FormIntegracaoViaFtp(args[0]));
            }

            Application.Exit();
        }
    }
}