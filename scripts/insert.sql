USE Gamesbakery;
GO

-- Очистка временных таблиц, если они существуют
SELECT * 
FROM tempdb.sys.tables 
WHERE name LIKE '#TempTable%';
DROP TABLE IF EXISTS #TempTable;
DROP TABLE IF EXISTS #TempTable2;
GO

-- Импорт данных в таблицу Users
CREATE TABLE #TempTable
(
    UserID UNIQUEIDENTIFIER,  -- Изменено с INT на UNIQUEIDENTIFIER
    Name NVARCHAR(50),
    Email NVARCHAR(100),
    RegistrationDate DATE,
    Country NVARCHAR(50),
    Password NVARCHAR(100),
    Status NVARCHAR(20),  -- Временное поле для строкового статуса
    Balance DECIMAL(10,2)
);
BULK INSERT #TempTable
FROM 'C:\Data\Gamesbakery\Users.csv'
WITH
(
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    KEEPNULLS
);
INSERT INTO Users (UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance)
SELECT 
    UserID, 
    Name, 
    Email, 
    RegistrationDate, 
    Country, 
    Password, 
    CASE 
        WHEN Status = 'заблокирован' THEN 1
        ELSE 0  -- По умолчанию: не заблокирован
    END,
    Balance
FROM #TempTable;
DROP TABLE #TempTable;
SELECT * FROM Users;
GO

-- Импорт данных в таблицу Sellers
CREATE TABLE #TempTable
(
    SellerID UNIQUEIDENTIFIER,  -- Изменено с INT на UNIQUEIDENTIFIER
    Name NVARCHAR(100),
    RegistrationDate DATE,
    AverageRating DECIMAL(3,2)
);
BULK INSERT #TempTable
FROM 'C:\Data\Gamesbakery\Sellers.csv'
WITH
(
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    KEEPNULLS
);
INSERT INTO Sellers (SellerID, Name, RegistrationDate, AverageRating)
SELECT SellerID, Name, RegistrationDate, AverageRating
FROM #TempTable;
DROP TABLE #TempTable;
SELECT * FROM Sellers;
GO

-- Импорт данных в таблицу Categories
CREATE TABLE #TempTable
(
    CategoryID UNIQUEIDENTIFIER,  -- Изменено с INT на UNIQUEIDENTIFIER
    Name NVARCHAR(50),
    Description NVARCHAR(255)
);
BULK INSERT #TempTable
FROM 'C:\Data\Gamesbakery\Categories.csv'
WITH
(
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    KEEPNULLS
);
INSERT INTO Categories (CategoryID, Name, Description)
SELECT CategoryID, Name, Description
FROM #TempTable;
DROP TABLE #TempTable;
SELECT * FROM Categories;
GO

-- Импорт данных в таблицу Games
CREATE TABLE #TempTable
(
    GameID UNIQUEIDENTIFIER,  -- Изменено с INT на UNIQUEIDENTIFIER
    CategoryID UNIQUEIDENTIFIER,  -- Изменено с INT на UNIQUEIDENTIFIER
    Title NVARCHAR(100),
    Price DECIMAL(10,2),
    ReleaseDate DATE,
    Description NVARCHAR(MAX),
    OriginalPublisher NVARCHAR(100),
    IsForSale BIT
);
BULK INSERT #TempTable
FROM 'C:\Data\Gamesbakery\Games.csv'
WITH
(
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    KEEPNULLS
);
INSERT INTO Games (GameID, CategoryID, Title, Price, ReleaseDate, Description, OriginalPublisher, IsForSale)
SELECT 
    GameID, CategoryID, Title, Price, ReleaseDate, Description, OriginalPublisher, IsForSale
FROM #TempTable;
DROP TABLE #TempTable;
SELECT * FROM Games;
GO

-- Импорт данных в таблицу Orders
CREATE TABLE #TempTable
(
    OrderID UNIQUEIDENTIFIER,  -- Изменено с INT на UNIQUEIDENTIFIER
    UserID UNIQUEIDENTIFIER,  -- Изменено с INT на UNIQUEIDENTIFIER
    OrderDate DATETIME,
    TotalPrice DECIMAL(10,2),
    Status NVARCHAR(20),  -- Временное поле для строкового статуса
    IsOverdue BIT
);
BULK INSERT #TempTable
FROM 'C:\Data\Gamesbakery\Orders.csv'
WITH
(
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    KEEPNULLS
);
INSERT INTO Orders (OrderID, UserID, OrderDate, TotalPrice, IsCompleted, IsOverdue)
SELECT 
    OrderID, 
    UserID, 
    OrderDate, 
    TotalPrice, 
    CASE 
        WHEN Status = 'выполнен' THEN 1
        ELSE 0  -- По умолчанию: в обработке
    END,
    IsOverdue
FROM #TempTable;
DROP TABLE #TempTable;
SELECT * FROM Orders;
GO

-- Импорт данных в таблицу OrderItems
CREATE TABLE #TempTable
(
    OrderItemID UNIQUEIDENTIFIER,  -- Изменено с INT на UNIQUEIDENTIFIER
    OrderID UNIQUEIDENTIFIER,
    GameID UNIQUEIDENTIFIER,
    SellerID UNIQUEIDENTIFIER,
    KeyText NVARCHAR(50)
);
BULK INSERT #TempTable
FROM 'C:\Data\Gamesbakery\OrderItems.csv'
WITH
(
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    KEEPNULLS
);
INSERT INTO OrderItems (OrderItemID, OrderID, GameID, SellerID, KeyText)
SELECT 
    OrderItemID, OrderID, GameID, SellerID, KeyText
FROM #TempTable;
DROP TABLE #TempTable;
SELECT * FROM OrderItems;
GO

-- Импорт данных в таблицу Reviews
CREATE TABLE #TempTable
(
    ReviewID UNIQUEIDENTIFIER,  -- Изменено с INT на UNIQUEIDENTIFIER
    UserID UNIQUEIDENTIFIER,
    GameID UNIQUEIDENTIFIER,
    Comment NVARCHAR(MAX),
    StarRating INT,
    CreationDate DATETIME
);
BULK INSERT #TempTable
FROM 'C:\Data\Gamesbakery\Reviews.csv'
WITH
(
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    KEEPNULLS
);
INSERT INTO Reviews (ReviewID, UserID, GameID, Comment, StarRating, CreationDate)
SELECT 
    ReviewID, UserID, GameID, Comment, StarRating, CreationDate
FROM #TempTable;
DROP TABLE #TempTable;
SELECT * FROM Reviews;
GO