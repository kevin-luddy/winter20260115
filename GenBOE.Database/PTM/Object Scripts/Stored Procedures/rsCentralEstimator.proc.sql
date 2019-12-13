IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsCentralEstimator]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsCentralEstimator];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsCentralEstimator]
(
	@CentralEstimator varchar(8000)
)	
AS
SET NOCOUNT ON
/*
SP Used for SSRS Report Header
*/

/*
This is being used for all Lead Estimators (removed central/field roletype)
*/
DECLARE @tblCentralEstimator TABLE (CentralEstimatorID int)

IF @CentralEstimator IS NULL OR @CentralEstimator = 'All'
	BEGIN
		INSERT INTO @tblCentralEstimator
		SELECT  -1
	END
ELSE	
	BEGIN
	IF RIGHT(@CentralEstimator, 1) <> ','
		  SET @CentralEstimator = @CentralEstimator + ','

	WHILE (SELECT CHARINDEX (',', @CentralEstimator) ) > 1
	BEGIN
	      
		  INSERT INTO @tblCentralEstimator
		  SELECT LEFT (@CentralEstimator, CHARINDEX (',', @CentralEstimator) -1)
		  SET @CentralEstimator = RIGHT (@CentralEstimator, LEN (@CentralEstimator) - CHARINDEX (',', @CentralEstimator) )
	      
	END

END

DECLARE @listStr VARCHAR(1000)

DECLARE @Results TABLE (DisplayName varchar(100)) /*USED TO TAKE CARE OF DUPLICATES*/
INSERT INTO @Results
SELECT DISTINCT U.DisplayName
FROM @tblCentralEstimator tFE
	INNER JOIN dbo.ProposalUserRole PUR ON tFE.CentralEstimatorID = PUR.UserID
						INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
					WHERE 
						PUR.RoleID = 3 /*Pricer*/ 


IF EXISTS (SELECT 1 FROM @tblCentralEstimator WHERE CentralEstimatorID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN
	SELECT @listStr = COALESCE(@listStr+',' ,'') + DisplayName
	FROM @Results
END



SELECT
	CASE 
		WHEN @CentralEstimator IS NULL THEN NULL 
		ELSE @listStr
	END

GO

GRANT EXECUTE ON OBJECT::dbo.rsCentralEstimator TO generationReporter;
GO