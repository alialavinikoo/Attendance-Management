USE AttendanceAppDB
GO

ALTER PROCEDURE sp_StagingCardLogs 
AS
BEGIN
    SET NOCOUNT ON

    BEGIN TRY

        -- 1. handle corrupted logs
        INSERT INTO CorruptedLogs (RawLine, ErrorReason)
        SELECT CONCAT(S.GateNumber, S.PersonelID, S.CardDate, S.CardTime), 
                        'SQL Rejection: Employee ID does not exist in Personel table.'
        FROM CardLogs_Staging S
        LEFT JOIN Personel P ON P.PersonelID = S.PersonelID
        WHERE P.PersonelID IS NULL

        -- Find rows in Staging that ALREADY exist in the real CardLogs table!
        INSERT INTO CorruptedLogs (RawLine, ErrorReason)
        SELECT 
            CONCAT('Gate:', S.GateNumber, ' | ID:', S.PersonelID, ' | Date:', S.CardDate, ' | Time:', S.CardTime), 
            'SQL Rejection: Duplicate log. This exact swipe already exists.'
        FROM CardLogs_Staging S
        INNER JOIN CardLogs C 
            ON S.PersonelID = C.PersonelID 
           AND S.CardDate = C.CardDate 
           AND S.CardTime = C.CardTime;

        
        -- 2. handle valid logs
        -- We use a CTE (RankedStaging) to ensure that if the text file ITSELF 
        -- has the exact same line pasted twice we only grab the first one!
        WITH RankedStaging AS (
            SELECT 
                S.PersonelID, S.CardDate, S.CardTime, S.CardStatus, S.GateNumber,
                ROW_NUMBER() OVER(PARTITION BY S.PersonelID, S.CardDate, S.CardTime ORDER BY S.GateNumber) as RowNum
            FROM CardLogs_Staging S
        )
        INSERT INTO CardLogs (PersonelID, CardDate, CardTime, CardStatus, GateNumber)
        SELECT 
            R.PersonelID, R.CardDate, R.CardTime, R.CardStatus, R.GateNumber
        FROM RankedStaging R
        INNER JOIN Personel P ON P.PersonelID = R.PersonelID
        WHERE R.RowNum = 1 -- Filters out duplicates inside the file
          AND NOT EXISTS (
              -- Filters out duplicates already in the database
              SELECT 1 FROM CardLogs C 
              WHERE C.PersonelID = R.PersonelID 
                AND C.CardDate = R.CardDate 
                AND C.CardTime = R.CardTime
          );

        -- 3. Clean up
        TRUNCATE TABLE CardLogs_Staging

    END TRY 
    BEGIN CATCH 
        THROW;
    END CATCH
END;

GO

Select * FROM CardLogs
Select * FROM CorruptedLogs
Select * FROM CardLogs_Staging
