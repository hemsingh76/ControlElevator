-- Run this in Microsoft Access (Create > Query Design > SQL View) ONLY if the program
-- could not create the database for you. The program itself creates exactly this table
-- automatically on first start, so normally you do not need this file.

CREATE TABLE ElevatorLog (
    LogId            COUNTER PRIMARY KEY,
    ElevatorId       INTEGER NOT NULL,
    CurrentFloor     INTEGER NOT NULL,
    DestinationFloor INTEGER,
    EventDescription TEXT(255) NOT NULL,
    EventTime        DATETIME NOT NULL
);
