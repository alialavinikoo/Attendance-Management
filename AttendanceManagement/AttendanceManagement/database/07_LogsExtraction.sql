USE AttendanceAppDB
GO

CREATE OR ALTER PROCEDURE sp_GetPagedCardLogs
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
        C.LogID, 
        C.PersonelID, 
        C.CardDate, 
        C.CardTime, 
        C.CardStatus, 
        C.GateNumber
    FROM CardLogs C
    INNER JOIN Personel P ON P.PersonelID = C.PersonelID
    WHERE 
        (@FromDate IS NULL OR C.CardDate >= @FromDate) 
        AND (@ToDate IS NULL OR C.CardDate <= @ToDate)
        AND (
            @Search IS NULL 
            OR LTRIM(RTRIM(@Search)) = '' 
            OR CONCAT(P.FirstName, ' ', P.LastName) LIKE '%' + @Search + '%'
            OR CAST(C.PersonelID AS NVARCHAR(50)) = @Search
        )
    ORDER BY 
        C.CardDate DESC, 
        C.CardTime DESC, 
        C.LogID DESC
    OFFSET @OFFSET ROWS
    FETCH NEXT @PageSize ROWS ONLY
    OPTION (RECOMPILE); 
END
GO


CREATE OR ALTER PROCEDURE sp_GetPagedCorruptedLogs
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
        C.RawLine, 
        C.ErrorReason, 
        C.CreatedAt
    FROM CorruptedLogs C
    WHERE 
        (@FromDate IS NULL OR CAST(C.CreatedAt AS DATE) >= @FromDate) 
        AND (@ToDate IS NULL OR CAST(C.CreatedAt AS DATE) <= @ToDate)
        AND (
            @Search IS NULL 
            OR LTRIM(RTRIM(@Search)) = '' 
            OR C.ErrorReason LIKE '%' + @Search + '%'
            OR C.RawLine LIKE '%' + @Search + '%' 
        )
    ORDER BY 
        C.CorruptedID DESC
    OFFSET @OFFSET ROWS
    FETCH NEXT @PageSize ROWS ONLY
    OPTION (RECOMPILE); 
END
GO

