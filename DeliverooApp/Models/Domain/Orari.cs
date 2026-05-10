namespace DeliverooApp.Models;

public class Orari
{
    public int Id { get; set; }
    public TimeSpan OrarioApertura { get; set; }
    public TimeSpan OrarioChiusura { get; set; }
    public string GiorniAperti { get; set; }
    public string MessaggioChiusura { get; set; }

    public bool IsAperto
    {
        get
        {
            var ora = DateTime.Now.TimeOfDay;
            int giornoSettimana = (int)DateTime.Now.DayOfWeek;
            if (giornoSettimana == 0) giornoSettimana = 7;
            var giorniLista = GiorniAperti.Split(',').Select(int.Parse).ToList();
            return giorniLista.Contains(giornoSettimana) &&
                   ora >= OrarioApertura &&
                   ora <= OrarioChiusura;
        }
    }
}