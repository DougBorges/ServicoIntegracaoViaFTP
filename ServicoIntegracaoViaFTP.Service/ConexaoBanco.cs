using System;
using System.Data;
using System.Data.OleDb;

namespace ServicoIntegracaoViaFtp.Service {
    public class ConexaoBanco {
        private static OleDbConnection cnOracle;
        private readonly String connectionString;

        public ConexaoBanco(String connectionString) {
            this.connectionString = connectionString;
        }

        private static void ConexaoOracle(String connectionString) {
            cnOracle = new OleDbConnection(connectionString);
            cnOracle.Open();
        }

        public DataTable ExecutarConsulta(String sql) {
            var dataTable = new DataTable();

            ConexaoOracle(connectionString);
            var cmdOracle = new OleDbCommand(sql, cnOracle);
            var readerOracle = cmdOracle.ExecuteReader();

            if (readerOracle == null) {
                return null;
            }

            dataTable.Load(readerOracle);
            cnOracle.Close();

            return dataTable;
        }

        public void ExecutarComando(String sql) {
            ConexaoOracle(connectionString);
            var cmdOracle = new OleDbCommand(sql, cnOracle);
            cmdOracle.ExecuteNonQuery();
            cnOracle.Close();
        }

        public String NovoCodigo(String sChave, String sCampo, Int32 iIncremento, Int32 iCheckLenChave) {
            ConexaoOracle(connectionString);

            var cmdOracle = new OleDbCommand("SP_GERANOVOCODIGO", cnOracle) {
                CommandType = CommandType.StoredProcedure
            };

            cmdOracle.Parameters.Add("vIdChave", OleDbType.VarChar).Value = sChave;
            cmdOracle.Parameters.Add("vIdCampo", OleDbType.VarChar).Value = sCampo;
            cmdOracle.Parameters.Add("vIncremento", OleDbType.Integer).Value = iIncremento;

            var oraParm = new OleDbParameter {
                OleDbType = OleDbType.Double,
                Direction = ParameterDirection.Output,
                ParameterName = "vReturn"
            };

            cmdOracle.Parameters.Add(oraParm);
            cmdOracle.ExecuteNonQuery();
            cnOracle.Close();

            return oraParm.Value.ToString();
        }

        public String ComandoFormatoData(DateTime? data) {
            return data.HasValue ? $" to_date('{data.Value:dd/MM/yyyy HH:mm}', 'DD/MM/YYYY HH24:MI:SS') " : "null";
        }
    }
}