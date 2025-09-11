CREATE TABLE [dbo].[User_USR] (
    [Id]                INT                 IDENTITY (1, 1) NOT NULL,
    [Firstname]         VARCHAR(50)         NOT NULL,
    [Lastname]          VARCHAR(50)         NOT NULL,
    [Nickname]          VARCHAR(50)         NOT NULL,
    [Email]             VARCHAR(100)        NOT NULL,
    [Password]          VARCHAR(MAX)        NOT NULL,
    CONSTRAINT [PK_USER_USR] PRIMARY KEY CLUSTERED ([Id] ASC),
);