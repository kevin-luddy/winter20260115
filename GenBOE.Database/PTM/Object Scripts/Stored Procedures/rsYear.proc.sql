IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsYear]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsYear];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsYear]
(
	@Year varchar (8000)
)	
AS
SET NOCOUNT ON
/*
SP Used for SSRS Report Header
*/

/*Process Year*/
DECLARE @tblYear TABLE ([Year] CHAR(4))
IF @Year IS NULL OR @Year = 'All'
	BEGIN
		INSERT INTO @tblYear
		SELECT -1
	END
ELSE	
	BEGIN
		IF RIGHT(@Year, 1) <> ','
	      SET @Year = @Year + ','
	
		WHILE (SELECT CHARINDEX (',', @Year) ) > 1
			BEGIN
			      
				  INSERT INTO @tblYear
				  SELECT LEFT (@Year, CHARINDEX (',', @Year) -1)
				  SET @Year = RIGHT (@Year, LEN (@Year) - CHARINDEX (',', @Year) )
			      
			END
	END




DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblYear WHERE Year = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN
SELECT @listStr = COALESCE(@listStr+',' ,'') + [Year]
FROM @tblYear
END



SELECT @listStr

GO

GRANT EXECUTE ON OBJECT::dbo.rsYear TO generationReporter;
GO