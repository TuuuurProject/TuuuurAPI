CREATE TABLE [dbo].[Theme_THM]
(
    [Id]                INT                 IDENTITY (1, 1) NOT NULL,
    [Label]             VARCHAR(50)         NOT NULL,
    CONSTRAINT [PK_Theme_THM] PRIMARY KEY CLUSTERED ([Id] ASC),
)