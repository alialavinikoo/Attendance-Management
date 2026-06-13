USE AttendanceAppDB 
GO


CREATE OR ALTER PROCEDURE sp_GetPagedVerifiedAttendance
    @PageNumber INT,
    @PageSize INT,
    @Search NVARCHAR(50) = NULL, 
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON; 

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 50;

    DECLARE @OFFSET INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        COUNT(*) OVER() AS TotalCount,
        DA.PersonelID,
        DA.WorkDate,
        DA.FirstInTime,
        DA.LastOutTime,
        DA.TotalWorkedMinutes,
        DA.ArrivalDeviationMinutes,
        DA.DepartureDeviationMinutes,
        DA.RecordStatus, -- 1=Normal, 2=Absent, 3=Missing Punch, 4=Unscheduled
        P.FirstName,
        P.LastName,
        S.ShiftName
    FROM DailyAttendance DA
    INNER JOIN Personel P ON P.PersonelID = DA.PersonelID
    INNER JOIN Shifts S ON S.ShiftID = P.ShiftID 
    WHERE 
        (@FromDate IS NULL OR DA.WorkDate >= @FromDate) 
        AND (@ToDate IS NULL OR DA.WorkDate <= @ToDate)
        AND (
            @Search IS NULL 
            OR LTRIM(RTRIM(@Search)) = '' 
            OR CONCAT(P.FirstName, ' ', P.LastName) LIKE '%' + @Search + '%'
            OR CAST(DA.PersonelID AS NVARCHAR(50)) = @Search
        )
    ORDER BY 
        DA.WorkDate DESC, 
        DA.PersonelID ASC
    OFFSET @OFFSET ROWS
    FETCH NEXT @PageSize ROWS ONLY
    OPTION (RECOMPILE); 
END
GO


CREATE OR ALTER PROCEDURE sp_CalculateDailyAttendance
    @PersonelID INT,
    @WorkDate DATE
AS
BEGIN
    SET NOCOUNT ON
    SET DATEFIRST 6
    
    DECLARE @FirstLogTime TIME(0), @LastLogTime Time(0), @LogCount INT

    SELECT 
    @FirstLogTime = MIN(C.CardTime), 
    @LastLogTime = CASE WHEN COUNT(*) > 1 THEN MAX(C.CardTime) ELSE NULL END,
    @LogCount = COUNT(*)
    FROM CardLogs C 
    WHERE C.CardDate = @WorkDate AND C.PersonelID = @PersonelID

    DECLARE @DayOfWeek TINYINT = DATEPART(WEEKDAY, @WorkDate)
    DECLARE @ShiftStart TIME(0), @ShiftEnd TIME(0)
    
    SELECT @ShiftStart = SS.StartTime, @ShiftEnd = SS.FinishTime
    FROM Personel P
    JOIN ShiftSchedules SS ON P.ShiftID = SS.ShiftID
    WHERE P.PersonelID = @PersonelID AND SS.DayOfWeek = @DayOfWeek 

    DECLARE @RecordStatus TINYINT = 1

    IF @ShiftStart IS NULL SET @RecordStatus = 4
    ELSE IF @FirstLogTime IS NULL SET @RecordStatus = 2
    ELSE IF @LastLogTime IS NULL SET @RecordStatus = 3
    
    MERGE DailyAttendance AS Target
    USING (SELECT @PersonelID as PID, @WorkDate as WDate) AS Source
    ON (Target.PersonelID = Source.PID AND Target.WorkDate = Source.WDate)
    WHEN MATCHED THEN
        UPDATE SET 
            FirstInTime = @FirstLogTime,
            LastOutTime = @LastLogTime,
            ArrivalDeviationMinutes = CASE WHEN @ShiftStart IS NOT NULL AND @FirstLogTime IS NOT NULL THEN DATEDIFF(MINUTE, @ShiftStart, @FirstLogTime) ELSE 0 END,
            DepartureDeviationMinutes = CASE WHEN @ShiftEnd IS NOT NULL AND @LastLogTime IS NOT NULL THEN DATEDIFF(MINUTE, @LastLogTime, @ShiftEnd) ELSE 0 END,
            TotalWorkedMinutes = ISNULL(DATEDIFF(MINUTE, @FirstLogTime, @LastLogTime), 0),
            RecordStatus = @RecordStatus
    WHEN NOT MATCHED THEN
        INSERT (PersonelID, WorkDate, FirstInTime, LastOutTime, ArrivalDeviationMinutes, DepartureDeviationMinutes, TotalWorkedMinutes, RecordStatus)
        VALUES (@PersonelID, @WorkDate, @FirstLogTime, @LastLogTime, 
                CASE WHEN @ShiftStart IS NOT NULL AND @FirstLogTime IS NOT NULL THEN DATEDIFF(MINUTE, @ShiftStart, @FirstLogTime) ELSE 0 END, 
                CASE WHEN @ShiftEnd IS NOT NULL AND @LastLogTime IS NOT NULL THEN DATEDIFF(MINUTE, @LastLogTime, @ShiftEnd) ELSE 0 END,
                ISNULL(DATEDIFF(MINUTE, @FirstLogTime, @LastLogTime), 0), @RecordStatus);    
END
GO


CREATE OR ALTER PROCEDURE sp_GetAttendanceGaps
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON

    ;WITH DateRange AS (
        SELECT @StartDate AS WorkDate
        UNION ALL 
        SELECT DATEADD(DAY, 1, WorkDate) FROM DateRange WHERE WorkDate < @EndDate
    ),
    AllPairs AS (
        SELECT P.PersonelID, D.WorkDate
        FROM Personel P 
        CROSS JOIN DateRange D
        WHERE P.IsActive = 1  
    )

    INSERT INTO AttendanceQueue (PersonelID, WorkDate)
    SELECT A.PersonelID, A.WorkDate
    FROM AllPairs A
    LEFT JOIN DailyAttendance D ON A.PersonelID = D.PersonelID AND A.WorkDate = D.WorkDate
    WHERE D.RecordID IS NULL AND NOT EXISTS (
        SELECT 1 
        FROM AttendanceQueue Q
        WHERE Q.PersonelID = A.PersonelID AND Q.WorkDate = A.WorkDate
    )
    OPTION (MAXRECURSION 0)
END
GO


CREATE OR ALTER TRIGGER trg_CardLogs_Insert_AttendanceQueue
ON CardLogs
AFTER INSERT 
AS
BEGIN 
    SET NOCOUNT ON

    INSERT INTO AttendanceQueue (PersonelID, WorkDate)
    SELECT DISTINCT i.PersonelID, i.CardDate
    FROM inserted i
    WHERE NOT EXISTS (
        SELECT 1
        FROM AttendanceQueue Q
        WHERE i.PersonelID = Q.PersonelID AND i.CardDate = Q.WorkDate
    )
END
GO