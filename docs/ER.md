# Entity-Relationship Diagram — Deliveroo Clone

```mermaid
erDiagram
    utente {
        INT id PK
        VARCHAR username
        VARCHAR nome
        VARCHAR cognome
        VARCHAR email
        VARCHAR telefono
        VARCHAR indirizzo
        VARCHAR password
        VARCHAR cartaIntestatario
        VARCHAR cartaUltime4
        VARCHAR cartaScadenza
        VARCHAR ruolo
        DATETIME dataCreazione
        VARCHAR domandaSicurezza
        VARCHAR rispostaSicurezza
    }

    articolo {
        INT id PK
        VARCHAR nome
        VARCHAR categoria
        VARCHAR fotoUrl
        DOUBLE prezzoListino
        INT numOrdini
        TEXT descrizione
        TEXT ingredienti
        TEXT allergeni
        INT tempoPreparazione
        BOOL disponibile
    }

    ordine {
        INT idOrdine PK
        INT idUtente FK
        DATETIME data
        DATETIME dataConferma
        DATETIME dataConsegna
        VARCHAR nomeCliente
        VARCHAR indirizzo
        DOUBLE importoTotale
        VARCHAR stato
        INT tempoStimato
        TEXT note
    }

    riga_dettaglio {
        INT id PK
        INT idOrdine FK
        INT idArticolo FK
        INT quantita
        DOUBLE prezzo
    }

    carrello {
        INT id PK
        INT idUtente FK
        INT idArticolo FK
        INT quantita
    }

    preferiti {
        INT id PK
        INT idUtente FK
        INT idArticolo FK
    }

    sconto {
        INT id PK
        INT idArticolo FK
        INT percentuale
        DATETIME dataInizio
        DATETIME dataFine
    }

    associazioni {
        INT idArticoloX FK
        INT idArticoloY FK
        INT numOrdini
    }

    carta {
        INT id PK
        INT idOrdine FK
        VARCHAR intestatario
        VARCHAR ultime4Cifre
        VARCHAR scadenza
        DATETIME dataInserimento
    }

    recensione {
        INT id PK
        INT idUtente FK
        INT idArticolo FK
        INT voto
        DATETIME data
    }

    impostazioni {
        INT id PK
        TIME orarioApertura
        TIME orarioChiusura
        VARCHAR giorniAperti
        VARCHAR messaggioChiusura
    }

    utente ||--o{ ordine : "effettua"
    utente ||--o{ carrello : "ha"
    utente ||--o{ preferiti : "salva"
    utente ||--o{ recensione : "scrive"
    ordine ||--o{ riga_dettaglio : "contiene"
    ordine ||--o| carta : "pagato con"
    articolo ||--o{ riga_dettaglio : "incluso in"
    articolo ||--o{ carrello : "aggiunto a"
    articolo ||--o{ preferiti : "preferito da"
    articolo ||--o{ sconto : "ha"
    articolo ||--o{ recensione : "riceve"
    articolo ||--o{ associazioni : "associato (X)"
    articolo ||--o{ associazioni : "associato (Y)"
```

## Relazioni

| Relazione | Cardinalità | Descrizione |
|---|---|---|
| `utente` → `ordine` | 1:N | Un utente può avere più ordini |
| `utente` → `carrello` | 1:N | Un utente ha un carrello con più righe |
| `utente` → `preferiti` | 1:N | Un utente può salvare più articoli preferiti |
| `utente` → `recensione` | 1:N | Un utente può recensire più prodotti |
| `utente.domandaSicurezza` | — | Domanda di sicurezza per il reset password (hash BCrypt sulla risposta) |
| `ordine` → `riga_dettaglio` | 1:N | Un ordine contiene più righe dettaglio |
| `ordine` → `carta` | 1:0..1 | Un ordine può essere associato a una carta |
| `articolo` → `riga_dettaglio` | 1:N | Un articolo può apparire in più ordini |
| `articolo` → `carrello` | 1:N | Un articolo può essere in più carrelli |
| `articolo` → `preferiti` | 1:N | Un articolo può essere salvato da più utenti |
| `articolo` → `sconto` | 1:N | Un articolo può avere più sconti (non sovrapposti) |
| `articolo` → `recensione` | 1:N | Un articolo può ricevere più recensioni |
| `articolo` ↔ `articolo` | M:N | Associazioni tra prodotti (via tabella `associazioni`) |
