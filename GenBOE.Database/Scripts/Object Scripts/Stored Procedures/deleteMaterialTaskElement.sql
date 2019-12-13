IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMaterialTaskElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMaterialTaskElement];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteMaterialTaskElement]
(
@MaterialTaskElementID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteMaterialTaskElement]
**		Desc: Delete Material Task Element in Material Section of BOE and all sub-elements
**			
**		
**
**		Auth: Don Canuso
**		Date: 6/22/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/18/11		dcanuso				Adding Custom Field Tables
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
**		8/27/14		dcanuso				Image Story
**		10/3/14		dcanuso				Image Story Removal
**      6/24/16     twilson3            Fix In-Use Flag for Custom Fields
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[MaterialTaskElement] WHERE MaterialTaskElementID = @MaterialTaskElementID) = @UpdateDT
		BEGIN

			/*Used to Handle In Use Processing*/
			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID 
			FROM dbo.BOE B
				INNER JOIN dbo.MaterialTaskElement TE ON B.BOEID = TE.BOEID
			WHERE TE.MaterialTaskElementID = @MaterialTaskElementID


			DELETE FROM dbo.MaterialTaskElement
			WHERE
				MaterialTaskElementID = @MaterialTaskElementID	
				
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Material Task Element with ID ' + CAST(@MaterialTaskElementID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
 
		END

GO