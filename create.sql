USE master;
GO

IF DB_ID (N'HRLeave') IS NULL
CREATE DATABASE HRLeave;
GO

USE HRLeave;
GO

CREATE TABLE 

-- 'user' is a resered word, so using 'employee' instead
CREATE TABLE [dbo].[employee] (
  [employee_id] NVARCHAR (10) PRIMARY KEY,
  [ihris_id] NVARCHAR (10) NOT NULL,
  [first_name] NVARCHAR (30) NOT NULL,
  [last_name] NVARCHAR (30) NOT NULL,
  [email] NVARCHAR (60) NOT NULL,
  -- [auth_id] CHAR (1) NOT NULL, -- not sure about mixed roles implementation
  [vacation] INT NOT NULL DEFAULT 0,
  []
);