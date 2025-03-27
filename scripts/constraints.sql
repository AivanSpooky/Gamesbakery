-- Используем базу данных
USE Gamesbakery;
GO

-- Добавление ограничений к таблице Users
ALTER TABLE Users
    ADD CONSTRAINT PK_Users PRIMARY KEY (UserID);
GO
ALTER TABLE Users
    ALTER COLUMN Name NVARCHAR(50) NOT NULL;
GO
ALTER TABLE Users
    ALTER COLUMN Email NVARCHAR(100) NOT NULL;
GO
ALTER TABLE Users
    ADD CONSTRAINT UQ_Users_Email UNIQUE (Email);
GO
ALTER TABLE Users
    ALTER COLUMN RegistrationDate DATE NOT NULL;
GO
ALTER TABLE Users
    ALTER COLUMN Password NVARCHAR(100) NOT NULL;
GO
ALTER TABLE Users
    ALTER COLUMN IsBlocked BIT NOT NULL;
GO
ALTER TABLE Users
    ADD CONSTRAINT DF_Users_IsBlocked DEFAULT 0 FOR IsBlocked;  -- По умолчанию: не заблокирован
GO
ALTER TABLE Users
    ALTER COLUMN Balance DECIMAL(10,2) NOT NULL;
GO
ALTER TABLE Users
    ADD CONSTRAINT DF_Users_Balance DEFAULT 0.00 FOR Balance;
GO
ALTER TABLE Users
    ADD CONSTRAINT CHK_Users_Balance CHECK (Balance >= 0);
GO

-- Добавление ограничений к таблице Sellers
ALTER TABLE Sellers
    ADD CONSTRAINT PK_Sellers PRIMARY KEY (SellerID);
GO
ALTER TABLE Sellers
    ALTER COLUMN Name NVARCHAR(100) NOT NULL;
GO
ALTER TABLE Sellers
    ALTER COLUMN RegistrationDate DATE NOT NULL;
GO
ALTER TABLE Sellers
    ADD CONSTRAINT DF_Sellers_AverageRating DEFAULT 0.00 FOR AverageRating;
GO
ALTER TABLE Sellers
    ADD CONSTRAINT CHK_Sellers_AverageRating CHECK (AverageRating >= 0 AND AverageRating <= 5);
GO

-- Добавление ограничений к таблице Categories
ALTER TABLE Categories
    ADD CONSTRAINT PK_Categories PRIMARY KEY (CategoryID);
GO
ALTER TABLE Categories
    ALTER COLUMN Name NVARCHAR(50) NOT NULL;
GO

-- Добавление ограничений к таблице Games
ALTER TABLE Games
    ADD CONSTRAINT PK_Games PRIMARY KEY (GameID);
GO
ALTER TABLE Games
    ALTER COLUMN CategoryID INT NOT NULL;
GO
ALTER TABLE Games
    ALTER COLUMN Title NVARCHAR(100) NOT NULL;
GO
ALTER TABLE Games
    ALTER COLUMN Price DECIMAL(10,2) NOT NULL;
GO
ALTER TABLE Games
    ADD CONSTRAINT CHK_Games_Price CHECK (Price >= 0);
GO
ALTER TABLE Games
    ALTER COLUMN ReleaseDate DATE NOT NULL;
GO
ALTER TABLE Games
    ALTER COLUMN OriginalPublisher NVARCHAR(100) NOT NULL;
GO
ALTER TABLE Games
    ALTER COLUMN IsForSale BIT NOT NULL;
GO
ALTER TABLE Games
    ADD CONSTRAINT DF_Games_IsForSale DEFAULT 1 FOR IsForSale;  -- По умолчанию: продаётся
GO
ALTER TABLE Games
    ADD CONSTRAINT FK_Games_Categories FOREIGN KEY (CategoryID) 
    REFERENCES Categories(CategoryID)
    ON DELETE CASCADE;
GO

-- Добавление ограничений к таблице Orders
ALTER TABLE Orders
    ADD CONSTRAINT PK_Orders PRIMARY KEY (OrderID);
GO
ALTER TABLE Orders
    ALTER COLUMN UserID INT NOT NULL;
GO
ALTER TABLE Orders
    ALTER COLUMN OrderDate DATETIME NOT NULL;
GO
ALTER TABLE Orders
    ALTER COLUMN TotalPrice DECIMAL(10,2) NOT NULL;
GO
ALTER TABLE Orders
    ADD CONSTRAINT CHK_Orders_TotalPrice CHECK (TotalPrice >= 0);
GO
ALTER TABLE Orders
    ALTER COLUMN IsCompleted BIT NOT NULL;
GO
ALTER TABLE Orders
    ADD CONSTRAINT DF_Orders_IsCompleted DEFAULT 0 FOR IsCompleted;  -- По умолчанию: в обработке
GO
ALTER TABLE Orders
    ALTER COLUMN IsOverdue BIT NOT NULL;
GO
ALTER TABLE Orders
    ADD CONSTRAINT DF_Orders_IsOverdue DEFAULT 0 FOR IsOverdue;  -- По умолчанию: не просрочен
GO
ALTER TABLE Orders
    ADD CONSTRAINT FK_Orders_Users FOREIGN KEY (UserID) 
    REFERENCES Users(UserID)
    ON DELETE CASCADE;
GO

-- Добавление ограничений к таблице OrderItems
ALTER TABLE OrderItems
    ADD CONSTRAINT PK_OrderItems PRIMARY KEY (OrderItemID);
GO
ALTER TABLE OrderItems
    ALTER COLUMN OrderID INT NOT NULL;
GO
ALTER TABLE OrderItems
    ALTER COLUMN GameID INT NOT NULL;
GO
ALTER TABLE OrderItems
    ALTER COLUMN SellerID INT NOT NULL;
GO
ALTER TABLE OrderItems
    ADD CONSTRAINT FK_OrderItems_Sellers FOREIGN KEY (SellerID) 
    REFERENCES Sellers(SellerID)
    ON DELETE CASCADE;
GO
CREATE UNIQUE NONCLUSTERED INDEX UQ_OrderItems_KeyText
    ON dbo.OrderItems (KeyText)
    WHERE KeyText IS NOT NULL;
GO
ALTER TABLE OrderItems
    ADD CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderID) 
    REFERENCES Orders(OrderID)
    ON DELETE CASCADE;
GO
ALTER TABLE OrderItems
    ADD CONSTRAINT FK_OrderItems_Games FOREIGN KEY (GameID) 
    REFERENCES Games(GameID)
    ON DELETE CASCADE;
GO

-- Добавление ограничений к таблице Reviews
ALTER TABLE Reviews
    ADD CONSTRAINT PK_Reviews PRIMARY KEY (ReviewID);
GO
ALTER TABLE Reviews
    ALTER COLUMN UserID INT NOT NULL;
GO
ALTER TABLE Reviews
    ALTER COLUMN GameID INT NOT NULL;
GO
ALTER TABLE Reviews
    ALTER COLUMN Comment NVARCHAR(MAX) NOT NULL;
GO
ALTER TABLE Reviews
    ALTER COLUMN StarRating INT NOT NULL;
GO
ALTER TABLE Reviews
    ALTER COLUMN CreationDate DATETIME NOT NULL;
GO
ALTER TABLE Reviews
    ADD CONSTRAINT CHK_Reviews_StarRating CHECK (StarRating >= 1 AND StarRating <= 5);
GO
ALTER TABLE Reviews
    ADD CONSTRAINT FK_Reviews_Users FOREIGN KEY (UserID) 
    REFERENCES Users(UserID)
    ON DELETE CASCADE;
GO
ALTER TABLE Reviews
    ADD CONSTRAINT FK_Reviews_Games FOREIGN KEY (GameID) 
    REFERENCES Games(GameID)
    ON DELETE CASCADE;
GO