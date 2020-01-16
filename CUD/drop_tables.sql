--USE [HRLeave]; -- chris local db
USE [HRLeaveTestDb]; -- dbserver
GO

DROP TABLE [dbo].[employeeposition];
DROP TABLE [dbo].[employmenttype];
DROP TABLE [dbo].[position];
DROP TABLE [dbo].[department];
DROP TABLE [dbo].[leavetransaction];
DROP TABLE [dbo].[leavetype];
DROP TABLE [dbo].[transactionstate];
DROP TABLE [dbo].[assignment];
DROP TABLE [dbo].[employeerole];
DROP TABLE [dbo].[employee];
DROP TABLE [dbo].[rolepermission];
DROP TABLE [dbo].[role];
DROP TABLE [dbo].[permission];
DROP TABLE [dbo].[auditlog];