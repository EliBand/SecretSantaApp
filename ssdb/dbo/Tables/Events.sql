CREATE TABLE [dbo].[Events] (
    [EventID]   INT          IDENTITY (1, 1) NOT NULL,
    [EventName] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED ([EventID] ASC)
);

