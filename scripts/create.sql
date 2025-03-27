-- Создание базы данных
CREATE DATABASE Gamesbakery;
GO
ALTER DATABASE Gamesbakery SET TRUSTWORTHY ON;
-- Использование базы данных
USE Gamesbakery;
GO

-- Создание таблицы Users без ограничений
CREATE TABLE Users (
    UserID INT IDENTITY(1,1),
    Name NVARCHAR(50),
    Email NVARCHAR(100),
    RegistrationDate DATE,
    Country NVARCHAR(50),
    Password NVARCHAR(100),
    IsBlocked BIT,  -- 0: не заблокирован, 1: заблокирован
    Balance DECIMAL(10,2)
);
GO

--COLLATE Latin1_General_BIN2 
  --      ENCRYPTED WITH (
    --        COLUMN_ENCRYPTION_KEY = CEK_AutoP2,
      --      ENCRYPTION_TYPE = Deterministic,
        --    ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
        --s)

-- Создание таблицы Sellers без ограничений
CREATE TABLE Sellers (
    SellerID INT IDENTITY(1,1),
    Name NVARCHAR(100),
    RegistrationDate DATE,
    AverageRating DECIMAL(3,2)
);
GO

-- Создание таблицы Categories без ограничений
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1),
    Name NVARCHAR(50),
    Description NVARCHAR(255)
);
GO

-- Создание таблицы Games без ограничений
CREATE TABLE Games (
    GameID INT IDENTITY(1,1),
    CategoryID INT,
    Title NVARCHAR(100),
    Price DECIMAL(10,2),
    ReleaseDate DATE,
    Description NVARCHAR(MAX),
    OriginalPublisher NVARCHAR(100),  -- Новый столбец
    IsForSale BIT  -- Новый столбец
);
GO

-- Создание таблицы Orders без ограничений
CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1),
    UserID INT,
    OrderDate DATETIME,
    TotalPrice DECIMAL(10,2),
    IsCompleted BIT,  -- 0: в обработке, 1: выполнен
    IsOverdue BIT  -- 0: не просрочен, 1: просрочен
);
GO

-- Создание таблицы OrderItems без ограничений
CREATE TABLE OrderItems (
    OrderItemID INT IDENTITY(1,1),
    OrderID INT,
    GameID INT,
    SellerID INT,  -- Новый столбец
    KeyText NVARCHAR(50)
);
GO

--COLLATE Latin1_General_BIN2 
  --      ENCRYPTED WITH (
    --        COLUMN_ENCRYPTION_KEY = CEK_AutoP3,
      --      ENCRYPTION_TYPE = Deterministic,
        --    ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
        --)

-- Создание таблицы Reviews без ограничений
CREATE TABLE Reviews (
    ReviewID INT IDENTITY(1,1),
    UserID INT,
    GameID INT,
    Comment NVARCHAR(MAX),
    StarRating INT,
    CreationDate DATETIME  -- Новый столбец
);
GO