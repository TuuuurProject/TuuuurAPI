CREATE TABLE [dbo].[UserAuth_UAT]
(
    [Id]            INT                 IDENTITY (1, 1) NOT NULL,
    [UserId]        INT                 NOT NULL,
    [Code]          VARCHAR(6)          NOT NULL,
    [ExpiresAt]     DATETIME2           NOT NULL CONSTRAINT [DF_UserAuth_ExpiresAt] DEFAULT (DATEADD(MINUTE, 15, SYSUTCDATETIME())),
    CONSTRAINT [PK_USERAUTH_UAT] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserAuth_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User_USR]([Id])
);

GO
-- Index utile pour retrouver rapidement le dernier OTP d’un user
CREATE NONCLUSTERED INDEX IX_UserOTP_UserId
    ON [dbo].[UserAuth_UAT]([UserId], [ExpiresAt]);
