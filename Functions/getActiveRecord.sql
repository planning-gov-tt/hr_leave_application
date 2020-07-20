USE [HRLeaveDevDb]
GO
/****** Object:  UserDefinedFunction [dbo].[getActiveRecord]    Script Date: 7/16/2020 1:12:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Tristan Sankar
-- Create date: 07/16/2020
-- Description:	This function is used to get the id of the active employment record
-- Parameters:
--				@empId - the id of the employee who records must be checked
-- =============================================
ALTER FUNCTION [dbo].[getActiveRecord]
(
	-- Add the parameters for the function here
	@empId INT
)
RETURNS INT
AS
BEGIN
	-- Declare the return variable here
	DECLARE @result INT;

	-- Add the T-SQL statements to compute the return value here
	SELECT TOP 1 @result = id
	FROM [dbo].employeeposition
	WHERE employee_id = @empId AND (start_date <= GETDATE() AND (actual_end_date IS NULL OR GETDATE() <= actual_end_date))
	ORDER BY is_substantive_or_acting ASC;

	-- Return the result of the function
	RETURN @result

END
