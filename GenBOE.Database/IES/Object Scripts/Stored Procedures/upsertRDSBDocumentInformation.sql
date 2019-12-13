IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRDSBDocumentInformation]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertRDSBDocumentInformation];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertRDSBDocumentInformation]
(
	 @Id INT
	,@LastUpdateDT DATETIME2(7)
	,@PTMProposalID INT
	,@CreatedBy VARCHAR(1000)
	,@RDMRevisionID INT
	,@StartYear INT
	,@EndYear INT
	,@ParentSection VARCHAR(10) = NULL
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertRDSBDocumentInformation]
	**		Desc:	Insert/Update RDSBDocumentInformation
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
	**		02/01/2018	brunworg			Added StartYear and EndYear fields.
	**		4/27/18		twilson3			BOEJ-3384 Added Parent Section
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF EXISTS (SELECT 1 FROM [dbo].[Revision] WHERE ID = @RDMRevisionID)
		BEGIN
			IF @Id  < 0 
				/* Insert */
				BEGIN
					DECLARE @Inserted AS Table (Id int)
					SET @LastUpdateDT = GETDATE()

					INSERT INTO [dbo].[RDSBDocumentInformation]
							   (PTMProposalID
							   ,CreatedBy
							   ,CreatedDate
							   ,LastUpdateDT
							   ,RDMRevisionID
							   ,StartYear
							   ,EndYear
							   ,ParentSection)
						 OUTPUT inserted.ID INTO @Inserted
						 VALUES
							   (@PTMProposalID
							   ,@CreatedBy
							   ,@LastUpdateDT
							   ,@LastUpdateDT
							   ,@RDMRevisionID
							   ,@StartYear
							   ,@EndYear
							   ,@ParentSection)

					SELECT @Id = Id FROM @Inserted
				END
			ELSE
				/* Update */
				BEGIN
					IF (SELECT LastUpdateDT FROM [dbo].[RDSBDocumentInformation] WHERE ID = @Id) = @LastUpdateDT
						BEGIN
							SET @LastUpdateDT = GETDATE()
							UPDATE [dbo].[RDSBDocumentInformation]
							   SET LastUpdateDT = @LastUpdateDT
								  ,RDMRevisionID = @RDMRevisionID
								  ,StartYear = @StartYear
								  ,EndYear = @EndYear
								  ,ParentSection = @ParentSection
								WHERE 
									ID = @Id
						END
					ELSE
						BEGIN
							SET @ErrorMessage =   'The RDSB Document Information with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
							RAISERROR (
									@ErrorMessage, -- Message text.
									11, -- Severity,/*Severity Changed to 11*/
									1 -- State,
									)
							RETURN
						END
				END
			END
		ELSE
			BEGIN
				SET @ErrorMessage =   'A Revision with ID ' + CAST(@RDMRevisionID  AS varchar(10)) + ' does not exist. Please select a valid Revision.'
							RAISERROR (
									@ErrorMessage, -- Message text.
									11, -- Severity,/*Severity Changed to 11*/
									1 -- State,
									)
							RETURN
			END

	IF @@ERROR = 0
		SELECT @Id AS NewId
GO