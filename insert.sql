-- USE [HRLeave]; -- chris local db
USE [HRLeaveTestDb]; -- dbserver
GO


INSERT INTO [dbo].[permission] ([permission_id]) VALUES
('emp_permissions'),
('sup_permissions'),
('hr1_permissions'),
('hr2_permissions'),
('hr3_permissions'),
('crud_emp_info'),
('approve_sick'),
('approve_casual'),
('approve_vacation'),
('approve_contract'),
('approve_public_officer'),
('assign_role');


INSERT INTO [dbo].[role] ([role_id]) VALUES
('emp'),
('sup'),
('hr3'),
('hr2'),
('hr1'),
('hr_contract'),
('hr_public_officer');


INSERT INTO [dbo].[rolepermission] ([role_id], [permission_id]) VALUES
('emp', 'emp_permissions'),

('sup', 'emp_permissions'),
('sup', 'sup_permissions'),

('hr3', 'emp_permissions'),
('hr3', 'crud_emp_info'),
('hr3', 'hr3_permissions'),

('hr2', 'emp_permissions'),
('hr2', 'sup_permissions'),
('hr2', 'hr2_permissions'),
('hr2', 'crud_emp_info'),
('hr2', 'approve_sick'),
('hr2', 'approve_casual'),

('hr1', 'emp_permissions'),
('hr1', 'hr1_permissions'),
('hr1', 'approve_sick'),
('hr1', 'approve_casual'),
('hr1', 'crud_emp_info'),
('hr1', 'approve_vacation'),
('hr1', 'assign_role'),

('hr_contract', 'approve_contract'),

('hr_public_officer', 'approve_public_officer');


INSERT INTO [dbo].[employee] ([employee_id], [ihris_id], [username], [first_name], [last_name], [email], [vacation], [personal], [casual], [sick], [bereavement], [maternity], [pre_retirement]) VALUES 
('1', '1', 'PLANNING\ Tristan Sankar', 'Tristan', 'Sankar', 'tristan.sankar@planning.gov.tt', 0, 5, 0, 14, 2, 0, 0),
('2', '2', 'PLANNING\ Clint Ramoutar', 'Clint', 'Ramoutar', 'clint.ramoutar@planning.gov.tt', 20, 5, 0, 14, 2, 0, 0),
('3', '3', 'PLANNING\ Christopher Sahadeo', 'Christopher', 'Sahadeo', 'christopher.sahadeo@planning.gov.tt', 0, 5, 0, 14, 2, 0, 0),
('4', '4', 'PLANNING\ Dale Cudjoe', 'Dale', 'Cudjoe', 'dale.cudjoe@planning.gov.tt', 15, 5, 0, 14, 2, 0, 0),
('5', '5', 'PLANNING\ Nandani Ramsaran', 'Nandani', 'Ramsaran', 'nandani.ramsaran@planning.gov.tt', 10, 5, 0, 14, 2, 0, 0),
('6', '6', 'PLANNING\ Melanie Noel', 'Melanie', 'Noel', 'melanie.noel@planning.gov.tt', 30, 0, 0, 14, 2, 0, 50);


INSERT INTO [dbo].[employeerole] ([employee_id], [role_id]) VALUES
('1', 'emp'),
('2', 'sup'),
('3', 'emp'),
('4', 'sup'),
('5', 'emp'),
('6', 'hr1');


INSERT INTO [dbo].[assignment] ([supervisee_id], [supervisor_id]) VALUES
('1', '2'),
('3', '2'),
('5', '4');


INSERT INTO [dbo].[transactionstate] ([state_id]) VALUES 
('Submitted'),
('Change Requested'),
('Supervisor Approved'),
('Supervisor Denied'),
('Leave Approved'), -- by HR 1
('Leave Denied'); -- by HR 1


INSERT INTO [dbo].[leavetype] ([type_id]) VALUES
('Sick'),
('Personal'),
('Casual'),
('Vacation'),
('No Pay'),
('Bereavement'),
('Maternity'),
('Pre-retirement');


SET IDENTITY_INSERT [dbo].[leavetransaction] ON;

INSERT INTO [dbo].[leavetransaction] ([transaction_id], [employee_id], [leave_type], [start_date], [end_date], [supervisor_id], [hr_manager_id], [state], [message], [file_path]) VALUES
(1, '1', 'Sick', '20191113', '20191114', '2', NULL, 'Submitted', NULL, '1/20191113_sick.pdf');

SET IDENTITY_INSERT [dbo].[leavetransaction] OFF;


SET IDENTITY_INSERT [dbo].[department] ON;

INSERT INTO [dbo].[department] ([dept_id], [dept_abbr], [dept_name]) VALUES
(1, 'IT', 'Information Technology'),
(2, 'HR', 'Human Resources'),
(3, 'AC', 'Accounts');

SET IDENTITY_INSERT [dbo].[department] OFF;


SET IDENTITY_INSERT [dbo].[position] ON;

INSERT INTO [dbo].[position] ([pos_id], [pos_name], [pos_description], [vacation]) VALUES 
(1, 'IT Technician', 'Does stuff.', 20),
(2, 'Security Specialist', 'Secures stuff.', 15),
(3, 'Database Specialist', NULL, 15),
(4, 'Director', NULL, 25),
(5, 'Associate Professional', NULL, 0),
(6, 'System''s Specialist', NULL, 30);

SET IDENTITY_INSERT [dbo].[position] OFF;


INSERT INTO [dbo].[employmenttype] ([type_id]) VALUES 
('Contract'),
('Public Service');


SET IDENTITY_INSERT [dbo].[employeeposition] ON;

INSERT INTO [dbo].[employeeposition] ([id], [employee_id], [position_id], [start_date], [expected_end_date], [actual_end_date], [employment_type], [dept_id]) VALUES
(1, '1', 5, '20190927', '20200927', NULL, 'Contract', 1),
(2, '3', 5, '20190927', '20200927', NULL, 'Contract', 1),

-- multiple periods of employment for one person
(3, '2', 6, '20101005', '20131005', NULL, 'Contract', 1),
(4, '2', 6, '20131005', '20160927', NULL, 'Contract', 1),
(5, '2', 6, '20160927', '20191225', NULL, 'Contract', 1);

SET IDENTITY_INSERT [dbo].[employeeposition] OFF;
