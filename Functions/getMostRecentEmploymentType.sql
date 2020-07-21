USE [HRLeaveDevDb]
GO
/****** Object:  UserDefinedFunction [dbo].[getMostRecentEmploymentType]    Script Date: 7/21/2020 2:13:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Tristan Sankar
-- Create date: 07/21/2020
-- Description:	This function is used to return the most recent employment type whether from an active or inactive method
-- Parameters:
--				@employeeId - the id of the employee
-- =============================================
ALTER FUNCTION [dbo].[getMostRecentEmploymentType]
(
	-- Add the parameters for the function here
	@employeeId NVARCHAR(10)
)
RETURNS NVARCHAR
AS
BEGIN
	-- Declare the return variable here
	DECLARE @result NVARCHAR(20)

	SELECT @result = EMP_INFO.employment_type
	FROM (
		SELECT ROW_NUMBER() OVER(PARTITION BY ep.employee_id ORDER BY ISNULL(ep.actual_end_date, CAST('1/1/9999' AS DATE)) DESC) as RowNum, ep.employment_type
		FROM dbo.employeeposition ep
		WHERE ep.employee_id = @employeeId 
	) EMP_INFO
	WHERE RowNum = 1 

	-- Return the result of the function
	RETURN @result

END
