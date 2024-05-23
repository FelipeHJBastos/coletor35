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



internal class Program
{
    //Controle de console
    private static void Main()
    {
        try
        {
            AppConfig.LoadConfig(); //Carrega variáveis de configuração.

            //instancia watchcomm para conexão com relogio.
            var watchComm = InstanciaWatchComm();
            watchComm.OpenConnection();

            var batidas = FetchMRPRecords(watchComm, AppConfig.NSR);

            watchComm.CloseConnection();

            var Cpfs = GetPis(batidas);

            Afd(batidas, Cpfs);
            //GenerateAfdFile(batidas);
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
    public static List<Fp_ColetaMarcacoes> TransformarBatidas(List<Marcacao> records)
    {
        List<Fp_ColetaMarcacoes> marcacoes = new List<Fp_ColetaMarcacoes>();
        foreach (var record in records)
        {
            var marcacao = new Fp_ColetaMarcacoes
            {
                Id = 0, // Id vai vazio
                ColetorMaquinaId = AppConfig.IdMaquina,
                Pis = "", // Pis vai vazio
                Data = record.DateTimeMarkingPoint,
                Hora = record.DateTimeMarkingPoint.TimeOfDay,
                Nsr = int.Parse(record.NSR),
                TipoRegistro = 3, // Tipo de registro sempre 3
                RegistroCru = "", // Registro Cru vai vazio
                Cpf = record.Cpf
            };
            marcacoes.Add(marcacao);
        }
        return marcacoes;
    }

    public static Dictionary<string, string> GetPis(List<Marcacao> records)
    {
        List<Fp_ColetaMarcacoes> marcacoes = TransformarBatidas(records);

        HashSet<string> cpfsUnicos = new HashSet<string>();
        foreach (var batida in marcacoes)
        {
            cpfsUnicos.Add(batida.Cpf);
        }

        var listaCpf = cpfsUnicos.ToList();

        var jsonPayload = JsonConvert.SerializeObject(new { listaCpf });

        if (records.Count > 0)
        {
            var request = (HttpWebRequest)WebRequest.Create($"{AppConfig.GespamUrl}/Fp_ColetaMarcacoes/BuscaPisporCpf");
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonPayload);
            request.ContentLength = byteArray.Length;

            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string responseString = reader.ReadToEnd();
                            var lista = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                            // Faça algo com a lista de CPFs e PIS recebidos
                            return lista;
                        }
                    }
                    else
                    {
                        throw new Exception("Erro ao enviar CPFs únicos");
                    }
                }
            }
            catch (WebException ex)
            {
                using (var response = ex.Response)
                {
                    if (response != null)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string errorText = reader.ReadToEnd();
                            Console.WriteLine($"Erro na requisição: {errorText}");
                        }
                    }
                }
            }
        }
        return null;
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
                else
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

