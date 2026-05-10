namespace DeliverooApp.Models;

public class Articolo
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string FotoUrl { get; set; }
    public double PrezzoListino { get; set; }
    public string Categoria { get; set; }
    public int NumOrdini { get; set; }
    public string Descrizione { get; set; }
    public string Ingredienti { get; set; }
    public int TempoPreparazione { get; set; }
    public string Allergeni { get; set; }
    public bool Disponibile { get; set; }

    
    public override string ToString()
    {
        return Id + "   " + Nome ;
    }
}