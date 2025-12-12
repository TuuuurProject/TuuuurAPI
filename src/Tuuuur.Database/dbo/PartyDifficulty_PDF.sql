CREATE TABLE [dbo].[PartyDifficulty_PDF]
(
    [Id]                INT                     IDENTITY (1,1) NOT NULL,
    [Id_Party]          UNIQUEIDENTIFIER        NOT NULL,
    [Id_Difficulty]     INT                     NOT NULL,
    CONSTRAINT [PK_PartyDifficulty_PDF] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PartyDifficulty_Party] FOREIGN KEY ([Id_Party]) REFERENCES [dbo].[Party_PTY]([Id]),
    CONSTRAINT [FK_PartyDifficulty_Theme] FOREIGN KEY ([Id_Difficulty]) REFERENCES [ref].[Difficulty_DFT]([Id])
)