namespace DeliverooApp.Models;

public class Sconto
{
    public int Id { get; set; }
    public int IdArticolo { get; set; }
    public int Percentuale { get; set; }
    public DateTime DataInizio { get; set; }
    public DateTime DataFine { get; set; }

    // Proprietà calcolata — non viene dal DB
    public bool IsAttivo => DateTime.Now >= DataInizio && DateTime.Now <= DataFine;
}