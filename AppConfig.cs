using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using coletor35;
using System.Runtime.InteropServices;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Linq;
using coletor35.Entidade;


namespace coletor35
{

    internal class AppConfig
    {
        public static int LogIdentifier { get; set; }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool HideConsole();

        public AppConfig()
        {
            LoadConfig();
        }

        public static Parametros Parametros { get; set; }
        public static string nsr{ get; set; }


        public static void LoadConfig()
        {
            try
            {
                string exePath = AppDomain.CurrentDomain.BaseDirectory; //Pega caminho da pasta raiz do projeto.
                string configFilePath = Path.Combine(exePath, "config.json"); //Raiz do projeto + nome do arquivo de configuração.

                //Verifica se projeto contém configuração.
                if (!File.Exists(configFilePath))
                {
                    CreateConfigFile(); //TODO: Ajustar saporra pra criar arquivo base de configuração direito.
                }

                string jsonConfig = File.ReadAllText(configFilePath);
                var config = JsonConvert.DeserializeObject<Parametros>(jsonConfig);

                Parametros = config;
                LogIdentifier = int.Parse(config.LogIdentifier);
                Logs.LogAction(LogIdentifier, "Parâmetros obtidos!");
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
