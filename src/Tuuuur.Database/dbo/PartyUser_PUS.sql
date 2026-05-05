CREATE TABLE [dbo].[PartyUser_PUS]
(
    [Id]        INT                 IDENTITY (1,1) NOT NULL,
    [Id_User]   UNIQUEIDENTIFIER    NOT NULL,
    [Id_Party]  UNIQUEIDENTIFIER    NOT NULL,
    [Elo]       INT                 NULL,
    [Winner]    BIT                 NULL,
    [FinalScore]INT                 NULL,
    CONSTRAINT [PK_PARTYUSER_PUS] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PartyUser_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User_USR]([Id]),
    CONSTRAINT [FK_PartyUser_Party] FOREIGN KEY ([Id_Party]) REFERENCES [dbo].[Party_PTY]([Id])
)

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PartyUserUniq]
    ON [dbo].[PartyUser_PUS]([Id_User], [Id_Party] ASC);
