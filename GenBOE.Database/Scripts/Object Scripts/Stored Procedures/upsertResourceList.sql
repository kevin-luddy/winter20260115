IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertResourceList]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertResourceList];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertResourceList]
(
@ResourceListID int,
@ResourceListName varchar(50),
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name:	[upsertResourceList]
**		Desc:	Insert/Update Values into the Resource List Table
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

DECLARE @InsertedResourceList AS Table (ResourceListID int)


IF @ResourceListID  < 0  /*Insert Record*/
	BEGIN
	
		SET @UpdateDT = GETDATE()
		
		INSERT INTO [dbo].[ResourceList]
           ([ResourceListName]
           ,[UpdateDT])
		OUTPUT inserted.ResourceListID INTO @InsertedResourceList            
		VALUES
           (
           @ResourceListName,
           @UpdateDT
           )

	SELECT @ResourceListID = ResourceListID FROM @InsertedResourceList
	
	END



ELSE
	/*Update*/
	BEGIN
		IF (SELECT UpdateDT FROM dbo.ResourceList WHERE ResourceListID = @ResourceListID) = @UpdateDT
			BEGIN
			
				SET @UpdateDT = GETDATE()
				
				
				UPDATE [dbo].[ResourceList]
					SET [ResourceListName] = @ResourceListName,
						[UpdateDT] = @UpdateDT
				WHERE ResourceListID = @ResourceListID
			
			END
		
		ELSE
			BEGIN
				DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Resource List with Name ' + @ResourceListName + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
           			
END

IF @@ERROR = 0
	SELECT	@ResourceListID AS ResourceListID
GO