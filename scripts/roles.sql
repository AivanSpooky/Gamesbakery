USE Gamesbakery;
GO

-- �������� �����
CREATE ROLE GuestRole;
CREATE ROLE UserRole;
CREATE ROLE SellerRole;
CREATE ROLE AdminRole;
GO

-- ���������� ���� ��� ���� Guest
GRANT SELECT ON Games TO GuestRole;
GRANT SELECT ON Categories TO GuestRole;
GO

-- ���������� ���� ��� ���� User
GRANT SELECT ON Games TO UserRole;
GRANT SELECT ON Categories TO UserRole;
GRANT SELECT ON Sellers TO UserRole;
GRANT SELECT, INSERT ON Orders TO UserRole;
GRANT SELECT ON OrderItems TO UserRole;
GRANT SELECT, INSERT, UPDATE ON Reviews TO UserRole;
GRANT SELECT, UPDATE ON Users TO UserRole;
GO

-- �����������: ���������� ����� ������ � ������������� ������ ���� ������
-- ������ ������������� � �������� ��� ����������� �������
CREATE VIEW UserOrders AS
SELECT o.*
FROM Orders o
WHERE o.UserID = CURRENT_USER; -- ��������������, ��� UserID ��������� � ������ ������������ � SQL Server
GO

CREATE VIEW UserOrderItems AS
SELECT oi.*
FROM OrderItems oi
JOIN Orders o ON oi.OrderID = o.OrderID
WHERE o.UserID = CURRENT_USER;
GO

CREATE VIEW UserReviews AS
SELECT r.*
FROM Reviews r
WHERE r.UserID = CURRENT_USER;
GO

CREATE VIEW UserProfile AS
SELECT u.*
FROM Users u
WHERE u.UserID = CURRENT_USER;
GO

-- �������������� ����� ��� UserRole ����� �������������
REVOKE SELECT, INSERT ON Orders FROM UserRole;
REVOKE SELECT ON OrderItems FROM UserRole;
REVOKE SELECT, UPDATE ON Reviews FROM UserRole;
REVOKE SELECT, UPDATE ON Users FROM UserRole;
GO

GRANT SELECT, INSERT ON UserOrders TO UserRole;
GRANT SELECT ON UserOrderItems TO UserRole;
GRANT SELECT, INSERT, UPDATE ON UserReviews TO UserRole;
GRANT SELECT, UPDATE ON UserProfile TO UserRole;
GO

-- ���������� ���� ��� ���� Seller
GRANT SELECT, INSERT ON Games TO SellerRole;
GRANT SELECT ON Categories TO SellerRole;
GRANT SELECT ON Orders TO SellerRole;
GRANT SELECT, UPDATE ON OrderItems TO SellerRole;
GRANT SELECT, UPDATE ON Sellers TO SellerRole;
GO

-- �����������: �������� ����� ������ � ������������� ������ ���� ������
CREATE VIEW SellerOrderItems AS
SELECT oi.*
FROM OrderItems oi
WHERE oi.SellerID = CURRENT_USER; -- ��������������, ��� SellerID ��������� � ������ ������������ � SQL Server
GO

CREATE VIEW SellerProfile AS
SELECT s.*
FROM Sellers s
WHERE s.SellerID = CURRENT_USER;
GO

-- �������������� ����� ��� SellerRole ����� �������������
REVOKE SELECT, UPDATE ON OrderItems FROM SellerRole;
REVOKE SELECT, UPDATE ON Sellers FROM SellerRole;
GO

GRANT SELECT, UPDATE ON SellerOrderItems TO SellerRole;
GRANT SELECT, UPDATE ON SellerProfile TO SellerRole;
GO

-- ���������� ���� ��� ���� Admin
GRANT SELECT, INSERT, UPDATE, DELETE ON Users TO AdminRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON Games TO AdminRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON Categories TO AdminRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON Sellers TO AdminRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON Orders TO AdminRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON OrderItems TO AdminRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON Reviews TO AdminRole;
GO

-- �������� �������� ������������� � ���������� �����
CREATE LOGIN GuestUser WITH PASSWORD = 'GuestPass123';
CREATE USER GuestUser FOR LOGIN GuestUser;
ALTER ROLE GuestRole ADD MEMBER GuestUser;
GO

CREATE LOGIN User1 WITH PASSWORD = 'UserPass123';
CREATE USER User1 FOR LOGIN User1;
ALTER ROLE UserRole ADD MEMBER User1;
GO

CREATE LOGIN Seller1 WITH PASSWORD = 'SellerPass123';
CREATE USER Seller1 FOR LOGIN Seller1;
ALTER ROLE SellerRole ADD MEMBER Seller1;
GO

CREATE LOGIN AdminUser WITH PASSWORD = 'AdminPass123';
CREATE USER AdminUser FOR LOGIN AdminUser;
ALTER ROLE AdminRole ADD MEMBER AdminUser;
GO