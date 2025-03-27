USE Gamesbakery;
GO

-- Шаг 1: Создание мастер-ключа столбца (Column Master Key)
-- Это делается через SSMS:
-- 1. В SSMS откройте базу данных Gamesbakery.
-- 2. Перейдите в Security -> Always Encrypted Keys -> Column Master Keys.
-- 3. Щёлкните правой кнопкой мыши и выберите "New Column Master Key".
-- 4. Укажите имя ключа, например, CMK_Password.
-- 5. Выберите хранилище ключей (например, Windows Certificate Store или Azure Key Vault).
-- 6. Создайте ключ.

-- Шаг 2: Создание ключа шифрования столбца (Column Encryption Key)
-- Это также делается через SSMS:
-- 1. Перейдите в Security -> Always Encrypted Keys -> Column Encryption Keys.
-- 2. Щёлкните правой кнопкой мыши и выберите "New Column Encryption Key".
-- 3. Укажите имя ключа, например, CEK_Password.
-- 4. Выберите мастер-ключ столбца (CMK_Password).
-- 5. Создайте ключ.

-- Шаг 3: Настройка шифрования столбца Password
-- Это можно сделать через SSMS:
-- 1. Щёлкните правой кнопкой мыши на таблицу Users и выберите "Encrypt Columns".
-- 2. В мастере выберите столбец Password.
-- 3. Укажите тип шифрования: DETERMINISTIC.
-- 4. Выберите ключ шифрования столбца (CEK_Password).
-- 5. Запустите процесс шифрования.

-- Примечание: После настройки Always Encrypted столбец Password будет автоматически шифроваться
-- при вставке данных через приложение, использующее драйвер с поддержкой Always Encrypted.