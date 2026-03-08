CREATE TABLE [dbo].[Elo_ELO]
(
    [Id_User]      UNIQUEIDENTIFIER NOT NULL,
    [Id_Theme]     INT              NOT NULL,
    [Value]        INT              NOT NULL CONSTRAINT [DF_Elo_Value] DEFAULT (800),
    [GamesPlayed]  INT              NOT NULL CONSTRAINT [DF_Elo_GamesPlayed] DEFAULT (0),
    CONSTRAINT [PK_ELO_ELO] PRIMARY KEY CLUSTERED ([Id_User], [Id_Theme]),
    CONSTRAINT [FK_Elo_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User_USR]([Id]),
    CONSTRAINT [FK_Elo_Theme] FOREIGN KEY ([Id_Theme]) REFERENCES [dbo].[Theme_THM]([Id])
);