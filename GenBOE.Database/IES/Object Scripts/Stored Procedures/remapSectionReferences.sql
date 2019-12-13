IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[remapSectionReferences]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[remapSectionReferences];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[remapSectionReferences]
(
	@Id			INT,
	@UpdateDate datetime2,
	@NewId		INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[remapSectionReferences] (originally remapRateCodesForSection)
	**		Desc:	Update all references to @Id (old section Id)
	**				to point to @NewId (new section Id)
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 9/24/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		01/11/2017	brunworg			Renamed and modified to update file 
	**                                      attachment references.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;

	IF (SELECT UpdateDate FROM [dbo].[Section] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			UPDATE [dbo].[RateCode] 
			SET SectionID = @NewId
			WHERE SectionID = @Id;

			UPDATE [dbo].[FileAttachment] 
			SET SectionID = @NewId
			WHERE SectionID = @Id;
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
