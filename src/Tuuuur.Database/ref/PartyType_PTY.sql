CREATE TABLE [ref].[PartyType_PTY]
(
    [Id]                INT                 IDENTITY (1, 1) NOT NULL,
    [Label]             VARCHAR(50)         NOT NULL,
    CONSTRAINT [PK_PartyType_PTT] PRIMARY KEY CLUSTERED ([Id] ASC),
)