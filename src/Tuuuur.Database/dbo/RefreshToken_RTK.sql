CREATE TABLE [dbo].[RefreshToken_RTK] (
    [UserId]    INT              NOT NULL,
    [Token]     VARCHAR (500)    NOT NULL,
    [ExpiresAt] DATETIME2 (7)    NOT NULL,
    [CreatedAt] DATETIME2 (7)    CONSTRAINT [DF_RefreshToken_CreatedAt] DEFAULT (GETUTCDATE()) NOT NULL,
    CONSTRAINT [PK_RefreshToken_RTK] PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [FK_RefreshToken_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User_USR] ([Id])
);

GO
CREATE NONCLUSTERED INDEX [IX_RefreshToken_Token]
    ON [dbo].[RefreshToken_RTK]([Token] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_RefreshToken_UserId]
    ON [dbo].[RefreshToken_RTK]([UserId] ASC);
