-- ============================================================
-- SCHEMA - Caffè Ritmo / DeliverooApp
-- Run this once on an empty database before loading seed_demo.sql
-- ============================================================

SET FOREIGN_KEY_CHECKS = 0;

CREATE TABLE IF NOT EXISTS utente (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    username            VARCHAR(100) NOT NULL UNIQUE,
    nome                VARCHAR(100) NOT NULL DEFAULT '',
    cognome             VARCHAR(100) NOT NULL DEFAULT '',
    email               VARCHAR(200) NOT NULL DEFAULT '',
    telefono            VARCHAR(30)  NOT NULL DEFAULT '',
    indirizzo           VARCHAR(300) NOT NULL DEFAULT '',
    password            VARCHAR(255) NOT NULL,
    cartaIntestatario   VARCHAR(200) NOT NULL DEFAULT '',
    cartaUltime4        VARCHAR(4)   NOT NULL DEFAULT '',
    cartaScadenza       VARCHAR(7)   NOT NULL DEFAULT '',
    ruolo               VARCHAR(20)  NOT NULL DEFAULT 'user',
    dataCreazione       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    domandaSicurezza    VARCHAR(300) NOT NULL DEFAULT '',
    rispostaSicurezza   VARCHAR(255) NOT NULL DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS articolo (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    nome                VARCHAR(200) NOT NULL,
    categoria           VARCHAR(100) NOT NULL DEFAULT '',
    fotoUrl             VARCHAR(500) NOT NULL DEFAULT '',
    prezzoListino       DOUBLE       NOT NULL DEFAULT 0,
    numOrdini           INT          NOT NULL DEFAULT 0,
    descrizione         TEXT,
    ingredienti         TEXT,
    allergeni           TEXT,
    tempoPreparazione   INT          NOT NULL DEFAULT 0,
    disponibile         TINYINT(1)   NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ordine (
    idOrdine            INT AUTO_INCREMENT PRIMARY KEY,
    idUtente            INT          NOT NULL,
    data                DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    dataConferma        DATETIME,
    dataConsegna        DATETIME,
    nomeCliente         VARCHAR(200) NOT NULL DEFAULT '',
    indirizzo           VARCHAR(300) NOT NULL DEFAULT '',
    importoTotale       DOUBLE       NOT NULL DEFAULT 0,
    stato               VARCHAR(50)  NOT NULL DEFAULT 'in attesa',
    tempoStimato        INT          NOT NULL DEFAULT 0,
    note                TEXT,
    FOREIGN KEY (idUtente) REFERENCES utente(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS riga_dettaglio (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    idOrdine            INT          NOT NULL,
    idArticolo          INT          NOT NULL,
    quantita            INT          NOT NULL DEFAULT 1,
    prezzo              DOUBLE       NOT NULL DEFAULT 0,
    FOREIGN KEY (idOrdine)   REFERENCES ordine(idOrdine),
    FOREIGN KEY (idArticolo) REFERENCES articolo(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS carrello (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    idUtente            INT          NOT NULL,
    idArticolo          INT          NOT NULL,
    quantita            INT          NOT NULL DEFAULT 1,
    FOREIGN KEY (idUtente)   REFERENCES utente(id),
    FOREIGN KEY (idArticolo) REFERENCES articolo(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS preferiti (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    idUtente            INT          NOT NULL,
    idArticolo          INT          NOT NULL,
    UNIQUE KEY uq_preferito (idUtente, idArticolo),
    FOREIGN KEY (idUtente)   REFERENCES utente(id),
    FOREIGN KEY (idArticolo) REFERENCES articolo(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS sconto (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    idArticolo          INT          NOT NULL,
    percentuale         INT          NOT NULL DEFAULT 0,
    dataInizio          DATETIME     NOT NULL,
    dataFine            DATETIME     NOT NULL,
    FOREIGN KEY (idArticolo) REFERENCES articolo(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS associazioni (
    idArticoloX         INT          NOT NULL,
    idArticoloY         INT          NOT NULL,
    numOrdini           INT          NOT NULL DEFAULT 1,
    PRIMARY KEY (idArticoloX, idArticoloY),
    FOREIGN KEY (idArticoloX) REFERENCES articolo(id),
    FOREIGN KEY (idArticoloY) REFERENCES articolo(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS carta (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    idOrdine            INT          NOT NULL,
    intestatario        VARCHAR(200) NOT NULL DEFAULT '',
    ultime4Cifre        VARCHAR(4)   NOT NULL DEFAULT '',
    scadenza            VARCHAR(7)   NOT NULL DEFAULT '',
    dataInserimento     DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (idOrdine) REFERENCES ordine(idOrdine)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS recensione (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    idUtente            INT          NOT NULL,
    idArticolo          INT          NOT NULL,
    voto                INT          NOT NULL DEFAULT 0,
    data                DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (idUtente)   REFERENCES utente(id),
    FOREIGN KEY (idArticolo) REFERENCES articolo(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS impostazioni (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    orarioApertura      TIME         NOT NULL DEFAULT '07:00:00',
    orarioChiusura      TIME         NOT NULL DEFAULT '22:00:00',
    giorniAperti        VARCHAR(200) NOT NULL DEFAULT 'Lunedì,Martedì,Mercoledì,Giovedì,Venerdì,Sabato,Domenica',
    messaggioChiusura   VARCHAR(500) NOT NULL DEFAULT 'Siamo momentaneamente chiusi.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT IGNORE INTO impostazioni (id, orarioApertura, orarioChiusura, giorniAperti, messaggioChiusura)
VALUES (1, '07:00:00', '22:00:00',
        'Lunedì,Martedì,Mercoledì,Giovedì,Venerdì,Sabato,Domenica',
        'Siamo momentaneamente chiusi. Torna presto!');

SET FOREIGN_KEY_CHECKS = 1;
