IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProPricerRateCodeXref]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProPricerRateCodeXref];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProPricerRateCodeXref]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteProPricerRateCodeXref]
	**		Desc:	Delete a ProPricer Rate Code cross reference
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

	IF (SELECT UpdateDate FROM [dbo].[ProPricerRateCodeXref] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.ProPricerRateCodeXref WHERE Id = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The ProPricer Rate Code Cross Reference with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO
