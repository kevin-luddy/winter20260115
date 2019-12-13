IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteAllBOEsInWs]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteAllBOEsInWs];
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[deleteAllBOEsInWs]
(
@WsId int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteAllBOEsInWs]
**		Desc: Delete all parts of BOEs within a workspace
**			
**		
**
**		Auth: Dusan
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		9/11/2017	twilson3			BOEJ-2519 Update for new Project Map Tables
**		10/11/2017	twilson3			BOEJ-2537 Removed custom field stuff not used in Project Map
*****************************************************************************/
SET NOCOUNT ON 
	IF (SELECT UpdateDT FROM [dbo].[Workspace] WHERE WorkspaceId = @wsId) = @UpdateDT
		BEGIN
			SET @UpdateDT = GETDATE()
			
			DELETE FROM dbo.ProjectMapSpread
			WHERE WorkspaceId = @wsId

			DELETE FROM dbo.ProjectMap
			WHERE WorkspaceId = @wsId
						
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =    'The Workspace with ID ' + CAST(@wsId  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO