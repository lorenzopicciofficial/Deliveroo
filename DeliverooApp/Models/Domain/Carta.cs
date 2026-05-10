namespace DeliverooApp.Models;

public class Carta
{
    public int Id { get; set; }
    public int IdOrdine { get; set; }
    public string Intestatario { get; set; }
    public string Ultime4Cifre { get; set; }
    public string Scadenza { get; set; }
    public DateTime DataInserimento { get; set; }
}