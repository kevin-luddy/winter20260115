IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspaceResourceInUseFlagByMultipleResources]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspaceResourceInUseFlagByMultipleResources];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getWorkspaceResourceInUseFlagByMultipleResources]
(
@ResourceID VARCHAR (MAX),
@ResourceListID int
)
AS
/******************************************************************************
**		 
**		Name:	[getWorkspaceResourceInUseFlagByMultipleResources]
**		Desc:	Returns ResourceID for those and In Use Flag
**			
**
**		Auth: Don Canuso
**		Date: 7/24/2012
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		11/28/12	dcanuso				Make sure Resource is not Deleted (Delete Flag)
**		9/26/13		dcanuso				New Stored Procedure based on another SP: 
**										[dbo].[getWorkspaceResourceInUseFlagByResourceID]
**										Notes kept for history
**										What the new stored procedure will need to do:
**										I need a stored procedure for a production issue 
**										that will take in multiple ResrouceIDs and 
**										one ResourceListID and return me only the ResourceIDs 
**										that are in use. 
**										This stored procedure will be very much like the 
**										existing getWorkspaceResourceInUseFlgByResourceID 
**										except it will take multiple Resource IDs and return 
**										only the list of the Resource IDs that are in use.
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
*******************************************************************************/
SET NOCOUNT ON 

IF @ResourceID IS NULL 
	BEGIN
	
		DECLARE @listStr VARCHAR(MAX)
		SELECT @listStr = COALESCE(@listStr+',' ,'') + CAST (ResourceID AS varchar(100))
		FROM [dbo].[Resource] 
		WHERE ResourceListID = @ResourceListID
		
		SET @ResourceID = @listStr
		
	END




/*Resource*/
IF RIGHT(@ResourceID, 1) <> ','
      SET @ResourceID = @ResourceID + ','

DECLARE @Resource TABLE (ResourceID INT PRIMARY KEY)


WHILE (SELECT CHARINDEX (',', @ResourceID) ) > 1
BEGIN
      
      INSERT INTO @Resource
      SELECT LEFT (@ResourceID, CHARINDEX (',', @ResourceID) -1)
      SET @ResourceID = RIGHT (@ResourceID, LEN (@ResourceID) - CHARINDEX (',', @ResourceID) )
      
END


DECLARE @ResultSet TABLE
(
	ResourceID int
)


INSERT INTO @ResultSet
	SELECT T.ResourceID FROM  [dbo].[ODCType] T 
		INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID	
		INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
		INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		INNER JOIN dbo.WorkspaceResource WR ON W.WorkspaceID = WR.WorkspaceID
		INNER JOIN @Resource tR ON T.ResourceID = tR.ResourceID
		INNER JOIN @Resource tR2 ON WR.SystemResourceID = tR2.ResourceID
	WHERE 
	/*T.ResourceID = @ResourceID AND */
	/*WR.SystemResourceID = @ResourceID AND*/
	W.ResourceListID = @ResourceListID

INSERT INTO @ResultSet
	SELECT T.ResourceID FROM  [dbo].[BOELaborType] T
		INNER JOIN dbo.BOETaskElement TE ON T.BOETaskElementID = TE.BOETaskElementID
		INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
		INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		INNER JOIN dbo.WorkspaceResource WR ON W.WorkspaceID = WR.WorkspaceID
		INNER JOIN @Resource tR ON T.ResourceID = tR.ResourceID
		INNER JOIN @Resource tR2 ON WR.SystemResourceID = tR2.ResourceID
	WHERE 
	/*T.ResourceID = @ResourceID AND */
	/*WR.SystemResourceID = @ResourceID AND*/
	W.ResourceListID = @ResourceListID

INSERT INTO @ResultSet
/* Special Processing for Travel */
		/* Special Processing for Travel */
		SELECT R.ResourceID
		FROM [dbo].[Resource] R
		INNER JOIN dbo.WorkspaceResource WR ON R.ResourceID = WR.SystemResourceID
		INNER JOIN dbo.Workspace W ON WR.WorkspaceID = W.WorkspaceID
		INNER JOIN 
			(
				SELECT BOEID, WorkspaceID
				FROM dbo.BOE
			) B ON W.WorkspaceID = B.WorkspaceID
		INNER JOIN 
			(
				SELECT [TravelTripTaskElementID]
				,[BOEID]
				FROM dbo.TravelTripTaskElement
			) TE ON B.BOEID = TE.BOEID
		INNER JOIN 
			(
				SELECT [TravelTripID]
				,[SegmentID]
				,[TravelTripTaskElementID]
				FROM dbo.TravelTrip
			) TT ON TE.TravelTripTaskElementID = TT.TravelTripTaskElementID 

		INNER JOIN @Resource tR ON R.ResourceID = tR.ResourceID
		INNER JOIN @Resource tR2 ON WR.SystemResourceID = tR2.ResourceID

		WHERE 
		WR.ResourceListID = @ResourceListID AND
		W.ResourceListID = @ResourceListID AND
		R.SegmentID = TT.SegmentID AND
		R.CostElementID = 6 /*Travel*/ AND
		R.DeletedFlag = 0

SELECT DISTINCT ResourceID AS [ResourceID]
FROM @ResultSet 
WHERE ResourceID IS NOT NULL

GO