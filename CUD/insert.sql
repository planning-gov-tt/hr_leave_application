--USE [HRLeave]; -- chris local db
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
('contract_permissions'),
('public_officer_permissions'),
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
('hr1', 'sup_permissions'),
('hr1', 'hr1_permissions'),
('hr1', 'approve_sick'),
('hr1', 'approve_casual'),
('hr1', 'crud_emp_info'),
('hr1', 'approve_vacation'),
('hr1', 'assign_role'),
('hr1', 'contract_permissions'),
('hr1', 'public_officer_permissions'),

('hr_contract', 'contract_permissions'),

('hr_public_officer', 'public_officer_permissions');


INSERT INTO [dbo].[employee] ([employee_id], [ihris_id], [username], [first_name], [last_name], [email], [vacation], [personal], [casual], [sick], [bereavement], [maternity], [pre_retirement]) VALUES 
-- IT
('1', '1', 'PLANNING\ Tristan Sankar', 'Tristan', 'Sankar', 'Tristan.Sankar@planning.gov.tt', 0, 5, 0, 14, 2, 0, 0),
('115245', '115245', 'PLANNING\ Clint Ramoutar', 'Clint', 'Ramoutar', 'Clint.Ramoutar@planning.gov.tt', 20, 5, 0, 14, 2, 0, 0),
('3', '3', 'PLANNING\ Christopher Sahadeo', 'Christopher', 'Sahadeo', 'Christopher.Sahadeo@planning.gov.tt', 0, 5, 0, 14, 2, 0, 0),
('184164', '184164', 'PLANNING\ Dale Cudjoe', 'Dale', 'Cudjoe', 'Dale.Cudjoe@planning.gov.tt', 15, 5, 0, 14, 2, 0, 0),
('157778', '157778', 'PLANNING\ Nandani Ramsaran', 'Nandani', 'Ramsaran', 'Nandani.Ramsaran@planning.gov.tt', 10, 5, 0, 14, 2, 0, 0),
('161720', '161720', 'PLANNING\ Rishi Jaimungalsingh', 'Rishi', 'Jaimungalsingh', 'Rishi.Jaimungalsingh@planning.gov.tt', 0, 5, 0, 10, 1, 0, 0),
('159118', '159118', 'PLANNING\ Carlisle McKay', 'Carlisle', 'McKay', 'Carlisle.McKay@planning.gov.tt', 0, 5, 0, 10, 1, 0, 0),
('15067', '15067', 'PLANNING\ Rohini Singh', 'Rohini', 'Singh', 'Rohini.Singh@planning.gov.tt', 20, 5, 0, 14, 2, 0, 0),
('123337', '123337', 'PLANNING\ Kene Bryan', 'Kene', 'Bryan', 'Kene.Bryan@planning.gov.tt', 20, 5, 0, 14, 2, 0, 0), -- Director
('140480', '140480', 'PLANNING\ Kelly DeLandro', 'Kelly', 'DeLandro', 'Kelly.DeLandro@planning.gov.tt', 9, 5, 0, 14, 2, 0, 0), -- Director
-- Pele
-- Deneyse

-- HR
('83612', '83612', 'PLANNING\ Melanie Noel', 'Melanie', 'Noel', 'Melanie.Noel@planning.gov.tt', 30, 0, 0, 14, 2, 0, 50), -- Director
('11948', '11948', 'PLANNING\ Charmaine Carmichael', 'Charmaine', 'Carmichael', 'Charmaine.Carmichael@planning.gov.tt', 30, 0, 0, 14, 2, 0, 50), -- HRO3
('01511', '01511', 'PLANNING\ Christine Bowen', 'Christine', 'Bowen', 'Christine.Bowen@planning.gov.tt', 30, 0, 0, 14, 2, 0, 50), -- HRO2
('13888', '13888', 'PLANNING\ Sharline Bharath-Jaggernauth', 'Sharline', 'Bharath-Jaggernauth', 'Sharline.Bharath-Jaggernauth@planning.gov.tt', 25, 0, 0, 14, 2, 0, 0), -- HRO2
('87465', '87465', 'PLANNING\ Kathy-Ann Williams', 'Kathy-Ann', 'Williams', 'Kathy-Ann.Williams@planning.gov.tt', 25, 0, 0, 14, 2, 0, 0), -- HRO1
('09246', '09246', 'PLANNING\ Suzette Maraj', 'Suzette', 'Maraj', 'Suzette.Maraj@planning.gov.tt', 25, 0, 0, 14, 2, 0, 0), -- AO2
('01548', '01548', 'PLANNING\ Nazmoon Khan', 'Nazmoon', 'Khan', 'Nazmoon.Khan@planning.gov.tt', 30, 0, 5, 14, 2, 0, 30), -- HRO3
-- testing hard filter
-- adding pauline under charmichael (hr2) as contract
('38137', '38137', 'PLANNING\ Pauline Solozano', 'Pauline', 'Solozano', 'Pauline.Solozano@planning.gov.tt', 30, 0, 5, 14, 2, 0, 30), -- HRO3
-- adding usha under nazmoon (hr2) as public servant
('05356', '05356', 'PLANNING\ Usha Balkaran', 'Usha', 'Balkaran', 'Usha.Balkaran@planning.gov.tt', 30, 0, 5, 14, 2, 0, 30), -- HRO3

-- PS
('07525', '07525', 'PLANNING\ Joanne Deoraj', 'Joanne', 'Deoraj', 'Joanne.Deoraj@planning.gov.tt', 50, 0, 5, 14, 2, 0, 0); -- PS
-- PM
-- P


INSERT INTO [dbo].[employeerole] ([employee_id], [role_id]) VALUES
-- IT
('1', 'emp'),
('115245', 'sup'),
('3', 'emp'),
('184164', 'sup'),
('157778', 'emp'),
('161720', 'emp'),
('159118', 'sup'),
('15067', 'emp'),
('123337', 'sup'),

-- HR
('83612', 'hr1'),

-- hr2 can see and approve sick and casual ONLY
-- can see and approve contract ONLY
('11948', 'hr2'),
('11948', 'hr_contract'),

-- can see and approve public_services ONLY
('01548', 'hr2'),
('01548', 'hr_public_officer'),

('01511', 'hr3'),
('01511', 'hr_contract'),

('13888', 'hr3'),
('13888', 'sup'),
('13888', 'hr_public_officer'),

('87465', 'emp'),

('09246', 'sup'),

('07525','hr1'),

('38137','emp'),

('05356','emp')

;


INSERT INTO [dbo].[assignment] ([supervisee_id], [supervisor_id]) VALUES
('1', '115245'),
('115245', '159118'),
('3', '115245'),
('184164', '123337'),
('157778', '184164'),
('161720', '184164'),
('159118', '123337'),
('15067', '159118'),
('123337', '07525'),
('140480', '123337'),
('83612', '07525'),
('11948', '83612'),
('01511', '01548'),
('13888', '01548'),
('87465', '13888'),
('09246', '87465'),
('01548', '83612'),
('38137', '11948'),
('05356', '01548')
;


INSERT INTO [dbo].[transactionstate] ([status_id]) VALUES 
('Pending'),
('Cancelled'),
('Date Change Requested'),
('Recommended'),
('Not Recommended'),
('Approved'), -- by HR 1/2
('Not Approved'); -- by HR 1/2


INSERT INTO [dbo].[leavetype] ([type_id]) VALUES
('Sick'),
('Personal'),
('Casual'),
('Vacation'),
('No Pay'),
('Bereavement'),
('Maternity'),
('Pre-retirement'),
('Leave Renewal'),
('Compensatory');


SET IDENTITY_INSERT [dbo].[leavetransaction] ON;

INSERT INTO [dbo].[leavetransaction] ([transaction_id], [created_at], [employee_id], [leave_type], [start_date], [end_date], [supervisor_id], [supervisor_edit_date], [hr_manager_id], [hr_manager_edit_date], [status], [emp_comment],[sup_comment],[hr_comment], [file_path]) VALUES
-- SENARIOS 
-- VIEWS: (emp, sup, hr)

-- emp applies for sick leave to sup, pending
(1, '20191208 10:00:00 AM', '1', 'Sick', '20191201', '20191207', '115245', NULL, NULL, NULL, 'Pending', NULL, 'Good employee, normally prompt and consistent', NULL, '1/20191201_doctor_note.pdf'),

-- emp applies for sick leave to sup, recommended
(2, '20191208 12:00:00 PM', '3', 'Sick', '20201201', '20201207', '115245', '20191208 12:10:00 PM', NULL, NULL, 'Recommended','I have swine flu', 'Good Standing',NULL, '2/20191201_doctor_note.pdf'),

-- emp applies for vacation leave to sup, to hr, approved
(3, '20191101 9:54:00 AM', '184164', 'Vacation', '20201201', '20201210', '123337', '20191104 8:01:00 AM', '11948', '20191105 1:10:04 PM', 'Approved',NULL, 'Good',NULL, NULL),

-- same employee applies for sick leave to sup, recommended
(4, '20191110 9:54:00 AM', '184164', 'Sick', '20191109', '20191109', '123337', '20191111 10:05:12 AM', NULL, NULL, 'Recommended', NULL,NULL, NULL,'184164/20191110_doctor_note.pdf'),

-- setup for demo
-- hr director applies to PS for vacation leave - after approval should not see your own app in hr gridview
(5, '20191112 9:54:00 AM', '83612', 'Vacation', '20191212', '20200101', '07525', NULL, NULL, NULL, 'Pending', NULL, NULL, NULL, NULL),

-- hr director applies to PS for sick leave
(6, '20191125 12:54:00 PM', '83612', 'Sick', '20191123', '20191124', '07525', NULL, NULL, NULL, 'Recommended', NULL,'No problem',NULL, NULL),

-- HRO3 applies to HR Director for sick leave (sup)
(7, '20191125 12:54:00 PM', '11948', 'Sick', '20201123', '20201124', '83612', NULL, NULL, NULL, 'Pending', NULL,NULL,NULL, NULL),

-- hr2
-- can see and approve sick and casual ONLY
-- can see and approve contract ONLY
(8, '20191125 12:54:00 PM', '38137', 'Sick', '20191123', '20191124', '11948', NULL, NULL, NULL, 'Recommended', 'Ebola take me','Feel Better',NULL, NULL),

-- can see and approve public_services ONLY
(9, '20191125 12:54:00 PM', '05356', 'Casual', '20191123', '20191124', '01548', NULL, NULL, NULL, 'Recommended', NULL,NULL,NULL, NULL)

-- hr1
-- can see and appprove all other types (vacation etc)
;


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
(6, 'System''s Specialist', NULL, 30),
(7, 'HR Staff', NULL, 30);

SET IDENTITY_INSERT [dbo].[position] OFF;


INSERT INTO [dbo].[employmenttype] ([type_id]) VALUES 
('Contract'),
('Public Service');


SET IDENTITY_INSERT [dbo].[employeeposition] ON;

INSERT INTO [dbo].[employeeposition] ([id], [employee_id], [position_id], [start_date], [expected_end_date], [actual_end_date], [employment_type], [dept_id]) VALUES
(1, '1', 5, '20190927', '20200927', NULL, 'Contract', 1),
(2, '3', 5, '20190927', '20200927', NULL, 'Public Service', 1),

-- multiple periods of employment for one person
(3, '115245', 6, '20101005', '20131005', NULL, 'Contract', 1),
(4, '115245', 6, '20131005', '20170927', NULL, 'Contract', 1),
(5, '115245', 6, '20160927', '20201225', NULL, 'Contract', 1),

-- contract and public servant
(7, '38137', 7, '20160927', '20201225', NULL, 'Contract', 2),
(8, '05356', 7, '20160927', '20201225', NULL, 'Public Service', 2)
;

SET IDENTITY_INSERT [dbo].[employeeposition] OFF;
