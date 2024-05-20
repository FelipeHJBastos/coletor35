using System;
namespace coletor35.Entidade
{
    public class Fp_ColetaMarcacoes
    {
        public virtual int Id { get; set; }
        public virtual int ColetorMaquinaId { get; set; }
        public virtual string Pis { get; set; }
        public virtual DateTime Data { get; set; }
        public virtual TimeSpan Hora { get; set; }
        public virtual int Nsr { get; set; }
        public virtual int TipoRegistro { get; set; }
        public virtual string RegistroCru { get; set; }
        public virtual string Cpf { get; set; }
    }
}