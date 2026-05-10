namespace DeliverooApp.Models;

public class RigaDettaglio
{
    public int Id { get; set; }
    public int IdOrdine { get; set; }
    public int IdArticolo { get; set; }
    public int Quantita { get; set; }
    public double Prezzo { get; set; }

    public override string ToString()
    {
        return IdArticolo + " " + Quantita;
    }
}