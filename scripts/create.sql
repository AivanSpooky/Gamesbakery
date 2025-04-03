-- �������� ���� ������
CREATE DATABASE Gamesbakery;
GO
ALTER DATABASE Gamesbakery SET TRUSTWORTHY ON;
-- ������������� ���� ������
USE Gamesbakery;
GO

-- �������� ������� Users ��� �����������
CREATE TABLE Users (
    UserID UNIQUEIDENTIFIER,
    Name NVARCHAR(50),
    Email NVARCHAR(100),
    RegistrationDate DATE,
    Country NVARCHAR(300),
    Password NVARCHAR(100),
    IsBlocked BIT,  -- 0: �� ������������, 1: ������������
    Balance DECIMAL(10,2)
);
GO

-- �������� ������� Sellers ��� �����������
CREATE TABLE Sellers (
    SellerID UNIQUEIDENTIFIER,
    Name NVARCHAR(100),
    RegistrationDate DATE,
    AverageRating DECIMAL(3,2),
	Password NVARCHAR(100)
);
GO

-- �������� ������� Categories ��� �����������
CREATE TABLE Categories (
    CategoryID UNIQUEIDENTIFIER,
    Name NVARCHAR(50),
    Description NVARCHAR(255)
);
GO

-- �������� ������� Games ��� �����������
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

-- �������� ������� Orders ��� �����������
CREATE TABLE Orders (
    OrderID UNIQUEIDENTIFIER,
    UserID UNIQUEIDENTIFIER,
    OrderDate DATETIME,
    TotalPrice DECIMAL(10,2),
    IsCompleted BIT,  -- 0: � ���������, 1: ��������
    IsOverdue BIT  -- 0: �� ���������, 1: ���������
);
GO

-- �������� ������� OrderItems ��� �����������
CREATE TABLE OrderItems (
    OrderItemID UNIQUEIDENTIFIER,
    OrderID UNIQUEIDENTIFIER,
    GameID UNIQUEIDENTIFIER,
    SellerID UNIQUEIDENTIFIER,
    KeyText NVARCHAR(50)
);
GO

-- �������� ������� Reviews ��� �����������
CREATE TABLE Reviews (
    ReviewID UNIQUEIDENTIFIER,
    UserID UNIQUEIDENTIFIER,
    GameID UNIQUEIDENTIFIER,
    Comment NVARCHAR(MAX),
    StarRating INT,
    CreationDate DATETIME
);
GO