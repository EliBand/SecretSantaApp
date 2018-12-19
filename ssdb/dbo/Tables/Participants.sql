CREATE TABLE [dbo].[Participants] (
    [Event_ID]       INT NOT NULL,
    [Person_ID]      INT NOT NULL,
    [ZielPerson_ID]  INT NOT NULL,
    [ParticipantsID] INT IDENTITY (1, 1) NOT NULL,
    CONSTRAINT [PK_Participants] PRIMARY KEY CLUSTERED ([ParticipantsID] ASC),
    CONSTRAINT [FK_Participants_Events] FOREIGN KEY ([Event_ID]) REFERENCES [dbo].[Events] ([EventID]),
    CONSTRAINT [FK_Participants_Person] FOREIGN KEY ([Person_ID]) REFERENCES [dbo].[Person] ([PersonID]),
    CONSTRAINT [FK_Participants_Person_02] FOREIGN KEY ([ZielPerson_ID]) REFERENCES [dbo].[Person] ([PersonID])
);


GO
CREATE NONCLUSTERED INDEX [IXFK_Participants_Events]
    ON [dbo].[Participants]([Event_ID] ASC);


GO
CREATE NONCLUSTERED INDEX [IXFK_Participants_Person]
    ON [dbo].[Participants]([Person_ID] ASC);


GO
CREATE NONCLUSTERED INDEX [IXFK_Participants_Person_02]
    ON [dbo].[Participants]([ZielPerson_ID] ASC);

