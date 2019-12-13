IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsSegment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsSegment];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsSegment]
(
	@Segment varchar(8000)
)	
AS
/*
Stored Procedure used for the header in SSRS
Specifically Proposal Log Report


Change Log:
8/20/13	dcanuso	Task #21084: Proposal Log: Report Header Segment needs to be short name

*/

SET NOCOUNT ON
/*
SP Used for SSRS Report Header
*/
/*@Segment*/
DECLARE @tblSegment TABLE (SegmentID int)

IF @Segment IS NULL OR @Segment = 'All'
	BEGIN
		INSERT INTO @tblSegment
		SELECT -1 
	END
ELSE	
	BEGIN
		IF RIGHT(@Segment, 1) <> ','
	      SET @Segment = @Segment + ','
	
		WHILE (SELECT CHARINDEX (',', @Segment) ) > 1
			BEGIN
			      
				  INSERT INTO @tblSegment
				  SELECT LEFT (@Segment, CHARINDEX (',', @Segment) -1)
				  SET @Segment = RIGHT (@Segment, LEN (@Segment) - CHARINDEX (',', @Segment) )
			      
			END
	END
	



DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblSegment WHERE SegmentID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN
SELECT @listStr = COALESCE(@listStr+',' ,'') + S.SegmentShortName
FROM @tblSegment tS
	INNER JOIN dbo.SegmentLU S ON tS.SegmentID = S.SegmentID
END



SELECT
	CASE 
		WHEN @Segment IS NULL THEN NULL 
		ELSE @listStr
	END

GO