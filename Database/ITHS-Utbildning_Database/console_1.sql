CREATE DATABASE Udemy;

USE Udemy;

CREATE TABLE contacts
(
    contatcsID INT NOT NULL AUTO_INCREMENT,
    name       TEXT,
    phone      INTEGER,
    email      TEXT,
    PRIMARY KEY (contatcsID)
);

INSERT INTO contacts (name, phone, email)
VALUES ('Avril', +6108, 'avril@hotmail.com');

SELECT * FROM contacts;

UPDATE contacts SET name = 'Vanja';



