IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertCobraFiscalYear]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertCobraFiscalYear];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertCobraFiscalYear]
(
	@Id						INT,
	@UpdateDate				datetime2(7),
	@Year					int,
	@FiscalYearStartDate	date
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertCobraFiscalYear]
	**		Desc:	Insert/Update a COBRA Fiscal Year 
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
	**		7/3/2017	brunworg			Check UpdateDate before upsert.
	**		7/11/2017	brunworg			Changed @Year from varchar(50) to int.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].CobraFiscalYearLU
						([UpdateDate]
						,[Year]
						,[FiscalYearStartDate]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
					    ,@Year
						,@FiscalYearStartDate
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[CobraFiscalYearLU] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].CobraFiscalYearLU
					   SET  UpdateDate = @UpdateDate
							,Year = @Year
							,FiscalYearStartDate = @FiscalYearStartDate
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The COBRA Fiscal Year with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
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