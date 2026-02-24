USE AttendanceAppDB;
GO

-- no constraints or keys
CREATE TABLE CardLogs_Staging (
    LogID BIGINT IDENTITY(1,1), 
    GateNumber TINYINT NOT NULL,
    PersonelID INT NOT NULL,
    CardDate DATE NOT NULL,
    CardTime TIME(0) NOT NULL,
    CardStatus TINYINT NOT NULL
);
GO