-- Personel Insert
USE AttendanceAppDB;
GO

SET NOCOUNT ON; 


GO

DECLARE @Counter INT = 1;
DECLARE @TotalToInsert INT = 5000; 
DECLARE @RandomShift INT;

BEGIN TRANSACTION;

WHILE @Counter <= @TotalToInsert
BEGIN
    -- Randomly pick a ShiftID between 1 and 8 (matching the 8 shifts we just created)
    SET @RandomShift = (ABS(CHECKSUM(NEWID())) % 8) + 1;

    INSERT INTO Personel (
        FirstName, 
        LastName, 
        Email, 
        Phone, 
        Position, 
        Department, 
        ShiftID, 
        IsActive
    )
    VALUES (
        'FName_' + CAST(@Counter AS NVARCHAR(10)),             -- FName_1
        'LName_' + CAST(@Counter AS NVARCHAR(10)),             -- LName_1
        'emp_' + CAST(@Counter AS VARCHAR(10)) + '@corp.com',  -- emp_1@corp.com 
        '0900' + RIGHT('0000000' + CAST(@Counter AS VARCHAR(7)), 7), -- 09000000001 
        'Staff', 
        'Operations', 
        @RandomShift, 
        1 
    );

    SET @Counter = @Counter + 1;
END

-- Commit all 5,000 rows to the database permanently
COMMIT TRANSACTION;

PRINT 'Successfully inserted ' + CAST(@TotalToInsert AS VARCHAR) + ' employees!';
GO


SELECT * FROM Personel