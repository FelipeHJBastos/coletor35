using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace coletor35.Entidade
{
    public class RelogioPonto
    {
        public string IP { get; set; }// ip que o relogio está cadastrado.
        public string ChaveRSA { get; set; } //chave de segurança do relogio.
        public string ExpoenteRSA { get; set; } //chave de segurança do relogio.
        public string NSR { get; set; } // Identificador do numero da batida que está sendo coletada.
        public int IdMaquina { get; set; } //Identificador da maquina em que as batidas serão adicionadas no gespam
    }
}
