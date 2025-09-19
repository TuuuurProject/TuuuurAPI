CREATE TABLE [ref].[Difficulty_DFT]
(
    [Code]              VARCHAR(10)         IDENTITY (1, 1) NOT NULL,
    [Label]             VARCHAR(50)         NOT NULL,
    CONSTRAINT [PK_Difficulty_DFT] PRIMARY KEY CLUSTERED ([Code] ASC),
)