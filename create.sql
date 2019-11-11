USE master;
GO

IF DB_ID (N'HRLeave') IS NULL
CREATE DATABASE HRLeave;
GO

USE HRLeave;
GO

-- 'user' is a resered word, so using 'employee' instead
CREATE TABLE dbo.user (

);