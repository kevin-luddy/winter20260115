IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MapToNewMoqType]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	DROP FUNCTION [dbo].[MapToNewMoqType];
GO

CREATE FUNCTION dbo.MapToNewMoqType(@MoqValue INT, @wsCreationDate DATE) RETURNS INT AS
/******************************************************************************
**
**	Name: MapToNewMoqType
**	Desc: Maps old MOQ Types to new ones, if appropriate. This matches the MOQ 
**			Types enum in C# & the method MapToNew that works on that enum.
**
*******************************************************************************
**	Change History
*******************************************************************************
**	Date:		Author:		Description:
**	--------	--------	---------------------------------------------------
**	2020-11-11	Dusan		Initial creation.
**	2020-11-24	Dusan		Updated to use WasWorkspaceCreatedAfterNewMoqTypes
**  2020-12-3	Dusan		BOEJ-4954: Fixed an issue w/ NULL input
**	2021-04-5	RJ			BOEJ-5120 - Mapping updates
*******************************************************************************/

BEGIN
	DECLARE @result INT = @MoqValue;
	IF((SELECT dbo.WasWorkspaceCreatedAfterNewMoqTypes(@wsCreationDate)) = 1)
		BEGIN
			SELECT @result = 
				CASE 
					WHEN @MoqValue IN (5001, 2001, 1008, 1001) THEN 5001
					WHEN @MoqValue IN (5002, 2002) THEN 5002
					WHEN @MoqValue IN (5003, 2003, 1003) THEN 5003
					WHEN @MoqValue IN (5004, 2004, 2005, 2006, 1007, 1005) THEN 5004
					WHEN @MoqValue IN (5005, 1004) THEN 5005
					WHEN @MoqValue IN (5006) THEN 5006
					WHEN @MoqValue IN (5007, 2007, 1006) THEN 5007
					WHEN @MoqValue IN (5008, 2008, 1002) THEN 5008
					WHEN @MoqValue IN (5009, 1009) THEN 5009
					ELSE NULL
				END
		END

	RETURN @result;
END
GO