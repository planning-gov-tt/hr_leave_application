USE [HRLeaveTestDb]; -- testing db
--USE [HRLeaveDevDb]; -- development db
GO
/****** Object:  StoredProcedure [dbo].[getSupervisors]    Script Date: 2/17/2020 11:43:24 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Tristan Sankar
-- Create date: 03/12/19
-- Description:	This procedure is used to get the names of all supervisors using sup_permissions as the check
-- Parameters:
--				@empId - the id of the employee requesting the list of supervisors
-- =============================================
ALTER PROCEDURE [dbo].[getSupervisors] @empId NVARCHAR(10)
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


	SELECT DISTINCT sup_e.employee_id, sup_e.first_name + ' ' + sup_e.last_name AS 'Supervisor Name', er.role_id
	FROM [dbo].[employee] sup_e

	--get dept of employee trying view supervisors
	JOIN dbo.employeeposition emp_ep
	ON emp_ep.employee_id = @empId

	--get supervisor's dept
	JOIN dbo.employeeposition sup_ep
	ON sup_ep.employee_id = sup_e.employee_id

	--get roles of each supervisor employee
	LEFT JOIN [dbo].[employeerole] er
	ON sup_e.employee_id = er.employee_id

	--ensure employee is supervisor
	INNER JOIN @roles AS r
	ON er.role_id = r.authorizedRoles

	WHERE sup_e.employee_id != @empId AND (sup_ep.dept_id = emp_ep.dept_id  OR (emp_ep.position_id = 4 AND sup_ep.position_id = 8));
	
END
