-- This file contains SQL statements that will be executed after the build script.

-- ======================
-- Difficulty_DFT
-- ======================
IF NOT EXISTS (SELECT 1 FROM [ref].[Difficulty_DFT])
    BEGIN
        SET IDENTITY_INSERT [ref].[Difficulty_DFT] ON;

        INSERT INTO [ref].[Difficulty_DFT] ([Id], [Label]) VALUES
                                                               (1, 'Easy'),
                                                               (2, 'Medium'),
                                                               (3, 'Hard'),
                                                               (4, 'Extrem');

        SET IDENTITY_INSERT [ref].[Difficulty_DFT] OFF;
    END;


-- ======================
-- PartyType_PTT
-- ======================
IF NOT EXISTS (SELECT 1 FROM [ref].[PartyType_PTY])
    BEGIN
        SET IDENTITY_INSERT [ref].[PartyType_PTY] ON;

        INSERT INTO [ref].[PartyType_PTY] ([Id], [Label]) VALUES
                                                              (1, 'Group'),
                                                              (2, 'Ranked'),
                                                              (3, 'Solo');

        SET IDENTITY_INSERT [ref].[PartyType_PTY] OFF;
    END;
GO

-- ======================
-- Theme_THM
-- ======================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Theme_THM])
    BEGIN
        SET IDENTITY_INSERT [dbo].[Theme_THM] ON;

        INSERT INTO [dbo].[Theme_THM] ([Id], [Icon], [Label]) VALUES
                                                          (1, N'icon', N'Général'),
                                                          (2, N'icon', N'Histoire'),
                                                          (3, N'icon', N'Science'),
                                                          (4, N'icon', N'Sport'),
                                                          (5, N'icon', N'Musique'),
                                                          (6, N'icon', N'Cinéma'),
                                                          (7, N'icon', N'Art'),
                                                          (8, N'icon', N'Géographie'),
                                                          (9, N'icon', N'Technologie'),
                                                          (10, N'icon', N'Jeux Vidéo');

        SET IDENTITY_INSERT [dbo].[Theme_THM] OFF;
    END;
GO

-- ======================
-- Question_QST + Anwser_ANS
-- ======================
:r .\PostDeploy_Questions.sql

