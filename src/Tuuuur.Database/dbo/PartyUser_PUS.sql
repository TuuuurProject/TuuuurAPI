CREATE TABLE [dbo].[PartyUser_PUS]
(
    [Id_User]   INT NOT NULL,
    [Id_Party]  INT NOT NULL,
    CONSTRAINT [PK_PARTYUSER_PUS] PRIMARY KEY CLUSTERED ([Id_User], [Id_Party]),
    CONSTRAINT [FK_PartyUser_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User_USR]([Id]),
    CONSTRAINT [FK_PartyUser_Party] FOREIGN KEY ([Id_Party]) REFERENCES [dbo].[Party_PTY]([Id])
);