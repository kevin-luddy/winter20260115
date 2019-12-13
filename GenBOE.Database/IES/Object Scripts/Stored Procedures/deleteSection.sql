IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSection]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSection];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteSection]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteSection]
	**		Desc:	Delete a document section
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 7/6/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		08/10/2017	brunworg			Remove transaction handling.
	**		08/22/2017	brunworg			Redesign Section and related tables.
	**		09/07/2017	brunworg			Modified to delete child sections.
	**		01/30/2018	brunworg			Remove associated RateCodes before deleting sections.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;

	IF (SELECT UpdateDate FROM [dbo].[Section] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			-- Retrieve section and all child sections.  
			-- Use a Common Table Expression (CTE) to gather all of the hierarchical section IDs.
			DECLARE @TempSectionIDs TABLE (ID INT, UpdateDate datetime2(7), level INT);

			WITH CteSectionIDs (ParentID, ID, UpdateDate, level)
			AS
			(
				-- start with specified section ID
				SELECT ParentID, ID, UpdateDate, 0 as level FROM dbo.[Section] WHERE ID = @ID
				UNION ALL
				-- recursive child sections
				SELECT child.ParentID, child.Id, child.UpdateDate, level + 1 FROM dbo.[Section] as child INNER JOIN CteSectionIDs as parent ON child.ParentID = parent.ID
			)
			-- Statement that executes the CTE
			INSERT INTO @TempSectionIDs (ID, UpdateDate, level)
			SELECT ID, UpdateDate, level from CteSectionIDs;

			-- Remove any rate code associations with the sections being deleted.
			UPDATE [dbo].[RateCode]
			SET SectionID = null
			WHERE SectionID in (SELECT ID from @TempSectionIDs);

			-- Delete the sections found by the CTE
			BEGIN TRY
				DECLARE @SectionId int, @SectionUpdateDate datetime2(7);
				DECLARE cur CURSOR LOCAL FOR
					SELECT s.ID, s.Updatedate 
					FROM [dbo].Section s
					JOIN @TempSectionIDs tmp on s.ID = tmp.ID
					ORDER BY tmp.level desc;

				OPEN cur
				FETCH NEXT FROM cur INTO @SectionId, @SectionUpdateDate

				WHILE @@FETCH_STATUS = 0 BEGIN
					DELETE FROM [dbo].[Section] WHERE Id = @SectionId AND UpdateDate = @SectionUpdateDate
					FETCH NEXT FROM cur INTO @SectionId, @SectionUpdateDate
				END
				CLOSE cur;
				DEALLOCATE cur;
			END TRY
			BEGIN CATCH
				SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(), @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE();

				SET @ErrorMessage =   'The Section with ID ' + CAST(@Id  AS varchar(10)) + ' (and child sections) could not be deleted.'
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
			SET @ErrorMessage =   'The Section with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO
