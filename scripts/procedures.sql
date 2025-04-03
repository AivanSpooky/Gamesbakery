USE Gamesbakery;
GO

CREATE PROCEDURE sp_ManageSqlLogin
    @Action NVARCHAR(10), -- 'CREATE' или 'DELETE'
    @Name NVARCHAR(100),
    @Password NVARCHAR(100) = NULL,
    @Role NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'CREATE'
        BEGIN
            -- Проверяем, существует ли уже логин
            IF EXISTS (SELECT * FROM sys.server_principals WHERE name = @Name)
            BEGIN
                INSERT INTO ErrorLog (ErrorMessage)
                VALUES ('SQL login already exists for Name: ' + @Name);
                RETURN;
            END

            -- Экранируем одинарные кавычки в пароле
            SET @Password = REPLACE(@Password, '''', '''''');
            -- Создаем SQL-логин
            EXEC('CREATE LOGIN [' + @Name + '] WITH PASSWORD = ''' + @Password + ''', CHECK_POLICY = OFF;');
            -- Создаем пользователя базы данных
            EXEC('CREATE USER [' + @Name + '] FOR LOGIN [' + @Name + '];');
            -- Добавляем пользователя в соответствующую роль
            EXEC('ALTER ROLE ' + @Role + ' ADD MEMBER [' + @Name + '];');
        END
        ELSE IF @Action = 'DELETE'
        BEGIN
            -- Удаляем пользователя базы данных
            EXEC('IF EXISTS (SELECT * FROM sys.database_principals WHERE name = ''' + @Name + ''') DROP USER [' + @Name + '];');
            -- Удаляем SQL-логин
            EXEC('IF EXISTS (SELECT * FROM sys.server_principals WHERE name = ''' + @Name + ''') DROP LOGIN [' + @Name + '];');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO ErrorLog (ErrorMessage)
        VALUES ('Error in sp_ManageSqlLogin for ' + @Name + ': ' + ERROR_MESSAGE());
        THROW;
    END CATCH
END;
GO

USE Gamesbakery;
GO
-- Хранимая процедура для регистрации пользователя
CREATE PROCEDURE sp_RegisterUser
    @UserID UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Email NVARCHAR(255),
    @Password NVARCHAR(100),
    @Country NVARCHAR(100),
    @RegistrationDate DATETIME,
    @IsBlocked BIT,
    @Balance DECIMAL(18, 2)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO Users (UserID, Name, Email, Password, Country, RegistrationDate, IsBlocked, Balance)
        VALUES (@UserID, @Name, @Email, @Password, @Country, @RegistrationDate, @IsBlocked, @Balance);
    END TRY
    BEGIN CATCH
        INSERT INTO ErrorLog (ErrorMessage, ErrorDate)
        VALUES (ERROR_MESSAGE(), GETDATE());
        THROW;
    END CATCH
END;
GO

-- Хранимая процедура для регистрации продавца
CREATE PROCEDURE sp_RegisterSeller
    @SellerID UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Password NVARCHAR(100),
    @RegistrationDate DATETIME,
    @AvgRating FLOAT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO Sellers (SellerID, Name, Password, RegistrationDate, AverageRating)
        VALUES (@SellerID, @Name, @Password, @RegistrationDate, @AvgRating);
    END TRY
    BEGIN CATCH
        INSERT INTO ErrorLog (ErrorMessage, ErrorDate)
        VALUES (ERROR_MESSAGE(), GETDATE());
        THROW;
    END CATCH
END;
GO

DROP PROCEDURE sp_RegisterSeller;
DROP PROCEDURE sp_RegisterUser;