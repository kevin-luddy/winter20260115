IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[refreshCustomFieldInUseFlagByWorkspaceID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[refreshCustomFieldInUseFlagByWorkspaceID];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[refreshCustomFieldInUseFlagByWorkspaceID]
(
@WorkspaceId int
)
AS
/******************************************************************************
**		 
**		Name: refreshCustomFieldInUseFlagByWorkspaceID
**		Desc: Refreshes the In Use flag on all Custom Fields for a Workspace

**
**		Auth: Timothy I. Wilson
**		Date: 6/24/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/3/18		ranzalon			Updated for RMS Zone Travel Trips
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
*******************************************************************************/
SET NOCOUNT ON 

UPDATE dbo.CustomFieldValue
		SET CustomFieldValueInUseFlag = CASE 
				WHEN tX.[CustomFieldValueID] IS NULL AND ttX.[CustomFieldValueID] IS NULL 
                    AND bX.[CustomFieldValueID] IS NULL AND blX.[CustomFieldValueID] IS NULL AND btX.[CustomFieldValueID] IS NULL
					AND mttX.[MSTCustomFieldValueID] IS NULL THEN 0
				ElSE 1
			END,
			UpdateDT = GETDATE()
		FROM dbo.CustomFieldValue CFV 
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			LEFT OUTER JOIN [dbo].[TravelTripCustomFieldValueXREF] tX ON CFV.[CustomFieldValueID] = tX.[CustomFieldValueID]
            LEFT OUTER JOIN [dbo].[TravelTripTaskElementCustomFieldValueXREF] ttX ON CFV.[CustomFieldValueID] = ttX.[CustomFieldValueID]
            LEFT OUTER JOIN [dbo].[BOECustomFieldValueXREF] bX ON CFV.[CustomFieldValueID] = bX.[CustomFieldValueID]
            LEFT OUTER JOIN [dbo].[BOELaborTypeCustomFieldValueXREF] blX ON CFV.[CustomFieldValueID] = blX.[CustomFieldValueID]
            LEFT OUTER JOIN [dbo].[BOETaskElementCustomFieldValueXREF] btX ON CFV.[CustomFieldValueID] = btX.[CustomFieldValueID]
			LEFT OUTER JOIN [dbo].[MSTTravelTripCustomFieldValueXREF] mttX ON CFV.[CustomFieldValueID] = mttX.[MSTCustomFieldValueID]
		WHERE 
			CF.WorkspaceID = @WorkspaceID

GO
