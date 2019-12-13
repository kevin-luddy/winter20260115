IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteODCTaskElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteODCTaskElement];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
 
CREATE PROCEDURE [dbo].[deleteODCTaskElement]
(
@ODCTaskElementID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteODCTaskElement]
**		Desc: Delete ODC Task Element in ODC Section of BOE and all sub-elements
**			
**		
**
**		Auth: Don Canuso
**		Date: 6/14/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/18/11		dcanuso				Adding Custom Field Tables
**		11/23/11	dcanuso				Added PO Processing of In Use
**		12/15/11	dcanuso				WI 6166
**		12/21/11	dcanuso				WI 6021
**										Need to check if a Travel Resource Rate 
**										is being used/not used
**										and update accordingly
**		1/10/12		dcanuso				WI 
**										6470: Trip In Use Fixes
**										https://eureka.isgs.lmco.com/#activity/151789
**										6624: After WI 6622, we now need to check that 
**										the originating Labor Resource Rate is in 
**										use as we did for trips.
**										6339: Confirm in-use is working for workspace
**										and labor resource rates
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		2/12/13		dcanuso				WI14842 Redesign Performing Organization
**		8/27/14		dcanuso				Image Story
**		10/3/14		dcanuso				Image Story Removal
**      6/24/16     twilson3            Fix In-Use Flag for Custom Fields
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[ODCTaskElement] WHERE ODCTaskElementID = @ODCTaskElementID) = @UpdateDT
		BEGIN
		

			DELETE FROM dbo.ODCSpread
				FROM dbo.ODCSpread S
				INNER JOIN dbo.ODCType T ON S.ODCTypeID = T.ODCTypeID
				INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID
			WHERE
				TE.ODCTaskElementID = @ODCTaskElementID

			DELETE FROM dbo.ODCType
				FROM dbo.ODCType T
				INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID
			WHERE
				TE.ODCTaskElementID = @ODCTaskElementID

			DELETE FROM dbo.ODCTaskElement
			WHERE
				ODCTaskElementID = @ODCTaskElementID	


			/*Used to Handle In Use Processing*/
			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID 
			FROM dbo.BOE B
				INNER JOIN dbo.ODCTaskElement TE ON B.BOEID = TE.BOEID
			WHERE TE.ODCTaskElementID = @ODCTaskElementID
			
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The ODC Task Element with ID ' + CAST(@ODCTaskElementID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
 
		END


GO