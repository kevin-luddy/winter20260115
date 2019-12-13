IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getPerformingOrganizationInUseFlagByPerformingOrganizationListID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getPerformingOrganizationInUseFlagByPerformingOrganizationListID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getPerformingOrganizationInUseFlagByPerformingOrganizationListID]
(
@PerformingOrganizationListID int
)
AS
/******************************************************************************
**		 
**		Name:	[getPerformingOrganizationInUseFlagByPerformingOrganizationListID]
**		Desc:	Returns PerformingOrganization In Use Flag
**			
**
**		Auth: Don Canuso
**		Date: 2/4/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**      4/6/2016    Timothy Wilson      Added Summary BOE selections if Performing Org Summary BOEs
**		11/9/2016	tglick				Added MST Travel Trips Perf Org Id 
**		12/7/17		twilson3			BOEJ-1994 - Remove Summary BOE
*******************************************************************************/
SET NOCOUNT ON 
	
IF @PerformingOrganizationListID >1  /*Not the Default List PerformingOrganization*/
BEGIN

DECLARE @Workspace TABLE (WorkspaceID INT, PerformingOrganizationListID int) 
INSERT INTO @Workspace
SELECT WorkspaceID, PerformingOrganizationListID
FROM dbo.Workspace 
WHERE PerformingOrganizationListID = @PerformingOrganizationListID

DECLARE @BOE TABLE
(
BOEID int PRIMARY KEY,
WorkspaceID int
)
INSERT INTO @BOE
SELECT B.BOEID, B.WorkspaceID
FROM dbo.BOE B
	INNER JOIN @Workspace W ON B.WorkspaceID = W.WorkspaceID


DECLARE @ResultSet TABLE (SystemPerformingOrganizationID int)

INSERT INTO @ResultSet (SystemPerformingOrganizationID)
SELECT DISTINCT  T.PerformingOrganizationID 
FROM  [dbo].[ODCType] T
		INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID	
		INNER JOIN @BOE B ON TE.BOEID = B.BOEID
			


INSERT INTO @ResultSet (SystemPerformingOrganizationID)
SELECT DISTINCT T.PerformingOrganizationID 
FROM  [dbo].[BOELaborType] T
		INNER JOIN dbo.BOETaskElement TE ON T.BOETaskElementID = TE.BOETaskElementID
		INNER JOIN @BOE B ON TE.BOEID = B.BOEID
	



INSERT INTO @ResultSet (SystemPerformingOrganizationID)
SELECT DISTINCT T.PerformingOrganizationID 
FROM  dbo.TravelTrip T
	INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN @BOE B ON TE.BOEID = B.BOEID
	

INSERT INTO @ResultSet (SystemPerformingOrganizationID)
SELECT DISTINCT TT.PerformingOrganizationID 
FROM  dbo.MSTTravelTrip TT
	INNER JOIN dbo.TravelTripTaskElement TE ON TE.TravelTripTaskElementID = TT.TravelTripTaskElementID
	INNER JOIN @BOE B ON TE.BOEID = B.BOEID
	
	SELECT DISTINCT SystemPerformingOrganizationID FROM  @ResultSet 
	WHERE SystemPerformingOrganizationID IS NOT NULL
	
END


GO

