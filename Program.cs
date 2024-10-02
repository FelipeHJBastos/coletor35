using coletor35;
using Newtonsoft.Json;
using org.cesar.dmplight.watchComm.api;
using org.cesar.dmplight.watchComm.impl;
using System;
using coletor35.Entidade;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using coletor35.Entidade;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Data.Odbc;
using System.Timers;
using System.ServiceProcess;



internal class Program
{

    private static void Main()
    {
        try
        {
            AppConfig.LoadConfig(); //Carrega variáveis de configuração.

            var maquinas = AppConfig.Parametros.Maquinas;
            Logs.LogAction(AppConfig.LogIdentifier, "Obteve maquinas de configurações: " + maquinas);

            //SendEmployees.GetEmployees();

            //Instancia watchcomm para conexão com relogio.
            foreach (var maquina in maquinas)
            {

                Logs.LogAction(AppConfig.LogIdentifier, "Iniciando leitura de Maquina: " + maquina);

                var watchComm = InstanciaWatchComm(maquina);

                Logs.LogAction(AppConfig.LogIdentifier, "Passou instância");

                watchComm.OpenConnection();

                Logs.LogAction(AppConfig.LogIdentifier, "Abriu conexão");

                var batidas = FetchMRPRecords(watchComm, maquina);
                watchComm.CloseConnection();
                EnviaMarcacoes(batidas);
                UpdateConfigNsr(batidas);
                AtualizaStatusMaquina(batidas);
            }

        }
        catch (Exception ex)
        {
            Logs.LogError("Erro inesperado: " + ex.Message);
        }
    }
    public static WatchComm InstanciaWatchComm(RelogioPonto maquina)
    {
        try
        {
            TCPComm tcpComm = new TCPComm(maquina.IP, 3000);
            tcpComm.SetTimeOut(15000);

            var watchComm = new WatchComm(
                WatchProtocolType.REPC, //modelo do relogio
                tcpComm, //ip no formato necessario
                1,
                "",
                WatchConnectionType.ConnectedMode,
                "01.00.0000",
                maquina.ChaveRSA,
                maquina.ExpoenteRSA,
                AppConfig.Parametros.User,
                AppConfig.Parametros.Password
            );

            Logs.LogAction(AppConfig.LogIdentifier, "Instanciou WatchComm.");

            return watchComm;
        }
        catch (Exception ex)
        {
            Logs.LogError("Erro ao instanciar a conexão WatchComm: " + ex.Message);
            return null;
        }
    }

    public static List<Marcacao> FetchMRPRecords(WatchComm watchComm, RelogioPonto maquina)
    {
        List<Marcacao> batidas = new List<Marcacao>();
        var nsr = maquina.NSR;
        try
        {
            int nsrReposiciona = int.Parse(nsr) + 1;
            nsr = nsrReposiciona.ToString();

            try
            {
                watchComm.RepositioningMRPRecordsPointer(nsr);//Reposiciona nsr para pegar batidas a partir do numero reposicionamento.
                Logs.LogAction(AppConfig.LogIdentifier, $@"Reposicionou NSR: {nsr}");
            }
            catch
            {
                Logs.LogError($@"Reposicionou NSR: {nsr}");
            }
            var records = watchComm.InquiryMRPRecords(false, false, true /* retorna marcação de ponto */, false, false);
            if (records == null)
            {
                Logs.LogAction(AppConfig.LogIdentifier, $@"Relogio não possui novas batidas");
            }
            while (records != null)
            {
                foreach (var record in records)
                {
                    var recordJson = JsonConvert.SerializeObject(record, Formatting.Indented);
                    var recordDeserialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(recordJson);
                    batidas.Add(new Marcacao
                    {
                        Cpf = recordDeserialized["Cpf"],
                        DateTimeMarkingPoint = DateTime.Parse(recordDeserialized["DateTimeMarkingPoint"]),
                        TimeZoneGmt = recordDeserialized["TimeZoneGmt"],
                        NSR = recordDeserialized["NSR"],
                        IdMaquina = maquina.IdMaquina
                    });

                    AppConfig.nsr = record.NSR;
                }
                records = watchComm.ConfirmationReceiptMRPRecords();
            }
            if (records != null)
            {
                Logs.LogAction(AppConfig.LogIdentifier, $@"Buscou batidas do relógio a partir do nsr {int.Parse(nsr + 1).ToString()}");
            }
            else
            {
                Logs.LogAction(AppConfig.LogIdentifier, $@"Não encontrou registros");
            }
        }
        catch (Exception ex)
        {
            Logs.LogError("Erro ao buscar registros MRP: " + ex.Message);
        }
        return batidas;
    }

    private static void UpdateConfigNsr(List<Marcacao> batidas)
    {
        string exePath = AppDomain.CurrentDomain.BaseDirectory;
        string configFilePath = Path.Combine(exePath, "config.json");
        try
        {
            var json = File.ReadAllText(configFilePath);
            var config = JsonConvert.DeserializeObject<Parametros>(json);

            Logs.LogAction(AppConfig.LogIdentifier, $"NSR atualizado para {AppConfig.nsr} no arquivo de configuração.");

            config.Maquinas.Where(x => x.IdMaquina == batidas.LastOrDefault().IdMaquina).SingleOrDefault().NSR = batidas.LastOrDefault().NSR; //atualiza relogio com o ultimo nsr no arquivo de parametro.

            string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(configFilePath, updatedJson); // Salva as alterações no arquivo de configuração

            Logs.LogAction(AppConfig.LogIdentifier, $"NSR atualizado para {AppConfig.nsr} no arquivo de configuração.");
        }
        catch (Exception ex)
        {
            Logs.LogError("Erro ao atualizar NSR no arquivo de configuração: " + ex.Message);
        }
    }

    public static void EnviaMarcacoes(List<Marcacao> batidas)
    {
        string connectionString = AppConfig.Parametros.ConnectionString;
        using (OdbcConnection connection = new OdbcConnection(connectionString))
        {
            Logs.LogAction(AppConfig.LogIdentifier, "Conectando com banco de dados");

            connection.Open();
            Logs.LogAction(AppConfig.LogIdentifier, "Conexão com banco de dados estabelecida com sucesso.");

            var fp28 = new List<string>();
            OdbcDataReader readerTables = new OdbcCommand("SELECT top 2 UPPER(table_name) AS tablename FROM SYS.SYSTABLE WHERE TABLE_NAME LIKE 'FP28%' ORDER BY table_id DESC", connection).ExecuteReader();

            while (readerTables.Read())
            {
                string tableName = readerTables.GetString(0);
                fp28.Add(tableName);
                Logs.LogAction(AppConfig.LogIdentifier, "Tabela de contratos obtida: " + tableName);
            }

            foreach (var marcacao in batidas)
            {
                var pisFuncionario = "";
                bool funcionarioEncontrado = false;

                foreach (var tableName in fp28)
                {
                    string selectFuncionario = $"SELECT nomefuncionario, pispasep, numerocpf FROM {tableName} WHERE numerocpf = {decimal.Parse(marcacao.Cpf)}";

                    using (OdbcCommand selectCommand = new OdbcCommand(selectFuncionario, connection))
                    {
                        using (OdbcDataReader reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                pisFuncionario = reader[1].ToString();
                                Logs.LogAction(AppConfig.LogIdentifier, "Pis obtido do funcionario: " + reader[0]);
                                funcionarioEncontrado = true;
                                break;
                            }
                        }
                    }

                    if (funcionarioEncontrado)
                    {
                        break;
                    }
                }

                if (!funcionarioEncontrado)
                {
                    Logs.LogError("Funcionário não encontrado. Cpf: " + marcacao.Cpf);
                    continue;
                }
                try
                {
                    string insertQuery = "insert into fp_coletamarcacoes (fcl_id, fcl_pis, fcl_data, fcl_hora, fcl_nsr, fcm_tiporegistro, fcm_registrocru )" +
                        "values (?, ?, ?, ?, ?, ?, ?)";
                    string registroCru = marcacao.NSR.Trim('0').PadLeft(9, '0') + "3" + marcacao.DateTimeMarkingPoint.ToString("ddMMyyyyhhmm") + pisFuncionario.PadLeft(12, '0');

                    using (OdbcCommand command = new OdbcCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@fcl_id", batidas.LastOrDefault().IdMaquina);
                        command.Parameters.AddWithValue("@fcl_pis", pisFuncionario.PadLeft(12, '0'));
                        command.Parameters.AddWithValue("@fcl_data", marcacao.DateTimeMarkingPoint);
                        command.Parameters.AddWithValue("@fcl_hora", marcacao.DateTimeMarkingPoint.ToString("HH:mm:ss"));
                        command.Parameters.AddWithValue("@fcl_nsr", Int32.Parse(marcacao.NSR));
                        command.Parameters.AddWithValue("@fcm_tiporegistro", 3);
                        command.Parameters.AddWithValue("@fcm_registrocru", registroCru);

                        int rowsAffected = command.ExecuteNonQuery();

                        Logs.LogAction(AppConfig.LogIdentifier, "Marcação inserida na tabela: " + marcacao);
                    }
                }
                catch (Exception error)
                {
                    Logs.LogError("Erro ao Inserir dados na tabela Fp_ColetaMarcacoes: " + error.Message);
                }

            }
        }
    }

    public static void AtualizaStatusMaquina(List<Marcacao> batidas)
    {
        string connectionString = AppConfig.Parametros.ConnectionString;
        Logs.LogAction(AppConfig.LogIdentifier, "Atualizando status da maquina");

        using (OdbcConnection connection = new OdbcConnection(connectionString))
        {
            connection.Open();
            Logs.LogAction(AppConfig.LogIdentifier, "Conexão com banco de dados estabelecida com sucesso.");

            try
            {
                string insertQuery = $"update FP_COLETORMAQUINA set fcl_ultimonsr = {batidas.LastOrDefault().NSR}, FCL_ULTIMAATUALIZACAO = '{DateTime.Now.ToString("yyyy-MM-dd")}'  where fcl_id = {batidas.LastOrDefault().IdMaquina}";
                using (OdbcCommand command = new OdbcCommand(insertQuery, connection))
                {
                    int rowsAffected = command.ExecuteNonQuery();
                    Logs.LogAction(AppConfig.LogIdentifier, "Status Maquina Coletora alterada");
                }
            }
            catch (Exception error)
            {
                Logs.LogError("Erro alterar status do relogio ponto" + error.Message);
            }
        }
    }
}

