namespace DeliverooApp.Models;

public class Recensione
{
    public int Id { get; set; }
    public int IdUtente { get; set; }
    public int IdArticolo { get; set; }
    public int Voto { get; set; }
    public DateTime Data { get; set; }
}