Create Table Result(
ResultId INT IDENTITY(1,1) PRIMARY KEY,
EnrolmentId INT NOT NULL UNIQUE,
FinidhTime TIME NOT NULL,
Position INT NOT NULL,
RecordedAt DATETIME NOT NULL DEFAULT GETDATE(),
CONSTRAINT FK_Result_Enrolment FOREIGN KEY (EnrolmentId) REFERENCES Enrolment(EnrolmentId)
)