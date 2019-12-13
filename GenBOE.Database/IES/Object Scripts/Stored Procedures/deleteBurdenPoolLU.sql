IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBurdenPoolLU]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBurdenPoolLU];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteBurdenPoolLU]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteBurdenPoolLU]
	**		Desc:	Delete a Burden Pool 
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
	**		08/03/2017	brunworg			Change stored procedure name.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[BurdenPoolLU] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			UPDATE dbo.RateCode SET GovernmentBurdenPoolId = null WHERE GovernmentBurdenPoolId = @Id
			UPDATE dbo.RateCode SET CommercialBurdenPoolId = null WHERE CommercialBurdenPoolId = @Id
			DELETE FROM dbo.ProPricerBurdenRateMap WHERE BurdenPoolID = @Id
			DELETE FROM dbo.BurdenPoolLU WHERE ID = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Burden Pool with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO
