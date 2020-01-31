USE [HRLeaveTestDb]
GO
/****** Object:  StoredProcedure [dbo].[getSupervisors]    Script Date: 1/29/2020 11:29:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Tristan Sankar
-- Create date: 03/12/19
-- Description:	This procedure is used to get the names of all supervisors using sup_permissions as the check
-- Parameteers:
--				@supervisorId - the id of the employee if they are a supervisor
--				@empId - the id of the employee requesting the list of supervisors
-- =============================================
ALTER PROCEDURE [dbo].[getSupervisors] @supervisorId NVARCHAR(10), @empId NVARCHAR(10)
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
	-- if user making call to get supervisor names is not a supervisor
		SELECT DISTINCT sup_e.employee_id, sup_e.first_name + ' ' + sup_e.last_name AS 'Supervisor Name'
		FROM [dbo].[employee] sup_e

		--get dept of employee trying view supervisors
		JOIN dbo.employeeposition emp_ep
		ON emp_ep.employee_id = @empId

		--get supervisor's dept
		JOIN dbo.employeeposition sup_ep
		ON sup_ep.employee_id = sup_e.employee_id

		--get roles of each employee
		LEFT JOIN [dbo].[employeerole] er
		ON sup_e.employee_id = er.employee_id

		--ensure employee is supervisor
		INNER JOIN @roles AS r
		ON er.role_id = r.authorizedRoles

		WHERE sup_ep.dept_id = emp_ep.dept_id OR sup_ep.position_id = '8' OR sup_ep.position_id = '9';
	END
	ELSE
	BEGIN
	-- if user making call to get supervisor names is a supervisor, in which case, his/her own name must not be returned
		SELECT DISTINCT sup_e.employee_id, sup_e.first_name + ' ' + sup_e.last_name AS 'Supervisor Name'
		FROM [dbo].[employee] sup_e

		--get employee dept
		JOIN dbo.employeeposition emp_ep
		ON emp_ep.employee_id = @empId

		--get supervisor's dept
		JOIN dbo.employeeposition sup_ep
		ON sup_ep.employee_id = sup_e.employee_id

		--get roles of each employee
		LEFT JOIN [dbo].[employeerole] er
		ON sup_e.employee_id = er.employee_id

		--ensure employee are supervisors
		INNER JOIN @roles AS r
		ON er.role_id = r.authorizedRoles

		WHERE (sup_ep.dept_id = emp_ep.dept_id OR sup_ep.position_id = '8' OR sup_ep.position_id = '9') AND sup_e.employee_id != @supervisorId;
	END
END
