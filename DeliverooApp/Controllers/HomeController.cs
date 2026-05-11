using System.Diagnostics;
using System.Security.Cryptography;
using DeliverooApp.Filters;
using Microsoft.AspNetCore.Mvc;
using DeliverooApp.Models;

namespace DeliverooApp.Controllers;

public class HomeController : Controller
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;
    private GestioneDati gestioneDati;
    private GestioneCarrello gestioneCarrello;

    public HomeController(ILogger<HomeController> logger, IHttpContextAccessor contextAccessor, IConfiguration configuration, IWebHostEnvironment env)
    {
        _contextAccessor = contextAccessor;
        _logger = logger;
        _configuration = configuration;
        _env = env;
        gestioneDati = new GestioneDati(_configuration.GetConnectionString("Default")!);
        gestioneCarrello = new GestioneCarrello(_contextAccessor.HttpContext.Session);

        // Usa il DB se loggato, la sessione altrimenti
        int? idUtente = _contextAccessor.HttpContext.Session.GetInt32("idUtente");
        if (idUtente != null)
            _contextAccessor.HttpContext.Items["numero"] = gestioneDati.NumeroArticoliCarrelloDB(idUtente.Value);
        else
            _contextAccessor.HttpContext.Items["numero"] = gestioneCarrello.NumeroElementiCarrello();
    }
 
    
    //-----PER AUTENTICAZIONE LOGIN-----
    public IActionResult Login()
    {
        if (_contextAccessor.HttpContext.Session.GetString("user") != null)
            return RedirectToAction("Index");
        return View();
    }
    
    [HttpPost]
    public IActionResult Login(Utente utente)
    {
        string adminUser = _configuration.GetRequiredSection("AdminCredentials").GetValue<string>("UserName");
        string adminPass = _configuration.GetRequiredSection("AdminCredentials").GetValue<string>("Password");

        if (utente.UserName == adminUser && BCrypt.Net.BCrypt.Verify(utente.Password, adminPass))
        {
            _contextAccessor.HttpContext.Session.SetString("user", adminUser);
            _contextAccessor.HttpContext.Session.SetString("ruolo", "admin");
            return RedirectToAction("Index");
        }

        Utente trovato = gestioneDati.RecuperaUtenteConNome(utente.UserName);
        if (trovato != null && BCrypt.Net.BCrypt.Verify(utente.Password, trovato.Password))
        {
            _contextAccessor.HttpContext.Session.SetString("user", trovato.UserName);
            _contextAccessor.HttpContext.Session.SetString("ruolo", trovato.Ruolo);
            _contextAccessor.HttpContext.Session.SetInt32("idUtente", trovato.Id);
            _contextAccessor.HttpContext.Session.SetString("nomeCompleto", 
                $"{trovato.Nome} {trovato.Cognome}".Trim());
            _contextAccessor.HttpContext.Session.SetString("indirizzoDefault", trovato.Indirizzo ?? "");
            _contextAccessor.HttpContext.Session.SetString("cartaIntestatario", trovato.CartaIntestatario ?? "");
            _contextAccessor.HttpContext.Session.SetString("cartaUltime4", trovato.CartaUltime4 ?? "");
            _contextAccessor.HttpContext.Session.SetString("cartaScadenza", trovato.CartaScadenza ?? "");
            return RedirectToAction("Index");
        }
        ViewData["errore"] = "Username o password errati.";
        return View();
    }

    //------REGISTRAZIONE--------
    public IActionResult Registrazione()
    {
        if (_contextAccessor.HttpContext.Session.GetString("user") != null)
            return RedirectToAction("Index");
        if (gestioneDati.RecuperaRegistrazioneBloccata())
            return RedirectToAction("Index");
        return View();
    }
    
    public static readonly string[] DomandeSicurezza = {
        "Come si chiamava il tuo primo animale?",
        "In quale città sei nato/a?",
        "Qual soprannome ti dava la tua famiglia?",
        "Come si chiamava la tua prima scuola?",
        "Come si chiamava il tuo migliore amico?"
    };

    [HttpPost]
    public IActionResult Registrazione(Utente utente)
    {
        if (gestioneDati.RecuperaRegistrazioneBloccata())
            return RedirectToAction("Index");
        Utente esistente = gestioneDati.RecuperaUtenteConNome(utente.UserName);
        if (esistente != null)
        {
            ViewData["errore"] = "Username già in uso, scegline un altro.";
            return View();
        }

        utente.Ruolo = "user";
        utente.DataCreazione = DateTime.Now;
        utente.RispostaSicurezza = BCrypt.Net.BCrypt.HashPassword(
            (utente.RispostaSicurezza ?? "").ToLower().Trim()
        );
        string esito = gestioneDati.InserisciUtente(utente);

        if (esito.StartsWith("Errore"))
        {
            ViewData["errore"] = esito;
            return View();
        }
        _contextAccessor.HttpContext.Session.SetString("user", utente.UserName);
        _contextAccessor.HttpContext.Session.SetString("ruolo", "user");
        return RedirectToAction("Index");
    }

    public IActionResult RichiestaReset() => View();

    [HttpPost]
    public IActionResult RichiestaReset(string username)
    {
        var utente = gestioneDati.RecuperaUtenteConNome(username);
        if (utente == null || string.IsNullOrEmpty(utente.RispostaSicurezza))
        {
            ViewData["errore"] = "Username non trovato.";
            return View();
        }
        TempData["resetUsername"] = username;
        return RedirectToAction("ResetPassword");
    }

    public IActionResult ResetPassword()
    {
        string username = TempData["resetUsername"] as string;
        if (string.IsNullOrEmpty(username)) return RedirectToAction("RichiestaReset");
        var utente = gestioneDati.RecuperaUtenteConNome(username);
        if (utente == null) return RedirectToAction("RichiestaReset");
        ViewBag.Domanda = utente.DomandaSicurezza;
        ViewBag.Username = username;
        return View();
    }

    [HttpPost]
    public IActionResult ResetPassword(string username, string risposta, string nuovaPassword)
    {
        var utente = gestioneDati.RecuperaUtenteConNome(username);
        if (utente == null) return RedirectToAction("RichiestaReset");

        if (!BCrypt.Net.BCrypt.Verify(risposta.ToLower().Trim(), utente.RispostaSicurezza))
        {
            ViewBag.Domanda = utente.DomandaSicurezza;
            ViewBag.Username = username;
            ViewData["errore"] = "Risposta errata. Riprova.";
            return View();
        }

        gestioneDati.AggiornaPassword(utente.Id, BCrypt.Net.BCrypt.HashPassword(nuovaPassword));
        TempData["successoReset"] = "Password aggiornata! Accedi con le nuove credenziali.";
        return RedirectToAction("Login");
    }

    //------PAGINA LOGOUT---------
    public IActionResult Logout()
    {
        _contextAccessor.HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
    
    //------------PAGINA HOME-----------
    public IActionResult Index()
    {
        List<Articolo> lista = gestioneDati.RecuperaTuttiGliArticoli()
            .Where(a => a.Disponibile)
            .ToList();
        ViewBag.QuantitaCarrello = gestioneCarrello.GetQuantita();
        ViewBag.ArticoliCarrello = gestioneCarrello.GetArticoliDistinti();
        ViewBag.TotaleCarrello = gestioneCarrello.TotaleCarrello();
        ViewBag.IsLoggato = _contextAccessor.HttpContext.Session.GetString("user") != null;
        ViewBag.MediaVoti = lista.ToDictionary(
            a => a.Id,
            a => gestioneDati.MediaVotiArticolo(a.Id)
        );
        var idArticoli = lista.Select(a => a.Id).ToList();
        ViewBag.ScontiAttivi = gestioneDati.RecuperaScontiAttiviPerArticoli(idArticoli);
        ViewBag.Preferiti = IsLoggato()
            ? gestioneDati.RecuperaPreferiti(GetIdUtente())
            : new HashSet<int>();
        Orari orari = gestioneDati.RecuperaOrari();
        ViewBag.IsAperto = orari?.IsAperto ?? true;
        ViewBag.OrarioApertura = orari?.OrarioApertura.ToString(@"hh\:mm") ?? "07:00";
        ViewBag.OrarioChiusura = orari?.OrarioChiusura.ToString(@"hh\:mm") ?? "22:00";
        ViewBag.MessaggioChiusura = orari?.MessaggioChiusura ?? "Siamo chiusi.";
        return View(lista);
    }

    [HttpPost]
    public IActionResult TogglePreferito(int idArticolo)
    {
        if (!IsLoggato()) return Json(new { success = false });

        bool preferito = gestioneDati.TogglePreferito(GetIdUtente(), idArticolo);
        return Json(new { success = true, preferito });
    }

    public IActionResult Preferiti()
    {
        if (!IsLoggato()) return RedirectToAction("Login");

        var lista = gestioneDati.RecuperaArticoliPreferiti(GetIdUtente());
        var idArticoli = lista.Select(a => a.Id).ToList();
        ViewBag.ScontiAttivi = gestioneDati.RecuperaScontiAttiviPerArticoli(idArticoli);
        ViewBag.MediaVoti = lista.ToDictionary(a => a.Id, a => gestioneDati.MediaVotiArticolo(a.Id));
        ViewBag.Preferiti = new HashSet<int>(lista.Select(a => a.Id));
        ViewBag.IsLoggato = true;
        return View(lista);
    }
    
    //------PAGINA ARTICOLI---------
    public IActionResult DettaglioArticolo(int id)
    {
        Articolo a = gestioneDati.RecuperaArticoloConID(id);
        if (a == null)
            return View("NonTrovato");

        List<Associazione> associazioni = gestioneDati.RecuperaAssociazioniByArticolo(id);
        List<Articolo> correlati = associazioni
            .Take(4)
            .Select(ass => gestioneDati.RecuperaArticoloConID(ass.IdArticoloY))
            .Where(art => art != null)
            .ToList();

        ViewBag.Correlati = correlati;
        ViewBag.MediaVoti = gestioneDati.MediaVotiArticolo(id);
        if (IsLoggato())
        {
            ViewBag.HaOrdinato = gestioneDati.HaOrdinatoArticolo(GetIdUtente(), id);
            ViewBag.VotoUtente = gestioneDati.RecuperaVotoUtente(GetIdUtente(), id);
            ViewBag.Preferito = gestioneDati.RecuperaPreferiti(GetIdUtente()).Contains(id);
            ViewBag.IsLoggato = true;
        }
        else
        {
            ViewBag.HaOrdinato = false;
            ViewBag.VotoUtente = null;
            ViewBag.Preferito = false;
            ViewBag.IsLoggato = false;
        }
        ViewBag.ScontoAttivo = gestioneDati.RecuperaScontoAttivoByArticolo(id);
        return View(a);
    }

    //-------CARRELLO-------
    private int GetIdUtente()
    {
        return _contextAccessor.HttpContext.Session.GetInt32("idUtente") ?? 0;
    }

    private bool IsLoggato()
    {
        return _contextAccessor.HttpContext.Session.GetString("user") != null;
    }

    public IActionResult MettiNelCarrello(int id)
    {
        Articolo a = gestioneDati.RecuperaArticoloConID(id);
        if (a == null) return Ok();

        if (IsLoggato())
            gestioneDati.AggiungiAlCarrelloDB(GetIdUtente(), id);
        else
            gestioneCarrello.AggiungiArticolo(a);

        return Ok();
    }

    public IActionResult RimuoviUno(int id)
    {
        if (IsLoggato())
            gestioneDati.RimuoviUnoCarrelloDB(GetIdUtente(), id);
        else
            gestioneCarrello.RimuoviUno(id);

        return Ok();
    }

    public IActionResult SvuotaCarrello()
    {
        if (IsLoggato())
            gestioneDati.SvuotaCarrelloDB(GetIdUtente());
        else
            gestioneCarrello.SvuotaCarrello();

        return Ok();
    }

    public IActionResult CarrelloJson()
{
    List<Articolo> articoliDistinti;
    Dictionary<int, int> quantita;
    int numeroArticoli;
    double totale;
    Dictionary<int, Sconto> scontiCarrello = new Dictionary<int, Sconto>();

    if (IsLoggato())
    {
        int idUtente = GetIdUtente();
        List<Articolo> lista = gestioneDati.RecuperaCarrelloDB(idUtente);
        articoliDistinti = lista.GroupBy(a => a.Id).Select(g => g.First()).ToList();
        quantita = lista.GroupBy(a => a.Id).ToDictionary(g => g.Key, g => g.Count());
        numeroArticoli = gestioneDati.NumeroArticoliCarrelloDB(idUtente);
        scontiCarrello = gestioneDati.RecuperaScontiAttiviPerArticoli(lista.Select(a => a.Id).Distinct().ToList());
        totale = lista.Sum(a => {
            double prezzo = a.PrezzoListino;
            if (scontiCarrello.ContainsKey(a.Id))
                prezzo = prezzo * (1 - scontiCarrello[a.Id].Percentuale / 100.0);
            return prezzo;
        });
    }
    else
    {
        articoliDistinti = gestioneCarrello.GetArticoliDistinti();
        quantita = gestioneCarrello.GetQuantita();
        numeroArticoli = gestioneCarrello.NumeroElementiCarrello();
        scontiCarrello = gestioneDati.RecuperaScontiAttiviPerArticoli(
            articoliDistinti.Select(a => a.Id).Distinct().ToList());
        totale = articoliDistinti.Sum(a => {
            double prezzo = a.PrezzoListino;
            int qty = quantita.ContainsKey(a.Id) ? quantita[a.Id] : 1;
            if (scontiCarrello.ContainsKey(a.Id))
                prezzo = prezzo * (1 - scontiCarrello[a.Id].Percentuale / 100.0);
            return prezzo * qty;
        });
    }

    var idsNelCarrello = articoliDistinti.Select(a => a.Id).ToList();
    Articolo consigliato = null;

    foreach (var id in idsNelCarrello)
    {
        var associazioni = gestioneDati.RecuperaAssociazioniByArticolo(id);
        var migliore = associazioni
            .Where(a => !idsNelCarrello.Contains(a.IdArticoloY))
            .OrderByDescending(a => a.NumOrdini)
            .FirstOrDefault();

        if (migliore != null)
        {
            consigliato = gestioneDati.RecuperaArticoloConID(migliore.IdArticoloY);
            break;
        }
    }

    Orari orari = gestioneDati.RecuperaOrari();
    bool isAperto = orari?.IsAperto ?? true;

    var result = new
    {
        numeroArticoli,
        totale,
        isLoggato = IsLoggato(),
        isAperto = isAperto,
        articoli = articoliDistinti.Select(a => new
        {
            id = a.Id,
            nome = a.Nome,
            prezzoListino = scontiCarrello.ContainsKey(a.Id)
                ? a.PrezzoListino * (1 - scontiCarrello[a.Id].Percentuale / 100.0)
                : a.PrezzoListino,
            quantita = quantita.ContainsKey(a.Id) ? quantita[a.Id] : 1
        }).ToList(),
        consigliato = consigliato == null ? null : new
        {
            id = consigliato.Id,
            nome = consigliato.Nome,
            prezzoListino = consigliato.PrezzoListino,
            fotoUrl = consigliato.FotoUrl
        }
    };

    return Json(result);
}
    
    //------ORDINI-------
    public IActionResult MieiOrdini()
    {
        string ruolo = _contextAccessor.HttpContext.Session.GetString("ruolo");
        if (ruolo == null)
            return RedirectToAction("Login");

        int idUtente = GetIdUtente();
        gestioneDati.AutoCompletaOrdiniScaduti(idUtente);
        List<Ordine> lista = gestioneDati.RecuperaOrdiniConIDUtente(idUtente);
        ViewBag.TotaleSpeso = gestioneDati.TotaleSpesoDaUtente(idUtente);
        ViewBag.Username = _contextAccessor.HttpContext.Session.GetString("user");

        return View(lista);
    }

    public IActionResult ElencoOrdini()
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");

        List<Ordine> lista = gestioneDati.RecuperaTuttiGliOrdini();
        ViewBag.IncassoTotale = gestioneDati.IncassoTotale();
        ViewBag.ArticoloPiuOrdinato = gestioneDati.ArticoloPiuOrdinato();

        return View(lista);
    }

    public IActionResult DettaglioOrdine(int id)
    {
        if (!IsLoggato())
            return RedirectToAction("Login");

        gestioneDati.AutoCompletaOrdiniScaduti(GetIdUtente());
        Ordine o = gestioneDati.RecuperaOrdineConIDOrdine(id);
        if (o == null)
            return View("NonTrovato");

        if (_contextAccessor.HttpContext.Session.GetString("ruolo") == "user")
        {
            int idUtente = GetIdUtente();
            if (o.IdUtente != idUtente)
                return RedirectToAction("MieiOrdini");
        }

        List<RigaDettaglio> righe = gestioneDati.RecuperaRigheConIDOrdine(id);
        List<Articolo> articoli = righe
            .Select(r => gestioneDati.RecuperaArticoloConID(r.IdArticolo))
            .Where(a => a != null)
            .ToList();

        ViewBag.Righe = righe;
        ViewBag.Articoli = articoli;
        ViewBag.Carta = gestioneDati.RecuperaCartaByOrdine(id);
        
        return View(o);
    }
    
[HttpPost]
public IActionResult ConfermaOrdine(string indirizzo = "", string intestatario = "", string ultime4 = "", string scadenza = "", bool skip = false, string note = "")
{
    if (!IsLoggato())
        return Json(new { success = false, message = "Non sei loggato" });

    int idUtente = GetIdUtente();
    List<Articolo> carrello = gestioneDati.RecuperaCarrelloDB(idUtente);

    if (carrello.Count == 0)
        return Json(new { success = false, message = "Carrello vuoto" });

    var idArticoli = carrello.Select(a => a.Id).Distinct().ToList();
    var scontiAttivi = gestioneDati.RecuperaScontiAttiviPerArticoli(idArticoli);

    double totale = carrello.Sum(a => {
        double prezzo = a.PrezzoListino;
        if (scontiAttivi.ContainsKey(a.Id))
            prezzo = prezzo * (1 - scontiAttivi[a.Id].Percentuale / 100.0);
        return prezzo;
    });

    Ordine ordine = new Ordine
    {
        IdUtente = idUtente,
        Data = DateTime.UtcNow,
        NomeCliente = _contextAccessor.HttpContext.Session.GetString("nomeCompleto")
                      ?? _contextAccessor.HttpContext.Session.GetString("user"),
        Indirizzo = indirizzo,
        ImportoTotale = totale,
        Stato = "in attesa",
        DataConferma = DateTime.UtcNow,
        TempoStimato = RandomNumberGenerator.GetInt32(5,31),
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
    };

    int idOrdine = gestioneDati.InserisciOrdineERestituisciId(ordine);
    if (idOrdine <= 0)
        return Json(new { success = false, message = "Errore nella creazione dell'ordine" });

    var gruppi = carrello.GroupBy(a => a.Id).ToList();
    foreach (var gruppo in gruppi)
    {
        double prezzoUnitario = gruppo.First().PrezzoListino;
        if (scontiAttivi.ContainsKey(gruppo.Key))
            prezzoUnitario = prezzoUnitario * (1 - scontiAttivi[gruppo.Key].Percentuale / 100.0);

        gestioneDati.InserisciRigaDettaglioSenzaId(new RigaDettaglio
        {
            IdOrdine = idOrdine,
            IdArticolo = gruppo.Key,
            Quantita = gruppo.Count(),
            Prezzo = prezzoUnitario
        });
        gestioneDati.AggiornaNumOrdini(gruppo.Key, gruppo.Count());
    }

    var ids = gruppi.Select(g => g.Key).ToList();
    for (int i = 0; i < ids.Count; i++)
        for (int j = 0; j < ids.Count; j++)
            if (i != j)
                gestioneDati.AggiornaOInserisciAssociazione(ids[i], ids[j]);

    gestioneDati.SvuotaCarrelloDB(idUtente);

    if (!skip && !string.IsNullOrEmpty(ultime4))
        gestioneDati.InserisciCarta(idOrdine, intestatario, ultime4, scadenza);

    return Json(new { success = true, idOrdine, tempoStimato = ordine.TempoStimato });
}

    [HttpPost]
    public IActionResult CompletaConsegna(int idOrdine)
    {
        if (!IsLoggato())
            return Json(new { success = false });

        int idUtente = GetIdUtente();
        Ordine ordine = gestioneDati.RecuperaOrdineConIDOrdine(idOrdine);
        if (ordine == null || ordine.IdUtente != idUtente || ordine.Stato != "in attesa")
            return Json(new { success = false });

        gestioneDati.AggiornaStatoOrdine(idOrdine, "consegnato");
        return Json(new { success = true });
    }

    //------ADMIN ARTICOLI-------
    public IActionResult ElencoArticoli()
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");

        List<Articolo> lista = gestioneDati.RecuperaTuttiGliArticoli();
        return View(lista);
    }

    [HttpPost]
    public IActionResult AggiungiArticolo(Articolo a, IFormFile? fotoFile)
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");
        a.NumOrdini = 0;
        a.FotoUrl = SalvaImmagine(fotoFile) ?? (string.IsNullOrEmpty(a.FotoUrl) ? "" : a.FotoUrl);
        a.Descrizione = string.IsNullOrEmpty(a.Descrizione) ? "" : a.Descrizione;
        a.Ingredienti = string.IsNullOrEmpty(a.Ingredienti) ? "" : a.Ingredienti;
        a.Allergeni = string.IsNullOrEmpty(a.Allergeni) ? "" : a.Allergeni;
        gestioneDati.InserisciArticolo(a);
        return RedirectToAction("ElencoArticoli");
    }

    [HttpPost]
    public IActionResult ModificaArticolo(Articolo a, IFormFile? fotoFile)
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");
        string? nuovaFoto = SalvaImmagine(fotoFile);
        if (nuovaFoto != null)
            a.FotoUrl = nuovaFoto;
        gestioneDati.ModificaArticolo(a);
        return RedirectToAction("ElencoArticoli");
    }

    private string? SalvaImmagine(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        string ext = Path.GetExtension(file.FileName);
        string nomeFile = Path.GetFileNameWithoutExtension(file.FileName)
            .Replace(" ", "_") + ext;
        string cartella = Path.Combine(_env.WebRootPath, "img");
        Directory.CreateDirectory(cartella);
        string percorso = Path.Combine(cartella, nomeFile);
        using var stream = new FileStream(percorso, FileMode.Create);
        file.CopyTo(stream);
        return "/img/" + nomeFile;
    }

    [HttpPost]
    public IActionResult EliminaArticolo(int id)
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");

        Articolo a = gestioneDati.RecuperaArticoloConID(id);
        if (a != null)
            gestioneDati.EliminaArticolo(a);
        return RedirectToAction("ElencoArticoli");
    }

    [HttpPost]
    public IActionResult AggiornaStato(int idOrdine, string stato)
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return Json(new { success = false });
        
        gestioneDati.AggiornaStatoOrdine(idOrdine, stato);
        return Json(new { success = true });
    }
    
    //------ADMIN ASSOCIAZIONI-------
    public IActionResult ElencoAssociazioni()
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");

        List<Associazione> lista = gestioneDati.RecuperaTutteLeAssociazioni();
    
        // Per ogni associazione recupera i nomi degli articoli
        ViewBag.Articoli = gestioneDati.RecuperaTuttiGliArticoli();
        return View(lista);
    }

    [HttpPost]
    public IActionResult ModificaAssociazione(int idX, int idY, int numOrdini)
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return Json(new { success = false });

        gestioneDati.ModificaNumOrdiniAssociazione(idX, idY, numOrdini);
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult EliminaAssociazione(int idX, int idY)
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");

        gestioneDati.EliminaAssociazione(idX, idY);
        return RedirectToAction("ElencoAssociazioni");
    }
    
    //------ADMIN UTENTI-------
    public IActionResult ElencoUtenti()
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");

        List<Utente> lista = gestioneDati.RecuperaTuttiGliUtenti();
        return View(lista);
    }
    
    [HttpPost]
    public IActionResult ModificaUtente(Utente u)
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");

        u.Nome = string.IsNullOrEmpty(u.Nome) ? "" : u.Nome;
        u.Cognome = string.IsNullOrEmpty(u.Cognome) ? "" : u.Cognome;
        u.Email = string.IsNullOrEmpty(u.Email) ? "" : u.Email;
        gestioneDati.ModificaUtente(u);
        return RedirectToAction("ElencoUtenti");
    }

    [HttpPost]
    public IActionResult EliminaUtente(int id)
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");

        gestioneDati.EliminaUtente(id);
        return RedirectToAction("ElencoUtenti");
    }
    
    [HttpPost]
    public IActionResult AggiornaRuolo(int idUtente, string ruolo)
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return Json(new { success = false });

        gestioneDati.AggiornaRuoloUtente(idUtente, ruolo);
        return Json(new { success = true });
    }
    
    public IActionResult Privacy()
    {
        return View();
    }
    
    public IActionResult Error(int code)
    {
        if (code == 404)
            return View("Error404");
        return View("Error");
    }
    
    [HttpPost]
    public IActionResult AggiornaDisponibilita(int id, bool disponibile)
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return Json(new { success = false });

        gestioneDati.AggiornaDisponibilitaArticolo(id, disponibile);
        return Json(new { success = true });
    }
    
    //------PROFILO UTENTE-------
    public IActionResult Profilo()
    {
        if (!IsLoggato())
            return RedirectToAction("Login");

        int idUtente = GetIdUtente();
        if (idUtente == 0)
            return RedirectToAction("Index");
        Utente u = gestioneDati.RecuperaUtenteConID(idUtente);
        if (u == null)
            return RedirectToAction("Index");
        return View(u);
    }

    [HttpPost]
    public IActionResult Profilo(Utente u)
    {
        if (!IsLoggato())
            return RedirectToAction("Login");

        u.Id = GetIdUtente();
        gestioneDati.AggiornaProfilo(u);

        // Aggiorna la sessione con i nuovi dati
        _contextAccessor.HttpContext.Session.SetString("nomeCompleto",
            $"{u.Nome} {u.Cognome}".Trim());

        // Salva indirizzo e carta in sessione per precompilazione
        _contextAccessor.HttpContext.Session.SetString("indirizzoDefault", u.Indirizzo ?? "");
        _contextAccessor.HttpContext.Session.SetString("cartaIntestatario", u.CartaIntestatario ?? "");
        _contextAccessor.HttpContext.Session.SetString("cartaUltime4", u.CartaUltime4 ?? "");
        _contextAccessor.HttpContext.Session.SetString("cartaScadenza", u.CartaScadenza ?? "");

        ViewBag.Successo = "Profilo aggiornato con successo!";
        return View(u);
    }
    
    public IActionResult GetDatiDefault()
    {
        return Json(new
        {
            indirizzo = _contextAccessor.HttpContext.Session.GetString("indirizzoDefault") ?? "",
            cartaIntestatario = _contextAccessor.HttpContext.Session.GetString("cartaIntestatario") ?? "",
            cartaUltime4 = _contextAccessor.HttpContext.Session.GetString("cartaUltime4") ?? "",
            cartaScadenza = _contextAccessor.HttpContext.Session.GetString("cartaScadenza") ?? ""
        });
    }
    
    //------EXPORT CSV-------
public IActionResult ExportArticoliCsv()
{
    if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
        return RedirectToAction("Index");

    var lista = gestioneDati.RecuperaTuttiGliArticoli();
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("Id,Nome,Categoria,Prezzo,NumOrdini,Disponibile,Descrizione,Ingredienti,Allergeni,TempoPreparazione");
    foreach (var a in lista)
        sb.AppendLine($"{a.Id},\"{a.Nome}\",\"{a.Categoria}\",{a.PrezzoListino.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},{a.NumOrdini},{a.Disponibile},\"{a.Descrizione}\",\"{a.Ingredienti}\",\"{a.Allergeni}\",{a.TempoPreparazione}");

    var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    return File(bytes, "text/csv", "articoli.csv");
}

public IActionResult ExportOrdiniCsv()
{
    if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
        return RedirectToAction("Index");

    var lista = gestioneDati.RecuperaTuttiGliOrdini();
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("Id,Cliente,Indirizzo,Data,Importo,Stato");
    foreach (var o in lista)
        sb.AppendLine($"{o.IdOrdine},\"{o.NomeCliente}\",\"{o.Indirizzo}\",{o.Data:yyyy-MM-dd HH:mm},{o.ImportoTotale.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},\"{o.Stato}\"");

    var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    return File(bytes, "text/csv", "ordini.csv");
}

public IActionResult ExportUtentiCsv()
{
    if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
        return RedirectToAction("Index");

    var lista = gestioneDati.RecuperaTuttiGliUtenti();
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("Id,Username,Nome,Cognome,Email,Telefono,Ruolo,DataCreazione");
    foreach (var u in lista)
        sb.AppendLine($"{u.Id},\"{u.UserName}\",\"{u.Nome}\",\"{u.Cognome}\",\"{u.Email}\",\"{u.Telefono}\",\"{u.Ruolo}\",{u.DataCreazione:yyyy-MM-dd}");

    var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    return File(bytes, "text/csv", "utenti.csv");
}

public IActionResult ExportAssociazioniCsv()
{
    if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
        return RedirectToAction("Index");

    var lista = gestioneDati.RecuperaTutteLeAssociazioni();
    var articoli = gestioneDati.RecuperaTuttiGliArticoli();
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("ArticoloX,ArticoloY,NumOrdini");
    foreach (var a in lista)
    {
        var nomeX = articoli.FirstOrDefault(art => art.Id == a.IdArticoloX)?.Nome ?? a.IdArticoloX.ToString();
        var nomeY = articoli.FirstOrDefault(art => art.Id == a.IdArticoloY)?.Nome ?? a.IdArticoloY.ToString();
        sb.AppendLine($"\"{nomeX}\",\"{nomeY}\",{a.NumOrdini}");
    }

    var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    return File(bytes, "text/csv", "associazioni.csv");
}

public IActionResult Insights()
{
    if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
        return RedirectToAction("Index");

    ViewBag.IncassoPerCategoria = gestioneDati.IncassoPerCategoria();
    ViewBag.OrdiniPerGiorno = gestioneDati.OrdiniPerGiorno();
    ViewBag.OrdiniPerStato = gestioneDati.OrdiniPerStato();
    ViewBag.Top5Articoli = gestioneDati.Top5Articoli();
    ViewBag.IncassoTotale = gestioneDati.IncassoTotale();
    ViewBag.ArticoloPiuOrdinato = gestioneDati.ArticoloPiuOrdinato();

    return View();
}

[HttpPost]
public IActionResult SalvaRecensione(int idArticolo, int voto)
{
    if (!IsLoggato())
        return Json(new { success = false, message = "Non loggato" });

    int idUtente = GetIdUtente();

    if (!gestioneDati.HaOrdinatoArticolo(idUtente, idArticolo))
        return Json(new { success = false, message = "Non hai ordinato questo articolo" });

    if (voto < 1 || voto > 5)
        return Json(new { success = false, message = "Voto non valido" });

    gestioneDati.SalvaRecensione(idUtente, idArticolo, voto);
    double nuovaMedia = gestioneDati.MediaVotiArticolo(idArticolo);
    return Json(new { success = true, nuovaMedia = Math.Round(nuovaMedia, 1) });
}

//------SCONTI-------
public IActionResult ElencoSconti()
{
    if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
        return RedirectToAction("Index");

    ViewBag.Sconti = gestioneDati.RecuperaTuttiGliSconti();
    ViewBag.Articoli = gestioneDati.RecuperaTuttiGliArticoli();
    return View();
}

[HttpPost]
public IActionResult AggiungiSconto(int idArticolo, int percentuale, DateTime dataInizio, DateTime dataFine)
{
    if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
        return RedirectToAction("Index");

    gestioneDati.InserisciSconto(new Sconto()
    {
        IdArticolo = idArticolo,
        Percentuale = percentuale,
        DataInizio = dataInizio,
        DataFine = dataFine
    });
    return RedirectToAction("ElencoSconti");
}

[HttpPost]
public IActionResult EliminaSconto(int id)
{
    if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
        return RedirectToAction("Index");

    gestioneDati.EliminaSconto(id);
    return RedirectToAction("ElencoSconti");
}

//------IMPOSTAZIONI ORARI-------
    public IActionResult Impostazioni()
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");

        Orari orari = gestioneDati.RecuperaOrari();
        return View("Orari", orari);
    }

    [HttpPost]
    public IActionResult Impostazioni(Orari orari)
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");

        gestioneDati.AggiornaOrari(orari);
        ViewBag.Successo = "Orari aggiornati con successo!";
        return View("Orari", orari);
    }

    //------SECURITY-------
    public IActionResult Security()
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");
        ViewBag.RegistrazioneBloccata = gestioneDati.RecuperaRegistrazioneBloccata();
        return View();
    }

    [HttpPost]
    public IActionResult ToggleQuarantena()
    {
        if (_contextAccessor.HttpContext.Session.GetString("ruolo") != "admin")
            return RedirectToAction("Index");
        bool attuale = gestioneDati.RecuperaRegistrazioneBloccata();
        gestioneDati.ImpostaRegistrazioneBloccata(!attuale);
        return RedirectToAction("Security");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            gestioneDati?.Dispose();
        base.Dispose(disposing);
    }

}
