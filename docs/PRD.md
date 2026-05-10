# PRD — Deliveroo Clone

## 1. Overview

**Prodotto:** Applicazione web di food delivery ispirata a Deliveroo.

**Obiettivo:** Simulare un sistema completo di ordinazione e consegna cibo, con catalogo prodotti, carrello, gestione ordini, sistema di pagamento e pannello amministrativo.

**Target:** Progetto scolastico — V superiore, Informatica.

**Utenti:**
- **Cliente** — Naviga il catalogo, aggiunge prodotti al carrello, effettua ordini, lascia recensioni
- **Amministratore** — Gestisce prodotti, ordini, utenti, sconti, orari e visualizza analytics

---

## 2. Tech Stack

| Componente | Tecnologia |
|---|---|
| **Framework** | ASP.NET Core MVC (.NET 10.0) |
| **Linguaggio** | C# |
| **Database** | MySQL 9.5 |
| **Driver DB** | MySql.Data 9.5.0 |
| **Hashing password** | BCrypt.Net-Next 4.1.0 |
| **Frontend** | HTML, CSS, JavaScript (vanilla) |
| **CSS Framework** | Bootstrap 5 |
| **Librerie JS** | jQuery 3.x, jQuery Validation |
| **Font** | Google Fonts (Syne, DM Sans) |
| **Sessioni** | ASP.NET Core Session (timeout 30 min) |

---

## 3. Architettura

### Pattern: MVC (Model-View-Controller)

```
DeliverooApp/
├── Controllers/
│   └── HomeController.cs          # Controller unico, gestisce tutte le route
├── Models/
│   ├── Domain/                    # Modelli di dominio puri
│   │   ├── Articolo.cs
│   │   ├── Associazione.cs
│   │   ├── Carta.cs
│   │   ├── Orari.cs
│   │   ├── Ordine.cs
│   │   ├── Recensione.cs
│   │   ├── RigaDettaglio.cs
│   │   ├── Sconto.cs
│   │   └── Utente.cs
│   ├── Data/                      # Accesso dati e gestione sessione
│   │   ├── GestioneDati.cs        # Data Access Layer (query MySQL parametrizzate)
│   │   └── GestioneCarrello.cs    # Carrello session-based (utenti anonimi)
│   └── ErrorViewModel.cs
├── Filters/
│   └── OnlyAdminAttribute.cs      # Filtro autorizzazione admin
├── Views/
│   ├── Home/                      # Viste Razor (catalogo, ordini, admin...)
│   └── Shared/                    # Layout principale e admin
├── wwwroot/
│   ├── css/site.css
│   ├── js/site.js
│   ├── img/                       # Immagini prodotti
│   └── lib/                       # Librerie third-party
└── Program.cs                     # Configurazione app
```

### Flusso dati
1. Il **Controller** riceve la richiesta HTTP
2. Chiama i metodi di **GestioneDati** (DAL) per interagire con MySQL
3. Passa i dati ai **Models** (POCO)
4. Ritorna la **View** Razor con i dati nel ViewBag o come modello tipizzato

---

## 4. Schema Database

### 4.1 `utente`
| Campo | Tipo | Note |
|---|---|---|
| id | INT, PK, AUTO_INCREMENT | |
| username | VARCHAR, UNIQUE | |
| nome, cognome | VARCHAR | |
| email | VARCHAR | |
| telefono | VARCHAR | |
| indirizzo | VARCHAR | Indirizzo di consegna default |
| password | VARCHAR | Hash BCrypt (workFactor=12) |
| cartaIntestatario | VARCHAR | Nome intestatario carta default |
| cartaUltime4 | VARCHAR | Ultime 4 cifre carta default |
| cartaScadenza | VARCHAR | Scadenza carta default |
| ruolo | VARCHAR | `user` o `admin` |
| dataCreazione | DATETIME | |
| domandaSicurezza | VARCHAR | Domanda per il reset password |
| rispostaSicurezza | VARCHAR | Hash BCrypt della risposta (lowercase, trimmed) |

### 4.2 `articolo`
| Campo | Tipo | Note |
|---|---|---|
| id | INT, PK, AUTO_INCREMENT | |
| nome | VARCHAR | |
| categoria | VARCHAR | Bevande, Dolci, Salati, Mini-Pasticceria |
| fotoUrl | VARCHAR | URL immagine prodotto |
| prezzoListino | DOUBLE | Prezzo unitario |
| numOrdini | INT | Contatore ordini (popolarita') |
| descrizione | TEXT | |
| ingredienti | TEXT | |
| allergeni | TEXT | |
| tempoPreparazione | INT | Minuti |
| disponibile | BOOL | Toggle disponibilita' |

### 4.3 `ordine`
| Campo | Tipo | Note |
|---|---|---|
| idOrdine | INT, PK, AUTO_INCREMENT | |
| idUtente | INT, FK → utente.id | |
| data | DATETIME | Data creazione ordine |
| dataConferma | DATETIME | Data conferma |
| dataConsegna | DATETIME | Data consegna effettiva |
| nomeCliente | VARCHAR | |
| indirizzo | VARCHAR | Indirizzo di consegna |
| importoTotale | DOUBLE | |
| stato | VARCHAR | `in attesa`, `consegnato`, `eliminato` |
| tempoStimato | INT | Minuti stimati per la consegna |
| note | TEXT | Note libere del cliente (gusto tè, preferenze, ecc.) — nullable |

### 4.4 `riga_dettaglio`
| Campo | Tipo | Note |
|---|---|---|
| id | INT, PK, AUTO_INCREMENT | |
| idOrdine | INT, FK → ordine.idOrdine | |
| idArticolo | INT, FK → articolo.id | |
| quantita | INT | |
| prezzo | DOUBLE | Prezzo unitario al momento dell'acquisto |

### 4.5 `carrello`
| Campo | Tipo | Note |
|---|---|---|
| id | INT, PK, AUTO_INCREMENT | |
| idUtente | INT, FK → utente.id | |
| idArticolo | INT, FK → articolo.id | |
| quantita | INT | |

### 4.6 `sconto`
| Campo | Tipo | Note |
|---|---|---|
| id | INT, PK | |
| idArticolo | INT, FK → articolo.id | |
| percentuale | INT | 0-100 |
| dataInizio | DATETIME | |
| dataFine | DATETIME | |

### 4.7 `associazione`
| Campo | Tipo | Note |
|---|---|---|
| idArticoloX | INT, FK → articolo.id | PK composita |
| idArticoloY | INT, FK → articolo.id | PK composita |
| numOrdini | INT | Volte ordinati insieme |

### 4.8 `carta`
| Campo | Tipo | Note |
|---|---|---|
| id | INT, PK | |
| idOrdine | INT, FK → ordine.idOrdine | |
| intestatario | VARCHAR | |
| ultime4Cifre | VARCHAR | |
| scadenza | VARCHAR | |
| dataInserimento | DATETIME | |

### 4.9 `recensione`
| Campo | Tipo | Note |
|---|---|---|
| id | INT, PK | |
| idUtente | INT, FK → utente.id | |
| idArticolo | INT, FK → articolo.id | |
| voto | INT | 1-5 stelle |
| data | DATETIME | |

### 4.10 `preferiti`
| Campo | Tipo | Note |
|---|---|---|
| id | INT, PK, AUTO_INCREMENT | |
| idUtente | INT, FK → utente.id | ON DELETE CASCADE |
| idArticolo | INT, FK → articolo.id | ON DELETE CASCADE |
| — | UNIQUE(idUtente, idArticolo) | Un utente può salvare ogni articolo una sola volta |

### 4.11 `impostazioni` (orari)
| Campo | Tipo | Note |
|---|---|---|
| id | INT, PK | |
| orarioApertura | TIME | |
| orarioChiusura | TIME | |
| giorniAperti | VARCHAR | Numeri separati da virgola (1-7) |
| messaggioChiusura | VARCHAR | Messaggio mostrato quando chiuso |

### Diagramma relazioni
```
utente (1) ──── (N) ordine
utente (1) ──── (N) carrello
utente (1) ──── (N) preferiti
utente (1) ──── (N) recensione
ordine (1) ──── (N) riga_dettaglio
ordine (1) ──── (1) carta
articolo (1) ──── (N) riga_dettaglio
articolo (1) ──── (N) carrello
articolo (1) ──── (N) preferiti
articolo (1) ──── (N) sconto
articolo (1) ──── (N) recensione
articolo (M) ──── (N) articolo  [via associazioni]
```

---

## 5. Funzionalita' Utente

### 5.1 Autenticazione
- **Registrazione** con validazione username univoco e scelta domanda di sicurezza
- **Login** con verifica BCrypt della password
- **Logout** con pulizia sessione
- **Login admin** separato con credenziali in `appsettings.json` (hash BCrypt)
- **Reset password** tramite domanda di sicurezza (risposta verificata con BCrypt, nuova password re-hashata)

### 5.2 Catalogo prodotti
- Elenco prodotti con immagine, nome, prezzo e categoria
- **Filtro per categoria** (Bevande, Dolci, Salati, Mini-Pasticceria)
- **Ricerca** per nome prodotto
- Visualizzazione sconti attivi con percentuale
- Indicatore di disponibilita'

### 5.3 Dettaglio prodotto
- Immagine, descrizione, ingredienti, allergeni
- Prezzo con eventuale sconto applicato
- Tempo di preparazione
- **Media voti** (stelle 1-5)
- **Prodotti consigliati** basati sulle associazioni (spesso ordinati insieme)
- Possibilita' di aggiungere al carrello

### 5.4 Carrello
- **Doppia persistenza:**
  - Session-based per utenti anonimi (serializzazione JSON)
  - Database per utenti autenticati
- Aggiunta/rimozione articoli con gestione quantita'
- Anteprima carrello via modal
- Calcolo automatico prezzo con sconti applicati
- **Suggerimenti** prodotti correlati (via associazioni)

### 5.5 Ordini
- Conferma ordine dal carrello
- **Note ordine** — campo testuale libero per preferenze (es. "Tè al limone", "Bignè alla crema")
- **Metodi di pagamento:** Carta di credito o contanti alla consegna
- Salvataggio dati carta (ultime 4 cifre, scadenza, intestatario)
- **Tempo di consegna stimato** (5-30 minuti)
- Stati ordine: `in attesa` → `consegnato` (o `eliminato` se annullato)
- **Storico ordini** personale con dettagli e visualizzazione note

### 5.6 Recensioni
- Sistema di valutazione a stelle (1-5)
- Solo utenti che hanno acquistato il prodotto possono recensire
- Una recensione per utente per prodotto (unique constraint)
- Media voti visibile nella pagina dettaglio

### 5.7 Profilo utente
- Visualizzazione e modifica dati personali (nome, cognome, telefono, email, indirizzo)
- Salvataggio dati carta di default
- Pre-compilazione dati al checkout

### 5.8 Preferiti
- Cuoricino su ogni card prodotto (visibile solo da utenti loggati)
- Toggle salva/rimuovi preferito via fetch (nessun reload pagina)
- Pagina `/Home/Preferiti` con elenco articoli salvati
- Stato cuore persistito nel DB (`preferiti` table, chiave unica per coppia utente-articolo)

### 5.9 Dark mode
- Toggle luce/buio nella navbar (icona sole/luna)
- Preferenza salvata in `localStorage` e applicata al ricaricamento
- Copertura completa: card prodotti, modal carrello, layout admin, badge, bottoni

---

## 6. Funzionalita' Admin

### 6.1 Gestione prodotti
- **CRUD completo:** Aggiungi, modifica, elimina prodotti
- Campi: nome, categoria, prezzo, tempo preparazione, foto URL, descrizione, ingredienti, allergeni
- Toggle disponibilita'
- **Export CSV** dell'elenco prodotti

### 6.2 Gestione ordini
- Elenco completo ordini con dati cliente
- **Aggiornamento stato:** `in attesa` → `in preparazione` → `in consegna` → `consegnato`
- Calcolo incasso totale
- Prodotto piu' ordinato
- **Export CSV** degli ordini

### 6.3 Gestione utenti
- Elenco utenti registrati
- Modifica dati utente
- Eliminazione utente
- **Cambio ruolo** (user ↔ admin)
- **Export CSV** degli utenti

### 6.4 Associazioni prodotti
- Creazione/modifica associazioni tra prodotti per il sistema di raccomandazione
- Tracciamento co-ordini (quante volte sono stati ordinati insieme)
- **Export CSV** delle associazioni

### 6.5 Sistema sconti
- Creazione sconti con percentuale e intervallo date
- Calcolo automatico stato attivo (proprieta' `IsAttivo` basata su date)
- Eliminazione sconti scaduti
- Applicazione automatica sconti nel carrello e checkout

### 6.6 Impostazioni orari
- Configurazione orario apertura e chiusura
- Selezione giorni operativi della settimana
- Messaggio personalizzato di chiusura
- Banner dinamico di disponibilita'

### 6.7 Dashboard Analytics (Insights)
- **Incasso totale**
- **Incasso per categoria**
- **Ordini per giorno** (timeline)
- **Distribuzione stati ordini**
- **Top 5 prodotti** piu' ordinati
- **Prodotto piu' ordinato**

### 6.8 Export dati
- Articoli → CSV
- Ordini → CSV
- Utenti → CSV
- Associazioni → CSV

---

## 7. API / Routes

### Autenticazione
| Metodo | Route | Descrizione |
|---|---|---|
| GET | `/Home/Login` | Form di login |
| POST | `/Home/Login` | Verifica credenziali (BCrypt) |
| GET | `/Home/Registrazione` | Form di registrazione |
| POST | `/Home/Registrazione` | Crea nuovo utente |
| GET | `/Home/Logout` | Logout e pulizia sessione |
| GET | `/Home/RichiestaReset` | Form inserimento username per reset |
| POST | `/Home/RichiestaReset` | Verifica username e mostra domanda sicurezza |
| GET | `/Home/ResetPassword` | Form nuova password (con token username in query string) |
| POST | `/Home/ResetPassword` | Verifica risposta sicurezza e aggiorna password |

### Profilo utente
| Metodo | Route | Descrizione |
|---|---|---|
| GET | `/Home/Profilo` | Visualizza profilo |
| POST | `/Home/Profilo` | Aggiorna profilo |
| GET | `/Home/GetDatiDefault` | JSON: dati pre-compilati checkout |

### Catalogo prodotti
| Metodo | Route | Descrizione |
|---|---|---|
| GET | `/Home/Index` | Catalogo con filtri |
| GET | `/Home/DettaglioArticolo/{id}` | Dettaglio prodotto + recensioni |
| POST | `/Home/AggiornaDisponibilita/{id}` | Toggle disponibilita' |

### Carrello
| Metodo | Route | Descrizione |
|---|---|---|
| POST | `/Home/MettiNelCarrello/{id}` | Aggiungi al carrello |
| POST | `/Home/RimuoviUno/{id}` | Rimuovi una unita' |
| POST | `/Home/SvuotaCarrello` | Svuota carrello |
| GET | `/Home/CarrelloJson` | JSON: stato carrello + suggerimenti |

### Ordini
| Metodo | Route | Descrizione |
|---|---|---|
| GET | `/Home/MieiOrdini` | Storico ordini utente |
| GET | `/Home/DettaglioOrdine/{id}` | Dettaglio ordine |
| POST | `/Home/ConfermaOrdine` | Crea ordine (indirizzo, carta, skip) |
| POST | `/Home/CompletaConsegna/{id}` | Segna come consegnato |

### Recensioni
| Metodo | Route | Descrizione |
|---|---|---|
| POST | `/Home/SalvaRecensione` | Salva recensione (idArticolo, voto) |

### Preferiti
| Metodo | Route | Descrizione |
|---|---|---|
| GET | `/Home/Preferiti` | Pagina articoli salvati (richiede login) |
| POST | `/Home/TogglePreferito` | Aggiunge/rimuove preferito, restituisce `{ success, preferito }` |

### Admin — Articoli
| Metodo | Route | Descrizione |
|---|---|---|
| GET | `/Home/ElencoArticoli` | Lista prodotti (admin) |
| POST | `/Home/AggiungiArticolo` | Crea prodotto |
| POST | `/Home/ModificaArticolo` | Modifica prodotto |
| POST | `/Home/EliminaArticolo/{id}` | Elimina prodotto |
| GET | `/Home/ExportArticoliCsv` | Export CSV |

### Admin — Ordini
| Metodo | Route | Descrizione |
|---|---|---|
| GET | `/Home/ElencoOrdini` | Lista ordini (admin) |
| POST | `/Home/AggiornaStato` | Aggiorna stato ordine |
| GET | `/Home/ExportOrdiniCsv` | Export CSV |

### Admin — Utenti
| Metodo | Route | Descrizione |
|---|---|---|
| GET | `/Home/ElencoUtenti` | Lista utenti (admin) |
| POST | `/Home/ModificaUtente` | Modifica utente |
| POST | `/Home/EliminaUtente/{id}` | Elimina utente |
| POST | `/Home/AggiornaRuolo` | Cambia ruolo |
| GET | `/Home/ExportUtentiCsv` | Export CSV |

### Admin — Associazioni
| Metodo | Route | Descrizione |
|---|---|---|
| GET | `/Home/ElencoAssociazioni` | Lista associazioni |
| POST | `/Home/ModificaAssociazione` | Modifica associazione |
| POST | `/Home/EliminaAssociazione` | Elimina associazione |
| GET | `/Home/ExportAssociazioniCsv` | Export CSV |

### Admin — Sconti
| Metodo | Route | Descrizione |
|---|---|---|
| GET | `/Home/ElencoSconti` | Lista sconti |
| POST | `/Home/AggiungiSconto` | Crea sconto |
| POST | `/Home/EliminaSconto/{id}` | Elimina sconto |

### Admin — Impostazioni e Analytics
| Metodo | Route | Descrizione |
|---|---|---|
| GET | `/Home/Impostazioni` | Orari di apertura |
| POST | `/Home/Impostazioni` | Aggiorna orari |
| GET | `/Home/Insights` | Dashboard analytics |

### Errori
| Metodo | Route | Descrizione |
|---|---|---|
| GET | `/Home/Error/{code}` | Pagina errore (404) |
| GET | `/Home/Privacy` | Privacy policy |

---

## 8. Sicurezza

### Misure implementate
| Area | Implementazione |
|---|---|
| **Password** | Hash BCrypt con workFactor=11 (BCrypt.Net-Next) |
| **SQL Injection** | Query parametrizzate su tutti i metodi di GestioneDati |
| **Autorizzazione admin** | Filtro `OnlyAdminAttribute` su tutte le action admin (POST) |
| **Credenziali admin** | Hash BCrypt in `appsettings.json`, non in chiaro |
| **Connection string** | In `appsettings.json` (escluso da Git via `.gitignore`) |
| **Sessioni** | Timeout 30 minuti di inattivita' |
| **ID auto-generati** | AUTO_INCREMENT MySQL (nessun calcolo manuale) |

### Aree di attenzione
- Le credenziali admin sono in `appsettings.json` — in produzione andrebbero in variabili d'ambiente
- CORS non configurato esplicitamente (applicazione server-rendered, non API-based)
- HTTPS non forzato nella configurazione attuale

---

## 9. UI/UX

### Layout
- **Navbar** con logo, link catalogo, carrello (con badge quantita'), login/profilo
- **Layout admin** separato (`_LayoutAdmin.cshtml`) con sidebar di navigazione
- **Design responsive** via Bootstrap 5
- **Font:** Syne (titoli), DM Sans (corpo)

### Componenti principali
- **Modal carrello** — Anteprima rapida senza cambiare pagina
- **Filtri categoria** — Bottoni per filtrare il catalogo
- **Barra di ricerca** — Ricerca prodotti per nome
- **Stelle recensioni** — Visualizzazione media voti
- **Banner orari** — Notifica apertura/chiusura dinamica
- **Chatbot Ritmo** — Assistente AI integrato nel layout, pannello collassabile

### Temi visivi
- **Light mode** (default) e **Dark mode** — toggle in navbar, persistito in `localStorage`
- Font: Syne (titoli/prezzi), DM Sans (corpo)
- Stili custom completi in `site.css` (no Bootstrap per il design visivo)

### Pagine (19 viste)
1. `Index` — Catalogo prodotti con filtro categoria, ricerca, cuoricini preferiti
2. `DettaglioArticolo` — Dettaglio prodotto, recensioni, prodotti correlati
3. `Login` — Form login con link reset password
4. `Registrazione` — Form registrazione con scelta domanda di sicurezza
5. `Profilo` — Gestione profilo utente
6. `MieiOrdini` — Storico ordini con filtro stato
7. `DettaglioOrdine` — Dettaglio ordine, note, tracking stato
8. `Preferiti` — Articoli salvati dall'utente
9. `RichiestaReset` — Inserimento username per reset password
10. `ResetPassword` — Risposta domanda sicurezza + nuova password
11. `ElencoArticoli` — Admin: gestione prodotti
12. `ElencoOrdini` — Admin: gestione ordini con cambio stato inline
13. `ElencoUtenti` — Admin: gestione utenti
14. `ElencoAssociazioni` — Admin: associazioni prodotti
15. `ElencoSconti` — Admin: gestione sconti
16. `Orari` — Admin: impostazioni orari
17. `Insights` — Admin: dashboard analytics
18. `Privacy` — Privacy policy
19. `Error404` — Pagina errore personalizzata

---

## 10. Requisiti non funzionali

| Requisito | Dettaglio |
|---|---|
| **Runtime** | .NET 10.0 |
| **Database** | MySQL su localhost:3306 |
| **Sessione** | In-process, timeout 30 min |
| **Cultura** | en-US (per formattazione numeri/date) |
| **Browser** | Compatibile con browser moderni (Bootstrap 5) |
| **Responsive** | Mobile-first via Bootstrap grid |
| **Immagini** | Servite da `wwwroot/img/` o URL esterni |

---

## 11. Setup e avvio

### Prerequisiti
- .NET 10.0 SDK
- MySQL Server (porta 3306)
- Database `deliveroo` creato e popolato

### Avvio
```bash
cd DeliverooApp
dotnet run
```
