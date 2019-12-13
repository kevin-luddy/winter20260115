IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsProgramArea]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsProgramArea];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsProgramArea]
(
	@PA varchar (8000)
)	
AS
/******************************************************************************
**		 
**		Name: [rsProgramArea]
**		Desc: SP Used for SSRS Report Header
**			
**
**		Auth: brunworg
**		Date: 6/08/2018 (copied from rsLineOfBusiness)
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON
/*Program Area*/
DECLARE @tblProgramArea TABLE (ProgramAreaID int)

IF @PA IS NULL OR @PA = 'All'
	BEGIN
		INSERT INTO @tblProgramArea
		SELECT -1 
	END
ELSE	
	BEGIN
		IF RIGHT(@PA, 1) <> ','
	      SET @PA = @PA + ','
	
		WHILE (SELECT CHARINDEX (',', @PA) ) > 1
			BEGIN
			      
				  INSERT INTO @tblProgramArea
				  SELECT LEFT (@PA, CHARINDEX (',', @PA) -1)
				  SET @PA = RIGHT (@PA, LEN (@PA) - CHARINDEX (',', @PA) )
			      
			END
	END


DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblProgramArea WHERE ProgramAreaID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN

SELECT @listStr = 
			COALESCE(@listStr+', ' ,'') + 
			PA.ProgramAreaName
FROM @tblProgramArea tPA
	INNER JOIN dbo.ProgramAreaLU PA ON PA.ProgramAreaID = tPA.ProgramAreaID
END


SELECT
	CASE 
		WHEN @PA IS NULL THEN NULL 
		ELSE @listStr
	END

GO

GRANT EXECUTE ON OBJECT::dbo.rsProgramArea TO generationReporter;
GO