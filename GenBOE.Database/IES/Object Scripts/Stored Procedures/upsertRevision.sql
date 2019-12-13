IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertRevision];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertRevision]
(
	 @Id int
	,@UpdateDate datetime2(7)
	,@Revision varchar(50)
    ,@History nvarchar(max)
    ,@CreatedBy varchar(1000) = NULL		-- only used on insert
	,@StartYear int
	,@EndYear int
	,@ReleaseNotes nvarchar(max)
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertRevision]
	**		Desc:	Insert/Update a PPR&D Revision 
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
	**      07/07/2017	brunworg			Adjusted parameters as follows:
	**										Removed DateCreated parameter (only set
	**										CreatedBy on insert).
	**										Removed DatePublished and PublishedBy
	**										parameters (created separate 
	**										publishRevision procedure).
	**										Removed InUse and Editing parameters
	**										(created separate lock/unlockRevision
	**										procedures).
	**		08/01/2017	brunworg			Added StartYear and EndYear columns.
	**		10/04/2017	ranzalon			Updating for History and Release Notes
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[Revision]
					   ([UpdateDate]
					   ,[Revision]
					   ,[History]
					   ,[DateCreated]
					   ,[CreatedBy]
					   ,[StartYear]
					   ,[EndYear]
					   ,[ReleaseNotes])
				 OUTPUT inserted.ID INTO @Inserted
				 VALUES
					   (@UpdateDate
					   ,@Revision
					   ,@History
					   ,GETDATE()		-- set DateCreated to current date
					   ,@CreatedBy
					   ,@StartYear
					   ,@EndYear
					   ,@ReleaseNotes)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[Revision] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].Revision
					   SET UpdateDate = @UpdateDate
						  ,Revision = @Revision
						  ,History = @History
						  ,StartYear = @StartYear
						  ,EndYear = @EndYear
						  ,ReleaseNotes = @ReleaseNotes
						WHERE 
							ID = @Id;
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
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId
GO