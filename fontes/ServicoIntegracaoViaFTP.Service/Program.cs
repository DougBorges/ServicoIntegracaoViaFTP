using System.ServiceProcess;

namespace ServicoIntegracaoViaFtp.Service {
    public class Program {
        public static void Main() {
            var servicos = new ServiceBase[] { new ProcessarEnvioDadosUsuarios() };
            ServiceBase.Run(servicos);
        }
    }
}