CREATE TABLE [dbo].[Answer_ANS]
(
    [Id]            INT             IDENTITY (1,1) NOT NULL,
    [Id_Question]   INT             NOT NULL,
    [Value]         VARCHAR(MAX)    NOT NULL,
    [Valid]         BIT             NOT NULL CONSTRAINT [DF_Answer_Valid] DEFAULT ((0)),
    CONSTRAINT [PK_ANSWER_ANS] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Answer_Question] FOREIGN KEY ([Id_Question]) REFERENCES [dbo].[Question_QST]([Id])
);
GO

CREATE NONCLUSTERED INDEX [IX_Answer_Question]
    ON [dbo].[Answer_ANS]([Id_Question]);
GO
