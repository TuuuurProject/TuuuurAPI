CREATE TABLE [dbo].[User_USR] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT (NEWID()),
    [NickName]          VARCHAR (50)        NULL,
    [Email]             VARCHAR (250)       NULL,
    [Password]          VARCHAR (250)       NULL,
    [Avatar]            VARCHAR (MAX)       NULL,
    [ResetPasswordCode] UNIQUEIDENTIFIER    NULL,
    [IsDeleted]         BIT                 DEFAULT ((0)) NOT NULL,
    [IsAdmin]           BIT                 CONSTRAINT [DF_User_IsAdmin] DEFAULT ((0)) NOT NULL,
    [IsNew]             BIT                 CONSTRAINT [DF_User_IsNew] DEFAULT ((1)) NOT NULL,
    [IsGoogleUser]      BIT                 NOT NULL,
    CONSTRAINT [PK_USER_USR] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_User_ActiveHasRequiredInfo]
      CHECK (([IsDeleted] = 0 AND [NickName] IS NOT NULL AND [Email] IS NOT NULL) OR ([IsDeleted] = 1))
);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_UserEmail]
    ON [dbo].[User_USR]([Email], [IsGoogleUser] ASC)
    WHERE [Email] IS NOT NULL;

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_UserNickName]
    ON [dbo].[User_USR]([NickName] ASC)
    WHERE [NickName] IS NOT NULL;

GO

/* Trigger to allow user deletion */
CREATE TRIGGER TR_User_DeleteCascade
    ON [dbo].[User_USR]
    INSTEAD OF DELETE
    AS
BEGIN
    SET NOCOUNT ON;

    DELETE UPQ
    FROM [dbo].[UserPartyQuestion_UPQ] UPQ
             INNER JOIN deleted d ON UPQ.Id_User = d.Id;

    DELETE PUS
    FROM [dbo].[PartyUser_PUS] PUS
             INNER JOIN deleted d ON PUS.Id_User = d.Id;

    DELETE UAT
    FROM [dbo].[UserAuth_UAT] UAT
             INNER JOIN deleted d ON UAT.UserId = d.Id;

    DELETE ELO
    FROM [dbo].[Elo_ELO] ELO
             INNER JOIN deleted d ON ELO.Id_User = d.Id;

    DELETE RefreshToken
    FROM [dbo].[RefreshToken_RTK] RefreshToken
             INNER JOIN deleted d ON RefreshToken.UserId = d.Id;

    DELETE PQT
    FROM [dbo].[PartyQuestion_PQT] PQT
             INNER JOIN [dbo].[Party_PTY] PTY ON PQT.Id_Party = PTY.Id
             INNER JOIN deleted d ON PTY.Id_User_Host = d.Id;

    DELETE PDF
    FROM [dbo].[PartyDifficulty_PDF] PDF
             INNER JOIN [dbo].[Party_PTY] PTY ON PDF.Id_Party = PTY.Id
             INNER JOIN deleted d ON PTY.Id_User_Host = d.Id;

    DELETE PTH
    FROM [dbo].[PartyTheme_PTH] PTH
             INNER JOIN [dbo].[Party_PTY] PTY ON PTH.Id_Party = PTY.Id
             INNER JOIN deleted d ON PTY.Id_User_Host = d.Id;

    DELETE PTY
    FROM [dbo].[Party_PTY] PTY
             INNER JOIN deleted d ON PTY.Id_User_Host = d.Id;

    DELETE FROM [dbo].[User_USR]
    WHERE Id IN (SELECT Id FROM deleted);
END;
