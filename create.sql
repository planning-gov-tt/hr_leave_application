-- USE master;
-- GO

-- IF DB_ID (N'HRLeave') IS NULL
-- CREATE DATABASE [HRLeave];
-- GO

-- USE [HRLeave]; -- chris local db
USE [HRLeaveTestDB]; -- dbserver
GO

-- DECLARE @Short int;
-- SET @Short = 10;
-- DECLARE @Medium int;
-- SET @Medium = 30
-- DECLARE @Long int;
-- SET @Long = 60


CREATE TABLE [dbo].[authorization] (
  [auth_id] CHAR (1) PRIMARY KEY, -- might not need the id - just use the auth_name
  [auth_name] NVARCHAR (15) NOT NULL,
  [auth_description] NVARCHAR (100)
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
  [employee_id] NVARCHAR (10) NOT NULL,
  [auth_id] CHAR (1) NOT NULL,

  PRIMARY KEY ([employee_id], [auth_id]),
  FOREIGN KEY ([employee_id])
    REFERENCES [dbo].[employee] ([employee_id]),
  FOREIGN KEY ([auth_id])
    REFERENCES [dbo].[authorization] ([auth_id])
);


-- problems with setting both to ON DELETE CASCADE ON UPDATE CASCADE,
CREATE TABLE [dbo].[assignment] (
  [supervisee_id] NVARCHAR (10) NOT NULL,
  [supervisor_id] NVARCHAR (10) NOT NULL,
  CHECK ([supervisee_id] != [supervisor_id]),

  PRIMARY KEY ([supervisee_id], supervisor_id),
  FOREIGN KEY ([supervisee_id])
    REFERENCES [dbo].[employee] ([employee_id]),
    -- ON DELETE CASCADE ON UPDATE CASCADE,
  FOREIGN KEY ([supervisor_id])
    REFERENCES [dbo].[employee] ([employee_id])
    -- ON DELETE CASCADE ON UPDATE CASCADE
);


-- CREATE TABLE [dbo].[notification] ();


CREATE TABLE [dbo].[transactionstate] (
  [state_id] NVARCHAR (20) PRIMARY KEY
);


CREATE TABLE [dbo].[leavetype] (
  [type_id] NVARCHAR (20) PRIMARY KEY
);


CREATE TABLE [dbo].[leavetransaction] (
  [transaction_id] INT IDENTITY (1, 1) PRIMARY KEY,
  [created_at] DATETIME DEFAULT CURRENT_TIMESTAMP,
  -- [last_modified] TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, -- will put in log table
  [employee_id] NVARCHAR (10) NOT NULL,
  [leave_type] NVARCHAR (20) NOT NULL,
  [start_date] DATE NOT NULL,
  [end_date] DATE NOT NULL,
  [supervisor_id] NVARCHAR (10) NOT NULL,
  [hr_manager_id] NVARCHAR (10), -- can be null if the HR 1 has not yet approved
  [state] NVARCHAR (20) NOT NULL,
  [message] NVARCHAR (120),
  [file_path] NVARCHAR (30),

  FOREIGN KEY ([employee_id])
    REFERENCES [dbo].[employee] ([employee_id]),
  FOREIGN KEY ([leave_type])
    REFERENCES [dbo].[leavetype] ([type_id])
    ON UPDATE CASCADE,
  FOREIGN KEY ([supervisor_id])
    REFERENCES [dbo].[employee] ([employee_id]),
  FOREIGN KEY ([hr_manager_id])
    REFERENCES [dbo].[employee] ([employee_id]),
  FOREIGN KEY ([state])
    REFERENCES [dbo].[transactionstate] ([state_id])
    ON UPDATE CASCADE
);
-- create update trigger for modified: https://stackoverflow.com/questions/22594567/sql-server-on-update-set-current-timestamp/22594779


CREATE TABLE [dbo].[department] (
  [dept_id] INT IDENTITY (1, 1) PRIMARY KEY,
  [dept_abbr] CHAR (5),
  [dept_name] NVARCHAR (30) NOT NULL
);


CREATE TABLE [dbo].[position] (
  [pos_id] INT IDENTITY (1, 1) PRIMARY KEY,
  [pos_name] NVARCHAR (30) NOT NULL,
  [pos_description] NVARCHAR (60),
  [vacation] INT NOT NULL -- the standard number of vacations days awarded per year per position 
);


CREATE TABLE [dbo].[employmenttype] (
  type_id NVARCHAR (15) PRIMARY KEY
);


CREATE TABLE [dbo].[employeeposition] (
  [id] INT IDENTITY (1, 1) PRIMARY KEY,
  [employee_id] NVARCHAR (10) NOT NULL,
  [position_id] INT NOT NULL,
  [start_date] DATETIME NOT NULL,
  [expected_end_date] DATETIME NOT NULL,
  [actual_end_date] DATETIME,
  [employment_type] NVARCHAR (15) NOT NULL,
  [dept_id] INT,

  FOREIGN KEY ([employee_id])
    REFERENCES [dbo].[employee] ([employee_id]),
  FOREIGN KEY ([position_id])
    REFERENCES [dbo].[position] ([pos_id]),
  FOREIGN KEY ([employment_type])
    REFERENCES [dbo].[employmenttype] (type_id),
  FOREIGN KEY ([dept_id])
    REFERENCES [dbo].[department] (dept_id)
);
