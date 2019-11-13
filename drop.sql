-- USE [HRLeave];
-- GO

-- DROP TABLE [dbo].[employeeauthorization];
-- DROP TABLE [dbo].[authorization];
-- DROP TABLE [dbo].[employee];

-- USE master;
-- GO
-- ALTER DATABASE [HRLeave] 
-- SET SINGLE_USER 
-- WITH ROLLBACK IMMEDIATE;
-- GO

-- DROP DATABASE [HRLeave];
-- GO

/* Delete Database Backup and Restore History from MSDB System Database */

EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'HRLeave'
GO

/* Query to Get Exclusive Access of SQL Server Database before Dropping the Database  */

USE [master]
GO
ALTER DATABASE [HRLeave] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
GO

/* Query to Drop Database in SQL Server  */

DROP DATABASE [HRLeave]
GO
