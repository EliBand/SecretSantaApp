CREATE TABLE [dbo].[Members] (
    [Group_ID]  INT NOT NULL,
    [Person_ID] INT NOT NULL,
    [MembersID] INT IDENTITY (1, 1) NOT NULL,
    CONSTRAINT [PK_Members] PRIMARY KEY CLUSTERED ([MembersID] ASC),
    CONSTRAINT [FK_Members_Group] FOREIGN KEY ([Group_ID]) REFERENCES [dbo].[Group] ([GroupID]),
    CONSTRAINT [FK_Members_Person] FOREIGN KEY ([Person_ID]) REFERENCES [dbo].[Person] ([PersonID])
);


GO
CREATE NONCLUSTERED INDEX [IXFK_Members_Group]
    ON [dbo].[Members]([Group_ID] ASC);


GO
CREATE NONCLUSTERED INDEX [IXFK_Members_Person]
    ON [dbo].[Members]([Person_ID] ASC);

