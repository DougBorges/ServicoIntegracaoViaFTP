using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Renci.SshNet;
using ServicoIntegracaoViaFTP.Service;

namespace ServicoIntegracaoViaFtp.Service {
    public class ProcessarEnvioDados {
        private readonly Boolean apagarArquivoFTP = ConfigurationManager.AppSettings.AllKeys.Contains("apagarArquivoFTP");
        private readonly ConexaoBanco database;
        private Boolean exportarZip;
        private String protocoloFtp;
        private String enderecoFtp;
        private String portaFtp;
        private String loginFtp;
        private String senhaFtp;
        private ProtocoloFtp tipoFtp;

        public ProcessarEnvioDados() {
            var connectionString = ConfigurationManager.AppSettings["connectionString"];
            database = new ConexaoBanco(connectionString);
            BuscarDadosConexao();
        }

        public ProcessarEnvioDados(String connectionString) {
            database = new ConexaoBanco(connectionString);
            BuscarDadosConexao();
        }

        public ProcessarEnvioDados(String protocoloFtp, String enderecoFtp, String portaFtp, String loginFtp, String senhaFtp) {
            this.protocoloFtp = protocoloFtp;
            this.enderecoFtp = enderecoFtp;
            this.portaFtp = portaFtp;
            this.loginFtp = loginFtp;
            this.senhaFtp = senhaFtp;
        }

        public Boolean TestarConexao() {
            tipoFtp = 0;

            try {
                if (protocoloFtp.ToLower().Equals("ftp")) {
                    tipoFtp = ProtocoloFtp.FTP;

                    enderecoFtp = "ftp://" + enderecoFtp.Trim('/') + ":" + portaFtp + "/";

                    var ftp = (FtpWebRequest) WebRequest.Create(enderecoFtp);

                    ftp.Method = WebRequestMethods.Ftp.ListDirectory;
                    ftp.Credentials = new NetworkCredential(loginFtp, senhaFtp);
                    ftp.KeepAlive = false;

                    var resposta = (FtpWebResponse) ftp.GetResponse();
                    resposta.Close();

                    return true;
                }

                if (protocoloFtp.ToLower().Equals("sftp")) {
                    tipoFtp = ProtocoloFtp.SFTP;

                    enderecoFtp = enderecoFtp.Trim('/');

                    using (var sFtp = new SftpClient(enderecoFtp, Int32.Parse(portaFtp), loginFtp, senhaFtp)) {
                        sFtp.Connect();
                        sFtp.Disconnect();
                    }

                    return true;
                }

                if (protocoloFtp.ToLower().Equals("ftps")) {
                    tipoFtp = ProtocoloFtp.FTPS;
                    return false;
                }

            } catch (Exception e) {
                var erros = new StringBuilder();
                erros.AppendLine("Não foi possível conectar ao FTP.");

                while (e != null) {
                    erros.AppendLine(e.Message);
                    e = e.InnerException;
                }

                throw new Exception(erros.ToString());
            }

            return false;
        }

        public String Processar() {
            if (!TestarConexao()) {
                return null;
            }

            exportarZip = ExportarZip();

            var arquivoProdutos = ProcessarProdutos();
            var arquivoEmpresas = ProcessarEmpresas();
            var arquivoTiposEmpresa = ProcessarTiposEmpresa();

            var arquivosCriados = new List<String>();

            if (!String.IsNullOrWhiteSpace(arquivoProdutos)) arquivosCriados.Add(arquivoProdutos);
            if (!String.IsNullOrWhiteSpace(arquivoEmpresas)) arquivosCriados.Add(arquivoEmpresas);
            if (!String.IsNullOrWhiteSpace(arquivoTiposEmpresa)) arquivosCriados.Add(arquivoTiposEmpresa);

            foreach (var arquivo in arquivosCriados) {
                var arquivoUpload = arquivo;

                if (exportarZip) {
                    var arquivoZip = arquivo.Replace(".csv", ".zip");
                    CompactaArquivo(arquivo, arquivoZip);
                    arquivoUpload = arquivoZip;
                    File.Delete(arquivo);
                }

                UploadArquivoFTP(arquivoUpload);
                File.Delete(arquivoUpload);

                if (apagarArquivoFTP) {
                    DeleteArquivoFTP(arquivoUpload);
                }
            }

            return String.Join(",", arquivosCriados);
        }

        private void BuscarDadosConexao() {
            var sql = @"SELECT Ds_Protocolo,
                               Nm_Endereco,
                               Nu_Porta,
                               Nm_Usuario,
                               Ds_Senha
                          FROM Configuracao_FTP
                         WHERE Sq_Sequencial = 1";

            try {
                var result = database.ExecutarConsulta(sql);
                if (result.Rows.Count.Equals(0)) {
                    throw new Exception("Não há dados armazenados na tabela.");
                }
                
                protocoloFtp = result.Rows[0]["Ds_Protocolo"].ToString();
                enderecoFtp = result.Rows[0]["Nm_Endereco"].ToString();
                portaFtp = result.Rows[0]["Nu_Porta"].ToString();
                loginFtp = result.Rows[0]["Nm_Usuario"].ToString();
                senhaFtp = result.Rows[0]["Ds_Senha"].ToString();

            } catch (Exception e) {
                var erros = new StringBuilder();
                erros.AppendLine("Não foi possível recuperar os dados da conexão com o FTP.");

                while (e != null) {
                    erros.AppendLine(e.Message);
                    e = e.InnerException;
                }

                throw new Exception(erros.ToString());
            }
        }

        public DateTime BuscarProximaExecucaoAgendada() {
            var sql = @"SELECT Ds_Recorrencia,
                               Nu_Dia_Recorrencia,
                               Nu_Hora_Programada
                          FROM Configuracao_FTP
                         WHERE Sq_Sequencial = 1";

            DateTime proximaExecucao;

            try {
                var result = database.ExecutarConsulta(sql);
                if (result.Rows.Count.Equals(0)) {
                    throw new Exception("Não há dados armazenados na tabela.");
                }

                var frequenciaRecorrencia = result.Rows[0]["Ds_Recorrencia"].ToString();
                var diaRecorrencia = Int32.Parse(result.Rows[0]["Nu_Dia_Recorrencia"].ToString());
                var horaProgramada = result.Rows[0]["Nu_Hora_Programada"].ToString();

                var horaExecucao = Int32.Parse(horaProgramada.Split(':')[0]);
                var minutoExecucao = Int32.Parse(horaProgramada.Split(':')[1]);

                switch (frequenciaRecorrencia) {
                    case "D":
                        proximaExecucao = DateTime.Today.AddHours(horaExecucao).AddMinutes(minutoExecucao);
                        proximaExecucao = proximaExecucao < DateTime.Now ? proximaExecucao.AddDays(1) : proximaExecucao;
                        break;

                    case "S":
                        diaRecorrencia = diaRecorrencia % 7;
                        proximaExecucao = DateTime.Today.AddDays(diaRecorrencia - (Int32) DateTime.Today.DayOfWeek)
                                                  .AddHours(horaExecucao).AddMinutes(minutoExecucao);
                        proximaExecucao = proximaExecucao < DateTime.Now ? proximaExecucao.AddDays(7) : proximaExecucao;
                        break;

                    case "M":
                        var mesAnoExecucao = DateTime.Today.AddHours(horaExecucao).AddMinutes(minutoExecucao);
                        mesAnoExecucao = mesAnoExecucao < DateTime.Now ? mesAnoExecucao.AddDays(1) : mesAnoExecucao;
                        mesAnoExecucao = mesAnoExecucao.Day > diaRecorrencia ? mesAnoExecucao.AddMonths(1) : mesAnoExecucao;

                        var ultimoDiaMes = DateTime.DaysInMonth(mesAnoExecucao.Year, mesAnoExecucao.Month);
                        proximaExecucao = new DateTime(mesAnoExecucao.Year, mesAnoExecucao.Month,
                                                       diaRecorrencia > ultimoDiaMes ? ultimoDiaMes : diaRecorrencia,
                                                       horaExecucao, minutoExecucao, 0);
                        break;

                    default:
                        throw new Exception($"A frequência de execução do serviço é inválida: {frequenciaRecorrencia}.");
                }

            } catch (Exception e) {
                var erros = new StringBuilder();
                erros.AppendLine("Não foi possível recuperar o agendamento da próxima execução do serviço.");

                while (e != null) {
                    erros.AppendLine(e.Message);
                    e = e.InnerException;
                }

                throw new Exception(erros.ToString());
            }

            return proximaExecucao;
        }

        private void CompactaArquivo(String arquivoOrigem, String arquivoDestino) {
            var streamLeitura = new FileStream(arquivoOrigem, FileMode.Open, FileAccess.Read);
            var streamEscrita = new FileStream(arquivoDestino, FileMode.Create, FileAccess.Write);

            var streamZip = new ZipOutputStream(streamEscrita);

            var buffer = new Byte[streamLeitura.Length];

            var entry = new ZipEntry(Path.GetFileName(arquivoOrigem));
            streamZip.PutNextEntry(entry);

            Int32 size;
            do {
                size = streamLeitura.Read(buffer, 0, buffer.Length);
                streamZip.Write(buffer, 0, size);
            } while (size > 0);

            streamZip.Close();
            streamEscrita.Close();
            streamLeitura.Close();
        }

        private void UploadArquivoFTP(String nomeArquivo) {
            try {
                switch (tipoFtp) {
                    case ProtocoloFtp.FTP:
                        var ftp = (FtpWebRequest) WebRequest.Create(enderecoFtp + nomeArquivo);
                        ftp.Method = WebRequestMethods.Ftp.UploadFile;
                        ftp.Credentials = new NetworkCredential(loginFtp, senhaFtp);
                        ftp.KeepAlive = false;

                        using (var leitorArquivo = File.OpenRead(nomeArquivo)) {
                            using (var transferencia = ftp.GetRequestStream()) {
                                leitorArquivo.CopyTo(transferencia);
                            }
                        }

                        var resposta = (FtpWebResponse) ftp.GetResponse();
                        resposta.Close();
                        break;

                    case ProtocoloFtp.SFTP:
                        using (var sFtp = new SftpClient(enderecoFtp, Int32.Parse(portaFtp), loginFtp, senhaFtp)) {
                            sFtp.Connect();
                            using (var fs = new FileStream(nomeArquivo, FileMode.Open)) {
                                sFtp.BufferSize = 4 * 1024;
                                sFtp.UploadFile(fs, Path.GetFileName(nomeArquivo));
                            }

                            sFtp.Disconnect();
                        }
                        break;

                    case ProtocoloFtp.FTPS:
                        throw new Exception("O sistema não suporta o protocolo FTPS.");

                    default:
                        throw new Exception("Protocolo de envio não informado.");
                }

            } catch (Exception e) {
                var erros = new StringBuilder();
                erros.AppendLine($"Upload do arquivo {nomeArquivo} falhou.");

                while (e != null) {
                    erros.AppendLine(e.Message);
                    e = e.InnerException;
                }

                throw new Exception(erros.ToString());
            }
        }

        private void DeleteArquivoFTP(String nomeArquivo) {
            try {
                switch (tipoFtp) {
                    case ProtocoloFtp.FTP:
                        var ftp = (FtpWebRequest) WebRequest.Create(enderecoFtp + nomeArquivo);
                        ftp.Method = WebRequestMethods.Ftp.DeleteFile;
                        ftp.Credentials = new NetworkCredential(loginFtp, senhaFtp);
                        ftp.KeepAlive = false;

                        var resposta = (FtpWebResponse) ftp.GetResponse();
                        resposta.Close();
                        break;

                    case ProtocoloFtp.SFTP:
                        using (var sFtp = new SftpClient(enderecoFtp, Int32.Parse(portaFtp), loginFtp, senhaFtp)) {
                            sFtp.Connect();
                            sFtp.DeleteFile(nomeArquivo);
                            sFtp.Disconnect();
                        }
                        break;

                    case ProtocoloFtp.FTPS:
                        throw new Exception("O sistema não suporta o protocolo FTPS.");

                    default:
                        throw new Exception("Protocolo de envio não informado.");
                }

            } catch (Exception e) {
                var erros = new StringBuilder();
                erros.AppendLine($"Delete do arquivo {nomeArquivo} falhou.");

                while (e != null) {
                    erros.AppendLine(e.Message);
                    e = e.InnerException;
                }

                throw new Exception(erros.ToString());
            }
        }

        private Boolean ExportarZip() {
            var sql = @"SELECT Vr_Parametro FROM ParametrosSistema WHERE Cd_Parametro = 'EXPORTAR_ZIP'";

            try {
                var result = database.ExecutarConsulta(sql);
                return result.Rows.Count > 0  && result.Rows[0]["Vr_Parametro"].ToString().Equals("1");

            } catch (Exception e) {
                var erros = new StringBuilder();
                erros.AppendLine("Não foi possível recuperar o parâmetro do sistema Exportar Zip.");

                while (e != null) {
                    erros.AppendLine(e.Message);
                    e = e.InnerException;
                }

                throw new Exception(erros.ToString());
            }
        }

        private String ProcessarProdutos() {
            var sql = @"SELECT * FROM vw_Produtos";

            var nomeArquivo = $"produtos_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv";

            try {
                var result = database.ExecutarConsulta(sql);
                if (result.Rows.Count.Equals(0)) {
                    throw new Exception("Não há dados armazenados na tabela.");
                }

                var dados = new StreamWriter(nomeArquivo, false, Encoding.UTF8);
                dados.WriteLine("codigo;descricao;id_empresa_fabricante;" +
                                "preco_compra;preco_venda;");

                foreach (DataRow linha in result.Rows) {
                    dados.WriteLine($"{String.Join(";", linha.ItemArray).Replace(Environment.NewLine, " ")};");
                }

                dados.Flush();
                dados.Close();

            } catch (Exception e) {
                var erros = new StringBuilder();
                erros.AppendLine($"Não foi possível gerar o arquivo {nomeArquivo}.");

                while (e != null) {
                    erros.AppendLine(e.Message);
                    e = e.InnerException;
                }

                throw new Exception(erros.ToString());
            }

            return nomeArquivo;
        }

        private String ProcessarEmpresas() {
            var sql = "SELECT * FROM vw_Empresas ";

            var nomeArquivo = $"empresas_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv";

            try {
                var result = database.ExecutarConsulta(sql);
                if (result.Rows.Count.Equals(0)) {
                    throw new Exception("Não há dados armazenados na tabela.");
                }

                var dados = new StreamWriter(nomeArquivo, false, Encoding.UTF8);
                dados.WriteLine("codigo;tipo_empresa;nome_empresa;endereco;numero;complemento;bairro;" +
                                "municipio;uf;cep;telefone_primario;telefone_secundario;email;site_url;" +
                                "cnpj;razao_social;nome_responsavel_tecnico;acessibilidade;atend_24_horas;observacoes;");

                foreach (DataRow linha in result.Rows) {
                    dados.WriteLine($"{String.Join(";", linha.ItemArray).Replace(Environment.NewLine, " ")};");
                }

                dados.Flush();
                dados.Close();

            } catch (Exception e) {
                var erros = new StringBuilder();
                erros.AppendLine($"Não foi possível gerar o arquivo {nomeArquivo}.");

                while (e != null) {
                    erros.AppendLine(e.Message);
                    e = e.InnerException;
                }

                throw new Exception(erros.ToString());
            }

            return nomeArquivo;
        }

        private String ProcessarTiposEmpresa() {
            var sql = "SELECT * FROM vw_TiposEmpresa";

            var nomeArquivo = $"tipos-empresa_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv";

            try {
                var result = database.ExecutarConsulta(sql);
                if (result.Rows.Count.Equals(0)) {
                    throw new Exception("Não há dados armazenados na tabela.");
                }

                var dados = new StreamWriter(nomeArquivo, false, Encoding.UTF8);
                dados.WriteLine("codigo;descricao;");

                foreach (DataRow linha in result.Rows) {
                    dados.WriteLine($"{String.Join(";", linha.ItemArray).Replace(Environment.NewLine, " ")};");
                }

                dados.Flush();
                dados.Close();

            } catch (Exception e) {
                var erros = new StringBuilder();
                erros.AppendLine($"Não foi possível gerar o arquivo {nomeArquivo}.");

                while (e != null) {
                    erros.AppendLine(e.Message);
                    e = e.InnerException;
                }

                throw new Exception(erros.ToString());
            }

            return nomeArquivo;
        }
    }
}