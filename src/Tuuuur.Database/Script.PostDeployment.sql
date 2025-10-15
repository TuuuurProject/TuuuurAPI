-- This file contains SQL statements that will be executed after the build script.

-- ======================
-- Difficulty_DFT
-- ======================
IF NOT EXISTS (SELECT 1 FROM [ref].[Difficulty_DFT])
    BEGIN
        SET IDENTITY_INSERT [ref].[Difficulty_DFT] ON;

        INSERT INTO [ref].[Difficulty_DFT] ([Id], [Label]) VALUES
                                                               (1, N'Facile'),
                                                               (2, N'Moyen'),
                                                               (3, N'Difficile'),
                                                               (4, N'Extrême');

        SET IDENTITY_INSERT [ref].[Difficulty_DFT] OFF;
    END;


-- ======================
-- PartyType_PTT
-- ======================
IF NOT EXISTS (SELECT 1 FROM [ref].[PartyType_PTY])
    BEGIN
        SET IDENTITY_INSERT [ref].[PartyType_PTY] ON;

        INSERT INTO [ref].[PartyType_PTY] ([Id], [Label]) VALUES
                                                              (1, N'Groupe'),
                                                              (2, N'Classée'),
                                                              (3, N'Solo');

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
                                                          (1, N'wand-magic-sparkles', N'Général'),
                                                          (2, N'building-columns', N'Histoire'),
                                                          (3, N'flask', N'Science'),
                                                          (4, N'medal', N'Sport'),
                                                          (5, N'music', N'Musique'),
                                                          (6, N'film', N'Cinéma'),
                                                          (7, N'palette', N'Art'),
                                                          (8, N'globe', N'Géographie'),
                                                          (9, N'laptop-code', N'Technologie'),
                                                          (10, N'gamepad', N'Jeux Vidéo');

        SET IDENTITY_INSERT [dbo].[Theme_THM] OFF;
    END;
GO