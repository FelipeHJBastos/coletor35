using coletor35;
using Newtonsoft.Json;
using org.cesar.dmplight.watchComm.api;
using org.cesar.dmplight.watchComm.impl;
using System;
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

            //SendEmployees.GetEmployees();

            //instancia watchcomm para conexão com relogio.
            var watchComm = InstanciaWatchComm();
            watchComm.OpenConnection();
            var batidas = FetchMRPRecords(watchComm, AppConfig.NSR);
            watchComm.CloseConnection();


//            var batidas = new List<Marcacao>
//            {
//    new Marcacao { Cpf = "66743397091", DateTimeMarkingPoint = DateTime.Parse("23/02/2024 17:42"), NSR = "7" },
//    new Marcacao { Cpf = "66743397091", DateTimeMarkingPoint = DateTime.Parse("23/02/2024 17:43"), NSR = "8" },
//    new Marcacao { Cpf = "66743397091", DateTimeMarkingPoint = DateTime.Parse("23/02/2024 17:43"), NSR = "9" },
//    new Marcacao { Cpf = "66743397091", DateTimeMarkingPoint = DateTime.Parse("23/02/2024 17:43"), NSR = "10" },
//    new Marcacao { Cpf = "66743397091", DateTimeMarkingPoint = DateTime.Parse("26/02/2024 08:52"), NSR = "12" },
//    new Marcacao { Cpf = "66743397091", DateTimeMarkingPoint = DateTime.Parse("26/02/2024 08:52"), NSR = "13" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("05/03/2024 13:46"), NSR = "26" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("05/03/2024 13:47"), NSR = "27" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("05/03/2024 14:04"), NSR = "28" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("05/03/2024 14:31"), NSR = "29" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("05/03/2024 17:12"), NSR = "30" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("05/03/2024 17:14"), NSR = "31" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("15/03/2024 09:57"), NSR = "36" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("15/03/2024 10:50"), NSR = "37" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("15/03/2024 14:10"), NSR = "48" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("15/03/2024 16:28"), NSR = "51" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("18/03/2024 07:11"), NSR = "55" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("03/04/2024 14:41"), NSR = "77" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("03/04/2024 14:41"), NSR = "78" },
//    new Marcacao { Cpf = "66743397091", DateTimeMarkingPoint = DateTime.Parse("03/04/2024 16:40"), NSR = "81" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("07/05/2024 22:12"), NSR = "114" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("20/05/2024 06:17"), NSR = "120" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("20/05/2024 06:18"), NSR = "121" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("20/05/2024 06:18"), NSR = "122" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("20/05/2024 12:27"), NSR = "124" },
//    new Marcacao { Cpf = "2938870043", DateTimeMarkingPoint = DateTime.Parse("20/05/2024 12:28"), NSR = "126" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("20/05/2024 12:32"), NSR = "127" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("20/05/2024 12:32"), NSR = "128" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("20/05/2024 12:42"), NSR = "132" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("20/05/2024 12:45"), NSR = "133" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("21/05/2024 06:00"), NSR = "134" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("21/05/2024 06:00"), NSR = "135" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("22/05/2024 06:55"), NSR = "137" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("22/05/2024 08:43"), NSR = "139" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("22/05/2024 14:32"), NSR = "140" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("22/05/2024 14:32"), NSR = "141" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("22/05/2024 14:59"), NSR = "142" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("22/05/2024 16:42"), NSR = "143" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 09:11"), NSR = "146" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 09:11"), NSR = "147" },
//    new Marcacao { Cpf = "66743397091", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 11:01"), NSR = "148" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 11:01"), NSR = "149" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 11:04"), NSR = "150" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 11:05"), NSR = "151" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 11:09"), NSR = "152" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 11:10"), NSR = "153" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 11:11"), NSR = "154" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 11:11"), NSR = "155" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 11:12"), NSR = "156" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 11:12"), NSR = "157" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 11:14"), NSR = "160" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("23/05/2024 15:36"), NSR = "161" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("24/05/2024 07:05"), NSR = "162" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("24/05/2024 07:05"), NSR = "164" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("28/05/2024 07:10"), NSR = "168" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("29/05/2024 15:50"), NSR = "170" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("29/05/2024 16:19"), NSR = "171" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("29/05/2024 16:51"), NSR = "172" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("04/06/2024 07:53"), NSR = "173" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("05/06/2024 11:55"), NSR = "174" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("05/06/2024 11:56"), NSR = "175" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("05/06/2024 16:07"), NSR = "176" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("05/06/2024 16:53"), NSR = "177" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("06/06/2024 07:09"), NSR = "178" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("06/06/2024 11:12"), NSR = "179" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("06/06/2024 11:16"), NSR = "180" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("06/06/2024 17:00"), NSR = "181" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("07/06/2024 11:28"), NSR = "182" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("07/06/2024 12:00"), NSR = "183" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("07/06/2024 13:23"), NSR = "184" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("12/06/2024 11:02"), NSR = "185" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("12/06/2024 11:04"), NSR = "186" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("12/06/2024 11:04"), NSR = "187" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("12/06/2024 11:06"), NSR = "188" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("12/06/2024 11:41"), NSR = "189" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("12/06/2024 11:47"), NSR = "190" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("12/06/2024 11:47"), NSR = "191" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("12/06/2024 15:09"), NSR = "192" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("13/06/2024 07:17"), NSR = "193" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("14/06/2024 08:53"), NSR = "194" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("17/06/2024 10:21"), NSR = "196" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("20/06/2024 09:05"), NSR = "197" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("20/06/2024 14:17"), NSR = "198" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("24/06/2024 16:55"), NSR = "199" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("02/07/2024 17:00"), NSR = "294" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 08:04"), NSR = "296" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 08:09"), NSR = "297" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 14:00"), NSR = "299" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 14:00"), NSR = "300" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 14:01"), NSR = "301" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 14:04"), NSR = "305" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 14:04"), NSR = "306" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 14:04"), NSR = "307" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 14:14"), NSR = "308" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 14:26"), NSR = "309" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 14:33"), NSR = "310" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 14:38"), NSR = "311" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 16:57"), NSR = "312" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("03/07/2024 16:57"), NSR = "313" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("04/07/2024 07:25"), NSR = "315" },
//    new Marcacao { Cpf = "2312579022", DateTimeMarkingPoint = DateTime.Parse("04/07/2024 07:38"), NSR = "316" },
//    new Marcacao { Cpf = "88430510087", DateTimeMarkingPoint = DateTime.Parse("04/07/2024 07:50"), NSR = "317" }
//};



            EnviaMarcacoes(batidas);
            UpdateConfigNsr();
            AtualizaStatusMaquina();

        }
        catch (Exception ex)
        {
            Logs.LogError("Erro inesperado: " + ex.Message);
        }
    }
    public static WatchComm InstanciaWatchComm()
    {
        try
        {
            TCPComm tcpComm = new TCPComm(AppConfig.IP, 3000);
            tcpComm.SetTimeOut(15000);

            var watchComm = new WatchComm(
                WatchProtocolType.REPC, //modelo do relogio
                tcpComm, //ip no formato necessario
                1,
                "",
                WatchConnectionType.ConnectedMode,
                "01.00.0000",
                AppConfig.ChaveRSA,
                AppConfig.ExpoenteRSA,
                AppConfig.User,
                AppConfig.Password
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

    public static List<Marcacao> FetchMRPRecords(WatchComm watchComm, string nsr)
    {
        List<Marcacao> batidas = new List<Marcacao>();
        try
        {
            int nsrReposiciona = int.Parse(nsr) + 1; 
            nsr = nsrReposiciona.ToString();

            try { 
                watchComm.RepositioningMRPRecordsPointer(nsr);//Reposiciona nsr para pegar batidas a partir do numero reposicionamento.
                Logs.LogAction(AppConfig.LogIdentifier, $@"Reposicionou NSR: {nsr}");
            }
            catch {
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
                        NSR = recordDeserialized["NSR"]
                    });

                    AppConfig.NSR = record.NSR;
                }
            records = watchComm.ConfirmationReceiptMRPRecords();
            }
            if(records != null)
            {
                Logs.LogAction(AppConfig.LogIdentifier, $@"Buscou batidas do relógio a partir do nsr {int.Parse(nsr + 1).ToString()}");
            } else
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

    private static void UpdateConfigNsr()
    {
        string exePath = AppDomain.CurrentDomain.BaseDirectory;
        string configFilePath = Path.Combine(exePath, "config.json");
        try
        {
            var json = File.ReadAllText(configFilePath);
            var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            config["nsr"] = AppConfig.NSR; // Atualiza o NSR com o novo valor

            string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(configFilePath, updatedJson); // Salva as alterações no arquivo de configuração

            Logs.LogAction(AppConfig.LogIdentifier, $"NSR atualizado para {AppConfig.NSR} no arquivo de configuração.");
        }
        catch (Exception ex)
        {
            Logs.LogError("Erro ao atualizar NSR no arquivo de configuração: " + ex.Message);
        }
    }

    public static void EnviaMarcacoes(List<Marcacao> batidas)
    {
        string connectionString = AppConfig.ConnectionString;
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
                        command.Parameters.AddWithValue("@fcl_id", AppConfig.IdMaquina);
                        command.Parameters.AddWithValue("@fcl_pis", pisFuncionario.PadLeft(12, '0'));
                        command.Parameters.AddWithValue("@fcl_data", marcacao.DateTimeMarkingPoint);
                        command.Parameters.AddWithValue("@fcl_hora", marcacao.DateTimeMarkingPoint.ToString("HH:mm:ss"));
                        command.Parameters.AddWithValue("@fcl_nsr", Int32.Parse(marcacao.NSR));
                        command.Parameters.AddWithValue("@fcm_tiporegistro", 3);
                        command.Parameters.AddWithValue("@fcm_registrocru", registroCru);

                        int rowsAffected = command.ExecuteNonQuery();
                        AppConfig.NSR = marcacao.NSR;

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

    public static void AtualizaStatusMaquina()
    {
        string connectionString = AppConfig.ConnectionString;
        Logs.LogAction(AppConfig.LogIdentifier, "Atualizando status da maquina");

        using (OdbcConnection connection = new OdbcConnection(connectionString))
        {
            connection.Open();
            Logs.LogAction(AppConfig.LogIdentifier, "Conexão com banco de dados estabelecida com sucesso.");

            try
            {
                string insertQuery = $"update FP_COLETORMAQUINA set fcl_ultimonsr = {AppConfig.NSR}, FCL_ULTIMAATUALIZACAO = '{DateTime.Now.ToString("yyyy-MM-dd")}'  where fcl_id = {AppConfig.IdMaquina}";
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
    /*
    public static void Afd(List<Marcacao> batidas, Dictionary<string, string> Cpfs)
    {
        string[] lines = File.ReadAllLines(AppConfig.FilePath);
        StringBuilder afd = new StringBuilder();
        StringBuilder afdCabecalho = new StringBuilder();

        afdCabecalho.Append("000000000");//Espaço antes do conteúdo do cabeçalho
        afdCabecalho.Append("1");// Tipo de registo. Cabeçalho = 1
        afdCabecalho.Append("1");// Tipo de Identificador do empregador. Cnpj = 1, Cpf = 2
        afdCabecalho.Append(AppConfig.Cnpj);//Cnpj da empresa
        afdCabecalho.Append("000000000000");//CNO ou CAEPF se existir
        afdCabecalho.Append(AppConfig.RazaoSocial.PadRight(150));//Razão Social- 150 caracteres por padrão
        afdCabecalho.Append(AppConfig.SerialNumber);//Numero Serial do relogio ponto
        afdCabecalho.Append(AppConfig.DataPrimeiroRegistro); //Data do primeiro registro do arquivo
        afdCabecalho.Append(AppConfig.DataUltimoRegistro); //Data do ultimo registro do arquivo
        afdCabecalho.Append(DateTime.Now.ToString("ddMMyyyyHH")); //Data e hora da geração do arquivo
        afdCabecalho.Append("03");// versão de layout do afd. padrão 003
        if (lines.Length == 0)
        {
            afdCabecalho.Append('\n');
        }

        using (StreamWriter writer = new StreamWriter(AppConfig.FilePath))
        {
            if (lines.Length == 0)
            {
                writer.WriteLine(afdCabecalho);
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    writer.WriteLine(afdCabecalho);
                }
                else if (lines[i] != "")
                {
                    writer.WriteLine(lines[i]);
                }
            }
        }

        for (var i = 0; i < batidas.Count; ++i)
        {
            StringBuilder afdBatidas = new StringBuilder();
            if (Cpfs.ContainsKey(batidas[i].Cpf))
            {
                afdBatidas.Append(batidas[i].NSR);//NSR da batida
                afdBatidas.Append('3');//padrão de layout
                afdBatidas.Append(batidas[i].DateTimeMarkingPoint.ToString("ddMMyyyyHHmm"));//Data e hora da marcacao
                afdBatidas.Append(Cpfs[batidas[i].Cpf].PadLeft(12, '0')); //retorna pis pelo dicionario retornado do gespam
                if (i != batidas.Count - 1)
                {
                    afdBatidas.Append('\n');
                }
                afd.Append(afdBatidas);
            }
        }

        using (StreamWriter writer = new StreamWriter(AppConfig.FilePath, true))
        {
            writer.WriteLine(afd.ToString());
        }
    }
    */
}

