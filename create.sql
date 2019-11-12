USE master;
GO

IF DB_ID (N'HRLeave') IS NULL
CREATE DATABASE [HRLeave];
GO

USE [HRLeave];
GO

CREATE TABLE [dbo].[authorization] (
  [auth_id] CHAR (1) PRIMARY KEY, -- might not need the id - just use the auth_name
  [auth_name] NVARCHAR (15) NOT NULL,
  [auth_description] NVARCHAR (50)
);

-- 'user' is a resered word, so using 'employee' instead
CREATE TABLE [dbo].[employee] (
  [employee_id] NVARCHAR (10) PRIMARY KEY,
  [ihris_id] NVARCHAR (10) NOT NULL,

  -- active directory info
  [username] NVARCHAR (60) NOT NULL,
  [first_name] NVARCHAR (30) NOT NULL, 
  [last_name] NVARCHAR (30) NOT NULL,
  [email] NVARCHAR (60) NOT NULL,
  
  -- balance per leave type
  [vacation] INT NOT NULL,
  [personal] INT NOT NULL,
  [casual] INT NOT NULL,
  [sick] INT NOT NULL,
  [bereavement] INT NOT NULL,
  [maternity] INT NOT NULL,
  [pre_retirement] INT NOT NULL
);

CREATE TABLE [dbo].[employeeauthorization] (
  [employee_id] NVARCHAR (10),
  [auth_id] CHAR (1),

  PRIMARY KEY ([employee_id], [auth_id]),
  FOREIGN KEY ([employee_id])
    REFERENCES [dbo].[employee] ([employee_id])
    ON DELETE CASCADE ON UPDATE CASCADE,
  FOREIGN KEY ([auth_id])
    REFERENCES [dbo].[authorization] ([auth_id])
    ON DELETE CASCADE ON UPDATE CASCADE
);