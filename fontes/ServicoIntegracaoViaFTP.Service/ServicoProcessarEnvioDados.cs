using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ServicoIntegracaoViaFtp.Service {
    internal partial class ProcessarEnvioDadosUsuarios : ServiceBase {
        private ProcessarEnvioDados processarEnvioDados;

        public ProcessarEnvioDadosUsuarios() {
            InitializeComponent();
        }

        protected override void OnStart(String[] args) {
            var thread = new Thread(Processar);
            thread.Start();
        }

        protected override void OnStop() { }

        private void Processar() {
            var logExecucao = new EventLog { Source = "ServicoIntegracaoViaFtp" };
            logExecucao.WriteEntry("Iniciando serviço.", EventLogEntryType.Information);

            try {
                processarEnvioDados = new ProcessarEnvioDados();
                logExecucao.WriteEntry("O serviço foi configurado com êxito.", EventLogEntryType.Information);

                while (true) {
                    var proximaExecucao = processarEnvioDados.BuscarProximaExecucaoAgendada();

                    if (proximaExecucao <= DateTime.Now) {
                        continue;
                    }

                    Thread.Sleep(proximaExecucao.Subtract(DateTime.Now));

                    try {
                        var arquivosEnviados = processarEnvioDados.Processar();
                        logExecucao.WriteEntry(String.IsNullOrWhiteSpace(arquivosEnviados)
                                                   ? "O serviço não gerou nenhum arquivo." : $"O serviço gerou os seguintes arquivos: {arquivosEnviados}.",
                                               EventLogEntryType.Information);
                    } catch (Exception excecao) {
                        RegistraLogErro(logExecucao, excecao);
                    }
                }

            } catch (Exception excecao) {
                RegistraLogErro(logExecucao, excecao);
            }
        }

        private static void RegistraLogErro(EventLog logExecucao, Exception excecao) {
            var erros = new StringBuilder();

            while (excecao != null) {
                erros.AppendLine(excecao.Message);
                erros.AppendLine(excecao.StackTrace);
                excecao = excecao.InnerException;
            }

            logExecucao.WriteEntry(erros.ToString(), EventLogEntryType.Error);
        }
    }
}