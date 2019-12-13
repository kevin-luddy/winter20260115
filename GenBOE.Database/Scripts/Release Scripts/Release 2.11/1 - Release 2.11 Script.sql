/*
	## START ##

	6/24/2016 [Tim Wilson] -- Fix In Use Flag for all Custom Fields for ALL Workspaces
*/

UPDATE dbo.CustomFieldValue
		SET CustomFieldValueInUseFlag = CASE 
				WHEN mX.[CustomFieldValueID] IS NULL AND mtX.[CustomFieldValueID] IS NULL AND oX.[CustomFieldValueID] IS NULL 
                    AND otX.[CustomFieldValueID] IS NULL AND tX.[CustomFieldValueID] IS NULL AND ttX.[CustomFieldValueID] IS NULL 
                    AND bX.[CustomFieldValueID] IS NULL AND blX.[CustomFieldValueID] IS NULL AND btX.[CustomFieldValueID] IS NULL THEN 0
				ElSE 1
			END,
			UpdateDT = GETDATE()
		FROM dbo.CustomFieldValue CFV 
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			LEFT OUTER JOIN [dbo].[MaterialCustomFieldValueXREF] mX ON CFV.[CustomFieldValueID] = mX.[CustomFieldValueID]
            LEFT OUTER JOIN [dbo].[MaterialTaskElementCustomFieldValueXREF] mtX ON CFV.[CustomFieldValueID] = mtX.[CustomFieldValueID]
			LEFT OUTER JOIN [dbo].[ODCTypeCustomFieldValueXREF] oX ON CFV.[CustomFieldValueID] = oX.[CustomFieldValueID]
            LEFT OUTER JOIN [dbo].[ODCTaskElementCustomFieldValueXREF] otX ON CFV.[CustomFieldValueID] = otX.[CustomFieldValueID]
            LEFT OUTER JOIN [dbo].[TravelTripCustomFieldValueXREF] tX ON CFV.[CustomFieldValueID] = tX.[CustomFieldValueID]
            LEFT OUTER JOIN [dbo].[TravelTripTaskElementCustomFieldValueXREF] ttX ON CFV.[CustomFieldValueID] = ttX.[CustomFieldValueID]
            LEFT OUTER JOIN [dbo].[BOECustomFieldValueXREF] bX ON CFV.[CustomFieldValueID] = bX.[CustomFieldValueID]
            LEFT OUTER JOIN [dbo].[BOELaborTypeCustomFieldValueXREF] blX ON CFV.[CustomFieldValueID] = blX.[CustomFieldValueID]
            LEFT OUTER JOIN [dbo].[BOETaskElementCustomFieldValueXREF] btX ON CFV.[CustomFieldValueID] = btX.[CustomFieldValueID];

GO

/*
	6/24/2016 [Tim Wilson] -- Fix In Use Flag for all Custom Fields for ALL Workspaces

	## END ##

	## START ##

	7/28/2016 [Dusan] -- BOEJ-1256 - Adding a "blank" record into the mileage rates, in case one never existed; Without it, updates fail.
*/


IF NOT (EXISTS (SELECT 1 FROM [dbo].[MileageReimbursementRate]))
BEGIN
	PRINT '### Inserting a default Mileage Rate ###';
	INSERT INTO [dbo].[MileageReimbursementRate] (UpdateDT, RatePerMile)
		VALUES (GETDATE(), 0);
END

/*
	7/28/2016 [Dusan] -- BOEJ-1256 - Adding a "blank" record into the mileage rates, in case one never existed; Without it, updates fail.

	## END ##

*/