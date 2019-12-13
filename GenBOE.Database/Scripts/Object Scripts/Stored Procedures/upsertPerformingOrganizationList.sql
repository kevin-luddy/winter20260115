IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertPerformingOrganizationList]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertPerformingOrganizationList];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertPerformingOrganizationList]
(
@PerformingOrganizationListID int,
@PerformingOrganizationListName varchar(50),
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name:	[upsertPerformingOrganizationList]
**		Desc:	Insert/Update Values into the PerformingOrganization List Table
**			
**		
**
**		Auth: Don Canuso
**		Date: 1/21/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedPerformingOrganizationList AS Table (PerformingOrganizationListID int)


IF @PerformingOrganizationListID  < 0  /*Insert Record*/
	BEGIN
	
		SET @UpdateDT = GETDATE()
		
		INSERT INTO [dbo].[PerformingOrganizationList]
           ([PerformingOrganizationListName]
           ,[UpdateDT])
		OUTPUT inserted.PerformingOrganizationListID INTO @InsertedPerformingOrganizationList            
		VALUES
           (
           @PerformingOrganizationListName,
           @UpdateDT
           )

	SELECT @PerformingOrganizationListID = PerformingOrganizationListID FROM @InsertedPerformingOrganizationList
	
	END



ELSE
	/*Update*/
	BEGIN
		IF (SELECT UpdateDT FROM dbo.PerformingOrganizationList WHERE PerformingOrganizationListID = @PerformingOrganizationListID) = @UpdateDT
			BEGIN
			
				SET @UpdateDT = GETDATE()
				
				
				UPDATE [dbo].[PerformingOrganizationList]
					SET [PerformingOrganizationListName] = @PerformingOrganizationListName,
						[UpdateDT] = @UpdateDT
				WHERE PerformingOrganizationListID = @PerformingOrganizationListID
			
			END
		
		ELSE
			BEGIN
				DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Performing Organization List with Name ' + @PerformingOrganizationListName + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
           			
END

IF @@ERROR = 0
	SELECT	@PerformingOrganizationListID AS PerformingOrganizationListID
GO