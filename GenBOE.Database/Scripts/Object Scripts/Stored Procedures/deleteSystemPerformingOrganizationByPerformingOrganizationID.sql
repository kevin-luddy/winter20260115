IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSystemPerformingOrganizationByPerformingOrganizationID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSystemPerformingOrganizationByPerformingOrganizationID];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteSystemPerformingOrganizationByPerformingOrganizationID]
(
@PerformingOrganizationID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deletePerformingOrganizationByPerformingOrganizationID]
**		Desc: Delete PerformingOrganization based on its PerformingOrganizationID
**			
**
**		Auth: Don Canuso
**		Date: 2/7/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
******************************************************************************/




SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)
IF (SELECT UpdateDT FROM [dbo].[PerformingOrganization] WHERE PerformingOrganizationID = @PerformingOrganizationID) = @UpdateDT
		BEGIN
			UPDATE  [dbo].[PerformingOrganization] 
				SET DeletedFlag = 1
				WHERE 
					PerformingOrganizationID = @PerformingOrganizationID AND
					PerformingOrganizationListID = 1 
		END
	ELSE
		BEGIN
			SET @ErrorMessage =   'The Performing Organization with ID ' + CAST(@PerformingOrganizationID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
		
GO