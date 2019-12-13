IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteCobraFiscalYear]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteCobraFiscalYear];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteCobraFiscalYear]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteCobraFiscalYear]
	**		Desc:	Delete a COBRA Fiscal Year
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
	**		7/3/2017	brunworg			Check UpdateDate before delete.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[CobraFiscalYearLU] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.CobraFiscalYearLU WHERE Id = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The COBRA Fiscal Year with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO
