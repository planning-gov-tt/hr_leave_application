--USE [HRLeave]; -- chris local db
 USE [HRLeaveTestDb]; -- dbserver
GO

CREATE PROCEDURE gridViewGetData @gridViewType NVARCHAR (5), @currentEmployeeId NVARCHAR (10)
AS
BEGIN
  SELECT
    lt.transaction_id transaction_id,
    lt.created_at date_submitted,		
                              
    e.employee_id employee_id,
    LEFT(e.first_name, 1) + '. ' + e.last_name AS employee_name,

    lt.leave_type leave_type,
    lt.start_date start_date,
    lt.end_date end_date,

    s.employee_id supervisor_id,
    LEFT(s.first_name, 1) + '. ' + s.last_name AS supervisor_name,
    lt.supervisor_edit_date supervisor_edit_date,

    hr.employee_id hr_manager_id,
    LEFT(hr.first_name, 1) + '. ' + hr.last_name AS hr_manager_name,
    lt.hr_manager_edit_date hr_manager_edit_date,
    
    lt.status status,
    lt.comments comments,
    lt.file_path file_path
  FROM 
    [dbo].[leavetransaction] lt 
    INNER JOIN [dbo].[employee] e ON e.employee_id = lt.employee_id
    INNER JOIN [dbo].[employee] s ON s.employee_id = lt.supervisor_id
    LEFT JOIN [dbo].[employee] hr ON hr.employee_id = lt.hr_manager_id 
  WHERE (
	((@gridViewType = 'emp') AND (@currentEmployeeId = e.employee_id))
	
	OR
	((@gridViewType = 'sup') AND (@currentEmployeeId = supervisor_id))

	OR -- hr
	((@gridViewType = 'hr') AND (status IN ('Recommended', 'Approved', 'Not Approved')))
  );	
END;  