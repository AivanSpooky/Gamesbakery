-- Создание базы данных
CREATE DATABASE Gamesbakery;
GO
ALTER DATABASE Gamesbakery SET TRUSTWORTHY ON;
-- Использование базы данных
USE Gamesbakery;
GO

-- Создание таблицы Users без ограничений
CREATE TABLE Users (
    UserID UNIQUEIDENTIFIER,
    Name NVARCHAR(50),
    Email NVARCHAR(100),
    RegistrationDate DATE,
    Country NVARCHAR(300),
    Password NVARCHAR(100),
    IsBlocked BIT,  -- 0: не заблокирован, 1: заблокирован
    Balance DECIMAL(10,2)
);
GO

-- Создание таблицы Sellers без ограничений
CREATE TABLE Sellers (
    SellerID UNIQUEIDENTIFIER,
    Name NVARCHAR(100),
    RegistrationDate DATE,
    AverageRating DECIMAL(3,2),
	Password NVARCHAR(100)
);
GO

-- Создание таблицы Categories без ограничений
CREATE TABLE Categories (
    CategoryID UNIQUEIDENTIFIER,
    Name NVARCHAR(50),
    Description NVARCHAR(255)
);
GO

-- Создание таблицы Games без ограничений
CREATE TABLE Games (
    GameID UNIQUEIDENTIFIER,
    CategoryID UNIQUEIDENTIFIER,
    Title NVARCHAR(100),
    Price DECIMAL(10,2),
    ReleaseDate DATE,
    Description NVARCHAR(MAX),
    OriginalPublisher NVARCHAR(100),
    IsForSale BIT
);
GO

-- Создание таблицы Orders без ограничений
CREATE TABLE Orders (
    OrderID UNIQUEIDENTIFIER,
    UserID UNIQUEIDENTIFIER,
    OrderDate DATETIME,
    TotalPrice DECIMAL(10,2),
    IsCompleted BIT,  -- 0: в обработке, 1: выполнен
    IsOverdue BIT  -- 0: не просрочен, 1: просрочен
);
GO

-- Создание таблицы OrderItems без ограничений
CREATE TABLE OrderItems (
    OrderItemID UNIQUEIDENTIFIER,
    OrderID UNIQUEIDENTIFIER,
    GameID UNIQUEIDENTIFIER,
    SellerID UNIQUEIDENTIFIER,
    KeyText NVARCHAR(50)
);
GO

-- Создание таблицы Reviews без ограничений
CREATE TABLE Reviews (
    ReviewID UNIQUEIDENTIFIER,
    UserID UNIQUEIDENTIFIER,
    GameID UNIQUEIDENTIFIER,
    Comment NVARCHAR(MAX),
    StarRating INT,
    CreationDate DATETIME
);
GO