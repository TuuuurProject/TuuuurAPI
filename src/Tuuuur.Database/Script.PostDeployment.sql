-- This file contains SQL statements that will be executed after the build script.
-- ======================
-- Difficulty_DFT
-- ======================
SET IDENTITY_INSERT [ref].[Difficulty_DFT] ON;

INSERT INTO [ref].[Difficulty_DFT] ([Id], [Label]) VALUES
                                                       (1, 'Easy'),
                                                       (2, 'Medium'),
                                                       (3, 'Hard'),
                                                       (4, 'Extrem');

SET IDENTITY_INSERT [ref].[Difficulty_DFT] OFF;


-- ======================
-- PartyType_PTT
-- ======================
SET IDENTITY_INSERT [ref].[PartyType_PTT] ON;

INSERT INTO [ref].[PartyType_PTT] ([Id], [Label]) VALUES
                                                      (1, 'Group'),
                                                      (2, 'Ranked'),
                                                      (3, 'Solo');

SET IDENTITY_INSERT [ref].[PartyType_PTT] OFF;
