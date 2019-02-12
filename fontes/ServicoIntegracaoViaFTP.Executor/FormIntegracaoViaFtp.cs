using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ServicoIntegracaoViaFtp.Service;

namespace ServicoIntegracaoViaFtp.Executor {
    public partial class FormIntegracaoViaFtp : Form {
        private readonly String connectionString;

        public FormIntegracaoViaFtp() {
            InitializeComponent();
        }

        public FormIntegracaoViaFtp(String connectionString) {
            this.connectionString = connectionString;
            InitializeComponent();
        }

        private String ProcessarEnvio() {
            var processarEnvio = String.IsNullOrEmpty(connectionString) ? new ProcessarEnvioDados() : new ProcessarEnvioDados(connectionString);
            return processarEnvio.Processar();
        }

        private void FormIntegracaoViaFtp_Shown(Object sender, EventArgs e) {
            Refresh();

            try {
                var arquivosGerados = ProcessarEnvio();

                UseWaitCursor = false;
                labelMensagem.UseWaitCursor = false;

                if (String.IsNullOrEmpty(arquivosGerados)) {
                    labelMensagem.Text = @"Processo concluído.";
                    MessageBox.Show(@"Nenhum arquivo foi gerado", @"Processo concluído", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var texto = new StringWriter();
                texto.WriteLine("O processo gerou os seguintes arquivos:");
                foreach (var arquivo in arquivosGerados.Split(',')) {
                    texto.WriteLine(arquivo);
                }

                labelMensagem.Text = @"Processo concluído.";
                MessageBox.Show(texto.ToString(), @"Processo concluído", MessageBoxButtons.OK, MessageBoxIcon.Information);

            } catch (Exception excecao) {
                UseWaitCursor = false;
                labelMensagem.UseWaitCursor = false;

                var mensagem = new StringWriter();
                mensagem.WriteLine(excecao.Message);

                if (excecao.InnerException != null) {
                    mensagem.WriteLine(excecao.InnerException.Message);
                    if (excecao.InnerException.InnerException != null) {
                        mensagem.WriteLine(excecao.InnerException.InnerException.Message);
                    }
                }

                labelMensagem.Text = @"Erro ao gerar arquivos.";
                MessageBox.Show(mensagem.ToString(), @"Erro ao gerar arquivos", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } finally {
                Close();
            }
        }
    }

    [Guid("CFEA1D92-08F9-4438-BEB1-CD669949A4BF")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IIntegracaoViaFtp {
        [DispId(1)]
        void ProcessarIntegracaoForm(String connectionString = "");

        [DispId(2)]
        String ProcessarIntegracao(String connectionString = "");

        [DispId(3)]
        Boolean TestarConexao(String protocoloFtp, String enderecoFtp, String portaFtp, String usuarioFtp, String senhaFtp);
    }

    [Guid("13F3BDB2-341B-4485-8A97-5558B33D809D")]
    [ClassInterface(ClassInterfaceType.None)]
    public class IntegracaoViaFtp : IIntegracaoViaFtp {
        public void ProcessarIntegracaoForm(String connectionString = "") {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(String.IsNullOrEmpty(connectionString) ? new FormIntegracaoViaFtp() : new FormIntegracaoViaFtp(connectionString));
            Application.Exit();
        }

        public String ProcessarIntegracao(String connectionString = "") {
            var processarEnvio = String.IsNullOrEmpty(connectionString) ? new ProcessarEnvioDados() : new ProcessarEnvioDados(connectionString);
            return processarEnvio.Processar();
        }

        public Boolean TestarConexao(String protocoloFtp, String enderecoFtp, String portaFtp, String usuarioFtp, String senhaFtp) {
            var conexaoFTP = new ProcessarEnvioDados(protocoloFtp, enderecoFtp, portaFtp, usuarioFtp, senhaFtp);
            return conexaoFTP.TestarConexao();
        }
    }
}