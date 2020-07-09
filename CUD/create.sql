--USE [HRLeaveTestDb]; -- testing db
USE [HRLeaveDevDb]; -- development db
GO


CREATE TABLE [dbo].[permission] (
  [permission_id] NVARCHAR (30) PRIMARY KEY,
  [permission_description] NVARCHAR (50)
);


CREATE TABLE [dbo].[role] (
  [role_id] NVARCHAR (20) PRIMARY KEY,
  [role_description] NVARCHAR (50)
);


CREATE TABLE [dbo].[rolepermission] (
  [role_id] NVARCHAR (20),
  [permission_id] NVARCHAR (30),
  
  PRIMARY KEY ([role_id], [permission_id]),
  FOREIGN KEY ([role_id])
    REFERENCES [dbo].[role] ([role_id])
    ON DELETE CASCADE ON UPDATE CASCADE,
  FOREIGN KEY ([permission_id])
    REFERENCES [dbo].[permission] ([permission_id])
    ON DELETE CASCADE ON UPDATE CASCADE
);


-- 'user' is a reserved word, so using 'employee' instead
CREATE TABLE [dbo].[employee] (
  [employee_id] NVARCHAR (10) PRIMARY KEY, -- employee's file number
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
  [paternity] INT NOT NULL,
  [pre_retirement] INT NOT NULL,
  
  -- created on timestamp
  [created_on] DATETIME DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE [dbo].[employeerole] (
  [employee_id] NVARCHAR (10),
  [role_id] NVARCHAR (20),

  PRIMARY KEY ([employee_id], [role_id]),
  FOREIGN KEY ([employee_id])
    REFERENCES [dbo].[employee] ([employee_id])
	ON DELETE CASCADE,
  FOREIGN KEY ([role_id])
    REFERENCES [dbo].[role] ([role_id])
)


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
  [status_id] NVARCHAR (30) PRIMARY KEY
);


CREATE TABLE [dbo].[leavetype] (
  [type_id] NVARCHAR (20) PRIMARY KEY
);

CREATE TABLE [dbo].[department] (
  [dept_id] INT IDENTITY (1, 1) PRIMARY KEY,
  [dept_abbr] CHAR (5),
  [dept_name] NVARCHAR (30) NOT NULL
);


CREATE TABLE [dbo].[position] (
  [pos_id] INT IDENTITY (1, 1) PRIMARY KEY,
  [pos_name] NVARCHAR (30) NOT NULL,
  [pos_description] NVARCHAR (60),
  [vacation] INT -- the standard number of vacations days awarded per year per position 
);

CREATE TABLE [dbo].[employmenttype] (
  [type_id] NVARCHAR (15) PRIMARY KEY
);

CREATE TABLE [dbo].[employeeposition] (
  [id] INT IDENTITY (1, 1) PRIMARY KEY,
  [employee_id] NVARCHAR (10) NOT NULL,
  [position_id] INT NOT NULL,
  [start_date] DATETIME NOT NULL,
  [expected_end_date] DATETIME NOT NULL,
  [actual_end_date] DATETIME,
  [employment_type] NVARCHAR (15) NOT NULL,
  [is_substantive_or_acting] BIT DEFAULT 1,
  [dept_id] INT NOT NULL,
  [years_worked] INT NOT NULL,
  [annual_vacation_amt] INT NOT NULL,
  [max_vacation_accumulation] INT NOT NULL,
  [can_accumulate_past_max] BIT DEFAULT 0,
  [has_received_notif_about_end_of_contract] BIT DEFAULT 0

  FOREIGN KEY ([employee_id])
    REFERENCES [dbo].[employee] ([employee_id])
	ON DELETE CASCADE,
  FOREIGN KEY ([position_id])
    REFERENCES [dbo].[position] ([pos_id]),
  FOREIGN KEY ([employment_type])
    REFERENCES [dbo].[employmenttype] (type_id),
  FOREIGN KEY ([dept_id])
    REFERENCES [dbo].[department] (dept_id)
);

CREATE TABLE [dbo].[leavetransaction] (
  [transaction_id] INT IDENTITY (1, 1) PRIMARY KEY,
  [created_at] DATETIME DEFAULT CURRENT_TIMESTAMP,
  -- [last_modified] TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, -- will put in log table
  [employee_id] NVARCHAR (10) NOT NULL,
  [employee_position_id] INT NOT NULL,
  [leave_type] NVARCHAR (20) NOT NULL,
  [start_date] DATE NOT NULL,
  [end_date] DATE NOT NULL,
  [qualified] NVARCHAR(10) NOT NULL,
  [days_taken] INT NOT NULL,

  [supervisor_id] NVARCHAR (10) NOT NULL,
  [supervisor_edit_date] DATETIME,

  [hr_manager_id] NVARCHAR (10), -- can be null if the HR 1 has not yet approved
  [hr_manager_edit_date] DATETIME,

  [status] NVARCHAR (30) NOT NULL,
  [emp_comment] NVARCHAR (120),
  [sup_comment] NVARCHAR (120),
  [hr_comment] NVARCHAR (120)

  FOREIGN KEY ([employee_id])
    REFERENCES [dbo].[employee] ([employee_id]),
  FOREIGN KEY ([employee_position_id])
    REFERENCES [dbo].[employeeposition] ([id])
	ON DELETE CASCADE,
  FOREIGN KEY ([leave_type])
    REFERENCES [dbo].[leavetype] ([type_id])
    ON UPDATE CASCADE,
  FOREIGN KEY ([supervisor_id])
    REFERENCES [dbo].[employee] ([employee_id]),
  FOREIGN KEY ([hr_manager_id])
    REFERENCES [dbo].[employee] ([employee_id]),
  FOREIGN KEY ([status])
    REFERENCES [dbo].[transactionstate] ([status_id])
    ON UPDATE CASCADE
);
-- create update trigger for modified: https://stackoverflow.com/questions/22594567/sql-server-on-update-set-current-timestamp/22594779


CREATE TABLE [dbo].[auditlog] (
  [log_id] INT IDENTITY (1, 1) PRIMARY KEY,
  [machine_name] NVARCHAR (100) NOT NULL,
  [ipv6_address] NVARCHAR(50) NOT NULL,
  [acting_employee_id] NVARCHAR (10) NOT NULL,
  [acting_employee_name] NVARCHAR (60)NOT NULL,
  [affected_employee_id] NVARCHAR (10) NOT NULL,
  [affected_employee_name] NVARCHAR (60) NOT NULL,
  [action] NVARCHAR (350) NOT NULL,
  [created_at] DATETIME DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY ([acting_employee_id])
    REFERENCES [dbo].[employee] ([employee_id])
);

CREATE TABLE [dbo].[filestorage](
  [file_id] UNIQUEIDENTIFIER PRIMARY KEY default NEWID(),
  [file_data] IMAGE NOT NULL,
  [file_name] NVARCHAR(200) NOT NULL,
  [file_extension] NVARCHAR (25) NOT NULL,
  [uploaded_on] DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE [dbo].[employeefiles](
  [id] INT IDENTITY (1, 1) PRIMARY KEY,
  [leave_transaction_id] INT,
  [emp_record_id] INT,
  [employee_id] NVARCHAR(10) NOT NULL,
  [file_id] UNIQUEIDENTIFIER NOT NULL,

  FOREIGN KEY ([leave_transaction_id]) REFERENCES [dbo].[leavetransaction] ([transaction_id]),
  FOREIGN KEY ([emp_record_id]) REFERENCES [dbo].[employeeposition] ([id]),
  FOREIGN KEY([employee_id]) REFERENCES [dbo].[employee] ([employee_id]),
  FOREIGN KEY([file_id]) REFERENCES [dbo].[filestorage] ([file_id])
	ON DELETE CASCADE
);

CREATE TABLE [dbo].[notifications](
  [id] INT IDENTITY(1, 1) PRIMARY KEY,
  [notification_header] NVARCHAR(100) NOT NULL,
  [notification] NVARCHAR(250) NOT NULL,
  [is_read] NVARCHAR(5) NOT NULL,
  [employee_id] NVARCHAR(10) NOT NULL,
  [created_at] DATETIME NOT NULL,

  FOREIGN KEY([employee_id]) REFERENCES [dbo].[employee] ([employee_id])

);
CREATE TABLE [dbo].[emptypeleavetype](
  [id] INT IDENTITY(1, 1) PRIMARY KEY,
  [employment_type] NVARCHAR(15) NOT NULL,
  [leave_type] NVARCHAR(20) NOT NULL,

  FOREIGN KEY([employment_type]) REFERENCES [dbo].[employmenttype] ([type_id]),
  FOREIGN KEY([leave_type]) REFERENCES [dbo].[leavetype] ([type_id])

);

CREATE TABLE [dbo].[accumulations](
  [id] INT IDENTITY(1, 1) PRIMARY KEY,
  [employment_type] NVARCHAR(15) NOT NULL,
  [leave_type] NVARCHAR(20) NOT NULL,
  [accumulation_period_text] NVARCHAR(40) NOT NULL,
  [accumulation_period_value] NVARCHAR(2) NOT NULL,
  [accumulation_type] NVARCHAR(15) NOT NULL,
  [num_days] INT NOT NULL,

  FOREIGN KEY([employment_type]) REFERENCES [dbo].[employmenttype] ([type_id]),
  FOREIGN KEY([leave_type]) REFERENCES [dbo].[leavetype] ([type_id]),
);