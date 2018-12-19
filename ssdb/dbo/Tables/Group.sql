CREATE TABLE [dbo].[Group] (
    [GroupID]   INT          IDENTITY (1, 1) NOT NULL,
    [GroupName] VARCHAR (50) NOT NULL,
    [User_ID]   INT          NOT NULL,
    CONSTRAINT [PK_Group] PRIMARY KEY CLUSTERED ([GroupID] ASC),
    CONSTRAINT [FK_Group_User] FOREIGN KEY ([User_ID]) REFERENCES [dbo].[User] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IXFK_Group_Person_02]
    ON [dbo].[Group]([User_ID] ASC);


GO
CREATE NONCLUSTERED INDEX [IXFK_Group_User]
    ON [dbo].[Group]([User_ID] ASC);

