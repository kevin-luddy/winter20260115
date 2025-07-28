EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.02';

/* Author: Oyeyemi Oyetoro
Story: SLMX_POLM_PROPH-2742 */

IF OBJECT_ID('dbo.[UQ_ProPricerRateCodeXref]') IS NULL 
	BEGIN
		WITH CTE AS (
			SELECT 
				*, 
				ROW_NUMBER() OVER (
					PARTITION BY RateCodeID, RateCodeExtensionID 
					ORDER BY ID
				) AS rn
			FROM [dbo].[ProPricerRateCodeXref]
		)

		DELETE FROM CTE 
			WHERE rn > 1

		-- Add constraint
		ALTER TABLE [dbo].[ProPricerRateCodeXref]
		ADD CONSTRAINT UQ_ProPricerRateCodeXref UNIQUE (RateCodeID, RateCodeExtensionID);

	END