USE Gamesbakery;
GO
DECLARE @UserID UNIQUEIDENTIFIER = NEWID();
DECLARE @RegDate DATETIME = GETDATE();
EXEC sp_RegisterUser 
    @UserID = @UserID,
    @Name = 'Ivaan',
    @Email = 'ivaan@example.com',
    @Password = 'secure_password123',
    @Country = 'Russia',
    @RegistrationDate = @RegDate,
    @IsBlocked = 0,
    @Balance = 0.00;

USE Gamesbakery;
GO
DECLARE @UserID UNIQUEIDENTIFIER = NEWID();
DECLARE @RegDate DATETIME = GETDATE();

INSERT INTO Users (UserID, Name, Email, Password, Country, RegistrationDate, IsBlocked, Balance)
VALUES (@UserID, 'Ivan', 'ivan@example.com', 'secure_password123', 'Russia', @RegDate, 0, 0.00);