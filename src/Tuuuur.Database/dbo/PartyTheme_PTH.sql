CREATE TABLE [dbo].[PartyTheme_PTH]
(
    [Id]                INT                     IDENTITY (1,1) NOT NULL,
    [Id_Party]          UNIQUEIDENTIFIER        NOT NULL,
    [Id_Theme]          INT                     NOT NULL,
    CONSTRAINT [PK_PartyTheme_PTH] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PartyTheme_Party] FOREIGN KEY ([Id_Party]) REFERENCES [dbo].[Party_PTY]([Id]),
    CONSTRAINT [FK_PartyTheme_Theme] FOREIGN KEY ([Id_Theme]) REFERENCES [dbo].[Theme_THM]([Id])
)