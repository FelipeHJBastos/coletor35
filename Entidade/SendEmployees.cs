using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;

namespace coletor35.Entidade
{
    internal class SendEmployees
    {
        public static void GetEmployees()
        {
            string connectionString = AppConfig.ConnectionString;

            using (OdbcConnection connection = new OdbcConnection(connectionString))
            {
                connection.Open();
                string selectFuncionario = $"select * from [DBA].FP_COLETORFUNCIONARIOMAQUINA where fcl_id = 7";

                using (OdbcCommand selectCommand = new OdbcCommand(selectFuncionario, connection))
                {
                    using (OdbcDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            for(var i = 0; i < reader.FieldCount; i++)
                            {
                                Console.WriteLine(reader);
                            }
                            Logs.LogAction(AppConfig.LogIdentifier, "Buscados funcionários associados ao relógio: " + reader[0]);
                        }
                        else
                        {
                            Logs.LogError("Erro ao buscar funcionários do relógio");
                            return;
                        }
                    }
                }
            }
        }
    }
}
