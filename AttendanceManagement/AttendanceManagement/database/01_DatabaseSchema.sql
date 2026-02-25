CREATE DATABASE AttendanceAppDB;
GO

USE AttendanceAppDB;
GO



CREATE TABLE Shifts (
	ShiftID INT IDENTITY(1, 1) PRIMARY KEY,
	ShiftName NVARCHAR(50) NOT NULL UNIQUE	
);

CREATE TABLE ShiftSchedules (
	ScheduleID INT IDENTITY(1, 1) PRIMARY KEY,
	ShiftID INT NOT NULL FOREIGN KEY REFERENCES Shifts(ShiftID),
	DayOfWeek TINYINT NOT NULL CHECK (DayOfWeek BETWEEN 1 AND 7),
	StartTime Time(0) NOT NULL,
	FinishTime Time(0) NOT NULL,
	NightShift BIT NOT NULL DEFAULT 0,
	CONSTRAINT UQ_SHIFT_PER_DAY UNIQUE (ShiftID, DayOfWeek)
);

CREATE TABLE Personel (
	PersonelID INT IDENTITY(1, 1) PRIMARY KEY,
	FirstName NVARCHAR(50) NOT NULL,
	LastName NVARCHAR(50) NOT NULL,
	Email VARCHAR(100) UNIQUE,
	Phone VARCHAR(20) UNIQUE,
	Position NVARCHAR(50),
	Department NVARCHAR(50),
	ShiftID INT NOT NULL FOREIGN KEY REFERENCES Shifts(ShiftID),
	IsActive BIT NOT NULL DEFAULT 1,
);

CREATE TABLE CardLogs (
    LogID BIGINT IDENTITY(1,1) PRIMARY KEY,
    PersonelID INT NOT NULL FOREIGN KEY REFERENCES Personel(PersonelID),
    CardDate DATE NOT NULL,
    CardTime TIME(0) NOT NULL,
    CardStatus TINYINT NOT NULL CHECK (CardStatus IN (1, 2, 3)), -- 1=Normal, 2=Leave, 3=Mission
    GateNumber TINYINT NOT NULL
);


CREATE TABLE DailyAttendance (
    RecordID BIGINT IDENTITY(1,1) PRIMARY KEY,
    PersonelID INT NOT NULL FOREIGN KEY REFERENCES Personel(PersonelID),
    WorkDate DATE NOT NULL,
    FirstInTime TIME(0),
    LastOutTime TIME(0),
    ArrivalLateMinutes INT DEFAULT 0 CHECK (ArrivalLateMinutes >= 0),
    DepartureLateMinutes INT DEFAULT 0 CHECK (DepartureLateMinutes >= 0),
    ArrivalOvertimeMinutes INT DEFAULT 0 CHECK (ArrivalOvertimeMinutes >= 0),
    DepartureOvertimeMinutes INT DEFAULT 0 CHECK (DepartureOvertimeMinutes >= 0),
	CONSTRAINT UQ_Personel_WorkDate UNIQUE (PersonelID, WorkDate) -- ONE SUMMARY PER DAY
);
GO
