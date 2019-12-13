-- This file pulls data about any open ended custom fields that are being referenced by multiple fields. They need to be fixed (other files)

-- DOUBLED UP CF Values for Labor
SELECT cF.CustomFieldID, cF.CustomFieldName, cV.CustomFieldValueID, cV.CustomFieldValueDescription, bT.BOEID, bL.BOETaskElementID, x.BOELaborTypeID
	FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
		INNER JOIN BOELaborTypeCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
		INNER JOIN BOELaborType bL ON bL.BOELaborTypeID = x.BOELaborTypeID
		INNER JOIN BoeTaskElement bT ON bL.BOETaskElementID = bT.BOETaskElementID
	WHERE cV.CustomFieldValueId IN (SELECT CustomFieldValueId
									FROM (
										SELECT x.*
											FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
												INNER JOIN BOELaborTypeCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
											WHERE cF.IsOpenEnded = 1) z
										GROUP BY CustomFieldValueId
										HAVING COUNT(*) > 1)
	ORDER BY cF.CustomFieldId, cV.CustomFieldValueDescription, bT.BOEID, bL.BOETaskElementID, x.BOELaborTypeID;

-- DOUBLED UP CF Values for Task Elements
SELECT cF.CustomFieldID, cF.CustomFieldName, cV.CustomFieldValueID, cV.CustomFieldValueDescription, bT.BOEID, x.BOETaskElementID
	FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
		INNER JOIN BOETaskElementCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
		INNER JOIN BoeTaskElement bT ON x.BOETaskElementID = bT.BOETaskElementID
	WHERE cV.CustomFieldValueId IN (SELECT CustomFieldValueId
									FROM (
										SELECT x.*
											FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
												INNER JOIN BOETaskElementCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
											WHERE cF.IsOpenEnded = 1) z
										GROUP BY CustomFieldValueId
										HAVING COUNT(*) > 1)
	ORDER BY cF.CustomFieldId, cV.CustomFieldValueDescription, bT.BOEID, x.BOETaskElementID;

-- DOUBLED UP CF Values for BOEs
SELECT cF.CustomFieldID, cF.CustomFieldName, cV.CustomFieldValueID, cV.CustomFieldValueDescription, x.BOEID
	FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
		INNER JOIN BOECustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
	WHERE cV.CustomFieldValueId IN (SELECT CustomFieldValueId
									FROM (
										SELECT x.*
											FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
												INNER JOIN BOECustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
											WHERE cF.IsOpenEnded = 1) z
										GROUP BY CustomFieldValueId
										HAVING COUNT(*) > 1)
	ORDER BY cF.CustomFieldId, cV.CustomFieldValueDescription, x.BOEID;

-- DOUBLED UP CF Values for Travel Tasks
SELECT cV.CustomFieldValueID, x.TravelTripTaskElementID
			FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
				INNER JOIN TravelTripTaskElementCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
				INNER JOIN TravelTripTaskElement t on x.TravelTripTaskElementID = t.TravelTripTaskElementID
			WHERE cV.CustomFieldValueId IN (SELECT CustomFieldValueId
											FROM (
												SELECT x.*
													FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
														INNER JOIN TravelTripTaskElementCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
													WHERE cF.IsOpenEnded = 1) z
												GROUP BY CustomFieldValueId
												HAVING COUNT(*) > 1)
			ORDER BY cF.CustomFieldId, cV.CustomFieldValueID, t.BOEID, x.TravelTripTaskElementID;

-- DOUBLED UP CF Values for Travel Trips
SELECT cV.CustomFieldValueID, x.MSTTravelTripID, tT.BOEID, t.TravelTripTaskElementID, cF.CustomFieldName, cV.CustomFieldValueDescription
	FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
		INNER JOIN MSTTravelTripCustomFieldValueXREF x ON cV.CustomFieldValueID = x.MSTCustomFieldValueID
		INNER JOIN MSTTravelTrip t ON t.MSTTravelTripID = x.MSTTravelTripID
		INNER JOIN TravelTripTaskElement tT ON t.TravelTripTaskElementID = tT.TravelTripTaskElementID
	WHERE cV.CustomFieldValueId IN (SELECT CustomFieldValueId
										FROM (
											SELECT x.*
												FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
													INNER JOIN MSTTravelTripCustomFieldValueXREF x ON cV.CustomFieldValueID = x.MSTCustomFieldValueID
												WHERE cF.IsOpenEnded = 1) z
											GROUP BY MSTCustomFieldValueId
											HAVING COUNT(*) > 1)
	ORDER BY cF.CustomFieldId, cV.CustomFieldValueID, tT.BOEID, t.TravelTripTaskElementID, x.MSTTravelTripID;
)