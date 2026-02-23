USE AttendanceAppDB
GO

INSERT INTO Shifts
    (ShiftName)
VALUES
    ('Morning Regular'),
    ('Morning Even'),
    ('Morning Odd'),
    ('Morning Weekend'),
    ('Evening Regular'),
    ('Evening Even'),
    ('Evening Odd'),
    ('Evening Weekend')
GO

INSERT INTO ShiftSchedules 
    (ShiftID, DayOfWeek, StartTime, FinishTime, NightShift)
VALUES
    -- SHIFT 1: Morning Regular 
    (1, 1, '08:00', '16:00', 0), 
    (1, 2, '08:00', '16:00', 0), 
    (1, 3, '08:00', '16:00', 0), 
    (1, 4, '08:00', '16:00', 0), 
    (1, 5, '08:00', '16:00', 0), 

    -- SHIFT 2: Morning Even 
    (2, 2, '08:00', '16:00', 0), 
    (2, 4, '08:00', '16:00', 0), 
    (2, 6, '08:00', '16:00', 0), 

    -- SHIFT 3: Morning Odd 
    (3, 1, '08:00', '16:00', 0), 
    (3, 3, '08:00', '16:00', 0), 
    (3, 5, '08:00', '16:00', 0), 

    -- SHIFT 4: Morning Weekend 
    (4, 6, '08:00', '14:00', 0), 
    (4, 7, '08:00', '14:00', 0), 

    -- SHIFT 5: Evening Regular 
    (5, 1, '16:00', '23:59', 0), 
    (5, 2, '16:00', '23:59', 0), 
    (5, 3, '16:00', '23:59', 0), 
    (5, 4, '16:00', '23:59', 0), 
    (5, 5, '16:00', '23:59', 0), 

    -- SHIFT 6 & 7: Evening Even / Odd
    (6, 2, '16:00', '23:59', 0), 
    (6, 4, '16:00', '23:59', 0), 
    (6, 6, '16:00', '23:59', 0), 
    
    (7, 1, '16:00', '23:59', 0), 
    (7, 3, '16:00', '23:59', 0), 
    (7, 5, '16:00', '23:59', 0), 

    -- SHIFT 8: Evening Weekend (Thu, Fri)
    (8, 6, '16:00', '23:59', 0), 
    (8, 7, '16:00', '23:59', 0); 
GO

SELECT * FROM Shifts S
JOIN ShiftSchedules SS ON S.ShiftID = SS.ShiftID 

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

