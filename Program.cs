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
            //var watchComm = InstanciaWatchComm();
            //watchComm.OpenConnection();

            //var batidas = FetchMRPRecords(watchComm, AppConfig.NSR);
            var batidas = new List<Marcacao>()
            {
                new Marcacao() {
                    Cpf = "02312579022",
                    DateTimeMarkingPoint = DateTime.Now,
                    NSR = "1"
                },
                new Marcacao() {
                    Cpf = "88430510087",
                    DateTimeMarkingPoint = DateTime.Now.AddDays(1),
                    NSR = "2"
                },
                new Marcacao() {
                    Cpf = "02938870043",
                    DateTimeMarkingPoint = DateTime.Now.AddDays(2),
                    NSR = "3"
                }
            };

            //watchComm.CloseConnection();

            EnviaMarcacoes(batidas);
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
        var lastNsr = 0;
        var dataPrimeiroRegistro = new DateTime();
        var dataUltimoRegistro = new DateTime();
        try
        {
            watchComm.RepositioningMRPRecordsPointer(nsr);//Reposiciona nsr para pegar batidas a partir do numero reposicionamento.
            var records = watchComm.InquiryMRPRecords(false, false, true /* retorna marcação de ponto */, false, false);
            var recordIndex = 0;

            while (records != null)
            {
                foreach (var record in records)
                {
                    {
                        if (record.IsValid)
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

                            if (recordIndex == 0)
                            {
                                dataPrimeiroRegistro = DateTime.Parse(recordDeserialized["DateTimeMarkingPoint"]);
                            }
                            dataUltimoRegistro = DateTime.Parse(recordDeserialized["DateTimeMarkingPoint"]);
                            lastNsr = Int32.Parse(record.NSR);
                        }
                    }
                    recordIndex++;
                }
                records = watchComm.ConfirmationReceiptMRPRecords();
            }
            UpdateConfigNsr(lastNsr.ToString());
            AppConfig.AtualizaDataRegistros(dataPrimeiroRegistro, dataUltimoRegistro);
            Logs.LogAction(AppConfig.LogIdentifier, $@"Buscou batidas do relógio a partir do nsr {nsr}");
        }
        catch (Exception ex)
        {
            Logs.LogError("Erro ao buscar registros MRP: " + ex.Message);
        }
        return batidas;
    }

    private static void UpdateConfigNsr(string newNsr)
    {
        string configPath = "config.json"; // Define o caminho do arquivo de configuração
        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            config["nsr"] = newNsr; // Atualiza o NSR com o novo valor

            string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(configPath, updatedJson); // Salva as alterações no arquivo de configuração

            Logs.LogAction(AppConfig.LogIdentifier, $"NSR atualizado para {newNsr} no arquivo de configuração.");
        }
        catch (Exception ex)
        {
            Logs.LogError("Erro ao atualizar NSR no arquivo de configuração: " + ex.Message);
        }
    }

    public static void EnviaMarcacoes(List<Marcacao> batidas)
    {

        string connectionString = AppConfig.ConnectionString;
        string fp28 = "";

        using (OdbcConnection connection = new OdbcConnection(connectionString))
        {
            // Abra a conexão
            connection.Open();
            Logs.LogAction(AppConfig.LogIdentifier, "Conexão com banco de dados estabelecida com sucesso.");

            // Primeiro, execute o SELECT para obter o nome da tabela
            string selectQuery = "SELECT UPPER(table_name) AS tablename FROM SYS.SYSTABLE WHERE TABLE_NAME LIKE 'FP28%' ORDER BY table_id DESC";
            string tableName = null;

            using (OdbcCommand selectCommand = new OdbcCommand(selectQuery, connection))
            {
                using (OdbcDataReader reader = selectCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        tableName = reader.GetString(0);
                        fp28 = tableName;
                        Logs.LogAction(AppConfig.LogIdentifier, "Tabela de contratos obtida: " + fp28);
                    }
                    else
                    {
                        Logs.LogError("Nenhuma tabela encontrada com o padrão 'FP28%'");
                        return;
                    }
                }
            }
            
            foreach (var marcacao in batidas)
            {
                var pisFuncionario = "";
                string selectFuncionario = $"select nomefuncionario, pispasep, numerocpf from {fp28} where numerocpf = {decimal.Parse(marcacao.Cpf)}";

                using (OdbcCommand selectCommand = new OdbcCommand(selectFuncionario, connection))
                {
                    using (OdbcDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pisFuncionario = reader[1].ToString();
                            Logs.LogAction(AppConfig.LogIdentifier, "Pis obtido do funcionario: " + reader[0]);
                        }
                        else
                        {
                            Logs.LogError("Funcionário não encontrado. Cpf: " + marcacao.Cpf);
                            return;
                        }
                    }
                }
                try 
                {
                    string insertQuery = "insert into fp_coletamarcacoes (fcl_id, fcl_pis, fcl_data, fcl_hora, fcl_nsr, fcm_tiporegistro, fcm_registrocru )" +
                        "values (?, ?, ?, ?, ?, ?, ?)";
                    string registroCru = marcacao.NSR.PadLeft(9, '0') + "3" + marcacao.DateTimeMarkingPoint.ToString("ddMMyyyyhhmm") + pisFuncionario.PadLeft(12, '0');
                    
                    using (OdbcCommand command = new OdbcCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@fcl_id", AppConfig.IdMaquina);
                        command.Parameters.AddWithValue("@fcl_pis", pisFuncionario);
                        command.Parameters.AddWithValue("@fcl_data", marcacao.DateTimeMarkingPoint);
                        command.Parameters.AddWithValue("@fcl_hora", marcacao.DateTimeMarkingPoint.ToString("HH:mm:ss"));
                        command.Parameters.AddWithValue("@fcl_nsr", Int32.Parse(marcacao.NSR));
                        command.Parameters.AddWithValue("@fcm_tiporegistro", 3);
                        command.Parameters.AddWithValue("@fcm_registrocru", registroCru);

                        int rowsAffected = command.ExecuteNonQuery();
                        Logs.LogAction(AppConfig.LogIdentifier, "Marcação inserida na tabela: " + marcacao);
                    }
                }
                catch(Exception error)
                {
                    Logs.LogError("Erro ao Inserir dados na tabela Fp_ColetaMarcacoes: " + error.Message);
                }

            }
        }
    }

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

}

