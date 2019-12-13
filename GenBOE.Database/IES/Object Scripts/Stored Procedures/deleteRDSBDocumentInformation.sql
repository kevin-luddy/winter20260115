IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRDSBDocumentInformation]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRDSBDocumentInformation];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteRDSBDocumentInformation]
(
	@Id	INT,
	@LastUpdateDT datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteRDSBDocumentInformation]
	**		Desc:	Delete RDSB Document Information
	**			
	**		
	**
	**		Auth: ranzalon
	**		Date: 1/25/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		02/19/2018	ranzalon			Updated to delete Xrefs
	*******************************************************************************/
	SET NOCOUNT ON 
	
	IF (SELECT LastUpdateDT FROM [dbo].[RDSBDocumentInformation] WHERE ID = @Id ) = @LastUpdateDT
		BEGIN
			DELETE FROM dbo.RDSBRateCodeXref WHERE RDSBDocumentInformationID = @Id;
			DELETE FROM dbo.RDSBSectionXref WHERE RDSBDocumentInformationID = @Id;
			DELETE FROM dbo.RDSBDocumentInformation WHERE ID = @Id;
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500);
			SET @ErrorMessage = 'The RDSB Document Information with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.';
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					);
			RETURN
		END
GO