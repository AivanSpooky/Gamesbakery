USE Gamesbakery;
GO

-- Очистка временных таблиц, если они существуют
SELECT * 
FROM tempdb.sys.tables 
WHERE name LIKE '#TempTable%';
DROP TABLE IF EXISTS #TempTable;
DROP TABLE IF EXISTS #TempTable2;
SET IDENTITY_INSERT Users OFF;
SET IDENTITY_INSERT Games OFF;
SET IDENTITY_INSERT Sellers OFF;
SET IDENTITY_INSERT Categories OFF;
SET IDENTITY_INSERT Orders OFF;
SET IDENTITY_INSERT OrderItems OFF;
SET IDENTITY_INSERT Reviews OFF;
GO

-- Импорт данных в таблицу Users
CREATE TABLE #TempTable
(
    UserID INT IDENTITY(1,1),
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
SET IDENTITY_INSERT Users ON;
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
SET IDENTITY_INSERT Users OFF;
DROP TABLE #TempTable;
SELECT * FROM Users;
GO

-- Импорт данных в таблицу Sellers
BULK INSERT Sellers
FROM 'C:\Data\Gamesbakery\Sellers.csv'
WITH
(
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    KEEPNULLS
);
SELECT * FROM Sellers;
GO

-- Импорт данных в таблицу Categories
BULK INSERT Categories
FROM 'C:\Data\Gamesbakery\Categories.csv'
WITH
(
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    KEEPNULLS
);
SELECT * FROM Categories;
GO

-- Импорт данных в таблицу Games
CREATE TABLE #TempTable2
(
    GameID INT IDENTITY(1,1),
    CategoryID INT,
    Title NVARCHAR(100),
    Price DECIMAL(10,2),
    ReleaseDate DATE,
    Description NVARCHAR(MAX),
    OriginalPublisher NVARCHAR(100),
    IsForSale BIT
);
BULK INSERT #TempTable2
FROM 'C:\Data\Gamesbakery\Games.csv'
WITH
(
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    KEEPNULLS
);
SET IDENTITY_INSERT Games ON;
INSERT INTO Games (GameID, CategoryID, Title, Price, ReleaseDate, Description, OriginalPublisher, IsForSale)
SELECT 
    GameID, CategoryID, Title, Price, ReleaseDate, Description, OriginalPublisher, IsForSale
FROM #TempTable2;
SET IDENTITY_INSERT Games OFF;
DROP TABLE #TempTable2;
SELECT * FROM Games;
GO

-- Импорт данных в таблицу Orders
CREATE TABLE #TempTable
(
    OrderID INT IDENTITY(1,1),
    UserID INT,
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
SET IDENTITY_INSERT Orders ON;
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
SET IDENTITY_INSERT Orders OFF;
DROP TABLE #TempTable;
SELECT * FROM Orders;
GO

-- Импорт данных в таблицу OrderItems
CREATE TABLE #TempTable
(
    OrderItemID INT IDENTITY(1,1),
    OrderID INT,
    GameID INT,
    SellerID INT,
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
SET IDENTITY_INSERT OrderItems ON;
INSERT INTO OrderItems (OrderItemID, OrderID, GameID, SellerID, KeyText)
SELECT 
    OrderItemID, OrderID, GameID, SellerID, KeyText
FROM #TempTable;
SET IDENTITY_INSERT OrderItems OFF;
DROP TABLE #TempTable;
SELECT * FROM OrderItems;
GO

-- Импорт данных в таблицу Reviews
CREATE TABLE #TempTable
(
    ReviewID INT IDENTITY(1,1),
    UserID INT,
    GameID INT,
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
SET IDENTITY_INSERT Reviews ON;
INSERT INTO Reviews (ReviewID, UserID, GameID, Comment, StarRating, CreationDate)
SELECT 
    ReviewID, UserID, GameID, Comment, StarRating, CreationDate
FROM #TempTable;
SET IDENTITY_INSERT Reviews OFF;
DROP TABLE #TempTable;
SELECT * FROM Reviews;
GO