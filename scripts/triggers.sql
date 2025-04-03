USE Gamesbakery;
GO

-- ������� ������� ��� ����� ������
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ErrorLog')
CREATE TABLE ErrorLog (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ErrorMessage NVARCHAR(MAX),
    ErrorDate DATETIME DEFAULT GETDATE()
);
GO

-- ������� ��� ������� Users
CREATE OR ALTER TRIGGER trg_Users_AfterInsertDelete
ON Users
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- ��������� �������
    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        DECLARE @Name NVARCHAR(50);
        DECLARE @Password NVARCHAR(100);
        DECLARE @UserID UNIQUEIDENTIFIER;
        DECLARE @Role NVARCHAR(50);

        -- ������ ��� ��������� ���� ����������� �������
        DECLARE user_cursor CURSOR FOR
        SELECT Name, Password, UserID
        FROM inserted;

        OPEN user_cursor;
        FETCH NEXT FROM user_cursor INTO @Name, @Password, @UserID;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- ���������, �������� �� ��� ������ ������������ (Admin)
            IF (SELECT COUNT(*) FROM Users) = 1
                SET @Role = 'AdminRole';
            ELSE
                SET @Role = 'UserRole';

            BEGIN TRY
                -- ���������, ���������� �� ��� �����
                IF EXISTS (SELECT * FROM sys.server_principals WHERE name = @Name)
                BEGIN
                    INSERT INTO ErrorLog (ErrorMessage)
                    VALUES ('SQL login already exists for Name: ' + @Name);
                END
                ELSE
                BEGIN
                    -- ���������� ��������� ������� � ������
                    SET @Password = REPLACE(@Password, '''', '''''');
                    -- ������� SQL-�����, ��������� Name
                    EXEC('CREATE LOGIN [' + @Name + '] WITH PASSWORD = ''' + @Password + ''', CHECK_POLICY = OFF;');
                    -- ������� ������������ ���� ������
                    EXEC('CREATE USER [' + @Name + '] FOR LOGIN [' + @Name + '];');
                    -- ��������� ������������ � ��������������� ����
                    EXEC('ALTER ROLE ' + @Role + ' ADD MEMBER [' + @Name + '];');
                END
            END TRY
            BEGIN CATCH
                -- �������� ������ � �������
                INSERT INTO ErrorLog (ErrorMessage)
                VALUES ('Error creating SQL user for ' + @Name + ': ' + ERROR_MESSAGE());
            END CATCH

            FETCH NEXT FROM user_cursor INTO @Name, @Password, @UserID;
        END

        CLOSE user_cursor;
        DEALLOCATE user_cursor;
    END

    -- ��������� ��������
    IF EXISTS (SELECT * FROM deleted)
    BEGIN
        DECLARE @NameToDelete NVARCHAR(50);

        -- ������ ��� ��������� ���� ��������� �������
        DECLARE delete_cursor CURSOR FOR
        SELECT Name
        FROM deleted;

        OPEN delete_cursor;
        FETCH NEXT FROM delete_cursor INTO @NameToDelete;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            BEGIN TRY
                -- ������� ������������ ���� ������
                EXEC('IF EXISTS (SELECT * FROM sys.database_principals WHERE name = ''' + @NameToDelete + ''') DROP USER [' + @NameToDelete + '];');
                -- ������� SQL-�����
                EXEC('IF EXISTS (SELECT * FROM sys.server_principals WHERE name = ''' + @NameToDelete + ''') DROP LOGIN [' + @NameToDelete + '];');
            END TRY
            BEGIN CATCH
                -- �������� ������ � �������
                INSERT INTO ErrorLog (ErrorMessage)
                VALUES ('Error dropping SQL user ' + @NameToDelete + ': ' + ERROR_MESSAGE());
            END CATCH

            FETCH NEXT FROM delete_cursor INTO @NameToDelete;
        END

        CLOSE delete_cursor;
        DEALLOCATE delete_cursor;
    END
END
GO

-- ������� ��� ������� Sellers
CREATE OR ALTER TRIGGER trg_Sellers_AfterInsertDelete
ON Sellers
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- ��������� �������
    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        DECLARE @Name NVARCHAR(100);
        DECLARE @Password NVARCHAR(100);

        -- ������ ��� ��������� ���� ����������� �������
        DECLARE seller_cursor CURSOR FOR
        SELECT Name, Password
        FROM inserted;

        OPEN seller_cursor;
        FETCH NEXT FROM seller_cursor INTO @Name, @Password;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            BEGIN TRY
                -- ���������, ���������� �� ��� �����
                IF EXISTS (SELECT * FROM sys.server_principals WHERE name = @Name)
                BEGIN
                    INSERT INTO ErrorLog (ErrorMessage)
                    VALUES ('SQL login already exists for Name: ' + @Name);
                END
                ELSE
                BEGIN
                    -- ���������� ��������� ������� � ������
                    SET @Password = REPLACE(@Password, '''', '''''');
                    -- ������� SQL-�����, ��������� Name
                    EXEC('CREATE LOGIN [' + @Name + '] WITH PASSWORD = ''' + @Password + ''', CHECK_POLICY = OFF;');
                    -- ������� ������������ ���� ������
                    EXEC('CREATE USER [' + @Name + '] FOR LOGIN [' + @Name + '];');
                    -- ��������� ������������ � ���� SellerRole
                    EXEC('ALTER ROLE SellerRole ADD MEMBER [' + @Name + '];');
                END
            END TRY
            BEGIN CATCH
                -- �������� ������ � �������
                INSERT INTO ErrorLog (ErrorMessage)
                VALUES ('Error creating SQL user for ' + @Name + ': ' + ERROR_MESSAGE());
            END CATCH

            FETCH NEXT FROM seller_cursor INTO @Name, @Password;
        END

        CLOSE seller_cursor;
        DEALLOCATE seller_cursor;
    END

    -- ��������� ��������
    IF EXISTS (SELECT * FROM deleted)
    BEGIN
        DECLARE @NameToDelete NVARCHAR(100);

        -- ������ ��� ��������� ���� ��������� �������
        DECLARE delete_cursor CURSOR FOR
        SELECT Name
        FROM deleted;

        OPEN delete_cursor;
        FETCH NEXT FROM delete_cursor INTO @NameToDelete;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            BEGIN TRY
                -- ������� ������������ ���� ������
                EXEC('IF EXISTS (SELECT * FROM sys.database_principals WHERE name = ''' + @NameToDelete + ''') DROP USER [' + @NameToDelete + '];');
                -- ������� SQL-�����
                EXEC('IF EXISTS (SELECT * FROM sys.server_principals WHERE name = ''' + @NameToDelete + ''') DROP LOGIN [' + @NameToDelete + '];');
            END TRY
            BEGIN CATCH
                -- �������� ������ � �������
                INSERT INTO ErrorLog (ErrorMessage)
                VALUES ('Error dropping SQL user ' + @NameToDelete + ': ' + ERROR_MESSAGE());
            END CATCH

            FETCH NEXT FROM delete_cursor INTO @NameToDelete;
        END

        CLOSE delete_cursor;
        DEALLOCATE delete_cursor;
    END
END
GO

SELECT * FROM ErrorLog;