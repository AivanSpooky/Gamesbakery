USE Gamesbakery;
GO

-- Создание ролей
CREATE ROLE GuestRole;
CREATE ROLE UserRole;
CREATE ROLE SellerRole;
CREATE ROLE AdminRole;
GO

-- Назначение прав для роли Guest
GRANT SELECT ON Games TO GuestRole;
GRANT SELECT ON Categories TO GuestRole;
GO

-- Назначение прав для роли User
GRANT SELECT ON Games TO UserRole;
GRANT SELECT ON Categories TO UserRole;
GRANT SELECT ON Sellers TO UserRole;
GRANT SELECT, INSERT ON Orders TO UserRole;
GRANT SELECT ON OrderItems TO UserRole;
GRANT SELECT, INSERT, UPDATE ON Reviews TO UserRole;
GRANT SELECT, UPDATE ON Users TO UserRole;
GO

-- Ограничение: Покупатель может видеть и редактировать только свои данные
-- Создаём представления и политики для ограничения доступа
CREATE VIEW UserOrders AS
SELECT o.*
FROM Orders o
WHERE o.UserID = CURRENT_USER; -- Предполагается, что UserID совпадает с именем пользователя в SQL Server
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

-- Переопределяем права для UserRole через представления
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

-- Назначение прав для роли Seller
GRANT SELECT, INSERT ON Games TO SellerRole;
GRANT SELECT ON Categories TO SellerRole;
GRANT SELECT ON Orders TO SellerRole;
GRANT SELECT, UPDATE ON OrderItems TO SellerRole;
GRANT SELECT, UPDATE ON Sellers TO SellerRole;
GO

-- Ограничение: Продавец может видеть и редактировать только свои данные
CREATE VIEW SellerOrderItems AS
SELECT oi.*
FROM OrderItems oi
WHERE oi.SellerID = CURRENT_USER; -- Предполагается, что SellerID совпадает с именем пользователя в SQL Server
GO

CREATE VIEW SellerProfile AS
SELECT s.*
FROM Sellers s
WHERE s.SellerID = CURRENT_USER;
GO

-- Переопределяем права для SellerRole через представления
REVOKE SELECT, UPDATE ON OrderItems FROM SellerRole;
REVOKE SELECT, UPDATE ON Sellers FROM SellerRole;
GO

GRANT SELECT, UPDATE ON SellerOrderItems TO SellerRole;
GRANT SELECT, UPDATE ON SellerProfile TO SellerRole;
GO

-- Назначение прав для роли Admin
GRANT SELECT, INSERT, UPDATE, DELETE ON Users TO AdminRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON Games TO AdminRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON Categories TO AdminRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON Sellers TO AdminRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON Orders TO AdminRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON OrderItems TO AdminRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON Reviews TO AdminRole;
GO

-- Создание тестовых пользователей и назначение ролей
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