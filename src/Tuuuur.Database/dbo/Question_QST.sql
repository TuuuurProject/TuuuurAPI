CREATE TABLE [dbo].[Question_QST]
(
    [Id]            INT             IDENTITY (1,1) NOT NULL,
    [Question]      VARCHAR(MAX)    NOT NULL,
    [Id_Difficulty] INT             NOT NULL,
    CONSTRAINT [PK_QUESTION_QST] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Question_Difficulty] FOREIGN KEY ([Id_Difficulty]) REFERENCES [ref].[Difficulty_DFT]([Id])
);