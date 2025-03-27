USE Gamesbakery;
GO

-- ��� 1: �������� ������-����� ������� (Column Master Key)
-- ��� �������� ����� SSMS:
-- 1. � SSMS �������� ���� ������ Gamesbakery.
-- 2. ��������� � Security -> Always Encrypted Keys -> Column Master Keys.
-- 3. ٸ������ ������ ������� ���� � �������� "New Column Master Key".
-- 4. ������� ��� �����, ��������, CMK_Password.
-- 5. �������� ��������� ������ (��������, Windows Certificate Store ��� Azure Key Vault).
-- 6. �������� ����.

-- ��� 2: �������� ����� ���������� ������� (Column Encryption Key)
-- ��� ����� �������� ����� SSMS:
-- 1. ��������� � Security -> Always Encrypted Keys -> Column Encryption Keys.
-- 2. ٸ������ ������ ������� ���� � �������� "New Column Encryption Key".
-- 3. ������� ��� �����, ��������, CEK_Password.
-- 4. �������� ������-���� ������� (CMK_Password).
-- 5. �������� ����.

-- ��� 3: ��������� ���������� ������� Password
-- ��� ����� ������� ����� SSMS:
-- 1. ٸ������ ������ ������� ���� �� ������� Users � �������� "Encrypt Columns".
-- 2. � ������� �������� ������� Password.
-- 3. ������� ��� ����������: DETERMINISTIC.
-- 4. �������� ���� ���������� ������� (CEK_Password).
-- 5. ��������� ������� ����������.

-- ����������: ����� ��������� Always Encrypted ������� Password ����� ������������� �����������
-- ��� ������� ������ ����� ����������, ������������ ������� � ���������� Always Encrypted.