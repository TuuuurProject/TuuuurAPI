CREATE TABLE [dbo].[Party_PTY]
(
    [Id]            UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
    [Dt]            DATETIME2        NOT NULL CONSTRAINT [DF_Party_Dt] DEFAULT (SYSUTCDATETIME()),
    [Id_Party_Type] INT              NOT NULL,
    [Id_User_Host]  INT              NULL,
    [Active]        BIT              NOT NULL CONSTRAINT [DF_Party_Active] DEFAULT ((1)),
    [Finish]        BIT              NOT NULL CONSTRAINT [DF_Party_Finish] DEFAULT ((0)),
    CONSTRAINT [PK_PARTY_PTY] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Party_Type] FOREIGN KEY ([Id_Party_Type]) REFERENCES [ref].[PartyType_PTY]([Id]),
    CONSTRAINT [FK_Party_HostUser] FOREIGN KEY ([Id_User_Host]) REFERENCES [dbo].[User_USR]([Id])
);