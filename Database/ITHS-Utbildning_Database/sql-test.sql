CREATE DATABASE demoDag320221118;

-- Bok och kategori (version 2)
CREATE TABLE kategori
(
    kategoriId   INT NOT NULL AUTO_INCREMENT,
    kategoriNamn VARCHAR(50),
    PRIMARY KEY (kategoriId)
);
CREATE TABLE bok
(
    bokId         INT NOT NULL AUTO_INCREMENT,
    bokTitel      VARCHAR(50),
    bokForfattare VARCHAR(50),
    bokBeskrivn   VARCHAR(50),
    bokIsbn       VARCHAR(50),
    bokPris       INT,
    bokKategoriId INT,
    PRIMARY KEY (bokId),
    FOREIGN KEY (bokKategoriId) REFERENCES kategori (kategoriId)
);

-- Skapa index för ISBN
CREATE UNIQUE INDEX index_isbn ON bok(bokIsbn);


-- Fyll tabellerna med lite data
INSERT INTO kategori(kategoriNamn)
VALUES ('Roman', 'Barnbok', 'Biografi');


INSERT INTO bok (bokTitel, bokForfattare, bokIsbn, bokPris, bokKategoriId)
VALUES ('Röda rummet', 'August Strindberg', '12345', '120', 1);

INSERT INTO bok(bokTitel, bokForfattare, bokIsbn, bokPris, bokKategoriId)
VALUES ('Vi på Saltkråkan', 'Astrid Lindgren', '23456', 150,
        (SELECT kategoriId FROM kategori WHERE kategoriNamn = 'Barnbok'));


INSERT INTO bok(bokTitel, bokForfattare, bokIsbn, bokPris, bokKategoriId)
VALUES ('Pippi på de sju haven', 'Astrid Lindgren', '34567', 200,
        (SELECT kategoriId FROM kategori WHERE kategoriNamn = 'Barnbok'));

-- Select från tabellen

SELECT *
FROM kategori;

SELECT *
FROM bok;

SELECT bok.bokTitel, bok.bokForfattare, kategori.kategoriNamn
FROM kategori
         INNER JOIN bok ON kategori.kategoriId = bok.bokKategoriId;

CREATE VIEW viewBokrea AS
SELECT bokTitel, bokForfattare, bokPris
FROM bok
WHERE bokPris < 170;

-- SQL för att visa view

SELECT *
FROM viewBokrea;

-- Raderar en view
DROP VIEW viewBokrea;

CREATE VIEW viewBokCatAvg AS
SELECT AVG(bok.bokPris) AS bokPrisAvg, kategori.kategoriNamn
FROM kategori
         INNER JOIN bok
                    ON kategori.kategoriId = bok.bokKategoriId
GROUP BY kategori.kategoriNamn
HAVING kategori.kategoriNamn = 'Barnbok';

-- SQL för att visa en view
SELECT *
FROM viewBokCatAvg;

-- Raderar en view
DROP VIEW viewBokCatAvg;

-- Kontrollerar databasens integritet
-- Försöker lägga till en bok med samma isbn som redan finns sparat (isbn har ett unikt index)
INSERT INTO bok(bokTitel, bokForfattare, bokIsbn, bokPris, bokKategoriId)
VALUES ('Vi på Saltkråkan', 'Astrid Lindgren', '23456', 150,
        (SELECT kategoriId FROM kategori WHERE kategoriNamn = 'Barnbok'));


-- Föräker radera en kategori som är koppplad till en bok
DELETE FROM kategori WHERE kategoriId = 1;

-- Uppdaterar alla böcker med beskrivning
UPDATE bok SET bokBeskrivn = 'En bra bok..';

-- DROP TABLE i rätt ordning dvs, jag kan inte radera tabeller som har en koppling i nogon tabel PK till FK
DROP TABLE bok;
DROP TABLE kategori;

INSERT INTO bok(bokTitel, bokForfattare, bokIsbn, bokPris, bokKategoriId)
VALUES ('Bröderna Lejonhjärta', 'Astrid Lindgren', '45677', 80,
        (SELECT kategoriId FROM kategori WHERE kategoriNamn = 'Barnbok'));

-- Visar enkel statistik
SELECT COUNT(bokId) AS bokCount FROM bok;
SELECT AVG(bokPris) bokMedelPris FROM bok;