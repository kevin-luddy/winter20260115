EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.24';
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRevision];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteRevision]
(
	@Id			int,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteRevision]
	**		Desc:	Delete a PPR&D Revision
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Corrected order of delete statements.
	**										Added code to delete associated Document
	**										and Sections.
	**		7/10/2017	brunworg			Added code to raise an exception if
	**										deleting Revision with associated 
	**										CostVolume.
	**		7/10/2017	brunworg			Remove references to ActivityTypeMap and
	**										ProPricerActivityTypeXref tables.
	**		8/04/2017	brunworg			Updated to reflect new data model with
	**										revisionId in BurdenPoolLU table.
	**		8/10/2017	brunworg			Remove transaction handling.
	**		8/21/2017	brunworg			Remove PPRD, Document, DocumentTypeLU,
	**										CostVolume, and CostVolumeRateCode tables.
	**		8/22/2017	brunworg			Redesign Section and related tables.
	**		8/24/2017	brunworg			Updated to delete ProPricerBurdenPoolMap  
	**										and ProPricerRateCodeXref entries before 
	**										RateCodes.
	**		9/8/2017	brunworg			Modified ProPricerBurdenRateMap delete statement.
	**		1/2/2018	twilson3			BOEJ-2704 File Attachments
	**		1/25/2018	ranzalon			BOEJ-2715 - RDSB Document Information
	**      7/12/2023	twilson3			PROPH-917 - Delete XRefs for RateCode and Section
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;

	IF (SELECT UpdateDate FROM [dbo].[Revision] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			BEGIN TRY
				DELETE FROM [dbo].[FileAttachment] WHERE [RevisionId] = @Id
				DELETE FROM [dbo].[ProPricerBurdenRateMap] 
				WHERE EXISTS
					(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
				DELETE FROM [dbo].[ProPricerRateCodeXref] 
				WHERE EXISTS 
					(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
				DELETE FROM [dbo].[RateCodeYear] 
				WHERE EXISTS 
					(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
				DELETE FROM [dbo].[RDSBRateCodeXref] 
				WHERE EXISTS
					(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
				DELETE FROM [dbo].[RateCode] WHERE RevisionID = @Id
				DELETE FROM [dbo].[BurdenPoolLU] where RevisionID = @Id	
				DELETE FROM [dbo].[RDSBSectionXref]
				WHERE EXISTS
					(SELECT * FROM [dbo].[Section] sec where  SectionID = sec.ID AND sec.RevisionID = @Id)
				DELETE FROM [dbo].[Section] WHERE RevisionID = @Id
				DELETE FROM [dbo].[RDSBDocumentInformation] WHERE RDMRevisionID = @Id
				DELETE FROM [dbo].[Revision] WHERE ID = @Id
			END TRY
			BEGIN CATCH
				SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(), @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE();

				SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' could not be deleted.'
				RAISERROR (
						@ErrorMessage, -- Message text.
						@ErrorSeverity, -- Severity,
						@ErrorState -- State,
						)
				RETURN
			END CATCH;
		END
	ELSE
		BEGIN
			SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO