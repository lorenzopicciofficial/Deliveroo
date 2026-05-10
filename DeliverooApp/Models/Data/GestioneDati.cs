using System;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.JavaScript;

namespace DeliverooApp.Models;

public class GestioneDati : IDisposable
{
    private MySqlConnection con;
    private int UltimaAggiunta { get; set; }

     public GestioneDati(string connectionString)
     {
         con = new MySqlConnection(connectionString);
         con.Open();
     }

     public void Dispose()
     {
         con?.Dispose();
     }
     
     //------ORDINI-------
     public List<Ordine> RecuperaTuttiGliOrdini()
     {
         string query="select * from ordine order by idOrdine,idUtente";
         MySqlCommand cmd = new MySqlCommand(query, con); 
         MySqlDataReader reader = cmd.ExecuteReader();
         
         List<Ordine> lista = new List<Ordine>();
         while (reader.Read())
         {
             Ordine a = new Ordine();
             a.IdOrdine = (int)reader["idOrdine"];
             a.IdUtente=(reader["idUtente"] is DBNull) ? 0 : (int)reader["idUtente"];
             a.NomeCliente = (reader["nomeCliente"] is DBNull) ? "" : (string)reader["nomeCliente"];
             a.Indirizzo = (reader["indirizzo"] is DBNull) ? "" : (string)reader["indirizzo"];
             a.ImportoTotale = (reader["importoTotale"] is DBNull) ? 0 : Convert.ToDouble(reader["importoTotale"]);
             a.Stato = (reader["stato"] is DBNull) ? "" : (string)reader["stato"];
             a.Data = (DateTime)reader["data"];
             a.DataConferma = (reader["dataConferma"] is DBNull) ? (DateTime?)null : (DateTime)reader["dataConferma"];
             a.DataConsegna = (reader["dataConsegna"] is DBNull) ? (DateTime?)null : (DateTime)reader["dataConsegna"];
             a.TempoStimato = (reader["tempoStimato"] is DBNull) ? 30 : (int)reader["tempoStimato"];

             lista.Add(a);
         }
         reader.Close();
         return lista;
     }
     
     public Ordine RecuperaOrdineConIDOrdine(int idOrdine)
     {
         Ordine o = null;   
         
         string query = @"select * from ordine where idOrdine=@idOrdine";
         MySqlCommand cmd = new MySqlCommand(query,con);
         cmd.Parameters.AddWithValue("@idOrdine", idOrdine);
         MySqlDataReader reader = cmd.ExecuteReader();

         if (reader.Read())
         {
             o = new Ordine()
             {
                 IdOrdine = (int)reader["idOrdine"],
                 IdUtente=(reader["idUtente"] is DBNull) ? 0 : (int)reader["idUtente"],
                 NomeCliente = (reader["nomeCliente"] is DBNull) ? "" : (string)reader["nomeCliente"],
                 Indirizzo = (reader["indirizzo"] is DBNull) ? "" : (string)reader["indirizzo"],
                 ImportoTotale = (reader["importoTotale"] is DBNull) ? 0 : Convert.ToDouble(reader["importoTotale"]),
                 Stato = (reader["stato"] is DBNull) ? "" : (string)reader["stato"],
                 Data = (DateTime)reader["data"],
                 DataConferma = (reader["dataConferma"] is DBNull) ? (DateTime?)null : (DateTime)reader["dataConferma"],
                 DataConsegna = (reader["dataConsegna"] is DBNull) ? (DateTime?)null : (DateTime)reader["dataConsegna"],
                 TempoStimato = (reader["tempoStimato"] is DBNull) ? 30 : (int)reader["tempoStimato"],
                 Note = (reader["note"] is DBNull) ? null : (string)reader["note"],
             };
         }
         reader.Close();
         return o;
     }
     
     public string InserisciOrdine(Ordine o)
     {
         string query = @"insert into ordine(idUtente, data, nomeCliente, indirizzo, importoTotale, stato, dataConferma, tempoStimato)
                 values (@idUtente, @data, @nomeCliente, @indirizzo, @importoTotale, @stato, @dataConferma, @tempoStimato)";
         MySqlCommand cmd=new MySqlCommand(query,con);
         cmd.Parameters.AddWithValue("@idUtente", o.IdUtente);
         cmd.Parameters.AddWithValue("@data", o.Data);
         cmd.Parameters.AddWithValue("@nomeCliente", o.NomeCliente);
         cmd.Parameters.AddWithValue("@indirizzo", o.Indirizzo);
         cmd.Parameters.AddWithValue("@importoTotale", o.ImportoTotale);
         cmd.Parameters.AddWithValue("@stato", o.Stato);
         cmd.Parameters.AddWithValue("@dataConsegna", o.DataConferma);
         cmd.Parameters.AddWithValue("@tempoStimato", o.TempoStimato);
         string errore = "Inserimento effettuato con successo";
         try
         {
             cmd.ExecuteNonQuery();
         }
         catch (Exception e)
         {
             errore = "Ordine già inserito";
         }
         return errore;
     }
     
     //------ARTICOLO-------
     public List<Articolo> RecuperaTuttiGliArticoli()
{
    string query = "select * from articolo order by id,nome";
    MySqlCommand cmd = new MySqlCommand(query, con);
    MySqlDataReader reader = cmd.ExecuteReader();

    List<Articolo> lista = new List<Articolo>();
    while (reader.Read())
    {
        Articolo a = new Articolo();
        a.Id = (int)reader["id"];
        a.Nome = (reader["nome"] is DBNull) ? "" : (string)reader["nome"];
        a.FotoUrl = (reader["fotoUrl"] is DBNull) ? "" : (string)reader["fotoUrl"];
        a.PrezzoListino = (reader["prezzoListino"] is DBNull) ? 0 : (double)reader["prezzoListino"];
        a.Categoria = (reader["categoria"] is DBNull) ? "" : (string)reader["categoria"];
        a.NumOrdini = (reader["numOrdini"] is DBNull) ? 0 : (int)reader["numOrdini"];
        a.Descrizione = (reader["descrizione"] is DBNull) ? "" : (string)reader["descrizione"];
        a.Ingredienti = (reader["ingredienti"] is DBNull) ? "" : (string)reader["ingredienti"];
        a.TempoPreparazione = (reader["tempoPreparazione"] is DBNull) ? 0 : (int)reader["tempoPreparazione"];
        a.Allergeni = (reader["allergeni"] is DBNull) ? "" : (string)reader["allergeni"];
        a.Disponibile = (reader["disponibile"] is DBNull) ? true : Convert.ToBoolean(reader["disponibile"]);
        lista.Add(a);
    }
    reader.Close();
    return lista;
}

public Articolo RecuperaArticoloConID(int id)
{
    Articolo o = null;

    string query = @"select * from articolo where id=@id";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@id", id);
    MySqlDataReader reader = cmd.ExecuteReader();

    if (reader.Read())
    {
        o = new Articolo()
        {
            Id = (int)reader["id"],
            Nome = (reader["nome"] is DBNull) ? "" : (string)reader["nome"],
            FotoUrl = (reader["fotoUrl"] is DBNull) ? "" : (string)reader["fotoUrl"],
            PrezzoListino = (reader["prezzoListino"] is DBNull) ? 0 : Convert.ToDouble(reader["prezzoListino"]),
            Categoria = (reader["categoria"] is DBNull) ? "" : (string)reader["categoria"],
            NumOrdini = (reader["numOrdini"] is DBNull) ? 0 : (int)reader["numOrdini"],
            Descrizione = (reader["descrizione"] is DBNull) ? "" : (string)reader["descrizione"],
            Ingredienti = (reader["ingredienti"] is DBNull) ? "" : (string)reader["ingredienti"],
            TempoPreparazione = (reader["tempoPreparazione"] is DBNull) ? 0 : (int)reader["tempoPreparazione"],
            Allergeni = (reader["allergeni"] is DBNull) ? "" : (string)reader["allergeni"],
            Disponibile = (reader["disponibile"] is DBNull) ? true : Convert.ToBoolean(reader["disponibile"]),
        };
    }
    reader.Close();
    return o;
}
     
     public string InserisciArticolo(Articolo a)
     {
         string query = @"insert into articolo(nome,fotoUrl,prezzoListino,categoria,numOrdini,descrizione,ingredienti,tempoPreparazione,allergeni)
                          values (@nome,@fotoUrl,@prezzoListino,@categoria,@numOrdini,@descrizione,@ingredienti,@tempoPreparazione,@allergeni)";
         MySqlCommand cmd=new MySqlCommand(query,con);
         cmd.Parameters.AddWithValue("@nome", a.Nome);
         cmd.Parameters.AddWithValue("@fotoUrl", a.FotoUrl);
         cmd.Parameters.AddWithValue("@prezzoListino", a.PrezzoListino);
         cmd.Parameters.AddWithValue("@categoria", a.Categoria);
         cmd.Parameters.AddWithValue("@numOrdini", a.NumOrdini);
         cmd.Parameters.AddWithValue("@descrizione", a.Descrizione);
         cmd.Parameters.AddWithValue("@ingredienti", a.Ingredienti);
         cmd.Parameters.AddWithValue("@tempoPreparazione", a.TempoPreparazione);
         cmd.Parameters.AddWithValue("@allergeni", a.Allergeni);
         
         string errore = "Inserimento effettuato con successo";
         try
         {
             cmd.ExecuteNonQuery();
         }
         catch (Exception e)
         {
             errore = "Articolo già inserito";
         }
         return errore;
     }
     
     public string ModificaArticolo(Articolo a)
     {
         string esito = "Modificato con successo!";
         string query = @"UPDATE articolo SET id=@id,nome=@nome,fotoUrl=@fotoUrl,prezzoListino=@prezzoListino,categoria=@categoria,
                    numOrdini=@numOrdini,descrizione=@descrizione,ingredienti=@ingredienti,tempoPreparazione=@tempoPreparazione,allergeni=@allergeni
                          WHERE id=@id";
         MySqlCommand cmd = new MySqlCommand(query,con);
         cmd.Parameters.AddWithValue("@id", a.Id);
         cmd.Parameters.AddWithValue("@nome", a.Nome);
         cmd.Parameters.AddWithValue("@fotoUrl", a.FotoUrl);
         cmd.Parameters.AddWithValue("@prezzoListino", a.PrezzoListino);
         cmd.Parameters.AddWithValue("@categoria", a.Categoria);
         cmd.Parameters.AddWithValue("@numOrdini", a.NumOrdini);
         cmd.Parameters.AddWithValue("@descrizione", a.Descrizione);
         cmd.Parameters.AddWithValue("@ingredienti", a.Ingredienti);
         cmd.Parameters.AddWithValue("@tempoPreparazione", a.TempoPreparazione);
         cmd.Parameters.AddWithValue("@allergeni", a.Allergeni);
         try
         {
             cmd.ExecuteNonQuery();
         }
         catch (MySqlException ex)
         {
             esito = "Errore di aggiornamento! " + ex.Message;
         }
         return esito;
     }
     
     public string EliminaArticolo(Articolo a)
     {
         string esito = "Eliminato con successo";
         string query = @"DELETE FROM articolo WHERE id=@id";
         MySqlCommand cmd = new MySqlCommand(query,con);
         cmd.Parameters.AddWithValue("@id", a.Id);
         try
         {
             cmd.ExecuteNonQuery();
         }
         catch (MySqlException ex)
         {
             esito = "Errore di aggiornamento! " + ex.Message;
         }
         return esito;
     }
     
    //--------UTENTE--------
    public Utente RecuperaUtenteConNome(string username)
{
    Utente o = null;
    string query = @"select * from utente where username=@username";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@username", username);
    MySqlDataReader reader = cmd.ExecuteReader();
    if (reader.Read())
    {
        o = new Utente()
        {
            Id = (int)reader["id"],
            UserName = (reader["username"] is DBNull) ? "" : (string)reader["username"],
            Nome = (reader["nome"] is DBNull) ? "" : (string)reader["nome"],
            Cognome = (reader["cognome"] is DBNull) ? "" : (string)reader["cognome"],
            Telefono = (reader["telefono"] is DBNull) ? "" : (string)reader["telefono"],
            Indirizzo = (reader["indirizzo"] is DBNull) ? "" : (string)reader["indirizzo"],
            CartaIntestatario = (reader["cartaIntestatario"] is DBNull) ? "" : (string)reader["cartaIntestatario"],
            CartaUltime4 = (reader["cartaUltime4"] is DBNull) ? "" : (string)reader["cartaUltime4"],
            CartaScadenza = (reader["cartaScadenza"] is DBNull) ? "" : (string)reader["cartaScadenza"],
            Password = (reader["password"] is DBNull) ? "" : (string)reader["password"],
            Email = (reader["email"] is DBNull) ? "" : (string)reader["email"],
            Ruolo = (reader["ruolo"] is DBNull) ? "" : (string)reader["ruolo"],
            DataCreazione = (DateTime)reader["dataCreazione"],
            DomandaSicurezza = (reader["domandaSicurezza"] is DBNull) ? "" : (string)reader["domandaSicurezza"],
            RispostaSicurezza = (reader["rispostaSicurezza"] is DBNull) ? "" : (string)reader["rispostaSicurezza"],
        };
    }
    reader.Close();
    return o;
}

public List<Utente> RecuperaTuttiGliUtenti()
{
    string query = "select * from utente order by username";
    MySqlCommand cmd = new MySqlCommand(query, con);
    MySqlDataReader reader = cmd.ExecuteReader();
    List<Utente> lista = new List<Utente>();
    while (reader.Read())
    {
        Utente c = new Utente();
        c.Id = (int)reader["id"];
        c.UserName = (string)reader["username"];
        c.Nome = (reader["nome"] is DBNull) ? "" : (string)reader["nome"];
        c.Cognome = (reader["cognome"] is DBNull) ? "" : (string)reader["cognome"];
        c.Telefono = (reader["telefono"] is DBNull) ? "" : (string)reader["telefono"];
        c.Indirizzo = (reader["indirizzo"] is DBNull) ? "" : (string)reader["indirizzo"];
        c.CartaIntestatario = (reader["cartaIntestatario"] is DBNull) ? "" : (string)reader["cartaIntestatario"];
        c.CartaUltime4 = (reader["cartaUltime4"] is DBNull) ? "" : (string)reader["cartaUltime4"];
        c.CartaScadenza = (reader["cartaScadenza"] is DBNull) ? "" : (string)reader["cartaScadenza"];
        c.Password = (string)reader["password"];
        c.Email = (string)reader["email"];
        c.Ruolo = (string)reader["ruolo"];
        c.DataCreazione = (DateTime)reader["dataCreazione"];
        lista.Add(c);
    }
    reader.Close();
    return lista;
}

public void AggiornaProfilo(Utente u)
{
    string query = @"UPDATE utente SET nome=@nome, cognome=@cognome, telefono=@telefono,
                     email=@email, indirizzo=@indirizzo, cartaIntestatario=@cartaIntestatario,
                     cartaUltime4=@cartaUltime4, cartaScadenza=@cartaScadenza WHERE id=@id";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@id", u.Id);
    cmd.Parameters.AddWithValue("@nome", u.Nome ?? "");
    cmd.Parameters.AddWithValue("@cognome", u.Cognome ?? "");
    cmd.Parameters.AddWithValue("@telefono", u.Telefono ?? "");
    cmd.Parameters.AddWithValue("@email", u.Email ?? "");
    cmd.Parameters.AddWithValue("@indirizzo", u.Indirizzo ?? "");
    cmd.Parameters.AddWithValue("@cartaIntestatario", u.CartaIntestatario ?? "");
    cmd.Parameters.AddWithValue("@cartaUltime4", u.CartaUltime4 ?? "");
    cmd.Parameters.AddWithValue("@cartaScadenza", u.CartaScadenza ?? "");
    cmd.ExecuteNonQuery();
}

public Utente RecuperaUtenteConID(int id)
{
    Utente o = null;
    string query = @"select * from utente where id=@id";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@id", id);
    MySqlDataReader reader = cmd.ExecuteReader();
    if (reader.Read())
    {
        o = new Utente()
        {
            Id = (int)reader["id"],
            UserName = (reader["username"] is DBNull) ? "" : (string)reader["username"],
            Nome = (reader["nome"] is DBNull) ? "" : (string)reader["nome"],
            Cognome = (reader["cognome"] is DBNull) ? "" : (string)reader["cognome"],
            Telefono = (reader["telefono"] is DBNull) ? "" : (string)reader["telefono"],
            Indirizzo = (reader["indirizzo"] is DBNull) ? "" : (string)reader["indirizzo"],
            CartaIntestatario = (reader["cartaIntestatario"] is DBNull) ? "" : (string)reader["cartaIntestatario"],
            CartaUltime4 = (reader["cartaUltime4"] is DBNull) ? "" : (string)reader["cartaUltime4"],
            CartaScadenza = (reader["cartaScadenza"] is DBNull) ? "" : (string)reader["cartaScadenza"],
            Password = (reader["password"] is DBNull) ? "" : (string)reader["password"],
            Email = (reader["email"] is DBNull) ? "" : (string)reader["email"],
            Ruolo = (reader["ruolo"] is DBNull) ? "" : (string)reader["ruolo"],
            DataCreazione = (DateTime)reader["dataCreazione"],
        };
    }
    reader.Close();
    return o;
}
     
    public string InserisciUtente(Utente u)
    {
        string query = @"INSERT INTO utente (username, nome, cognome, password, email, ruolo, dataCreazione, domandaSicurezza, rispostaSicurezza)
                VALUES (@username, @nome, @cognome, @password, @email, @ruolo, @dataCreazione, @domanda, @risposta)";
        MySqlCommand cmd = new MySqlCommand(query, con);
        cmd.Parameters.AddWithValue("@username", u.UserName);
        cmd.Parameters.AddWithValue("@nome", u.Nome ?? "");
        cmd.Parameters.AddWithValue("@cognome", u.Cognome ?? "");
        cmd.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(u.Password));
        cmd.Parameters.AddWithValue("@email", u.Email);
        cmd.Parameters.AddWithValue("@ruolo", u.Ruolo);
        cmd.Parameters.AddWithValue("@dataCreazione", u.DataCreazione);
        cmd.Parameters.AddWithValue("@domanda", u.DomandaSicurezza ?? "");
        cmd.Parameters.AddWithValue("@risposta", u.RispostaSicurezza ?? "");
        string esito = "Registrazione effettuata con successo";
        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (MySqlException ex)
        {
            esito = "Errore: " + ex.Message;
        }
        return esito;
    }

    public void AggiornaPassword(int idUtente, string nuovaPasswordHash)
    {
        string query = "UPDATE utente SET password=@password WHERE id=@id";
        MySqlCommand cmd = new MySqlCommand(query, con);
        cmd.Parameters.AddWithValue("@password", nuovaPasswordHash);
        cmd.Parameters.AddWithValue("@id", idUtente);
        cmd.ExecuteNonQuery();
    }

    public void AggiornaDomandaSicurezza(int idUtente, string domanda, string rispostaHash)
    {
        string query = "UPDATE utente SET domandaSicurezza=@domanda, rispostaSicurezza=@risposta WHERE id=@id";
        MySqlCommand cmd = new MySqlCommand(query, con);
        cmd.Parameters.AddWithValue("@domanda", domanda);
        cmd.Parameters.AddWithValue("@risposta", rispostaHash);
        cmd.Parameters.AddWithValue("@id", idUtente);
        cmd.ExecuteNonQuery();
    }

    public void MigraUtentiSenzaRisposta(string domandaDefault, string rispostaHash)
    {
        string query = "UPDATE utente SET domandaSicurezza=@domanda, rispostaSicurezza=@risposta WHERE rispostaSicurezza IS NULL";
        MySqlCommand cmd = new MySqlCommand(query, con);
        cmd.Parameters.AddWithValue("@domanda", domandaDefault);
        cmd.Parameters.AddWithValue("@risposta", rispostaHash);
        cmd.ExecuteNonQuery();
    }

    // Aggiunge la colonna note alla tabella ordine se non esiste ancora
    public void MigraColonnaNote()
    {
        try
        {
            new MySqlCommand("ALTER TABLE ordine ADD COLUMN note TEXT NULL", con).ExecuteNonQuery();
        }
        catch { /* colonna già presente */ }
    }
    
     //-------RIGA DETTAGLIO--------
     public string InserisciRigaDettaglio(RigaDettaglio r)
     {
         string esito = "Inserito con successo";
         string query = @"INSERT INTO riga_dettaglio (id,idOrdine,idArticolo,quantità,prezzo)  
                        VALUES (@id,@idOrdine,@idArticolo,@quantità,@prezzo)";
         
         MySqlCommand cmd = new MySqlCommand(query,con);
         cmd.Parameters.AddWithValue("@id", r.Id);
         cmd.Parameters.AddWithValue("@idOrdine", r.IdOrdine);
         cmd.Parameters.AddWithValue("@idArticolo", r.IdArticolo);
         cmd.Parameters.AddWithValue("@quantita", r.Quantita);
         cmd.Parameters.AddWithValue("@prezzo", r.Prezzo);
         try
         {
             cmd.ExecuteNonQuery();
             UltimaAggiunta = (int)cmd.LastInsertedId;
         }
         catch (MySqlException ex)
         {
             esito = "Errore di aggiornamento! " + ex.Message;
         }
         return esito;
     }
     
     public List<RigaDettaglio> RecuperaRigheConIDOrdine(int idOrdine)
     {
         string query = @"select * from riga_dettaglio where idOrdine=@idOrdine";
         MySqlCommand cmd = new MySqlCommand(query, con);
         cmd.Parameters.AddWithValue("@idOrdine", idOrdine);
         MySqlDataReader reader = cmd.ExecuteReader();

         List<RigaDettaglio> lista = new List<RigaDettaglio>();
         while (reader.Read())
         {
             RigaDettaglio r = new RigaDettaglio()
             {
                 Id = (int)reader["id"],
                 IdOrdine = (reader["idOrdine"] is DBNull) ? 0 : (int)reader["idOrdine"],
                 IdArticolo = (reader["idArticolo"] is DBNull) ? 0 : (int)reader["idArticolo"],
                 Quantita = (reader["quantita"] is DBNull) ? 0 : (int)reader["quantita"],
                 Prezzo = (reader["prezzo"] is DBNull) ? 0 : Convert.ToDouble(reader["prezzo"]),
             };
             lista.Add(r);
         }
         reader.Close();
         return lista;
     }
     
     //-------ASSOCIAZIONE-------
     public List<Associazione> RecuperaAssociazioniByArticolo(int idArticolo)
     {
         string query = "SELECT * FROM associazioni WHERE idArticoloX = @idArticolo ORDER BY numOrdini DESC";
         MySqlCommand cmd = new MySqlCommand(query, con);
         cmd.Parameters.AddWithValue("@idArticolo", idArticolo);
         MySqlDataReader reader = cmd.ExecuteReader();

         List<Associazione> lista = new List<Associazione>();
         while (reader.Read())
         {
             Associazione a = new Associazione();
             a.IdArticoloX = (int)reader["idArticoloX"];
             a.IdArticoloY = (int)reader["idArticoloY"];
             a.NumOrdini = (int)reader["numOrdini"];

             lista.Add(a);
         }
         reader.Close();
         return lista;
     }
     
     public List<Associazione> RecuperaTutteLeAssociazioni()
     {
         string query = "SELECT * FROM associazioni ORDER BY numOrdini DESC";
         MySqlCommand cmd = new MySqlCommand(query, con);
         MySqlDataReader reader = cmd.ExecuteReader();

         List<Associazione> lista = new List<Associazione>();
         while (reader.Read())
         {
             Associazione a = new Associazione();
             a.IdArticoloX = (int)reader["idArticoloX"];
             a.IdArticoloY = (int)reader["idArticoloY"];
             a.NumOrdini = (int)reader["numOrdini"];

             lista.Add(a);
         }
         reader.Close();
         return lista;
     }
     
     public void AggiornaOInserisciAssociazione(int idX, int idY)
     {
         string query = @"INSERT INTO associazioni (idArticoloX, idArticoloY, numOrdini)
                     VALUES (@idX, @idY, 1)
                     ON DUPLICATE KEY UPDATE numOrdini = numOrdini + 1";

         MySqlCommand cmd = new MySqlCommand(query, con);
         cmd.Parameters.AddWithValue("@idX", idX);
         cmd.Parameters.AddWithValue("@idY", idY);
         cmd.ExecuteNonQuery();
     }
     
     //------ALTRO--------
     public double IncassoTotale()
     {
         string query = "SELECT SUM(importoTotale) FROM ordine";
         MySqlCommand cmd = new MySqlCommand(query, con);
         object result = cmd.ExecuteScalar();
         return (result is DBNull || result == null) ? 0 : Convert.ToDouble(result);
     }

     public Articolo ArticoloPiuOrdinato()
     {
         string query = @"SELECT * FROM articolo ORDER BY numOrdini DESC LIMIT 1";
         MySqlCommand cmd = new MySqlCommand(query, con);
         MySqlDataReader reader = cmd.ExecuteReader();
         Articolo a = null;
         if (reader.Read())
         {
             a = new Articolo()
             {
                 Id = (int)reader["id"],
                 Nome = (reader["nome"] is DBNull) ? "" : (string)reader["nome"],
                 FotoUrl = (reader["fotoUrl"] is DBNull) ? "" : (string)reader["fotoUrl"],
                 PrezzoListino = (reader["prezzoListino"] is DBNull) ? 0 : (double)reader["prezzoListino"],
                 Categoria = (reader["categoria"] is DBNull) ? "" : (string)reader["categoria"],
                 NumOrdini = (reader["numOrdini"] is DBNull) ? 0 : (int)reader["numOrdini"],
             };
         }
         reader.Close();
         return a;
     }
     
     public List<Ordine> RecuperaOrdiniConIDUtente(int idUtente)
     {
         string query = "SELECT * FROM ordine WHERE idUtente = @idUtente ORDER BY data DESC";
         MySqlCommand cmd = new MySqlCommand(query, con);
         cmd.Parameters.AddWithValue("@idUtente", idUtente);
         MySqlDataReader reader = cmd.ExecuteReader();

         List<Ordine> lista = new List<Ordine>();
         while (reader.Read())
         {
             Ordine o = new Ordine()
             {
                 IdOrdine = (int)reader["idOrdine"],
                 IdUtente = (reader["idUtente"] is DBNull) ? 0 : (int)reader["idUtente"],
                 NomeCliente = (reader["nomeCliente"] is DBNull) ? "" : (string)reader["nomeCliente"],
                 Indirizzo = (reader["indirizzo"] is DBNull) ? "" : (string)reader["indirizzo"],
                 ImportoTotale = (reader["importoTotale"] is DBNull) ? 0 : Convert.ToDouble(reader["importoTotale"]),
                 Stato = (reader["stato"] is DBNull) ? "" : (string)reader["stato"],
                 Data = (DateTime)reader["data"],
                 DataConferma = (reader["dataConferma"] is DBNull) ? (DateTime?)null : (DateTime)reader["dataConferma"],
                 DataConsegna = (reader["dataConsegna"] is DBNull) ? (DateTime?)null : (DateTime)reader["dataConsegna"],
                 TempoStimato = (reader["tempoStimato"] is DBNull) ? 30 : (int)reader["tempoStimato"],
                 Note = (reader["note"] is DBNull) ? null : (string)reader["note"],
             };
             lista.Add(o);
         }
         reader.Close();
         return lista;
     }

     public double TotaleSpesoDaUtente(int idUtente)
     {
         string query = "SELECT SUM(importoTotale) FROM ordine WHERE idUtente = @idUtente";
         MySqlCommand cmd = new MySqlCommand(query, con);
         cmd.Parameters.AddWithValue("@idUtente", idUtente);
         object result = cmd.ExecuteScalar();
         return (result is DBNull || result == null) ? 0 : Convert.ToDouble(result);
     }
     
     //-------CARRELLO DB-------
public List<Articolo> RecuperaCarrelloDB(int idUtente)
{
    string query = @"SELECT a.*, c.quantita FROM articolo a
                     JOIN carrello c ON a.id = c.idArticolo
                     WHERE c.idUtente = @idUtente";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idUtente", idUtente);
    MySqlDataReader reader = cmd.ExecuteReader();

    List<Articolo> lista = new List<Articolo>();
    while (reader.Read())
    {
        int quantita = (int)reader["quantita"];
        for (int i = 0; i < quantita; i++)
        {
            lista.Add(new Articolo()
            {
                Id = (int)reader["id"],
                Nome = (reader["nome"] is DBNull) ? "" : (string)reader["nome"],
                FotoUrl = (reader["fotoUrl"] is DBNull) ? "" : (string)reader["fotoUrl"],
                PrezzoListino = (reader["prezzoListino"] is DBNull) ? 0 : Convert.ToDouble(reader["prezzoListino"]),
                Categoria = (reader["categoria"] is DBNull) ? "" : (string)reader["categoria"],
                NumOrdini = (reader["numOrdini"] is DBNull) ? 0 : (int)reader["numOrdini"],
            });
        }
    }
    reader.Close();
    return lista;
}

public void AggiungiAlCarrelloDB(int idUtente, int idArticolo)
{
    // Se esiste già incrementa la quantità, altrimenti inserisce
    string query = @"INSERT INTO carrello (idUtente, idArticolo, quantita)
                     VALUES (@idUtente, @idArticolo, 1)
                     ON DUPLICATE KEY UPDATE quantita = quantita + 1";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idUtente", idUtente);
    cmd.Parameters.AddWithValue("@idArticolo", idArticolo);
    cmd.ExecuteNonQuery();
}

public void RimuoviUnoCarrelloDB(int idUtente, int idArticolo)
{
    // Decrementa di 1, se arriva a 0 elimina la riga
    string query = @"UPDATE carrello SET quantita = quantita - 1
                     WHERE idUtente = @idUtente AND idArticolo = @idArticolo";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idUtente", idUtente);
    cmd.Parameters.AddWithValue("@idArticolo", idArticolo);
    cmd.ExecuteNonQuery();

    // Rimuovi righe con quantità 0
    string queryClean = @"DELETE FROM carrello 
                          WHERE idUtente = @idUtente AND quantita <= 0";
    MySqlCommand cmdClean = new MySqlCommand(queryClean, con);
    cmdClean.Parameters.AddWithValue("@idUtente", idUtente);
    cmdClean.ExecuteNonQuery();
}

public void SvuotaCarrelloDB(int idUtente)
{
    string query = "DELETE FROM carrello WHERE idUtente = @idUtente";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idUtente", idUtente);
    cmd.ExecuteNonQuery();
}

public int NumeroArticoliCarrelloDB(int idUtente)
{
    string query = "SELECT SUM(quantita) FROM carrello WHERE idUtente = @idUtente";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idUtente", idUtente);
    object result = cmd.ExecuteScalar();
    return (result is DBNull || result == null) ? 0 : Convert.ToInt32(result);
}

public bool TogglePreferito(int idUtente, int idArticolo)
{
    string queryCheck = @"SELECT EXISTS(
                            SELECT 1 FROM preferiti
                            WHERE idUtente = @idUtente AND idArticolo = @idArticolo
                         )";
    MySqlCommand cmdCheck = new MySqlCommand(queryCheck, con);
    cmdCheck.Parameters.AddWithValue("@idUtente", idUtente);
    cmdCheck.Parameters.AddWithValue("@idArticolo", idArticolo);
    bool esiste = Convert.ToBoolean(cmdCheck.ExecuteScalar());

    if (esiste)
    {
        string queryDelete = "DELETE FROM preferiti WHERE idUtente = @idUtente AND idArticolo = @idArticolo";
        MySqlCommand cmdDelete = new MySqlCommand(queryDelete, con);
        cmdDelete.Parameters.AddWithValue("@idUtente", idUtente);
        cmdDelete.Parameters.AddWithValue("@idArticolo", idArticolo);
        cmdDelete.ExecuteNonQuery();
        return false;
    }

    string queryInsert = "INSERT INTO preferiti (idUtente, idArticolo) VALUES (@idUtente, @idArticolo)";
    MySqlCommand cmdInsert = new MySqlCommand(queryInsert, con);
    cmdInsert.Parameters.AddWithValue("@idUtente", idUtente);
    cmdInsert.Parameters.AddWithValue("@idArticolo", idArticolo);
    cmdInsert.ExecuteNonQuery();
    return true;
}

public HashSet<int> RecuperaPreferiti(int idUtente)
{
    string query = "SELECT idArticolo FROM preferiti WHERE idUtente = @idUtente";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idUtente", idUtente);
    MySqlDataReader reader = cmd.ExecuteReader();

    HashSet<int> preferiti = new HashSet<int>();
    while (reader.Read())
    {
        preferiti.Add((int)reader["idArticolo"]);
    }
    reader.Close();
    return preferiti;
}

public List<Articolo> RecuperaArticoliPreferiti(int idUtente)
{
    string query = @"SELECT a.* FROM articolo a
                     JOIN preferiti p ON a.id = p.idArticolo
                     WHERE p.idUtente = @idUtente
                     ORDER BY a.nome";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idUtente", idUtente);
    MySqlDataReader reader = cmd.ExecuteReader();

    List<Articolo> lista = new List<Articolo>();
    while (reader.Read())
    {
        lista.Add(new Articolo()
        {
            Id = (int)reader["id"],
            Nome = (reader["nome"] is DBNull) ? "" : (string)reader["nome"],
            FotoUrl = (reader["fotoUrl"] is DBNull) ? "" : (string)reader["fotoUrl"],
            PrezzoListino = (reader["prezzoListino"] is DBNull) ? 0 : Convert.ToDouble(reader["prezzoListino"]),
            Categoria = (reader["categoria"] is DBNull) ? "" : (string)reader["categoria"],
            NumOrdini = (reader["numOrdini"] is DBNull) ? 0 : (int)reader["numOrdini"],
            Descrizione = (reader["descrizione"] is DBNull) ? "" : (string)reader["descrizione"],
            Ingredienti = (reader["ingredienti"] is DBNull) ? "" : (string)reader["ingredienti"],
            TempoPreparazione = (reader["tempoPreparazione"] is DBNull) ? 0 : (int)reader["tempoPreparazione"],
            Allergeni = (reader["allergeni"] is DBNull) ? "" : (string)reader["allergeni"],
            Disponibile = (reader["disponibile"] is DBNull) ? true : Convert.ToBoolean(reader["disponibile"]),
        });
    }
    reader.Close();
    return lista;
}

public int InserisciOrdineERestituisciId(Ordine o)
{
    string query = @"INSERT INTO ordine(idUtente, data, nomeCliente, indirizzo, importoTotale, stato, dataConferma, tempoStimato, note)
            VALUES (@idUtente, @data, @nomeCliente, @indirizzo, @importoTotale, @stato, @dataConferma, @tempoStimato, @note)";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idUtente", o.IdUtente);
    cmd.Parameters.AddWithValue("@data", o.Data);
    cmd.Parameters.AddWithValue("@nomeCliente", o.NomeCliente);
    cmd.Parameters.AddWithValue("@indirizzo", o.Indirizzo);
    cmd.Parameters.AddWithValue("@importoTotale", o.ImportoTotale);
    cmd.Parameters.AddWithValue("@stato", o.Stato);
    cmd.Parameters.AddWithValue("@dataConferma", (object)o.DataConferma ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@tempoStimato", o.TempoStimato);
    cmd.Parameters.AddWithValue("@note", (object)o.Note ?? DBNull.Value);
    try { cmd.ExecuteNonQuery(); return (int)cmd.LastInsertedId; }
    catch (Exception) { return -1; }
}

public void InserisciRigaDettaglioSenzaId(RigaDettaglio r)
{
    string query = @"INSERT INTO riga_dettaglio (idOrdine, idArticolo, quantita, prezzo)
                     VALUES (@idOrdine, @idArticolo, @quantita, @prezzo)";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idOrdine", r.IdOrdine);
    cmd.Parameters.AddWithValue("@idArticolo", r.IdArticolo);
    cmd.Parameters.AddWithValue("@quantita", r.Quantita);
    cmd.Parameters.AddWithValue("@prezzo", r.Prezzo);
    cmd.ExecuteNonQuery();
}

public void AggiornaNumOrdini(int idArticolo, int quantita)
{
    string query = "UPDATE articolo SET numOrdini = numOrdini + @q WHERE id = @id";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@q", quantita);
    cmd.Parameters.AddWithValue("@id", idArticolo);
    cmd.ExecuteNonQuery();
}

public void AggiornaStatoOrdine(int idOrdine, string stato)
{
    string query = stato == "consegnato"
        ? "UPDATE ordine SET stato = @stato, dataConsegna = NOW() WHERE idOrdine = @idOrdine"
        : "UPDATE ordine SET stato = @stato WHERE idOrdine = @idOrdine";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@stato", stato);
    cmd.Parameters.AddWithValue("@idOrdine", idOrdine);
    cmd.ExecuteNonQuery();
}

public void AutoCompletaOrdiniScaduti(int idUtente)
{
    string query = @"UPDATE ordine
                     SET stato = 'consegnato',
                         dataConsegna = DATE_ADD(dataConferma, INTERVAL tempoStimato MINUTE)
                     WHERE idUtente = @idUtente
                       AND stato = 'in attesa'
                       AND dataConferma IS NOT NULL
                       AND DATE_ADD(dataConferma, INTERVAL tempoStimato MINUTE) < NOW()";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idUtente", idUtente);
    cmd.ExecuteNonQuery();
}

public void ModificaNumOrdiniAssociazione(int idX, int idY, int numOrdini)
{
    string query = "UPDATE associazioni SET numOrdini = @numOrdini WHERE idArticoloX = @idX AND idArticoloY = @idY";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@numOrdini", numOrdini);
    cmd.Parameters.AddWithValue("@idX", idX);
    cmd.Parameters.AddWithValue("@idY", idY);
    cmd.ExecuteNonQuery();
}

public void EliminaAssociazione(int idX, int idY)
{
    string query = "DELETE FROM associazioni WHERE idArticoloX = @idX AND idArticoloY = @idY";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idX", idX);
    cmd.Parameters.AddWithValue("@idY", idY);
    cmd.ExecuteNonQuery();
}

public void ModificaUtente(Utente u)
{
    bool cambiaPassword = !string.IsNullOrEmpty(u.Password);
    string query = cambiaPassword
        ? @"UPDATE utente SET nome=@nome, cognome=@cognome, email=@email,
                     password=@password, ruolo=@ruolo WHERE id=@id"
        : @"UPDATE utente SET nome=@nome, cognome=@cognome, email=@email,
                     ruolo=@ruolo WHERE id=@id";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@id", u.Id);
    cmd.Parameters.AddWithValue("@nome", u.Nome);
    cmd.Parameters.AddWithValue("@cognome", u.Cognome);
    cmd.Parameters.AddWithValue("@email", u.Email);
    if (cambiaPassword)
        cmd.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(u.Password));
    cmd.Parameters.AddWithValue("@ruolo", u.Ruolo);
    cmd.ExecuteNonQuery();
}

public void EliminaUtente(int id)
{
    string query = "DELETE FROM utente WHERE id=@id";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@id", id);
    cmd.ExecuteNonQuery();
}

public void AggiornaRuoloUtente(int idUtente, string ruolo)
{
    string query = "UPDATE utente SET ruolo = @ruolo WHERE id = @idUtente";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@ruolo", ruolo);
    cmd.Parameters.AddWithValue("@idUtente", idUtente);
    cmd.ExecuteNonQuery();
}

public void InserisciCarta(int idOrdine, string intestatario, string ultime4Cifre, string scadenza)
{
    string query = @"INSERT INTO carta (idOrdine, intestatario, ultime4Cifre, scadenza, dataInserimento)
                     VALUES (@idOrdine, @intestatario, @ultime4Cifre, @scadenza, @dataInserimento)";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idOrdine", idOrdine);
    cmd.Parameters.AddWithValue("@intestatario", intestatario);
    cmd.Parameters.AddWithValue("@ultime4Cifre", ultime4Cifre);
    cmd.Parameters.AddWithValue("@scadenza", scadenza);
    cmd.Parameters.AddWithValue("@dataInserimento", DateTime.Now);
    cmd.ExecuteNonQuery();
}

public Carta RecuperaCartaByOrdine(int idOrdine)
{
    string query = "SELECT * FROM carta WHERE idOrdine = @idOrdine";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idOrdine", idOrdine);
    MySqlDataReader reader = cmd.ExecuteReader();

    Carta c = null;
    if (reader.Read())
    {
        c = new Carta()
        {
            Id = (int)reader["id"],
            IdOrdine = (int)reader["idOrdine"],
            Intestatario = (reader["intestatario"] is DBNull) ? "" : (string)reader["intestatario"],
            Ultime4Cifre = (reader["ultime4Cifre"] is DBNull) ? "" : (string)reader["ultime4Cifre"],
            Scadenza = (reader["scadenza"] is DBNull) ? "" : (string)reader["scadenza"],
            DataInserimento = (DateTime)reader["dataInserimento"]
        };
    }
    reader.Close();
    return c;
}

public void AggiornaDisponibilitaArticolo(int id, bool disponibile)
{
    string query = "UPDATE articolo SET disponibile = @disponibile WHERE id = @id";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@disponibile", disponibile ? 1 : 0);
    cmd.Parameters.AddWithValue("@id", id);
    cmd.ExecuteNonQuery();
}

public Dictionary<string, double> IncassoPerCategoria()
{
    string query = @"SELECT a.categoria, SUM(r.prezzo * r.quantita) as totale
                     FROM riga_dettaglio r
                     JOIN articolo a ON r.idArticolo = a.id
                     GROUP BY a.categoria";
    MySqlCommand cmd = new MySqlCommand(query, con);
    MySqlDataReader reader = cmd.ExecuteReader();
    var result = new Dictionary<string, double>();
    while (reader.Read())
        result[(string)reader["categoria"]] = Convert.ToDouble(reader["totale"]);
    reader.Close();
    return result;
}

public Dictionary<string, int> OrdiniPerGiorno()
{
    string query = @"SELECT DATE(data) as giorno, COUNT(*) as totale
                     FROM ordine
                     GROUP BY DATE(data)
                     ORDER BY giorno DESC
                     LIMIT 30";
    MySqlCommand cmd = new MySqlCommand(query, con);
    MySqlDataReader reader = cmd.ExecuteReader();
    var result = new Dictionary<string, int>();
    while (reader.Read())
        result[((DateTime)reader["giorno"]).ToString("dd/MM")] = Convert.ToInt32(reader["totale"]);
    reader.Close();
    return result;
}

public Dictionary<string, int> OrdiniPerStato()
{
    string query = "SELECT stato, COUNT(*) as totale FROM ordine GROUP BY stato";
    MySqlCommand cmd = new MySqlCommand(query, con);
    MySqlDataReader reader = cmd.ExecuteReader();
    var result = new Dictionary<string, int>();
    while (reader.Read())
        result[(string)reader["stato"]] = Convert.ToInt32(reader["totale"]);
    reader.Close();
    return result;
}

public List<Articolo> Top5Articoli()
{
    string query = "SELECT * FROM articolo ORDER BY numOrdini DESC LIMIT 5";
    MySqlCommand cmd = new MySqlCommand(query, con);
    MySqlDataReader reader = cmd.ExecuteReader();
    var lista = new List<Articolo>();
    while (reader.Read())
    {
        lista.Add(new Articolo()
        {
            Id = (int)reader["id"],
            Nome = (reader["nome"] is DBNull) ? "" : (string)reader["nome"],
            NumOrdini = (reader["numOrdini"] is DBNull) ? 0 : (int)reader["numOrdini"],
        });
    }
    reader.Close();
    return lista;
}

public double MediaVotiArticolo(int idArticolo)
{
    string query = "SELECT AVG(voto) FROM recensione WHERE idArticolo = @idArticolo";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idArticolo", idArticolo);
    object result = cmd.ExecuteScalar();
    return result is DBNull || result == null ? 0 : Convert.ToDouble(result);
}

public int? RecuperaVotoUtente(int idUtente, int idArticolo)
{
    string query = "SELECT voto FROM recensione WHERE idUtente = @idUtente AND idArticolo = @idArticolo";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idUtente", idUtente);
    cmd.Parameters.AddWithValue("@idArticolo", idArticolo);
    object result = cmd.ExecuteScalar();
    return result is DBNull || result == null ? null : (int?)Convert.ToInt32(result);
}

public bool HaOrdinatoArticolo(int idUtente, int idArticolo)
{
    string query = @"SELECT COUNT(*) FROM riga_dettaglio r
                     JOIN ordine o ON r.idOrdine = o.idOrdine
                     WHERE o.idUtente = @idUtente AND r.idArticolo = @idArticolo";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idUtente", idUtente);
    cmd.Parameters.AddWithValue("@idArticolo", idArticolo);
    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
}

public void SalvaRecensione(int idUtente, int idArticolo, int voto)
{
    string query = @"INSERT INTO recensione (idUtente, idArticolo, voto, data)
                     VALUES (@idUtente, @idArticolo, @voto, @data)
                     ON DUPLICATE KEY UPDATE voto = @voto, data = @data";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idUtente", idUtente);
    cmd.Parameters.AddWithValue("@idArticolo", idArticolo);
    cmd.Parameters.AddWithValue("@voto", voto);
    cmd.Parameters.AddWithValue("@data", DateTime.Now);
    cmd.ExecuteNonQuery();
}

public List<Sconto> RecuperaTuttiGliSconti()
{
    string query = @"SELECT s.*, a.nome as nomeArticolo FROM sconto s
                     JOIN articolo a ON s.idArticolo = a.id
                     ORDER BY s.dataInizio DESC";
    MySqlCommand cmd = new MySqlCommand(query, con);
    MySqlDataReader reader = cmd.ExecuteReader();
    List<Sconto> lista = new List<Sconto>();
    while (reader.Read())
    {
        lista.Add(new Sconto()
        {
            Id = (int)reader["id"],
            IdArticolo = (int)reader["idArticolo"],
            Percentuale = (int)reader["percentuale"],
            DataInizio = (DateTime)reader["dataInizio"],
            DataFine = (DateTime)reader["dataFine"]
        });
    }
    reader.Close();
    return lista;
}

public Sconto RecuperaScontoAttivoByArticolo(int idArticolo)
{
    string query = @"SELECT * FROM sconto
                     WHERE idArticolo = @idArticolo
                     AND dataInizio <= NOW()
                     AND dataFine >= NOW()
                     ORDER BY dataFine DESC
                     LIMIT 1";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idArticolo", idArticolo);
    MySqlDataReader reader = cmd.ExecuteReader();
    Sconto s = null;
    if (reader.Read())
    {
        s = new Sconto()
        {
            Id = (int)reader["id"],
            IdArticolo = (int)reader["idArticolo"],
            Percentuale = (int)reader["percentuale"],
            DataInizio = (DateTime)reader["dataInizio"],
            DataFine = (DateTime)reader["dataFine"]
        };
    }
    reader.Close();
    return s;
}

public Dictionary<int, Sconto> RecuperaScontiAttiviPerArticoli(List<int> idArticoli)
{
    if (idArticoli == null || idArticoli.Count == 0)
        return new Dictionary<int, Sconto>();

    var paramNames = idArticoli.Select((_, i) => $"@id{i}").ToList();
    string query = $@"SELECT s1.* FROM sconto s1
                      INNER JOIN (
                          SELECT idArticolo, MAX(dataFine) as maxFine
                          FROM sconto
                          WHERE dataInizio <= NOW() AND dataFine >= NOW()
                          AND idArticolo IN ({string.Join(",", paramNames)})
                          GROUP BY idArticolo
                      ) s2 ON s1.idArticolo = s2.idArticolo AND s1.dataFine = s2.maxFine";
    MySqlCommand cmd = new MySqlCommand(query, con);
    for (int i = 0; i < idArticoli.Count; i++)
        cmd.Parameters.AddWithValue(paramNames[i], idArticoli[i]);
    MySqlDataReader reader = cmd.ExecuteReader();
    var result = new Dictionary<int, Sconto>();
    while (reader.Read())
    {
        int idArt = (int)reader["idArticolo"];
        result[idArt] = new Sconto()
        {
            Id = (int)reader["id"],
            IdArticolo = idArt,
            Percentuale = (int)reader["percentuale"],
            DataInizio = (DateTime)reader["dataInizio"],
            DataFine = (DateTime)reader["dataFine"]
        };
    }
    reader.Close();
    return result;
}

public void InserisciSconto(Sconto s)
{
    string query = @"INSERT INTO sconto (idArticolo, percentuale, dataInizio, dataFine)
                     VALUES (@idArticolo, @percentuale, @dataInizio, @dataFine)";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@idArticolo", s.IdArticolo);
    cmd.Parameters.AddWithValue("@percentuale", s.Percentuale);
    cmd.Parameters.AddWithValue("@dataInizio", s.DataInizio);
    cmd.Parameters.AddWithValue("@dataFine", s.DataFine);
    cmd.ExecuteNonQuery();
}

public void EliminaSconto(int id)
{
    string query = "DELETE FROM sconto WHERE id = @id";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@id", id);
    cmd.ExecuteNonQuery();
}

public void MigraColonnaRegistrazioneBloccata()
{
    string query = @"ALTER TABLE impostazioni
                     ADD COLUMN IF NOT EXISTS registrazioneBloccata TINYINT(1) NOT NULL DEFAULT 0";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.ExecuteNonQuery();
}

public bool RecuperaRegistrazioneBloccata()
{
    string query = "SELECT registrazioneBloccata FROM impostazioni LIMIT 1";
    MySqlCommand cmd = new MySqlCommand(query, con);
    object result = cmd.ExecuteScalar();
    return result != null && Convert.ToBoolean(result);
}

public void ImpostaRegistrazioneBloccata(bool bloccata)
{
    string query = "UPDATE impostazioni SET registrazioneBloccata = @v WHERE id = 1";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@v", bloccata ? 1 : 0);
    cmd.ExecuteNonQuery();
}

public Orari RecuperaOrari()
{
    string query = "SELECT * FROM impostazioni LIMIT 1";
    MySqlCommand cmd = new MySqlCommand(query, con);
    MySqlDataReader reader = cmd.ExecuteReader();
    Orari o = null;
    if (reader.Read())
    {
        o = new Orari()
        {
            Id = (int)reader["id"],
            OrarioApertura = (TimeSpan)reader["orarioApertura"],
            OrarioChiusura = (TimeSpan)reader["orarioChiusura"],
            GiorniAperti = (string)reader["giorniAperti"],
            MessaggioChiusura = (string)reader["messaggioChiusura"],
            RegistrazioneBloccata = Convert.ToBoolean(reader["registrazioneBloccata"])
        };
    }
    reader.Close();
    return o;
}

public void AggiornaOrari(Orari o)
{
    string query = @"UPDATE impostazioni SET 
                     orarioApertura = @orarioApertura,
                     orarioChiusura = @orarioChiusura,
                     giorniAperti = @giorniAperti,
                     messaggioChiusura = @messaggioChiusura
                     WHERE id = @id";
    MySqlCommand cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@orarioApertura", o.OrarioApertura);
    cmd.Parameters.AddWithValue("@orarioChiusura", o.OrarioChiusura);
    cmd.Parameters.AddWithValue("@giorniAperti", o.GiorniAperti);
    cmd.Parameters.AddWithValue("@messaggioChiusura", o.MessaggioChiusura);
    cmd.Parameters.AddWithValue("@id", o.Id);
    cmd.ExecuteNonQuery();
}

}
