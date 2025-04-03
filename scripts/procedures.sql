USE Gamesbakery;
GO

CREATE PROCEDURE sp_ManageSqlLogin
    @Action NVARCHAR(10), -- 'CREATE' ��� 'DELETE'
    @Name NVARCHAR(100),
    @Password NVARCHAR(100) = NULL,
    @Role NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'CREATE'
        BEGIN
            -- ���������, ���������� �� ��� �����
            IF EXISTS (SELECT * FROM sys.server_principals WHERE name = @Name)
            BEGIN
                INSERT INTO ErrorLog (ErrorMessage)
                VALUES ('SQL login already exists for Name: ' + @Name);
                RETURN;
            END

            -- ���������� ��������� ������� � ������
            SET @Password = REPLACE(@Password, '''', '''''');
            -- ������� SQL-�����
            EXEC('CREATE LOGIN [' + @Name + '] WITH PASSWORD = ''' + @Password + ''', CHECK_POLICY = OFF;');
            -- ������� ������������ ���� ������
            EXEC('CREATE USER [' + @Name + '] FOR LOGIN [' + @Name + '];');
            -- ��������� ������������ � ��������������� ����
            EXEC('ALTER ROLE ' + @Role + ' ADD MEMBER [' + @Name + '];');
        END
        ELSE IF @Action = 'DELETE'
        BEGIN
            -- ������� ������������ ���� ������
            EXEC('IF EXISTS (SELECT * FROM sys.database_principals WHERE name = ''' + @Name + ''') DROP USER [' + @Name + '];');
            -- ������� SQL-�����
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
-- �������� ��������� ��� ����������� ������������
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

-- �������� ��������� ��� ����������� ��������
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