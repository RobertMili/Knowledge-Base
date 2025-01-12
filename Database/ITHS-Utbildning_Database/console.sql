 CREATE TABLE dealer (
     car_id INT NOT NULL PRIMARY KEY,
     deal_name TEXT,
     car_place TEXT

 );

INSERT INTO dealer(car_id, deal_name, car_place)
VALUES (1, 'Hedin bil Stockholm', 'Stockholm' );

INSERT INTO dealer(car_id, deal_name, car_place)
VALUES (2, 'Hedin bil Götebor', 'Göteborg' );

SELECT * FROM dealer;

DROP TABLE dealer;

CREATE TABLE cars(
    car_id INT NOT NULL,
    car_model TEXT,
    car_registering_number TEXT,
    car_place TEXT,
    car_selling_number INT
);

INSERT INTO cars(car_id, car_model, car_registering_number, car_place, car_selling_number)
VALUES (1, 'Volvo', '123-A-456', 'Stockholm', 0001);

INSERT INTO cars(car_id, car_model, car_registering_number, car_place, car_selling_number)
VALUES (1, 'BMW', '321-A-654', 'Stockholm', 0002);

INSERT INTO cars(car_id, car_model, car_registering_number, car_place, car_selling_number)
VALUES (1, 'Audi', '231-W-322', 'Stockholm', 0003);

INSERT INTO cars(car_id, car_model, car_registering_number, car_place, car_selling_number)
VALUES (2,'Volvo', '435-W-456', 'Göteborg', 0004);

INSERT INTO cars(car_id, car_model, car_registering_number, car_place, car_selling_number)
VALUES (2, 'BMW', '093-D-456', 'Göteborg', 0005);

INSERT INTO cars(car_id, car_model, car_registering_number, car_place, car_selling_number)
VALUES (2, 'Audi', '543-W-123', 'Göteborg', 0006);

SELECT * FROM cars;

DELETE FROM cars WHERE car_registering_number = '543-W-123';

