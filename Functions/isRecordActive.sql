USE [HRLeaveDevDb]
GO
/****** Object:  UserDefinedFunction [dbo].[isRecordActive]    Script Date: 7/20/2020 12:05:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Tristan Sankar
-- Create date: 07/16/2020
-- Description:	This function is used to return a true or false value determining whether a given employment record is active or not
-- Parameters:
--				@recordId - the id of the employment record
-- =============================================
ALTER FUNCTION [dbo].[isRecordActive]
(
	-- Add the parameters for the function here
	@recordId INT
)
RETURNS BIT
AS
BEGIN
	-- Declare the return variable here
	DECLARE @result BIT;

	-- Add the T-SQL statements to compute the return value here
	SELECT @result = IIF(start_date <= GETDATE() AND (actual_end_date IS NULL OR GETDATE() <= actual_end_date), 1, 0)
	FROM [dbo].employeeposition
	WHERE id = @recordId 

	--SELECT @result = IIF(start_date <= Cast('7/16/2020' as datetime) AND (actual_end_date IS NULL OR Cast('7/16/2020' as datetime) <= actual_end_date), 1, 0)
	--FROM [dbo].employeeposition
	--WHERE id = @recordId 

	-- Return the result of the function
	RETURN @result

END
