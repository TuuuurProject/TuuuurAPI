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
IF NOT EXISTS (SELECT 1 FROM [ref].[PartyType_PTT])
    BEGIN
        SET IDENTITY_INSERT [ref].[PartyType_PTT] ON;

        INSERT INTO [ref].[PartyType_PTT] ([Id], [Label]) VALUES
                                                              (1, 'Group'),
                                                              (2, 'Ranked'),
                                                              (3, 'Solo');

        SET IDENTITY_INSERT [ref].[PartyType_PTT] OFF;
    END;