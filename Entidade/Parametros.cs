using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace coletor35.Entidade
{
    public class Parametros
    {
        [JsonProperty("maquinas")]
        public List<RelogioPonto> Maquinas { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("logIdentifier")]
        public string LogIdentifier { get; set; }

        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }
    }
    //{
    //    public static List<Relogio> Maquinas { get; set; }
    //    public static string User { get; set; }// padrao = "login"
    //    public static string Password { get; set; } // padrão = "senha"
    //    public static int LogIdentifier { get; set; } //Identificador se log está ativo. 1 = sim, 2 = não
    //    public static string ConnectionString { get; set; } //URL do gespam da prefeitura
    //}
}
