IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsLineOfBusiness]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsLineOfBusiness];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsLineOfBusiness]
(
	@LOB varchar (8000)
)	
AS
/******************************************************************************
**		 
**		Name: [rsLineOfBusiness]
**		Desc: SP Used for SSRS Report Header
**			
**
**		Auth: Unknown
**		Date: Unknown
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		unknown		unknown				unknown
**
**		4/13/2017	ranzalon			BOEJ-2096 Update Line Of Business to use correct table (Product Line)
**		6/06/2018	brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
*******************************************************************************/
SET NOCOUNT ON
/*Line of Business*/
DECLARE @tblLineOfBusiness TABLE (LineOfBusinessID int)

IF @LOB IS NULL OR @LOB = 'All'
	BEGIN
		INSERT INTO @tblLineOfBusiness
		SELECT -1 
	END
ELSE	
	BEGIN
		IF RIGHT(@LOB, 1) <> ','
	      SET @LOB = @LOB + ','
	
		WHILE (SELECT CHARINDEX (',', @LOB) ) > 1
			BEGIN
			      
				  INSERT INTO @tblLineOfBusiness
				  SELECT LEFT (@LOB, CHARINDEX (',', @LOB) -1)
				  SET @LOB = RIGHT (@LOB, LEN (@LOB) - CHARINDEX (',', @LOB) )
			      
			END
	END


DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblLineOfBusiness WHERE LineOfBusinessID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN

SELECT @listStr = 
			COALESCE(@listStr+', ' ,'') + 
			LOB.LineOfBusinessName
FROM @tblLineOfBusiness tLOB
	INNER JOIN dbo.LineOfBusinessLU LOB ON LOB.LineOfBusinessID = tLOB.LineOfBusinessID
END


SELECT
	CASE 
		WHEN @LOB IS NULL THEN NULL 
		ELSE @listStr
	END

GO

GRANT EXECUTE ON OBJECT::dbo.rsLineOfBusiness TO generationReporter;
GO