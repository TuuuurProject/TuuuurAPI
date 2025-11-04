CREATE TABLE [dbo].[UserPartyQuestion_UPQ]
(
    [Id]                INT                 IDENTITY (1,1) NOT NULL,
    [Id_Party_Question] INT                 NOT NULL,
    [Id_User]           INT                 NOT NULL,
    [DtPresentedAt]     DATETIME2           NOT NULL CONSTRAINT [DF_UserPartyQuestion_Dt] DEFAULT (SYSUTCDATETIME()),
    [DtAnsweredAt]      DATETIME2           NULL,
    [Score]             INT                 NULL,
    [Id_Answer]         INT                 NULL,
    [Correct]           BIT                 NULL,
    [AnswersOrder]      UNIQUEIDENTIFIER    NOT NULL CONSTRAINT [DF_UserPartyQuestion_AnswerOrder] DEFAULT (NEWID()),
    CONSTRAINT [PK_USERPARTYQUESTION_UPQ] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserPartyQuestion_PartyQuestion] FOREIGN KEY ([Id_Party_Question]) REFERENCES [dbo].[PartyQuestion_PQT]([Id]),
    CONSTRAINT [FK_UserPartyQuestion_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User_USR]([Id]),
    CONSTRAINT [FK_UserPartyQuestion_Answer] FOREIGN KEY ([Id_Answer]) REFERENCES [dbo].[Answer_ANS]([Id]),
);
GO

CREATE NONCLUSTERED INDEX [IX_UserPartyQuestion_User]
    ON [dbo].[UserPartyQuestion_UPQ]([Id_User], [Id_Party_Question]);