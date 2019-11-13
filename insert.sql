USE [HRLeave];
GO


INSERT INTO [dbo].[authorization] ([auth_id], [auth_name], [auth_description]) VALUES 
('1', 'Employee', 'You can apply for leave.'),
('2', 'Supervisor', 'You can apply for and approve leave for supervisees.'),
('3', 'HR 2', 'You can apply for leave, add employees and edit vacation leave.'),
('4', 'HR 1', 'You can approve leave that supervisors have pre-approved.');


INSERT INTO [dbo].[employee] ([employee_id], [ihris_id], [username], [first_name], [last_name], [email], [vacation], [personal], [casual], [sick], [bereavement], [maternity], [pre_retirement]) VALUES 
('1', '1', 'PLANNING\ Tristan Sankar', 'Tristan', 'Sankar', 'tristan.sankar@planning.gov.tt', 0, 5, 0, 14, 2, 0, 0),
('2', '2', 'PLANNING\ Clint Ramoutar', 'Clint', 'Ramoutar', 'clint.ramoutar@planning.gov.tt', 20, 5, 0, 14, 2, 0, 0),
('3', '3', 'PLANNING\ Christopher Sahadeo', 'Christopher', 'Sahadeo', 'christopher.sahadeo@planning.gov.tt', 0, 5, 0, 14, 2, 0, 0),
('4', '4', 'PLANNING\ Dale Cudjoe', 'Dale', 'Cudjoe', 'dale.cudjoe@planning.gov.tt', 15, 5, 0, 14, 2, 0, 0),
('5', '5', 'PLANNING\ Nandani Ramsaran', 'Nandani', 'Ramsaran', 'nandani.ramsaran@planning.gov.tt', 10, 5, 0, 14, 2, 0, 0),
('6', '6', 'PLANNING\ Melanie Noel', 'Melanie', 'Noel', 'melanie.noel@planning.gov.tt', 30, 0, 0, 14, 2, 0, 50);


INSERT INTO [dbo].[employeeauthorization] ([employee_id], [auth_id]) VALUES 
('1', '1'),
('2', '2'),
('3', '1'),
('4', '2'),
('5', '1'),
('6', '4');


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
