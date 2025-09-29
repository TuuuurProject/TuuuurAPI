CREATE TABLE [dbo].[User_USR]
(
	[Id]                INT                 IDENTITY (1, 1) NOT NULL,
    [NickName]          VARCHAR(50)         NOT NULL,
    [Email]             VARCHAR(250)        NOT NULL,
    [Password]          VARCHAR(250)        NULL,
    [Avatar]            VARBINARY(MAX)      NULL,
    [ResetPasswordCode] UNIQUEIDENTIFIER    NULL,
    [IsAdmin]           BIT                 CONSTRAINT [DF_User_IsAdmin] DEFAULT ((0)) NOT NULL,
    [IsNew]             BIT                 CONSTRAINT [DF_User_IsNew] DEFAULT ((1)) NOT NULL,
    [IsGoogleUser]      BIT                 NOT NULL,
    CONSTRAINT [PK_USER_USR] PRIMARY KEY CLUSTERED ([Id] ASC)
)

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserEmail]
    ON [dbo].[User_USR]([Email], [IsGoogleUser] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserNickName]
    ON [dbo].[User_USR]([NickName] ASC);

