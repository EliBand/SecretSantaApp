CREATE TABLE [dbo].[User] (
    [UserID]   INT          NOT NULL,
    [UserName] VARCHAR (50) NOT NULL,
    [Password] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Table1] PRIMARY KEY CLUSTERED ([UserID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IXFK_User_Person]
    ON [dbo].[User]([UserID] ASC);

