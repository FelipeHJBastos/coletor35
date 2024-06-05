using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace coletor35
{
    internal class Logs
    {
        public static void LogAction(int logIdentifier, string actionDescription, string jsonData = null)
        {
            if (logIdentifier == 1)
            {
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                string configFilePath = Path.Combine(exePath, "log.txt");

                string json = jsonData != null ? JsonConvert.SerializeObject(jsonData) : ""; //Se recebe objeto json imprime. Se não, permanece nulo

                string logMessage = $"{DateTime.Now}: {actionDescription}\n";
                if (!string.IsNullOrEmpty(json))
                {
                    logMessage += $"Dados: {json}\n";
                }
                // Escrevendo no arquivo de log
                File.AppendAllText(configFilePath, logMessage);
            }
        }
        public static void LogError(string message)
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string configFilePath = Path.Combine(exePath, "errorLog.txt");

            string errorMessage = $"{DateTime.Now}: {message}\n";
            File.AppendAllText(configFilePath, errorMessage);
        }
    }
}
