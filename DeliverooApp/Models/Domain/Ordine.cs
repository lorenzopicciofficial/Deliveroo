namespace DeliverooApp.Models;

public class Ordine
{
    public int IdOrdine { get; set; }
    public int IdUtente { get; set; }
    public DateTime Data { get; set; }
    public string NomeCliente { get; set; }
    public string Indirizzo { get; set; }
    public double ImportoTotale { get; set; }
    public string Stato { get; set; }
    public DateTime? DataConferma { get; set; }
    public DateTime? DataConsegna { get; set; }
    public int TempoStimato { get; set; } = 30;
    public string Note { get; set; }

    public override string ToString()
    {
        return IdOrdine + " " + IdUtente + " " + NomeCliente;
    }
}