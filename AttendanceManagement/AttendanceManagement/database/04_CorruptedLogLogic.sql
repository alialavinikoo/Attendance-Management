USE AttendanceAppDB;
GO

CREATE TABLE CorruptedLogs (
    CorruptedID BIGINT IDENTITY(1, 1) PRIMARY KEY,
    
    RawLine NVARCHAR(500) NOT NULL, 
    
    ErrorReason NVARCHAR(255) NOT NULL,
    
    CreatedAt DATETIME DEFAULT GETDATE() 
);
GO