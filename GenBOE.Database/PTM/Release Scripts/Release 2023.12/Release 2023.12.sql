EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.12';
GO

/*
                ## START ##

                4/13/23 [RJ] - PROPH-247 - Update FAR Checklist
*/

-- Create the new SPs here because release scripts run before SP scripts
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CopyPARChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CopyPARChecklist];
GO

CREATE PROCEDURE dbo.CopyPARChecklist
(@newChecklistId AS INT) AS
/******************************************************************************
**		 
**		Name: CopyPARChecklist
**		Desc: Copies PAR Checklist into a new Checklist version. This is executed manually.
**
**		Auth: RJ
**		Date: 4/13/23
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ProposalAdequacyReview WHERE ProposalAdequacyReviewID = @newChecklistId)
	BEGIN
		UPDATE ProposalAdequacyReview SET IsCurrent = 0;

		SET IDENTITY_INSERT ProposalAdequacyReview ON;
		INSERT INTO ProposalAdequacyReview (ProposalAdequacyReviewID, ChecklistVersion, IsCurrent, ProposalChecklistTypeID)
			VALUES (@newChecklistId, @newChecklistId - 1, 1, 1);
		SET IDENTITY_INSERT ProposalAdequacyReview OFF;

		INSERT INTO PARChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalAdequacyReviewID, QuestionNumber, Reference, SubmissionItem, YesOnly)
			SELECT ChecklistText, TextTypeID, SortOrder, ColumnOrder, @newChecklistId, QuestionNumber, Reference, SubmissionItem, YesOnly
				FROM PARChecklistContent
				WHERE ProposalAdequacyReviewId = @newChecklistId - 1;
	END
END
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CopyPPRChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CopyPPRChecklist];
GO

CREATE PROCEDURE dbo.CopyPPRChecklist
(@newChecklistId AS INT) AS
/******************************************************************************
**		 
**		Name: CopyPPRChecklist
**		Desc: Copies PPR Checklist into a new Checklist version. This is executed manually.
**
**		Auth: RJ
**		Date: 4/13/23
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ProposalPricingReview WHERE ProposalPricingReviewID = @newChecklistId)
	BEGIN
		UPDATE ProposalPricingReview SET IsCurrent = 0;

		SET IDENTITY_INSERT ProposalPricingReview ON;
		INSERT INTO ProposalPricingReview (ProposalPricingReviewID, ChecklistVersion, IsCurrent, ProposalChecklistTypeID)
			VALUES (@newChecklistId, @newChecklistId - 1, 1, 1);
		SET IDENTITY_INSERT ProposalPricingReview OFF;

		INSERT INTO PPRChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalPricingReviewID)
			SELECT ChecklistText, TextTypeID, SortOrder, ColumnOrder, @newChecklistId
				FROM PPRChecklistContent
				WHERE ProposalPricingReviewID = @newChecklistId - 1;
	END
END
GO

-- New Checklist version 15, ID 16
DECLARE @newChecklistId INT = 16;

IF NOT EXISTS (SELECT 1 FROM ProposalAdequacyReview WHERE ProposalAdequacyReviewID = @newChecklistId)
BEGIN
	EXEC CopyPARChecklist @newChecklistId;
	EXEC CopyCannedResponsesPAR @newChecklistId;
	EXEC CopyPPRChecklist @newChecklistId;

	-- Question 6
	DECLARE @q6 INT; 
	SELECT @q6 = PARChecklistContentID FROM dbo.PARChecklistContent WHERE QuestionNumber = '6' AND ProposalAdequacyReviewID = @newChecklistId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q6;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - No Exception to submission of Certified Cost and Pricing Data', @q6);

	-- Question 14
	DECLARE @q14 INT;
	DECLARE @q14SortOrder INT;
	DECLARE @q14InstructionsId INT;
	SELECT @q14 = PARChecklistContentID, @q14SortOrder = SortOrder FROM dbo.PARChecklistContent WHERE QuestionNumber = '14' AND ColumnOrder = 1 AND ProposalAdequacyReviewID = @newChecklistId;
	SELECT @q14InstructionsId = PARChecklistContentID FROM dbo.PARChecklistContent WHERE SortOrder = @q14SortOrder AND ColumnOrder = 2 AND ProposalAdequacyReviewID = @newChecklistId;
	UPDATE [dbo].[PARChecklistContent] SET ChecklistText = '<p>CBOM is required for all proposals that include Material and Subcontracts. In either descending dollar or part number order.</p><br /><p>Separate CBOM may be required to identify CLIN pricing.</p><br /><p>For Subcontract only effort, Subcontract Summary Table will satisfy the requirement for a CBOM.</p><br /><p>IWTAs are not included on CBOM unless required by RFP.</p><p><a href=''__BASE_URL__Instruction_14.docx'' target=''_blank''>Additional Instructions</a></p>' WHERE PARChecklistContentId = @q14InstructionsId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q14;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - There are no material or subcontract costs in the proposal', @q14);
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - All material/subcontracts are Actuals', @q14);

	-- Question 16
	DECLARE @q16 INT;
	DECLARE @q16SortOrder INT;
	DECLARE @q16InstructionsId INT;
	SELECT @q16 = PARChecklistContentID, @q16SortOrder = SortOrder FROM dbo.PARChecklistContent WHERE QuestionNumber = '16' AND ColumnOrder = 1 AND ProposalAdequacyReviewID = @newChecklistId;
	SELECT @q16InstructionsId = PARChecklistContentID FROM dbo.PARChecklistContent WHERE SortOrder = @q16SortOrder AND ColumnOrder = 2 AND ProposalAdequacyReviewID = @newChecklistId;
	UPDATE [dbo].[PARChecklistContent] SET ChecklistText = '<p>Subcontractor Proposals (If S/C proposal >= $15M or if S/C proposal > CCoPD threshold and 10% of the Prime Proposal price) Must be included with proposal or include statement how the subcontracts are submitted.</p><br /><p>Note: This includes material suppliers as well; NA for actuals that have previously been definitized.</p><p><a href=''__BASE_URL__Instruction_16.docx'' target=''_blank''>Additional Instructions</a></p>' WHERE PARChecklistContentId = @q16InstructionsId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q16;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - There are no subcontracts that meet/exceed the FAR 15.404-3(c) thresholds', @q16);
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - Subcontracts proposals for those that meet/exceed the FAR 15.404-3(c) thresholds are being submitted directly to the government customer', @q16);
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - All subcontracts are Actuals with no outstanding scope and have previously been definitized', @q16);

	-- Question 17
	DECLARE @q17 INT;
	DECLARE @q17SortOrder INT;
	DECLARE @q17InstructionsId INT;
	SELECT @q17 = PARChecklistContentID, @q17SortOrder = SortOrder FROM dbo.PARChecklistContent WHERE QuestionNumber = '17' AND ColumnOrder = 1 AND ProposalAdequacyReviewID = @newChecklistId;
	SELECT @q17InstructionsId = PARChecklistContentID FROM dbo.PARChecklistContent WHERE SortOrder = @q17SortOrder AND ColumnOrder = 2 AND ProposalAdequacyReviewID = @newChecklistId;
	UPDATE [dbo].[PARChecklistContent] SET ChecklistText = '<p>Reference to Subcontract summary with references to Price analysis, Cost Analysis, PBOEs. PBOEs are required for Subcontracts and for any material items over the CCOPD threshold. </p><br /><p>Cost Analysis and Price Analyses are required for all subcontracts over CCOPD unless you get written concurrence, from the customer, to provide at a later date. If providing at a later date, agreed to milestone dates should be included on the PBOE/MPBOE.</p><br /><p></p>Note: NA for actuals that have previously been definitized.<p><a href=''__BASE_URL__Instruction_17.docx'' target=''_blank''>Additional Instructions</a></p>' WHERE PARChecklistContentId = @q17InstructionsId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q17;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - There are no subcontracts/suppliers that require price/cost analysis to be submitted', @q17);
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - All subcontracts/suppliers are Actuals with no outstanding scope and have previously been definitized', @q17);

	-- Question 18
	DECLARE @q18 INT;
	DECLARE @q18SortOrder INT;
	DECLARE @q18InstructionsId INT;
	SELECT @q18 = PARChecklistContentID, @q18SortOrder = SortOrder FROM dbo.PARChecklistContent WHERE QuestionNumber = '18' AND ColumnOrder = 1 AND ProposalAdequacyReviewID = @newChecklistId;
	SELECT @q18InstructionsId = PARChecklistContentID FROM dbo.PARChecklistContent WHERE SortOrder = @q18SortOrder AND ColumnOrder = 2 AND ProposalAdequacyReviewID = @newChecklistId;
	UPDATE [dbo].[PARChecklistContent] SET ChecklistText = '<p>18. Has the offeror submitted an exception to the submission of certified cost or pricing data for commercial products or commercial services proposed either at the prime or subcontractor level, in accordance with provision 52.215-20?<br />  a.  Has the offeror specifically identified the type of commercial product or commercial service claim (FAR 2.101 “commercial product” or “commercial service” definition), and the basis on which the commercial product or commercial service meets the definition?<br />  b.  For modified commercial products (FAR 2.101 “commercial product” definition); did the offeror classify the modification(s) as either—<br />  &nbsp;&nbsp;i. A modification of a type customarily available in the commercial marketplace (paragraph (3)(i)); or<br />  &nbsp;&nbsp;ii.  A minor modification (paragraph (3)(ii)) of a type not customarily available in the commercial marketplace made to meet Federal Government requirements not exceeding the thresholds in FAR 15.403-1(c)(3)(iii)(B)?<br />  c.  For proposed commercial products “of a type”, or “evolved” or modified (FAR 2.101 “commercial product” definition), did the contractor provide a technical description of the differences between the proposed item and the comparison item(s)?<br />  (FAR 52.215-20; FAR 2.101, "commercial item")</p>', SubmissionItem = 'Has the offeror submitted an exception to the submission of certified cost or pricing data for commercial products or commercial services proposed either at the prime or subcontractor level, in accordance with provision 52.215-20?   a.  Has the offeror specifically identified the type of commercial product or commercial service claim (FAR 2.101 “commercial product” or “commercial service” definition), and the basis on which the commercial product or commercial service meets the definition?  b.  For modified commercial products (FAR 2.101 “commercial product” definition); did the offeror classify the modification(s) as either—      i.  A modification of a type customarily available in the commercial marketplace (paragraph (3)(i)); or       ii. A minor modification (paragraph (3)(ii)) of a type not customarily available in the commercial marketplace made to meet Federal Government requirements not exceeding the thresholds in FAR 15.403-1(c)(3)(iii)(B)?  c.  For proposed commercial products “of a type”, or “evolved” or modified (FAR 2.101 “commercial product” definition), did the contractor provide a technical description of the differences between the proposed item and the comparison item(s)?' WHERE PARChecklistContentId = @q18;
	UPDATE [dbo].[PARChecklistContent] SET ChecklistText = '<p>If a supplier submits a proposal that includes a claim for commercial item exception for an item that is over the CCOPD threshold include the following: LMAP Forms F 335, F 340; F 345 (as applicable).</p><br /><p>The respective LMAP forms are required to be submitted unless you get written concurrence, from the customer, to provide at a later date. If providing at a later date, agreed to milestone dates should be included on the PBOE/MPBOE.</p><br /><p>Note: NA for actuals that have previously been definitized.</p><p><a href=''__BASE_URL__Instruction_18.docx'' target=''_blank''>Additional Instructions</a></p>' WHERE PARChecklistContentId = @q18InstructionsId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q18;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - There are no Exceptions to Certified Cost or Pricing Data for Commercial Item Determination (CID)', @q18);
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - All subcontracts/suppliers are Actuals with no outstanding scope and have previously been definitized', @q18);

	-- Question 20
	DECLARE @q20 INT;
	DECLARE @q20SortOrder INT;
	DECLARE @q20InstructionsId INT;
	SELECT @q20 = PARChecklistContentID, @q20SortOrder = SortOrder FROM dbo.PARChecklistContent WHERE QuestionNumber = '20' AND ColumnOrder = 1 AND ProposalAdequacyReviewID = @newChecklistId;
	SELECT @q20InstructionsId = PARChecklistContentID FROM dbo.PARChecklistContent WHERE SortOrder = @q20SortOrder AND ColumnOrder = 2 AND ProposalAdequacyReviewID = @newChecklistId;
	UPDATE [dbo].[PARChecklistContent] SET ChecklistText = '<p>PBOE, MPBOE and CBOM must clearly state the degree of competition</p><p><a href=''__BASE_URL__Instruction_20.docx'' target=''_blank''>Additional Instructions</a></p>' WHERE PARChecklistContentId = @q20InstructionsId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q20;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - There are no subcontracts/suppliers over CCOPD Threshold that are based on competition', @q20);
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - All subcontracts/suppliers are Actuals', @q20);

	-- Question 21
	DECLARE @q21 INT; 
	SELECT @q21 = PARChecklistContentID FROM dbo.PARChecklistContent WHERE QuestionNumber = '21' AND ProposalAdequacyReviewID = @newChecklistId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q21;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - There are no IWTAs proposed at Cost (IWTA-C) in this proposal', @q21);
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - All IWTAs proposed at Cost (IWTA-C) are Actuals', @q21);

	-- Question 22
	DECLARE @q22 INT; 
	SELECT @q22 = PARChecklistContentID FROM dbo.PARChecklistContent WHERE QuestionNumber = '22' AND ProposalAdequacyReviewID = @newChecklistId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q22;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - There are no IWTAs proposed at Price (IWTA-P) in this proposal', @q22);
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - All IWTAs proposed at Price (IWTA-P) are Actuals', @q22);

	-- Question 23
	DECLARE @q23 INT; 
	SELECT @q23 = PARChecklistContentID FROM dbo.PARChecklistContent WHERE QuestionNumber = '23' AND ProposalAdequacyReviewID = @newChecklistId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q23;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - There are no Labor Hours estimated in this proposal', @q23);
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - All Labor Hours are Actuals', @q23);

	-- Question 24
	DECLARE @q24 INT; 
	SELECT @q24 = PARChecklistContentID FROM dbo.PARChecklistContent WHERE QuestionNumber = '24' AND ProposalAdequacyReviewID = @newChecklistId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q24;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - There are no Labor Hours estimated in this proposal', @q24);
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - All Labor Hours are Actuals', @q24);

	-- Question 27
	DECLARE @q27 INT; 
	SELECT @q27 = PARChecklistContentID FROM dbo.PARChecklistContent WHERE QuestionNumber = '27' AND ProposalAdequacyReviewID = @newChecklistId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q27;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - This proposal does not contain Non-Labor Service Centers or other ODCs', @q27);
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - All Non-Labor Service Centers or other ODCs are Actuals', @q27);

	-- Question 29
	DECLARE @q29 INT;
	DECLARE @q29SortOrder INT;
	DECLARE @q29InstructionsId INT;
	SELECT @q29 = PARChecklistContentID, @q29SortOrder = SortOrder FROM dbo.PARChecklistContent WHERE QuestionNumber = '29' AND ColumnOrder = 1 AND ProposalAdequacyReviewID = @newChecklistId;
	SELECT @q29InstructionsId = PARChecklistContentID FROM dbo.PARChecklistContent WHERE SortOrder = @q29SortOrder AND ColumnOrder = 2 AND ProposalAdequacyReviewID = @newChecklistId;
	UPDATE [dbo].[PARChecklistContent] SET ChecklistText = '<p>Provide 1861 or equivalent forms which show the calculations of the proposed FCCOM/CAS 414 cost. Ensure reference letter are included for the latest FCCOM/CAS 414 rates.</p><p><a href=''__BASE_URL__Instruction_29.docx'' target=''_blank''>Additional Instructions</a></p>' WHERE PARChecklistContentId = @q29InstructionsId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q29;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - FCCOM was not proposed', @q29);
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - FCCOM costs are Actuals', @q29);

	-- Question 36
	DECLARE @q36 INT; 
	SELECT @q36 = PARChecklistContentID FROM dbo.PARChecklistContent WHERE QuestionNumber = '36' AND ProposalAdequacyReviewID = @newChecklistId;
	DELETE FROM [dbo].[CannedResponsesPAR] WHERE QuestionId = @q36;
	INSERT INTO [dbo].[CannedResponsesPAR] (Text, QuestionId) VALUES ('NA - Total procurement does not exceed 70% of the total cost', @q36);
END

/*
                ## END ##

                4/13/23 [RJ] - PROPH-247 - Update FAR Checklist
*/