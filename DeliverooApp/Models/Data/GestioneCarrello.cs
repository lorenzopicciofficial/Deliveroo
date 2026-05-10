using System.Text.Json;
using MySqlX.XDevAPI;

namespace DeliverooApp.Models;

public class GestioneCarrello
{
    private ISession sessioneCarrello;

    public GestioneCarrello(ISession sessionCarrello)
    {
        this.sessioneCarrello = sessionCarrello;
    }
    
    public void SalvaCarrello(List<Articolo> articoloSelezionato)
    {
        string json = JsonSerializer.Serialize(articoloSelezionato);
        sessioneCarrello.SetString("carrello",json);
    }

    public int NumeroElementiCarrello()
    {
        return RecuperaCarrello().Count;
    }

    public List<Articolo> RecuperaCarrello()
    {
        List<Articolo> lista;
        string json = sessioneCarrello.GetString("carrello");
        if (json == null)
        {
            lista = new List<Articolo>();
        }
        else
        {
            lista= JsonSerializer.Deserialize<List<Articolo>>(json);
        }

        return lista;
    }

    //ancora da implementare
    public void RimuoviArticolo(int idArticolo)
    {
        List<Articolo> lista = RecuperaCarrello();
        lista.RemoveAll(a => a.Id == idArticolo);
        SalvaCarrello(lista);
    }

    public void SvuotaCarrello()
    {
        sessioneCarrello.Remove("carrello");
    }
    
    public double TotaleCarrello()
    {
        return RecuperaCarrello().Sum(a => a.PrezzoListino);
    }
    
    public void AggiungiArticolo(Articolo articolo)
    {
        List<Articolo> lista = RecuperaCarrello();
        lista.Add(articolo);
        SalvaCarrello(lista);
    }

    public void RimuoviUno(int idArticolo)
    {
        List<Articolo> lista = RecuperaCarrello();
        int index = lista.FindIndex(a => a.Id == idArticolo);
        if (index >= 0)
            lista.RemoveAt(index);
        SalvaCarrello(lista);
    }

    public void RimuoviTutti(int idArticolo)
    {
        List<Articolo> lista = RecuperaCarrello();
        lista.RemoveAll(a => a.Id == idArticolo);
        SalvaCarrello(lista);
    }

    public Dictionary<int, int> GetQuantita()
    {
        return RecuperaCarrello()
            .GroupBy(a => a.Id)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public List<Articolo> GetArticoliDistinti()
    {
        return RecuperaCarrello()
            .GroupBy(a => a.Id)
            .Select(g => g.First())
            .ToList();
    }
}