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
GRANT SELECT ON Reviews TO GuestRole;
ALTER SERVER ROLE securityadmin ADD MEMBER AdminUser;
GO

USE Gamesbakery;
GO

-- 1. ������� ����������
CREATE CERTIFICATE RegistrationCert
   ENCRYPTION BY PASSWORD = 'INSERTCert'
   WITH SUBJECT = 'Certificate for Registration Procedures',
   EXPIRY_DATE = '20261231';
GO
-- 2. ������� ������������, ���������� � ������������
CREATE USER RegistrationCertUser FROM CERTIFICATE RegistrationCert;
GO
-- 3. ���� ������������ ����������� ����� �� INSERT � ������� Users � Sellers
GRANT INSERT ON Users TO RegistrationCertUser;
GRANT INSERT ON Sellers TO RegistrationCertUser;
GO
-- 4. ����������� �������� ��������� sp_RegisterUser ������������
ADD SIGNATURE TO sp_RegisterUser
   BY CERTIFICATE RegistrationCert
   WITH PASSWORD = 'INSERTCert';
GO
-- 5. ����������� �������� ��������� sp_RegisterSeller ������������
ADD SIGNATURE TO sp_RegisterSeller
   BY CERTIFICATE RegistrationCert
   WITH PASSWORD = 'INSERTCert';
GO
USE Gamesbakery;
GO
-- ����������� ��������� ������������
ADD SIGNATURE TO sp_ManageSqlLogin
   BY CERTIFICATE RegistrationCert
   WITH PASSWORD = 'INSERTCert';
GO
-- ���� GuestRole ����� �� ���������� ���������
GRANT EXECUTE ON sp_ManageSqlLogin TO GuestRole;
GRANT EXECUTE ON sp_RegisterUser TO GuestRole;
GRANT EXECUTE ON sp_RegisterSeller TO GuestRole;
GO
SELECT 
    p.name AS principal_name,
    perm.permission_name,
    perm.state_desc,
    obj.name AS object_name
FROM sys.database_permissions perm
JOIN sys.database_principals p ON perm.grantee_principal_id = p.principal_id
JOIN sys.objects obj ON perm.major_id = obj.object_id
WHERE p.name = 'GuestRole' AND obj.name IN ('sp_RegisterUser', 'sp_RegisterSeller');
GO

REVOKE INSERT ON Users TO GuestRole;
REVOKE INSERT ON Sellers TO GuestRole;
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
USE Gamesbakery;
GO
CREATE OR ALTER VIEW UserOrders AS
SELECT o.*
FROM Orders o
JOIN Users u ON o.UserID = u.UserID
WHERE u.Name = CURRENT_USER;
GO

CREATE OR ALTER VIEW UserOrderItems AS
SELECT oi.*
FROM OrderItems oi
JOIN Orders o ON oi.OrderID = o.OrderID
JOIN Users u ON o.UserID = u.UserID
WHERE u.Name = CURRENT_USER;
GO

CREATE OR ALTER VIEW UserReviews AS
SELECT r.*
FROM Reviews r
JOIN Users u ON r.UserID = u.UserID
WHERE u.Name = CURRENT_USER;
GO

CREATE OR ALTER VIEW UserProfile AS
SELECT u.*
FROM Users u
WHERE u.Name = CURRENT_USER;
GO

-- �������������� ����� ��� UserRole ����� �������������
REVOKE SELECT, INSERT ON Orders FROM UserRole;
REVOKE SELECT ON OrderItems FROM UserRole;
REVOKE UPDATE ON Reviews FROM UserRole;
REVOKE UPDATE ON Users FROM UserRole;
GO

GRANT SELECT, INSERT ON UserOrders TO UserRole;
GRANT SELECT ON UserOrderItems TO UserRole;
GRANT SELECT, INSERT, UPDATE ON UserReviews TO UserRole;
GRANT SELECT, UPDATE ON UserProfile TO UserRole;
GO

CREATE OR ALTER VIEW SellerOrderItems AS
SELECT oi.*
FROM OrderItems oi
JOIN Sellers s ON oi.SellerID = s.SellerID
WHERE s.Name = CURRENT_USER;
GO

CREATE OR ALTER VIEW SellerProfile AS
SELECT s.*
FROM Sellers s
WHERE s.Name = CURRENT_USER;
GO

-- ���������� ���� ��� ���� Seller
GRANT SELECT ON Users TO SellerRole;
GRANT SELECT, INSERT ON Games TO SellerRole;
GRANT SELECT ON Categories TO SellerRole;
GRANT SELECT ON Orders TO SellerRole;
GRANT SELECT, UPDATE ON OrderItems TO SellerRole;
GRANT SELECT, UPDATE ON Sellers TO SellerRole;
GRANT SELECT ON UserProfile TO SellerRole;
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
REVOKE SELECT, INSERT, UPDATE, DELETE ON UserProfile TO AdminRole;
REVOKE SELECT, INSERT, UPDATE, DELETE ON UserReviews TO AdminRole;
REVOKE SELECT, INSERT, UPDATE, DELETE ON UserOrderItems TO AdminRole;
REVOKE SELECT, INSERT, UPDATE, DELETE ON UserOrders TO AdminRole;
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