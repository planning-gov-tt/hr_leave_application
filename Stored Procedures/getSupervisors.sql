USE [HRLeaveTestDb]
GO
/****** Object:  StoredProcedure [dbo].[getSupervisors]    Script Date: 12/4/2019 10:06:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Tristan Sankar
-- Create date: 03/12/19
-- Description:	This procedure is used to get the names of all supervisors with sup_permissions 
-- =============================================
ALTER PROCEDURE [dbo].[getSupervisors] @supervisorId NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @roles TABLE (
		authorizedRoles nvarchar(20) NOT NULL
	)
	INSERT INTO @roles (authorizedRoles)
	SELECT [role_id] AS 'authorizedRoles'
	FROM [dbo].[rolepermission] rp
	WHERE rp.permission_id = 'sup_permissions';

	IF (@supervisorId = '-1')
	BEGIN
		SELECT e.employee_id, e.first_name + ' ' + e.last_name AS 'Supervisor Name'
		FROM [dbo].[employee] e
		LEFT JOIN [dbo].[employeerole] er
		ON e.employee_id = er.employee_id
		INNER JOIN @roles AS r
		ON er.role_id = r.authorizedRoles;
	END
	ELSE
	BEGIN
		SELECT e.employee_id, e.first_name + ' ' + e.last_name AS 'Supervisor Name'
		FROM [dbo].[employee] e
		LEFT JOIN [dbo].[employeerole] er
		ON e.employee_id = er.employee_id
		INNER JOIN @roles AS r
		ON er.role_id = r.authorizedRoles
		WHERE e.employee_id != @supervisorId
	END
END
