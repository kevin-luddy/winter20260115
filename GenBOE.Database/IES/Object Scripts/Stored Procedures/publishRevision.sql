IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[publishRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[publishRevision];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[publishRevision]
(
	 @Id int
	,@UpdateDate datetime2(7)
    ,@PublishedBy varchar(1000)
	,@History nvarchar(max)
	,@ReleaseNotes nvarchar(max)
	,@DatePublished datetime2(7) = NULL
)
AS
	/******************************************************************************
	**		 
	**		Name:	[publishRevision]
	**		Desc:	Publish a PPR&D Revision.  This includes creating the new Work
	**				In Progress (WIP) revision by making a copy of the published 
	**				revision and incrementing the revision number by 1.
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 7/7/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**      07/13/2017	brunworg			Add code to make a copy of the 
	**										previous revision.
	**		08/10/2017	brunworg			Remove transaction handling.
	**		08/24/2017	brunworg			Change return signature to contain
	**										new WIP Revision ID and Revision name.
	**		10/05/2017	ranzalon			Update for History and Release Notes
	**		10/11/2017	ranzalon			Update to take in History and Release
	**										Note inputs, don't copy release notes
	**		05/08/2018	brunworg			Added optional DatePublished parameter.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;
	DECLARE @Revision varchar(50), @NewRevision int;

	SELECT @Revision = Revision FROM [dbo].Revision WHERE ID = @Id;
	IF @@ROWCOUNT = 0	
		BEGIN
			SET @ErrorMessage = 'Publish failed - Revision could not be found.'
			RAISERROR (
					@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN		
		END

	IF (SELECT UpdateDate FROM [dbo].[Revision] WHERE ID = @Id) = @UpdateDate
		BEGIN
			BEGIN TRY
				-- Mark the revision as published
				SET @UpdateDate = GETDATE()
				UPDATE [dbo].Revision
					SET UpdateDate = @UpdateDate
						,DatePublished = ISNULL(@DatePublished, @UpdateDate)
						,PublishedBy = @PublishedBy
						,History = @History
						,ReleaseNotes = @ReleaseNotes
					WHERE 
						ID = @Id;

				-- Create a new Work-In-Progress revision by making a copy of the published revision 
				SET @NewRevision = CONVERT(int, @Revision) + 1;	-- increment the revision number
				EXECUTE dbo.copyRevision @Id=@Id, @NewRevision=@NewRevision, @NewHistory=@History, @NewCreatedBy=@PublishedBy, @NewReleaseNotes=null;
			END TRY
			BEGIN CATCH
				SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(), @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE();

				IF @ErrorMessage IS NULL
					BEGIN
						SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' could not be published.'
					END
								
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

	IF @@ERROR = 0
		SELECT @Id AS Id, @Revision as Revision;
GO