IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteFullWorkspace]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteFullWorkspace];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteFullWorkspace]
(
@WorkspaceID int 
)
AS
/******************************************************************************
**		 
**		Name: [deleteFullWorkspace]
**		Desc: Delete the entire Workspace
**			
**		
**
**		Auth: Don Canuso
**		Date: 01/23/12
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**      1/17/17		twilson3			BOEJ-1604 Cleanup DB Project
**		3/3/2017	twilson3			BOEJ-1861 Remove DTS
**		5/4/2017	brunworg			BOEJ-2125 Add T&M Resource Rates
**		12/7/2017	twilson3			BOEJ-2250 Remove DTC
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		6/29/2018	twilson3			BOEJ-3551 New Homepage Table
**		10/25/2018	twilson3			BOEJ-3878 Email Table and WorkspaceOffloadRate Table.  Moved deletes around to mimic deleteMockTestData.sql for easier Compare in future
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
*******************************************************************************/
SET NOCOUNT ON 
IF @WorkspaceID IS NULL
	RETURN


BEGIN TRANSACTION
DECLARE @MockWorkspace TABLE
(
	WorkspaceID int PRIMARY KEY
)	
INSERT INTO @MockWorkspace VALUES (@WorkspaceID)



DECLARE @VersionID int
		
DECLARE @Metric TABLE (MetricID int)

DECLARE @TripAffected TABLE (TripID int)	
			
DECLARE @WorkspaceVersion TABLE
	(
		WorkspaceID int,
		VersionID int,
		Processed bit
	)
INSERT INTO @WorkspaceVersion
SELECT WV.WorkspaceID, WV.VersionID, 0 
FROM dbo.WorkspaceVersion WV
	INNER JOIN @MockWorkspace W ON WV.WorkspaceID = W.WorkspaceID
	
	/*Delete the Versions*/	
	WHILE EXISTS (SELECT 1 FROM @WorkspaceVersion WHERE Processed = 0)
	BEGIN
		SELECT TOP 1 
					@WorkspaceID = WorkspaceID, 
					@VersionID = VersionID 
		FROM @WorkspaceVersion WHERE Processed = 0

		EXECUTE dbo.deleteWorkspaceVersion @WorkspaceID, @VersionID

		UPDATE @WorkspaceVersion 
		SET Processed = 1
		WHERE	WorkspaceID = @WorkspaceID AND
				VersionID = @VersionID
	END
	
	
	/*
		Harder to use the SP because InUse would
		need to be set to 0 and Update Date would
		need to be obtained for each table/row
		that needs to be deleted
		Easier to delete from the entire table
	*/

			DELETE FROM dbo.ProPricerCustomFieldXREF
			FROM dbo.ProPricerCustomFieldXREF X
			INNER JOIN dbo.CustomField CF ON X.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID

	

			DELETE FROM dbo.BOELaborSpread
			FROM dbo.BOELaborSpread LS
				INNER JOIN dbo.BOELaborType LT ON LS.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID




			DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID



			DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
			INNER JOIN dbo.CustomFieldValue CFV ON CFV.CustomFieldValueID = X.CustomFieldValueID
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID
		


			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF
			FROM dbo.SumOfBOE_OrdinaryVariableXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID

				
				
			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF 
				FROM dbo.SumOfBOE_OrdinaryVariableXREF X
				INNER JOIN dbo.BOE B ON X.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID


			DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF
			FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID



			DELETE FROM dbo.OrdinaryVariable
			FROM dbo.OrdinaryVariable OV
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID



		DELETE FROM [dbo].[BOECustomFieldValueXREF]
		FROM [dbo].[BOECustomFieldValueXREF] X
			INNER JOIN dbo.BOE B ON X.BOEID = B.BOEID
			INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		


		DELETE FROM [dbo].[BOECustomFieldValueXREF]
		FROM [dbo].[BOECustomFieldValueXREF] X
			INNER JOIN dbo.CustomFieldValue CFV ON CFV.CustomFieldValueID = X.CustomFieldValueID
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID



			DELETE FROM dbo.BOELaborType
			FROM dbo.BOELaborType LT 
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID


			DELETE FROM dbo.BOEApprovalHistory 
				FROM dbo.BOEApprovalHistory BAH 
				INNER JOIN dbo.BOE B ON BAH.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID

			
						
			DELETE FROM dbo.BOECommentHistory
				FROM dbo.BOECommentHistory BCH 
				INNER JOIN dbo.BOE B ON BCH.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID

						
				
				
			DELETE FROM dbo.BOETaskElementCustomFieldValueXREF
			FROM dbo.BOETaskElementCustomFieldValueXREF X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
				

			DELETE FROM dbo.BOETaskElementCustomFieldValueXREF
			FROM dbo.BOETaskElementCustomFieldValueXREF X 
			INNER JOIN dbo.CustomFieldValue CFV ON CFV.CustomFieldValueID = X.CustomFieldValueID
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID
			
			DELETE FROM [dbo].[BOETaskElementMetricDetailXREF]
				FROM [dbo].[BOETaskElementMetricDetailXREF]  X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
				
			
						
			DELETE FROM dbo.BOETaskElementWorkspaceVariableXREF
			FROM  dbo.BOETaskElementWorkspaceVariableXREF X
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
					
			
			
			DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF
				FROM dbo.SumOfBOE_WorkspaceVariableXREF TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
			
			
			

			DELETE FROM dbo.BOEUserRoleHistory
				FROM dbo.BOEUserRoleHistory TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
						
						
						
			DELETE FROM dbo.BOEUserRole
				FROM dbo.BOEUserRole TE 
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
			
			
			
			DELETE FROM dbo.BOETaskElement
			FROM dbo.BOETaskElement TE 
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID

				
					
			
			DELETE FROM dbo.BOEComment
				FROM dbo.BOEComment TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
			
			
			
			DELETE FROM dbo.BOEApproval
				FROM dbo.BOEApproval TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
			
			
			
			DELETE FROM dbo.BOEStateHistory
				FROM dbo.BOEStateHistory TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
            
 
 
 
			DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF
				FROM dbo.SumOfBOE_WorkspaceVariableXREF TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
 
 
          	
			DELETE FROM dbo.WBS_CLIN_BOE_XREF
				FROM dbo.WBS_CLIN_BOE_XREF TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
			                 
			
			DELETE FROM dbo.MaterialTaskElement
			FROM dbo.MaterialTaskElement TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
			
			
			DELETE FROM dbo.ODCSpread                 
			FROM dbo.ODCSpread S
				INNER JOIN dbo.ODCType T ON S.ODCTypeID = T.ODCTypeID
				INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
			
			
			DELETE FROM dbo.ODCType                 
			FROM dbo.ODCType T 
				INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID


			DELETE FROM dbo.ODCTaskElement                 
			FROM dbo.ODCTaskElement TE 
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID


			DELETE FROM dbo.TravelTripCustomFieldValueXREF
				FROM dbo.TravelTripCustomFieldValueXREF X
				INNER JOIN dbo.TravelTrip T ON X.TravelTripID = T.TravelTripID
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID


			DELETE FROM dbo.TravelTripCustomFieldValueXREF
				FROM dbo.TravelTripCustomFieldValueXREF X	
			INNER JOIN dbo.CustomFieldValue CFV ON CFV.CustomFieldValueID = X.CustomFieldValueID
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID

			-- Start of "RMS Zone Travel"
			DELETE FROM dbo.MSTTravelTripCustomFieldValueXREF
				FROM dbo.MSTTravelTripCustomFieldValueXREF cI
				INNER JOIN dbo.MSTTravelTrip tt ON cI.MSTTravelTripID = tt.MSTTravelTripID
				INNER JOIN dbo.TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
			
			DELETE FROM dbo.MSTTravelTripCustomFieldValueXREF
				FROM dbo.MSTTravelTripCustomFieldValueXREF X	
			INNER JOIN dbo.CustomFieldValue CFV ON CFV.CustomFieldValueID = X.MSTCustomFieldValueID
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID
	
			DELETE FROM dbo.MSTTravelTrip 
			  FROM dbo.MSTTravelTrip tt
				INNER JOIN dbo.TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
			-- End of "RMS Zone Travel"

			/*WI 6072*/

			INSERT INTO @TripAffected 
			SELECT TripID
			FROM dbo.TravelTrip T
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
			

			
			
			DELETE FROM dbo.TravelTrip                 
			FROM dbo.TravelTrip T 
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
			
			
/*				
			DELETE FROM dbo.Trip	
				FROM dbo.Trip T 
					LEFT OUTER JOIN dbo.TravelTrip TT ON T.TripID = TT.TripID
			WHERE TT.TripID IS NULL /*Unused Trips*/
			AND T.DepartureLocationID IN (SELECT LocationID FROM dbo.Location WHERE LocationName LIKE 'MOCK%')
			AND T.DestinationLocationID IN (SELECT LocationID FROM dbo.Location WHERE LocationName LIKE 'MOCK%')
*/


			DELETE FROM dbo.TravelTripTaskElementCustomFieldValueXREF				
				FROM dbo.TravelTripTaskElementCustomFieldValueXREF X
				INNER JOIN dbo.TravelTripTaskElement TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID


			DELETE FROM dbo.TravelTripTaskElementCustomFieldValueXREF				
			FROM dbo.TravelTripTaskElementCustomFieldValueXREF X
			INNER JOIN dbo.CustomFieldValue CFV ON CFV.CustomFieldValueID = X.CustomFieldValueID
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID
				
			
			DELETE FROM dbo.TravelTripTaskElement                 
			FROM dbo.TravelTripTaskElement TE 
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
				
	
			DELETE FROM dbo.CustomFieldValue 
			FROM dbo.CustomFieldValue CFV
				INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
				INNER JOIN @MockWorkspace W ON CF.WorkspaceID = W.WorkspaceID
				
			DELETE FROM dbo.CustomField 
			FROM dbo.CustomField CF 
				INNER JOIN @MockWorkspace W ON CF.WorkspaceID = W.WorkspaceID
				 
			/*WI 6072*/
			UPDATE dbo.Trip
				SET TripInUse = CASE 
					WHEN TT.TripID IS NULL THEN 0
					WHEN TT.TripID IS NOT NULL THEN 1
					END,
					UpdateDT = GETDATE()
			FROM dbo.Trip T  
				INNER JOIN @TripAffected TA ON T.TripID = TA.TripID
				LEFT OUTER JOIN 
					(
						SELECT TripID AS TripID FROM dbo.TravelTrip
						
					) TT ON T.TripID = TT.TripID

	
				
	
			DELETE FROM dbo.WBS_CLIN_BOE_XREF
			FROM dbo.WBS_CLIN_BOE_XREF X 
				INNER JOIN dbo.WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID
				INNER JOIN @MockWorkspace W ON WBS.WorkspaceID = W.WorkspaceID


		DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF 
		FROM dbo.SumOfBOE_WorkspaceVariableXREF X
			INNER JOIN dbo.CLIN C ON X.CLINID = C.CLINID
			INNER JOIN @MockWorkspace WS ON C.WorkspaceID = WS.WorkspaceID


		DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF 
		FROM dbo.SumOfBOE_WorkspaceVariableXREF X
			INNER JOIN dbo.WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID
			INNER JOIN @MockWorkspace WS ON WBS.WorkspaceID = WS.WorkspaceID



		DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF 
		FROM dbo.SumOfBOE_WorkspaceVariableXREF X
			INNER JOIN dbo.BOE B ON X.BOEID = B.BOEID
			INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID



			DELETE FROM [dbo].[WorkspaceEmailXREF]
			FROM [dbo].[WorkspaceEmailXREF] xref
				INNER JOIN @MockWorkspace W ON xref.WorkspaceID = W.WorkspaceID
			
			DELETE FROM dbo.WorkBreakdownStructure
			FROM dbo.WorkBreakdownStructure WBS 
				INNER JOIN @MockWorkspace W ON WBS.WorkspaceID = W.WorkspaceID

			/* New INL Forms */
			DELETE FROM [dbo].[BOEFormIBOEResourcesXREF]
				FROM [dbo].[BOEFormIBOEResourcesXREF] x
					INNER JOIN [dbo].[BOEFormIBOE] BF ON x.[IBOEFormID] = BF.[IBOEFormID]
					INNER JOIN @MockWorkspace W ON BF.WorkspaceID = W.WorkspaceID

			DELETE FROM [dbo].[BOEFormIBOECLINsXREF]
				FROM [dbo].[BOEFormIBOECLINsXREF] x
					INNER JOIN [dbo].[BOEFormIBOE] BF ON x.[IBOEFormID] = BF.[IBOEFormID]
					INNER JOIN @MockWorkspace W ON BF.WorkspaceID = W.WorkspaceID

			DELETE FROM [dbo].[BOEFormIBOE]
				FROM [dbo].[BOEFormIBOE] BF
					INNER JOIN @MockWorkspace W ON BF.WorkspaceID = W.WorkspaceID

			DELETE FROM [dbo].[BOEFormPBOEResourcesXREF]
				FROM [dbo].[BOEFormPBOEResourcesXREF] x
					INNER JOIN [dbo].[BOEFormPBOE] BF ON x.[PBOEFormID] = BF.[PBOEFormID]
					INNER JOIN @MockWorkspace W ON BF.WorkspaceID = W.WorkspaceID

			DELETE FROM [dbo].[BOEFormPBOECLINsXREF]
				FROM [dbo].[BOEFormPBOECLINsXREF] x
					INNER JOIN [dbo].[BOEFormPBOE] BF ON x.[PBOEFormID] = BF.[PBOEFormID]
					INNER JOIN @MockWorkspace W ON BF.WorkspaceID = W.WorkspaceID

			DELETE FROM [dbo].[BOEFormPBOE]
				FROM [dbo].[BOEFormPBOE] BF
					INNER JOIN @MockWorkspace W ON BF.WorkspaceID = W.WorkspaceID
			
			DELETE FROM dbo.BOE	
			FROM dbo.BOE B 
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID





				

				DELETE FROM [dbo].[WorkspaceVariableSumVariableResourceTypeXREF]
				FROM [dbo].[WorkspaceVariableSumVariableResourceTypeXREF] X 
					INNER JOIN dbo.WorkspaceVariable WV ON X.WorkspaceVariableID = WV.WorkspaceVariableID
					INNER JOIN dbo.Workspace W ON WV.WorkspaceID = W.WorkspaceID
					INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID					


				
			DELETE FROM [dbo].[WorkspaceStateHistory]
			FROM [dbo].[WorkspaceStateHistory] WH 
					INNER JOIN  dbo.Workspace W ON WH.WorkspaceID = W.WorkspaceID
					INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID


			DELETE FROM [dbo].[WorkspaceUserRole]
			FROM [dbo].[WorkspaceUserRole] WUR
				INNER JOIN  @MockWorkspace W ON WUR.WorkspaceID = W.WorkspaceID

			
			DELETE FROM dbo.WorkspaceVariable 
				FROM dbo.WorkspaceVariable  WV
					INNER JOIN @MockWorkspace W ON WV.WorkspaceID = W.WorkspaceID

		
		/*should not be required since all mock cf and cfv were deleted

		/*Only should be looking within the scope
		of the Custom Field, so if the scope is BOE
		then only look in BOECustomFieldValueXREF
		*/
				
		UPDATE dbo.CustomFieldValue
			SET CustomFieldValueInUseFlag = CASE 
												WHEN X.[CustomFieldValueID] IS NULL THEN 0
												WHEN X.[CustomFieldValueID] IS NOT NULL THEN 1
											END,
				UpdateDT = GETDATE()
		FROM dbo.CustomFieldValue CFV 
			LEFT OUTER JOIN [dbo].[BOECustomFieldValueXREF] X ON CFV.[CustomFieldValueID] = X.[CustomFieldValueID]
		*/
		
		
		DELETE FROM dbo.WBS_CLIN_BOE_XREF 
		FROM dbo.WBS_CLIN_BOE_XREF X
			INNER JOIN dbo.CLIN C ON X.CLINID = C.CLINID
			INNER JOIN @MockWorkspace WS ON C.WorkspaceID = WS.WorkspaceID


			
		DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF
		FROM dbo.SumOfBOE_OrdinaryVariableXREF X
			INNER JOIN dbo.CLIN C ON X.CLINID = C.CLINID
			INNER JOIN @MockWorkspace WS ON C.WorkspaceID = WS.WorkspaceID

			




			DELETE FROM dbo.CLIN
			FROM dbo.CLIN C 
				INNER JOIN @MockWorkspace WS ON C.WorkspaceID = WS.WorkspaceID


			DELETE FROM [dbo].[BOEPotentialRole]
			FROM dbo.BOEPotentialRole PR
				INNER JOIN @MockWorkspace W ON PR.WorkspaceID = W.WorkspaceID

			DELETE FROM dbo.[WorkspaceUserXREF]
			FROM dbo.[WorkspaceUserXREF] xref
			INNER JOIN  @MockWorkspace W ON xref.WorkspaceID = W.WorkspaceID
			
			DELETE FROM dbo.WorkspaceUserRole 
			FROM dbo.WorkspaceUserRole WUR
			INNER JOIN @MockWorkspace W ON WUR.WorkspaceID = W.WorkspaceID							 
			DELETE FROM dbo.TMResourceRate
     		FROM dbo.TMResourceRate TMRR
				INNER JOIN @MockWorkspace W ON TMRR.WorkspaceID = W.WorkspaceID								 
			DELETE FROM dbo.WorkspaceOffloadRate
			FROM dbo.WorkspaceOffloadRate WOR
				INNER JOIN @MockWorkspace W ON WOR.WorkspaceID = W.WorkspaceID	
			DELETE FROM [dbo].[WorkspaceResource]
			FROM [dbo].[WorkspaceResource] WR
				INNER JOIN @MockWorkspace MW ON WR.WorkspaceID = MW.WorkspaceID
			DELETE FROM [dbo].[WorkspacePerformingOrganization]
			FROM [dbo].[WorkspacePerformingOrganization] WR
				INNER JOIN @MockWorkspace MW ON WR.WorkspaceID = MW.WorkspaceID
			DELETE FROM [dbo].[PerformingOrganization] 
			FROM [dbo].[PerformingOrganization] PO
				INNER JOIN dbo.PerformingOrganizationList PL ON PO.PerformingOrganizationListID = PL.PerformingOrganizationListID
				INNER JOIN dbo.Workspace W ON PL.PerformingOrganizationListID = W.PerformingOrganizationListID
				INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID				
			WHERE PO.PerformingOrganizationListID <> 1
			DELETE FROM [dbo].[Resource] 
			FROM [dbo].[Resource] R
				INNER JOIN dbo.ResourceList RL ON R.ResourceListID = RL.ResourceListID
				INNER JOIN dbo.Workspace W ON RL.ResourceListID = W.ResourceListID
				INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID
			WHERE R.ResourceListID <> 1				 
			
			DELETE FROM dbo.WorkspaceRestoreLog
			FROM dbo.WorkspaceRestoreLog WRL 
			INNER JOIN @MockWorkspace W ON WRL.WorkspaceID = W.WorkspaceID
			
			
			DELETE FROM dbo.OutputFormatTemplateWorkspaceXREF
			FROM dbo.OutputFormatTemplateWorkspaceXREF X
			INNER JOIN @MockWorkspace W ON X.WorkspaceID = W.WorkspaceID


			DELETE FROM dbo.CustomFieldValue
			FROM dbo.CustomFieldValue CFV
				INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
				INNER JOIN @MockWorkspace W ON CF.WorkspaceID = W.WorkspaceID

			
			DELETE FROM dbo.CustomField
			FROM dbo.CustomField CF
			INNER JOIN @MockWorkspace W ON CF.WorkspaceID = W.WorkspaceID
							
			
				


				
			/*
			Fix All In Use Flags
			*/
			/*Check if Resource is used elsewhere*/
			/*
			DECLARE @ActiveWorkspace TABLE
				(
					WorkspaceID int PRIMARY KEY,
					Processed bit DEFAULT 0
				)
			INSERT INTO @ActiveWorkspace
			SELECT WorkspaceID,0 FROM dbo.Workspace 
			WHERE WorkspaceStateID IN (1,2,3)
*/

/*
	DELETE FROM dbo.Trip
	FROM dbo.Trip T 
		INNER JOIN dbo.TravelMiscRate TMR ON T.TravelMiscRateID = TMR.TravelMiscRateID
	WHERE TMR.TransportationMode LIKE 'Mock%'		



	DELETE FROM dbo.TravelMiscRate
	FROM dbo.TravelMiscRate 
	WHERE TransportationMode LIKE 'Mock%'		

	DELETE FROM dbo.OutputFormatTemplate
	FROM dbo.OutputFormatTemplate T
	LEFT OUTER JOIN dbo.OutputFormatTemplateWorkspaceXREF X ON T.TemplateID = X.TemplateID
	WHERE Template  LIKE 'Mock%' AND X.TemplateID IS NULL
	


	DELETE FROM dbo.Location
	WHERE LocationName LIKE 'MOCK%'
*/


DELETE FROM dbo.WorkspaceLockedTrip
  FROM dbo.WorkspaceLockedTrip L
	INNER JOIN dbo.Workspace W ON L.WorkspaceID = W.WorkspaceID
	INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID				


DELETE FROM dbo.WorkspaceLockedTravelMiscRate
  FROM dbo.WorkspaceLockedTravelMiscRate L
	INNER JOIN dbo.Workspace W ON L.WorkspaceID = W.WorkspaceID
	INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID				



DELETE FROM dbo.WorkspaceLockedPerDiem
  FROM dbo.WorkspaceLockedPerDiem L
	INNER JOIN dbo.Workspace W ON L.WorkspaceID = W.WorkspaceID
	INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID				


DELETE FROM dbo.WorkspaceLockedTravelEscalationRate
  FROM dbo.WorkspaceLockedTravelEscalationRate L
	INNER JOIN dbo.Workspace W ON L.WorkspaceID = W.WorkspaceID
	INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID				




DELETE FROM dbo.WorkspaceContractTypeXREF
FROM dbo.WorkspaceContractTypeXREF X
INNER JOIN @MockWorkspace W ON X.WorkspaceID = W.WorkspaceID



DELETE FROM [dbo].[ProPricerCustomFieldXREF]
FROM [dbo].[ProPricerCustomFieldXREF] X
INNER JOIN  dbo.ProPricerExport PE ON X.[ProPricerExportID] = PE.[ProPricerExportID]
INNER JOIN  @MockWorkspace W ON PE.WorkspaceID = W.WorkspaceID


DELETE FROM [dbo].[ProPricerFieldXREF]
FROM [dbo].[ProPricerFieldXREF] X
INNER JOIN  dbo.ProPricerExport PE ON X.[ProPricerExportID] = PE.[ProPricerExportID]
INNER JOIN  @MockWorkspace W ON PE.WorkspaceID = W.WorkspaceID

DELETE FROM dbo.ProPricerExport
FROM dbo.ProPricerExport PE
INNER JOIN  @MockWorkspace W ON PE.WorkspaceID = W.WorkspaceID

DELETE FROM dbo.[WorkspaceRMSTravelEscalationRate]
FROM dbo.[WorkspaceRMSTravelEscalationRate] R
INNER JOIN  @MockWorkspace W ON R.WorkspaceID = W.WorkspaceID
DELETE FROM dbo.[WorkspaceRMSTravelNonzoneFeesAndCosts]
FROM dbo.[WorkspaceRMSTravelNonzoneFeesAndCosts] F
INNER JOIN  @MockWorkspace W ON F.WorkspaceID = W.WorkspaceID
			DELETE FROM dbo.Workspace
			FROM dbo.Workspace W
			INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID

			DELETE FROM dbo.PerformingOrganizationList
			FROM dbo.PerformingOrganizationList PL
				INNER JOIN dbo.Workspace W ON PL.PerformingOrganizationListID = W.PerformingOrganizationListID
				INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID				
				
			DELETE FROM dbo.ResourceList
			FROM dbo.ResourceList RL
				INNER JOIN dbo.Workspace W ON RL.ResourceListID = W.ResourceListID
				INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID

IF @@ERROR = 0
	COMMIT TRANSACTION
ELSE
	ROLLBACK TRANSACTION

GO