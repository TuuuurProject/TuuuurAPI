CREATE TABLE [dbo].[User_USR] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT (NEWID()),
    [NickName]          VARCHAR (50)        NULL,
    [Email]             VARCHAR (250)       NULL,
    [Password]          VARCHAR (250)       NULL,
    [Avatar]            VARCHAR (MAX)       NULL,
    [ResetPasswordCode] UNIQUEIDENTIFIER    NULL,
    [IsActive]          BIT                 DEFAULT ((1)) NOT NULL,
    [IsAdmin]           BIT                 CONSTRAINT [DF_User_IsAdmin] DEFAULT ((0)) NOT NULL,
    [IsNew]             BIT                 CONSTRAINT [DF_User_IsNew] DEFAULT ((1)) NOT NULL,
    [IsGoogleUser]      BIT                 NOT NULL,
    CONSTRAINT [PK_USER_USR] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_User_ActiveHasRequiredInfo]
      CHECK (([IsActive] = 1 AND [NickName] IS NOT NULL AND [Email] IS NOT NULL) OR ([IsActive] = 0))
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