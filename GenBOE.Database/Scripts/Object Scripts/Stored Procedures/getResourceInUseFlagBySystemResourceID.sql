IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getResourceInUseFlagBySystemResourceID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getResourceInUseFlagBySystemResourceID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getResourceInUseFlagBySystemResourceID]
(
@ResourceID int
)
AS
/******************************************************************************
**		 
**		Name:	[getResourceInUseFlagBySystemResourceID]
**		Desc:	Returns Resource In Use Flag 
**			
**
**		Auth: Don Canuso
**		Date: 8/7/2012
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		11/26/12	dcanuso				WI 13308 Make SP go faster
**		11/28/12	dcanuso				Resource.DeletedFlag = 0
**		11/29/12	DCANUSO				REWRITE BECAUSE OF CASES OF DELETES
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
**		5/4/2017	brunworg			BOEJ-2125 Add T&M Resource Rates
**		5/24/2017	brunworg			BOEJ-2125 Fix typo in table name
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
**										
*******************************************************************************/

SET NOCOUNT ON 

IF @ResourceID IS NULL
	RETURN
	
IF EXISTS (
			SELECT DISTINCT R.ResourceID 
				FROM [dbo].[Resource] R
					INNER JOIN dbo.BOELaborType LT ON R.ResourceID = LT.ResourceID
					INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
					INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
					INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
			WHERE 
				R.ResourceID = @ResourceID AND
				R.DeletedFlag = 0 AND
				W.WorkspaceStateID NOT IN (4,5) /*Not Closed or Complete*/
			UNION
			SELECT DISTINCT R.ResourceID
				FROM [dbo].[Resource] R
					INNER JOIN dbo.TMResourceRate TMRR ON R.ResourceID = TMRR.TMResourceID	
					INNER JOIN dbo.Workspace W ON TMRR.WorkspaceID = W.WorkspaceID
			WHERE 
				R.ResourceID = @ResourceID AND
				R.DeletedFlag = 0 AND
				W.WorkspaceStateID NOT IN (4,5) /*Not Closed or Complete*/ 				
			)
BEGIN 
	SELECT 1 AS InUse
	RETURN
END
ELSE
BEGIN 
	SELECT 0 AS InUse
	RETURN
END

GO