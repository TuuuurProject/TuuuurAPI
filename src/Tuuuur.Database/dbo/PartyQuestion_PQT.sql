CREATE TABLE [dbo].[PartyQuestion_PQT]
(
    [Id]            INT                 IDENTITY (1,1) NOT NULL,
    [Id_Question]   INT                 NOT NULL,
    [Id_Party]      UNIQUEIDENTIFIER    NOT NULL,
    [Order]         INT                 NOT NULL,
    CONSTRAINT [PK_PARTYQUESTION_PQT] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PartyQuestion_Question] FOREIGN KEY ([Id_Question]) REFERENCES [dbo].[Question_QST]([Id]),
    CONSTRAINT [FK_PartyQuestion_Party] FOREIGN KEY ([Id_Party]) REFERENCES [dbo].[Party_PTY]([Id])
);
GO

CREATE NONCLUSTERED INDEX [IX_PartyQuestion_Party]
    ON [dbo].[PartyQuestion_PQT]([Id_Party], [Order]);
GO