IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getSystemResourceInUseFlag]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getSystemResourceInUseFlag];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getSystemResourceInUseFlag]
AS
/******************************************************************************
**		 
**		Name:	[getResourceInUseFlag]
**		Desc:	Returns Resources In Use for a SP Call
**			
**
**		Auth: Don Canuso
**		Date: 8/14/2012
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		11/11/12	dcanuso				SP taking too long - need re-write
**		11/27/12	dcanuso				Rewrote the SP for WI 13195
**		11/28/12	dcanuso				Resource.DeletedFlag should be = 0
**		12/4/12		dcanuso				Remove: Resource.DeletedFlag should be = 0
**		3/4/13		dcanuso				Bug #16304: 
**										HOTFIX - [System Administration >Manage Default Resources] 
**										Resource should be marked "In Use" only when 
**										a labor resource rate is associated with it
**										Resource should be marked "In Use" only when 
**										a labor resource rate is associated with it.
**										
**										Current Rules according to Don:
**										
**										It has a Labor Resource Rate associated with it
**										It has a Workspace Resource Rate associated with it
**										It is being used in ODC, Material, BOE, or Travel
**										It has a Workspace Resource with the same name and the Workspace Resource is in use
**										Fix should be:
**										
**										Only Rule 1 should apply.
**										
**										Rule 2 applies by default because a Workspace Resource Rate 
**										can only be mapped to an System Labor Resource Rate.
**										
**										Rule 3 should be removed.
**										
**										Rule 4 should be removed.
**										
**										See discussion here: https://eureka.isgs.lmco.com/#activity/283358
**	
**		3/25/13		dcanuso				Bug 16927 
**										Update the SPs for the "in use" rules for System Resource Rates 
**										(LRR) and System Admin Resources.
**										
**										"In Use" applys when:
**										It has a Labor Resource Rate associated with it 
**										AND the Resource is being used in a BOE
**										
**										It has a Labor Resource Rate associated with it 
**										AND a Workspace Resource Rate Mapped to it
**
**										Per SE: In Use rules are determined based off WS State not closed and complete
**										
**										Addional Notes:
**										If a System Resource + Rate is no longer being used in a BOE, 
**										the "in use" is removed from both
**										If a user attempts to delete the System Resource that has a Rate, 
**										a popup will be displayed asking the user to confirm the deletion of both.
**										See wireframe https://isgs-gen.external.lmco.com/sites/Estimating_Init/doclib14/Wireframes/Resource%20Rates.mht
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
**      4/3/24      e302876             PROPH-1617 - update stored procs for BRC
**										
*******************************************************************************/


SET NOCOUNT ON 

SELECT DISTINCT R.ResourceID AS SystemResourceID
	FROM [dbo].[Resource] R
		INNER JOIN dbo.BOELaborType LT ON R.ResourceID = LT.ResourceID
		INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
		INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
		INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
WHERE
	R.ResourceID IS NOT NULL AND
	R.ResourceListID = 1 AND
	R.DeletedFlag = 0 AND
	W.WorkspaceStateID NOT IN (4,5) /*Not Closed or Complete*/ 
UNION
SELECT DISTINCT R.BRCResourceID AS SystemResourceID
	FROM [dbo].[Resource] R
		INNER JOIN dbo.BOELaborType LT ON R.BRCResourceID = LT.BRCResourceID
		INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
		INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
		INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
WHERE 
	R.BRCResourceID IS NOT NULL AND
	R.ResourceListID = 1 AND
	R.DeletedFlag = 0 AND
	W.WorkspaceStateID NOT IN (4,5) /*Not Closed or Complete*/ 
				
GO