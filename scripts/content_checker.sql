USE Gamesbakery;
SELECT * FROM Sellers;
SELECT * FROM Categories;

SELECT *
FROM OrderItems oi
WHERE oi.KeyText = 'NEWKEY';

SELECT *
FROM Orders o

SELECT *
FROM OrderItems oi
WHERE oi.KeyText = 'NEWKEY';

SELECT *
FROM Games g
WHERE g.Title = 'BOOMD3';

SELECT *
FROM Sellers s

SELECT *
FROM Users u
WHERE u.Name = 'I';

SELECT *
FROM UserProfile u
WHERE u.Name = 'I';

SELECT *
FROM Users u
WHERE u.Balance = 300.0;

DELETE FROM Users
WHERE Name = 'Ivan';

SELECT * FROM Games;


SELECT 
    pr.name AS PrincipalName,
    pe.permission_name,
    pe.state_desc,
    OBJECT_NAME(pe.major_id) AS ObjectName
FROM sys.database_permissions pe
JOIN sys.database_principals pr ON pe.grantee_principal_id = pr.principal_id
WHERE pr.name = 'UserRole';


SELECT name, type_desc
FROM sys.server_principals
WHERE name = 'Clement93';
SELECT name, type_desc
FROM sys.database_principals
WHERE name = 'Clement93';

SELECT * FROM ErrorLog;

SELECT COUNT(*)
FROM sys.database_principals dp
JOIN sys.database_role_members drm ON dp.principal_id = drm.member_principal_id
JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
WHERE dp.name = 'AdminUser' AND r.name = 'AdminRole';


SELECT 
    p.name AS principal_name,
    p.type_desc AS principal_type,
    pm.permission_name,
    pm.state_desc
FROM sys.database_permissions pm
JOIN sys.database_principals p ON pm.grantee_principal_id = p.principal_id
WHERE p.name = 'RegistrationCertUser';

SELECT 
    blocking.session_id AS blocking_session_id,
    blocked.session_id AS blocked_session_id,
    blocking_text.text AS blocking_text,
    blocked_text.text AS blocked_text
FROM sys.dm_exec_connections AS blocking
JOIN sys.dm_exec_requests AS blocked ON blocking.session_id = blocked.blocking_session_id
CROSS APPLY sys.dm_exec_sql_text(blocking.most_recent_sql_handle) AS blocking_text
CROSS APPLY sys.dm_exec_sql_text(blocked.sql_handle) AS blocked_text;