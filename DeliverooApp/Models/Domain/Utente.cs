namespace DeliverooApp.Models;

public class Utente
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Nome { get; set; }
    public string Cognome { get; set; }
    public string Telefono { get; set; }
    public string Indirizzo { get; set; }
    public string CartaIntestatario { get; set; }
    public string CartaUltime4 { get; set; }
    public string CartaScadenza { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Ruolo { get; set; }
    public DateTime DataCreazione { get; set; }
    public string DomandaSicurezza { get; set; }
    public string RispostaSicurezza { get; set; }
}