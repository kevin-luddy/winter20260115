IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateOfflineApplication]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateOfflineApplication];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[updateOfflineApplication]
(
	@ApplicationName varchar(10),
	@IsOffline bit,
	@UpdateDate datetime2(7)
)
AS
/******************************************************************************
**          
**          Name: [updateOfflineApplication]
**          Desc: Update Offline Application
**
**          Auth: RJ Anzalone
**          Date: 1/16/2019
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                 Description:
**          --------    --------                ---------------------------------------
**
*******************************************************************************/

SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDate FROM [dbo].[OfflineApplication] WHERE [ApplicationName] = @ApplicationName) = @UpdateDate
	BEGIN
		SET @UpdateDate = GETDATE()
		UPDATE [dbo].[OfflineApplication]
			SET UpdateDate = @UpdateDate
				,IsOffline = @IsOffline
			WHERE
				ApplicationName = @ApplicationName
	END
ELSE
	BEGIN
		SET @ErrorMessage = 'The offline status of ' + @ApplicationName + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
				@ErrorMessage, -- Message text.
				11, -- Severity,
				1 -- State,
				)
		RETURN
	END

GO