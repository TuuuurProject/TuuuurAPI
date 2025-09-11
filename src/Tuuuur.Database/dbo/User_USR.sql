CREATE TABLE [dbo].[User_USR]
(
	[Id]                INT                 IDENTITY (1, 1) NOT NULL,
    [FirstName]         VARCHAR(50)         NOT NULL,
    [LastName]          VARCHAR(50)         NOT NULL,
    [NickName]          VARCHAR(50)         NOT NULL,
    [Email]             VARCHAR(100)        NOT NULL,
    [Password]          VARCHAR(MAX)        NOT NULL,
    [IsAdmin]           BIT                 NOT NULL,
    CONSTRAINT [PK_USER_USR] PRIMARY KEY CLUSTERED ([Id] ASC),
)