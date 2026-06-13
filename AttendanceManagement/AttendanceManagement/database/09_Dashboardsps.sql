USE AttendanceAppDB
GO


CREATE OR ALTER PROCEDURE sp_GetDashboardStats
AS
BEGIN
    SET NOCOUNT ON;
    SET DATEFIRST 6;

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    DECLARE @Now TIME = CAST(GETDATE() AS TIME);
    DECLARE @WeekDay INT = DATEPART(WEEKDAY, @Today);

    SELECT 
        (SELECT COUNT(*) 
         FROM Personel 
         WHERE IsActive = 1) AS PersonelCount,

        (SELECT COUNT(*) 
         FROM DailyAttendance DA
         JOIN Personel P ON P.PersonelID = DA.PersonelID
         WHERE DA.WorkDate = @Today
         AND DA.RecordStatus IN (1,3)
         AND P.IsActive = 1) AS PresentCount,

        (SELECT COUNT(*)
         FROM DailyAttendance DA
         JOIN Personel P ON P.PersonelID = DA.PersonelID
         JOIN ShiftSchedules SS 
              ON SS.ShiftID = P.ShiftID 
              AND SS.DayOfWeek = @WeekDay
         WHERE DA.WorkDate = @Today
         AND DA.RecordStatus = 2
         AND P.IsActive = 1
         AND @Now > SS.StartTime) AS AbsentCount,

        (SELECT COUNT(*)
         FROM DailyAttendance DA
         JOIN Personel P ON P.PersonelID = DA.PersonelID
         WHERE DA.WorkDate = @Today
         AND DA.RecordStatus = 1
         AND P.IsActive = 1
         AND DA.ArrivalDeviationMinutes > 0) AS LateCount;
END
GO

CREATE OR ALTER PROCEDURE sp_GetLatePersonel
AS
BEGIN
     SET NOCOUNT ON;
     SET DATEFIRST 6;

     DECLARE @Today DATE = CAST(GETDATE() AS DATE);
     DECLARE @WeekDay INT = DATEPART(WEEKDAY, @Today);

     SELECT TOP (10) 
     DA.PersonelID, P.FirstName, P.LastName, P.Department, DA.FirstInTime, SS.StartTime, DA.ArrivalDeviationMinutes
     FROM DailyAttendance DA 
     JOIN Personel P ON P.PersonelID = DA.PersonelID
     JOIN ShiftSchedules SS 
          ON SS.ShiftID = P.ShiftID 
          AND SS.DayOfWeek = @WeekDay
     WHERE DA.WorkDate = @Today
     AND DA.RecordStatus = 1
     AND P.IsActive = 1
     AND DA.ArrivalDeviationMinutes IS NOT NULL
     AND DA.ArrivalDeviationMinutes > 0
     ORDER BY ArrivalDeviationMinutes DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAbsentPersonel
AS
BEGIN
     SET NOCOUNT ON;
     SET DATEFIRST 6;

     DECLARE @Today DATE = CAST(GETDATE() AS DATE);
     DECLARE @Now TIME = CAST(GETDATE() AS TIME);
     DECLARE @WeekDay INT = DATEPART(WEEKDAY, @Today);

     SELECT TOP (10) 
     DA.PersonelID, P.FirstName, P.LastName, P.Department, SS.StartTime
     FROM DailyAttendance DA
     JOIN Personel P ON P.PersonelID = DA.PersonelID
     JOIN ShiftSchedules SS 
          ON SS.ShiftID = P.ShiftID 
          AND SS.DayOfWeek = @WeekDay
     WHERE DA.WorkDate = @Today
     AND DA.RecordStatus = 2
     AND P.IsActive = 1
     AND @Now > SS.StartTime
     ORDER BY SS.StartTime ASC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetRecentCardLogs 
AS 
BEGIN 
     SET NOCOUNT ON

     SELECT TOP (10)
     C.PersonelID, P.FirstName, P.LastName, P.Department, C.CardDate, C.CardTime, C.CardStatus, C.GateNumber
     FROM CardLogs C
     JOIN Personel P ON P.PersonelID = C.PersonelID
     AND P.IsActive = 1
     ORDER BY C.CardDate DESC, C.CardTime DESC
END
GO


CREATE OR ALTER PROCEDURE sp_GetDashboardData
AS
BEGIN
    SET NOCOUNT ON;
    SET DATEFIRST 6;

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    DECLARE @Now TIME = CAST(GETDATE() AS TIME);
    DECLARE @WeekDay INT = DATEPART(WEEKDAY, @Today);

    -- 1. Dashboard Stats 

    SELECT 
        (SELECT COUNT(*) 
         FROM Personel 
         WHERE IsActive = 1) AS PersonelCount,

        (SELECT COUNT(*) 
         FROM DailyAttendance DA
         JOIN Personel P ON P.PersonelID = DA.PersonelID
         WHERE DA.WorkDate = @Today
         AND DA.RecordStatus IN (1,3)
         AND P.IsActive = 1) AS PresentCount,

        (SELECT COUNT(*)
         FROM DailyAttendance DA
         JOIN Personel P ON P.PersonelID = DA.PersonelID
         JOIN ShiftSchedules SS 
              ON SS.ShiftID = P.ShiftID 
              AND SS.DayOfWeek = @WeekDay
         WHERE DA.WorkDate = @Today
         AND DA.RecordStatus = 2
         AND P.IsActive = 1
         AND @Now > SS.StartTime) AS AbsentCount,

        (SELECT COUNT(*)
         FROM DailyAttendance DA
         JOIN Personel P ON P.PersonelID = DA.PersonelID
         JOIN ShiftSchedules SS 
            ON SS.ShiftID = P.ShiftID
            AND SS.DayOfWeek = @WeekDay
         WHERE DA.WorkDate = @Today
         AND DA.RecordStatus IN (1, 3)
         AND P.IsActive = 1
         AND DA.ArrivalDeviationMinutes > 0) AS LateCount;


    -- 2. Late Employees (Top 10)

    SELECT TOP (10)
        DA.PersonelID,
        P.FirstName,
        P.LastName,
        P.Department,
        DA.FirstInTime,
        SS.StartTime,
        DA.ArrivalDeviationMinutes
    FROM DailyAttendance DA
    JOIN Personel P ON P.PersonelID = DA.PersonelID
    JOIN ShiftSchedules SS 
        ON SS.ShiftID = P.ShiftID
        AND SS.DayOfWeek = @WeekDay
    WHERE DA.WorkDate = @Today
    AND DA.RecordStatus IN (1, 3)
    AND P.IsActive = 1
    AND DA.ArrivalDeviationMinutes > 0
    ORDER BY DA.ArrivalDeviationMinutes DESC;


    -- 3. Absent Employees (Top 10)

    SELECT TOP (10)
        DA.PersonelID,
        P.FirstName,
        P.LastName,
        P.Department,
        SS.StartTime
    FROM DailyAttendance DA
    JOIN Personel P ON P.PersonelID = DA.PersonelID
    JOIN ShiftSchedules SS 
        ON SS.ShiftID = P.ShiftID
        AND SS.DayOfWeek = @WeekDay
    WHERE DA.WorkDate = @Today
    AND DA.RecordStatus = 2
    AND P.IsActive = 1
    AND @Now > SS.StartTime
    ORDER BY SS.StartTime;


    -- 4. Recent Card Logs (Top 10) 

    SELECT TOP (10)
        C.PersonelID,
        P.FirstName,
        P.LastName,
        P.Department,
        C.CardDate,
        C.CardTime,
        C.CardStatus,
        C.GateNumber
    FROM CardLogs C
    JOIN Personel P ON P.PersonelID = C.PersonelID
    WHERE C.CardDate = @Today
    ORDER BY C.CardDate DESC, C.CardTime DESC;
END
GO

EXEC sp_GetDashboardData

