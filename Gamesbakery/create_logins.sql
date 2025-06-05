USE master;
GO
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'Zulauf, Prosacco and Reilly')
    CREATE LOGIN [Zulauf, Prosacco and Reilly] WITH PASSWORD = 'TempPass123!';
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'Zulauf Group')
    CREATE LOGIN [Zulauf Group] WITH PASSWORD = 'TempPass123!';
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'Zora59')
    CREATE LOGIN [Zora59] WITH PASSWORD = 'TempPass123!';
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'Abbott - Bosco')
    CREATE LOGIN [Abbott - Bosco] WITH PASSWORD = 'TempPass123!';
-- Добавь другие логины, если знаешь их
GO