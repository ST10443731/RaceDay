Create Table Enrolment(
EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
ParticipantId INT NOT NULL,
CategoryId INT NOT NULL,
EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
Status VARCHAR(30) NOT NULL DEFAULT 'Confirmed',
CONSTRAINT FK_Enrolment_Participant FOREIGN KEY (ParticipantId) REFERENCES [User](UserId),
CONSTRAINT FK_Enrolment_Categoty FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId),
CONSTRAINT UQ_Enrolment_Participant_Category UNIQUE(ParticipantId,CategoryId)
)