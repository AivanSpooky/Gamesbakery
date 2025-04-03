USE Gamesbakery;
GO

-- Создаем таблицу для логов ошибок
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ErrorLog')
CREATE TABLE ErrorLog (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ErrorMessage NVARCHAR(MAX),
    ErrorDate DATETIME DEFAULT GETDATE()
);
GO

-- Триггер для таблицы Users
CREATE OR ALTER TRIGGER trg_Users_AfterInsertDelete
ON Users
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Обработка вставки
    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        DECLARE @Name NVARCHAR(50);
        DECLARE @Password NVARCHAR(100);
        DECLARE @UserID UNIQUEIDENTIFIER;
        DECLARE @Role NVARCHAR(50);

        -- Курсор для обработки всех вставленных записей
        DECLARE user_cursor CURSOR FOR
        SELECT Name, Password, UserID
        FROM inserted;

        OPEN user_cursor;
        FETCH NEXT FROM user_cursor INTO @Name, @Password, @UserID;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Проверяем, является ли это первый пользователь (Admin)
            IF (SELECT COUNT(*) FROM Users) = 1
                SET @Role = 'AdminRole';
            ELSE
                SET @Role = 'UserRole';

            BEGIN TRY
                -- Проверяем, существует ли уже логин
                IF EXISTS (SELECT * FROM sys.server_principals WHERE name = @Name)
                BEGIN
                    INSERT INTO ErrorLog (ErrorMessage)
                    VALUES ('SQL login already exists for Name: ' + @Name);
                END
                ELSE
                BEGIN
                    -- Экранируем одинарные кавычки в пароле
                    SET @Password = REPLACE(@Password, '''', '''''');
                    -- Создаем SQL-логин, используя Name
                    EXEC('CREATE LOGIN [' + @Name + '] WITH PASSWORD = ''' + @Password + ''', CHECK_POLICY = OFF;');
                    -- Создаем пользователя базы данных
                    EXEC('CREATE USER [' + @Name + '] FOR LOGIN [' + @Name + '];');
                    -- Добавляем пользователя в соответствующую роль
                    EXEC('ALTER ROLE ' + @Role + ' ADD MEMBER [' + @Name + '];');
                END
            END TRY
            BEGIN CATCH
                -- Логируем ошибку в таблицу
                INSERT INTO ErrorLog (ErrorMessage)
                VALUES ('Error creating SQL user for ' + @Name + ': ' + ERROR_MESSAGE());
            END CATCH

            FETCH NEXT FROM user_cursor INTO @Name, @Password, @UserID;
        END

        CLOSE user_cursor;
        DEALLOCATE user_cursor;
    END

    -- Обработка удаления
    IF EXISTS (SELECT * FROM deleted)
    BEGIN
        DECLARE @NameToDelete NVARCHAR(50);

        -- Курсор для обработки всех удаленных записей
        DECLARE delete_cursor CURSOR FOR
        SELECT Name
        FROM deleted;

        OPEN delete_cursor;
        FETCH NEXT FROM delete_cursor INTO @NameToDelete;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            BEGIN TRY
                -- Удаляем пользователя базы данных
                EXEC('IF EXISTS (SELECT * FROM sys.database_principals WHERE name = ''' + @NameToDelete + ''') DROP USER [' + @NameToDelete + '];');
                -- Удаляем SQL-логин
                EXEC('IF EXISTS (SELECT * FROM sys.server_principals WHERE name = ''' + @NameToDelete + ''') DROP LOGIN [' + @NameToDelete + '];');
            END TRY
            BEGIN CATCH
                -- Логируем ошибку в таблицу
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

-- Триггер для таблицы Sellers
CREATE OR ALTER TRIGGER trg_Sellers_AfterInsertDelete
ON Sellers
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Обработка вставки
    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        DECLARE @Name NVARCHAR(100);
        DECLARE @Password NVARCHAR(100);

        -- Курсор для обработки всех вставленных записей
        DECLARE seller_cursor CURSOR FOR
        SELECT Name, Password
        FROM inserted;

        OPEN seller_cursor;
        FETCH NEXT FROM seller_cursor INTO @Name, @Password;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            BEGIN TRY
                -- Проверяем, существует ли уже логин
                IF EXISTS (SELECT * FROM sys.server_principals WHERE name = @Name)
                BEGIN
                    INSERT INTO ErrorLog (ErrorMessage)
                    VALUES ('SQL login already exists for Name: ' + @Name);
                END
                ELSE
                BEGIN
                    -- Экранируем одинарные кавычки в пароле
                    SET @Password = REPLACE(@Password, '''', '''''');
                    -- Создаем SQL-логин, используя Name
                    EXEC('CREATE LOGIN [' + @Name + '] WITH PASSWORD = ''' + @Password + ''', CHECK_POLICY = OFF;');
                    -- Создаем пользователя базы данных
                    EXEC('CREATE USER [' + @Name + '] FOR LOGIN [' + @Name + '];');
                    -- Добавляем пользователя в роль SellerRole
                    EXEC('ALTER ROLE SellerRole ADD MEMBER [' + @Name + '];');
                END
            END TRY
            BEGIN CATCH
                -- Логируем ошибку в таблицу
                INSERT INTO ErrorLog (ErrorMessage)
                VALUES ('Error creating SQL user for ' + @Name + ': ' + ERROR_MESSAGE());
            END CATCH

            FETCH NEXT FROM seller_cursor INTO @Name, @Password;
        END

        CLOSE seller_cursor;
        DEALLOCATE seller_cursor;
    END

    -- Обработка удаления
    IF EXISTS (SELECT * FROM deleted)
    BEGIN
        DECLARE @NameToDelete NVARCHAR(100);

        -- Курсор для обработки всех удаленных записей
        DECLARE delete_cursor CURSOR FOR
        SELECT Name
        FROM deleted;

        OPEN delete_cursor;
        FETCH NEXT FROM delete_cursor INTO @NameToDelete;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            BEGIN TRY
                -- Удаляем пользователя базы данных
                EXEC('IF EXISTS (SELECT * FROM sys.database_principals WHERE name = ''' + @NameToDelete + ''') DROP USER [' + @NameToDelete + '];');
                -- Удаляем SQL-логин
                EXEC('IF EXISTS (SELECT * FROM sys.server_principals WHERE name = ''' + @NameToDelete + ''') DROP LOGIN [' + @NameToDelete + '];');
            END TRY
            BEGIN CATCH
                -- Логируем ошибку в таблицу
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