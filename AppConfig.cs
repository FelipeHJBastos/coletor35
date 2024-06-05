using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using coletor35;
using System.Runtime.InteropServices;
using System.IO;
using System.Net.NetworkInformation;


namespace coletor35
{
    internal class AppConfig
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool HideConsole();

        public static string IP { get; private set; }// ip que o relogio está cadastrado.
        public static string ChaveRSA { get; private set; } //chave de segurança do relogio.
        public static string ExpoenteRSA { get; private set; }
        public static string User { get; private set; }// padrao = "login"
        public static string Password { get; private set; } // padrão = "senha"
        public static string NSR { get; set; } // Identificador do numero da batida que está sendo coletada.
        public static int LogIdentifier { get; private set; } //Identificador se log está ativo. 1 = sim, 2 = não
        public static int IdMaquina { get; private set; } //Identificador da maquina em que as batidas serão adicionadas no gespam
        public static string ConnectionString { get; private set; } //URL do gespam da prefeitura

        public AppConfig()
        {
            LoadConfig();
        }

        public static void LoadConfig()
        {
            try
            {
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                string configFilePath = Path.Combine(exePath, "config.json");

                //Verifica se projeto contém configuração.
                if (!File.Exists(configFilePath))
                {
                    CreateConfigFile();
                }

                string jsonConfig = File.ReadAllText(configFilePath);
                var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonConfig);

                IP = config["ip"];
                ChaveRSA = config["chaveRSA"];
                ExpoenteRSA = config["expoenteRSA"];
                User = config["user"];
                Password = config["password"];
                NSR = config["nsr"];
                LogIdentifier = Int32.Parse(config["logIdentifier"]);
                ConnectionString = config["connectionString"];
                IdMaquina = Int32.Parse(config["idMaquina"]);

                Logs.LogAction(LogIdentifier, "Dados de Configuração obtida!");

            }
            catch (Exception ex)
            {
                Logs.LogError("Erro ao buscar parâmetros de configurações: " + ex.Message);
            }
        }

        public static void CreateConfigFile()
        {
            AllocConsole();

            Console.WriteLine("Configuração do arquivo 'config.json' não encontrada.");
            Console.WriteLine("Por favor, insira as configurações necessárias:");

            var config = new Dictionary<string, string>();

            Console.Write("IP do relógio ponto: ");
            config["ip"] = Console.ReadLine();

            Console.Write("Chave RSA: ");
            config["chaveRSA"] = Console.ReadLine();

            Console.Write("Expoente RSA: ");
            config["expoenteRSA"] = Console.ReadLine();

            Console.Write("Usuário (login): ");
            config["user"] = Console.ReadLine();

            Console.Write("Senha (senha): ");
            config["password"] = Console.ReadLine();

            Console.Write("NSR (Número Sequencial de Registro): ");
            config["nsr"] = Console.ReadLine();

            Console.Write("Caminho do arquivo de destino (filePath): ");
            config["filePath"] = Console.ReadLine();

            Console.Write("Quer que seja criado um arquivo de logs da aplicação? (1 = Sim, 2 = Não)     ATENÇÃO: Não deixe esta opção como SIM após o teste da aplicação");
            config["logIdentifier"] = Console.ReadLine();

            Console.Write("Id do relógio dentro do sistema: ");
            config["idMaquina"] = Console.ReadLine();

            config["connectionString"] = "DSN=ODBCDATABASE;Uid=USER;Pwd=PASSWORD;";

            string jsonConfig = JsonConvert.SerializeObject(config, Formatting.Indented);

            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string configFilePath = Path.Combine(exePath, "config.json");

            File.WriteAllText(configFilePath, jsonConfig);

            Logs.LogAction(LogIdentifier, "Arquivo 'config.json' criado com sucesso.");
            HideConsole();
        }
    }
}
