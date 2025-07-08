namespace BancoDigitalAna.Core.Entities
{
    public class Transferencia
    {
        public int IdTransferencia { get; set; }
        public int IdContaCorrente_Origem { get; set; }
        public int IdContaCorrente_Destino { get; set; }
        public DateTime DataMovimento { get; set; }
        public float Valor { get; set; }
    }
}
