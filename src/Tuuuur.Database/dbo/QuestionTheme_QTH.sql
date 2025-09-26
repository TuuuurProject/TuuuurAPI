CREATE TABLE [dbo].[QuestionTheme_QTH]
(
    [Id]        INT                 IDENTITY (1,1) NOT NULL,
    [Id_Question]   INT NOT NULL,
    [Id_Theme]      INT NOT NULL,
    CONSTRAINT [PK_QUESTION_THEME_QTH] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_QuestionTheme_Question] FOREIGN KEY ([Id_Question]) REFERENCES [dbo].[Question_QST]([Id]),
    CONSTRAINT [FK_QuestionTheme_Theme] FOREIGN KEY ([Id_Theme]) REFERENCES [dbo].[Theme_THM]([Id])
)

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_QuestionTheme]
    ON [dbo].[QuestionTheme_QTH]([Id_Question], [Id_Theme] ASC);
        