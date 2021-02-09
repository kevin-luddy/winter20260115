EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.3';
GO
/*
       ## START ##
       
       3/05/2018     brunworg      BOEJ-3124 Add Forecasted Tracking Number into PTM
*/
       IF NOT EXISTS (
                           SELECT * FROM sys.all_columns C
                                  INNER JOIN sys.tables T on C.object_id = T.object_id
                                  INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
                           WHERE
                                  T.name = 'Proposal' AND
                                  C.name = 'ForecastedTrackingID' AND
                                  S.name = 'dbo'
                           )
BEGIN
       ALTER TABLE [dbo].[Proposal] ADD ForecastedTrackingID varchar(13) NULL
END
GO
/*
       3/05/2018     brunworg      BOEJ-3124 Add Forecasted Tracking Number into PTM

       ## END ##
*/

/*
       ## START ##
       
       3/05/2018     brunworg      BOEJ-3116 Add "IsReadOnly" flag with default of 0 into PTM ProposalClassLU table.
                                                Also, add a row with value 'Forecasted', with the IsReadOnly flag being true.
*/
       IF NOT EXISTS (
                           SELECT * FROM sys.all_columns C
                                  INNER JOIN sys.tables T on C.object_id = T.object_id
                                  INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
                           WHERE
                                  T.name = 'ProposalClassLU' AND
                                  C.name = 'IsReadOnly' AND
                                  S.name = 'dbo'
                           )
BEGIN
       ALTER TABLE [dbo].[ProposalClassLU] ADD IsReadOnly bit NOT NULL Default (0)
END
GO

IF(NOT EXISTS(SELECT 1 FROM ProposalClassLU WHERE ProposalClass = 'Forecasted'))
BEGIN
INSERT INTO ProposalClassLU (ProposalClass, IsActive, IsReadOnly)
       VALUES ('Forecasted', 1, 1);
END
GO
/*
       3/05/2018     brunworg      BOEJ-3116 Add "IsReadOnly" flag with default of 0 into PTM ProposalClassLU table.
                                                Also, add a row with value 'Forecasted', with the IsReadOnly flag being true.

       ## END ##
*/
/*
       ## START ##
       
       3/13/2018     twilson3      BOEJ-3156 Make some columns in Proposal Nullable for Forecast Proposals
*/
IF EXISTS (
                    SELECT * FROM sys.all_columns C
                            INNER JOIN sys.tables T on C.object_id = T.object_id
                            INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
                    WHERE
                            T.name = 'Proposal' AND
                            C.name = 'RFPNumber' AND
                            S.name = 'dbo' AND
							is_nullable = 0
                    )
BEGIN
       ALTER TABLE Proposal ALTER COLUMN RFPNumber VARCHAR(40) NULL
END
GO

IF EXISTS (
                    SELECT * FROM sys.all_columns C
                            INNER JOIN sys.tables T on C.object_id = T.object_id
                            INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
                    WHERE
                            T.name = 'Proposal' AND
                            C.name = 'ProposalTrackingID' AND
                            S.name = 'dbo' AND
							is_nullable = 0
                    )
BEGIN
       ALTER TABLE Proposal ALTER COLUMN ProposalTrackingID VARCHAR(13) NULL
END
GO

IF EXISTS (
                    SELECT * FROM sys.all_columns C
                            INNER JOIN sys.tables T on C.object_id = T.object_id
                            INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
                    WHERE
                            T.name = 'Proposal' AND
                            C.name = 'RequestTypeID' AND
                            S.name = 'dbo' AND
							is_nullable = 0
                    )
BEGIN
       ALTER TABLE Proposal ALTER COLUMN RequestTypeID int NULL
END
GO
/*
       3/13/2018     twilson3      BOEJ-3156 Make some columns in Proposal Nullable for Forecast Proposals

       ## END ##
*/
/*
       ## START ##
       
       3/14/2018     twilson3      BOEJ-3118 Email notification for Forecasted Proposal
*/
IF NOT EXISTS (
                           SELECT * FROM sys.all_columns C
                                  INNER JOIN sys.tables T on C.object_id = T.object_id
                                  INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
                           WHERE
                                  T.name = 'Proposal' AND
                                  C.name = 'ForecastEmailSent' AND
                                  S.name = 'dbo'
                           )
BEGIN
       ALTER TABLE [dbo].[Proposal] ADD ForecastEmailSent bit NOT NULL Default (0)
END
GO

/*
       3/14/2018     twilson3      BOEJ-3118 Email notification for Forecasted Proposal

       ## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '2', @AppVersion = '2018.3';
GO

/*
        ## START ##
                
        4/02/2018 [brunworg] - BOEJ-3256 - Change SSC -> Space in PTM
		4/23/2018 Dusan - BOEJ-3423 - Change $750K to CCoPD Threshold in question 16
*/
IF NOT EXISTS (SELECT 1 FROM [dbo].[ProposalPricingReview] WHERE [ChecklistVersion] = 10)
BEGIN

    BEGIN TRANSACTION
    BEGIN TRY
        -- Deactivate previous PPR versions
        UPDATE [dbo].[ProposalPricingReview] 
        SET [IsCurrent] = 0

        -- Create new PPR version
        SET IDENTITY_INSERT [dbo].[ProposalPricingReview] ON
        INSERT [dbo].[ProposalPricingReview] ([ProposalPricingReviewID], [ChecklistVersion], [IsCurrent], [ProposalChecklistTypeID]) VALUES (11, 10, 1, 1)
        SET IDENTITY_INSERT [dbo].[ProposalPricingReview] OFF

        -- Deactivate previous PAR version
        UPDATE [dbo].[ProposalAdequacyReview]
        SET [IsCurrent] = 0

        -- Create new PAR version
        SET IDENTITY_INSERT [dbo].[ProposalAdequacyReview] ON
        INSERT [dbo].[ProposalAdequacyReview] ([ProposalAdequacyReviewID], [ChecklistVersion], [IsCurrent], [ProposalChecklistTypeID]) VALUES (11, 10, 1, 1)
        SET IDENTITY_INSERT [dbo].[ProposalAdequacyReview] OFF

        -- Insert PPR Checklist rows
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>&nbsp;<span style="font-style:italic; font-weight: bold;">To be completed by the Lead Estimator when Certified Cost or Pricing Data is Required.</span></p><br/>', 1, 1, 1, 11);
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>&nbsp;<span style="font-style:italic; font-weight: bold; color:red">Note: If you answer "No" to any of these questions, provide an explanation in the Comments section below.</span></p>', 1, 2, 1, 11);
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<b>Proposal Pricing Review Items</b> ', 2, 3, 1, 11);
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>1. Is the application of Fee evident? </p>', 4, 4, 1, 11);
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>2. Are contract closeout costs included in the estimate, or have such costs been deferred by contractual provision? </p>', 4, 5, 1, 11);
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>3. Does the proposal contain the appropriate proprietary data legends, including legends on any electronic media and on any PC-based cost model (e.g., Excel) spreadsheets? </p>', 4, 6, 1, 11);
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>4. Have all IWTA Facilities Capital Cost of Money (FCCM) been separately identified and excluded from the base for fee calculations? </p>', 4, 7, 1, 11);
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>5. Is the application shown of any rate not provided on the cost summaries, as well as escalation (including hours/dollars base, factor, and resulting hours/dollars)? </p>', 4, 8, 1, 11);
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>6. Are the derivation/source of skill mix, and the resulting composite activity type labor rate provided? </p>', 4, 9, 1, 11);
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>7. Are the applicable mandatory disclosure items included in the proposal? </p>', 4, 10, 1, 11);
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>8. Did you provide the PREMIUM overtime dollars, to Contracts, required to complete the FAR contract clause 22.103-5(b)? </p>', 4, 11, 1, 11);
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>9. Did you include the solicitation # and the CAGE code on your cover sheet? </p>', 4, 12, 1, 11);
        INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p><span style="font-style:italic; font-weight: bold;">Comments required for all “No” responses. All information must be unclassified and non-export controlled.</span></p>', 5, 13, 1, 11);

        -- Insert PAR Checklist rows
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<span style="font-style:italic; font-weight: bold;">To be completed by the Lead Estimator for all Proposals that require Certified Cost or Pricing Data.</span>', 1, 1, 1, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<span style="color: #002060; font-style:italic; font-weight: bold; font-size: 10px;">&nbsp;&nbsp;- This includes all IWTA proposals, regardless of value, if Prime requires Certified Cost or Pricing Data</span>', 1, 2, 1, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<br/><span style="color: red; font-style:italic; font-weight:bold;">Note: Answer "Yes" if items are included in proposal. If any of the items are not included, select "No" and provide an explanation in the Comments section.</span><br/>', 1, 3, 1, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<br/>', 1, 4, 1, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<b>DFARS Proposal Adequacy Checklist Items</b> ', 2, 5, 1, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<b>General Instructions</b>', 2, 5, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>General Instructions</b></p>', 2, 6, 1, 11, NULL, NULL, 'GENERAL INSTRUCTIONS');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 6, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p> 1. Is there a properly completed first page of the proposal per FAR 15.408 Table 15-2 I.A or as specified in the solicitation? (FAR 15.408, Table 15-2, Section I Paragraph A) </p>', 4, 7, 1, 11, '1', 'FAR 15.408, Table 15-2, Section I Paragraph A', 'Is there a properly completed first page of the proposal per FAR 15.408 Table 15-2 I.A or as specified in the solicitation?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><span>Use the Space Cover Sheet for CCOPD available on the Estimating SharePoint. This cover sheet shall be page one of your proposal.</span></p><br /><p><span>A separate breakout by CLIN may be included as an attachment. Ensure all fields are completed.</span></p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_01.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 7, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>2. Does the proposal identify the need for Government-furnished material/tooling/test equipment? Include the accountable contract number and contracting officer contact information if known. (FAR 15.408, Table 15-2, Section I Paragraph A(7))</p>', 4, 8, 1, 11, '2', 'FAR 15.408, Table 15-2, Section I Paragraph A(7)', 'Does the proposal identify the need for Government-furnished material/tooling/test equipment? Include the accountable contract number and contracting officer contact information if known. ');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><span>This criteria is applicable for existing and new Government-furnished material/tooling/test equipment.</span></p><br /><p><span>Include the accountable contract number and contracting officer contact information if known.</span></p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_02.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 8, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>3. Does the proposal identify and explain notifications of noncompliance with Cost Accounting Standards Board or Cost Accounting Standards (CAS); any proposal inconsistencies with your disclosed practices or applicable CAS; and inconsistencies with your established estimating and accounting principles and procedures? (FAR 15.408, Table 15-2, Section I Paragraph A(8)) </p>', 4, 9, 1, 11, '3', 'FAR 15.408, Table 15-2, Section I Paragraph A(8)', 'Does the proposal identify and explain notifications of noncompliance with Cost Accounting Standards Board or Cost Accounting Standards (CAS); any proposal inconsistencies with your disclosed practices or applicable CAS; and inconsistencies with your established estimating and accounting principles and procedures?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Provide Space Disclosures for any CAS violations or inconsistencies.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_03.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 9, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>4. Does the proposal disclose any other known activity that could materially impact the costs? This may include, but is not limited to, such factors as— <br />  (1) Vendor quotations;<br />  (2) Nonrecurring costs;<br />  (3) Information on changes in production methods and in production or purchasing volume;<br />  (4) Data supporting projections of business prospects and objectives and related operations costs;<br />  (5) Unit-cost trends such as those associated with labor efficiency;<br />  (6) Make-or-buy decisions;<br />  (7) Estimated resources to attain business goals; and<br />  (8) Information on management decisions that could have a significant bearing on costs.<br />  (FAR 15.408, Table 15-2, Section I, Paragraph C(1); FAR 2.101, “Cost or pricing data”)  </p>', 4, 10, 1, 11, '4', 'FAR 15.408, Table 15-2, Section I, Paragraph C(1); FAR 2.101, Cost or pricing data', 'Does the proposal disclose any other known activity that could materially impact the costs? This may include, but is not limited to, such factors as—    (1) Vendor quotations;   (2) Nonrecurring costs;   (3) Information on changes in production methods and in production or purchasing volume;   (4) Data supporting projections of business prospects and objectives and related operations costs;   (5) Unit-cost trends such as those associated with labor efficiency;   (6) Make-or-buy decisions;   (7) Estimated resources to attain business goals; and   (8) Information on management decisions that could have a significant bearing on costs.');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Provide all applicable Space Disclosures.</p><p>All proposal must be priced based on current information. If specific information is known that is not included in pricing this information must be disclosed and referenced to in the comment section.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_04.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 10, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>5. Is an Index of all certified cost or pricing data and information accompanying or identified in the proposal provided and appropriately referenced? (FAR 15.408, Table 15-2, Section I Paragraph B)</p>', 4, 11, 1, 11, '5', 'FAR 15.408, Table 15-2, Section I Paragraph B', 'Is an Index of all certified cost or pricing data and information accompanying or identified in the proposal provided and appropriately referenced?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Table of Contents with Section references is required.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_05.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 11, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>6. Are there any exceptions to submission of certified cost or pricing data pursuant to FAR 15.403-1(b)? If so, is supporting documentation included in the proposal? (Note questions 18-20.) (FAR 15.403-1(b))</p>', 4, 12, 1, 11, '6', 'FAR 15.403-1(b)', 'Are there any exceptions to submission of certified cost or pricing data pursuant to FAR 15.403-1(b)? If so, is supporting documentation included in the proposal? (Note questions 18-20.)');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Are there any exceptions to submission of certified cost or pricing data within the proposal (i.e. Subs/Material) in accordance with FAR 15.403-1(b)?<br />  (1) When prices agreed upon are based on adequate price competition <br />  (2) When prices agreed upon are based on prices set by law or regulation <br />   (3) When a commercial item is being acquired <br />  (4) When a waiver has been granted <span><u>(by the head of the government contracting agency)</u></span>; or<br />  (5) When modifying a contract or subcontract for commercial items<br /><br />If Yes, included supporting documentation in the cost volume (Note questions 18-20)</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_06.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 12, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>7. Does the proposal disclose the judgmental factors applied and the mathematical or other methods used in the estimate, including those used in projecting from known data? (FAR 15.408, Table 15-2, Section I Paragraph C(2)(i))</p>', 4, 13, 1, 11, '7', 'FAR 15.408, Table 15-2, Section I Paragraph C(2)(i)', 'Does the proposal disclose the judgmental factors applied and the mathematical or other methods used in the estimate, including those used in projecting from known data?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Ensure BOEs are documented and any judgment or factors are fully explained. References all sections with BOEs.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_07.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 13, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>8. Does the proposal disclose the nature and amount of any contingencies included in the proposed price? (FAR 15.408, Table 15-2, Section I Paragraph C(2)(ii))</p>', 4, 14, 1, 11, '8', 'FAR 15.408, Table 15-2, Section I Paragraph C(2)(ii)', 'Does the proposal disclose the nature and amount of any contingencies included in the proposed price? ');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>The LOB Estimating manager and Central Estimating should be fully informed of the nature and amount of any contingencies proposed.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_08.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 14, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>9. Does the proposal explain the basis of all cost estimating relationships (labor hours or material) proposed on other than a discrete basis? (FAR 15.408 Table 15-2, Section II, Paragraph A or B)</p>', 4, 15, 1, 11, '9', 'FAR 15.408 Table 15-2, Section II, Paragraph A or B', 'Does the proposal explain the basis of all cost estimating relationships (labor hours or material) proposed on other than a discrete basis?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Insert section # of any Cost Estimating Relationship (CER) and/or Historical Experience Factor (HEF).</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_09.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 15, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>10. Is there a summary of total cost by element of cost and are the elements of cost cross-referenced to the supporting cost or pricing data? (Breakdowns for each cost element must be consistent with your cost accounting system, including breakdown by year.) (FAR 15.408, Table 15-2, Section I Paragraphs D and E)</p>', 4, 16, 1, 11, '10', 'FAR 15.408, Table 15-2, Section I Paragraphs D and E', 'Is there a summary of total cost by element of cost and are the elements of cost cross-referenced to the supporting cost or pricing data? (Breakdowns for each cost element must be consistent with your cost accounting system, including breakdown by year.)');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Total Cost Summary by Element of Cost, indexed to supporting documentation in the Cost Volume.</p><p>Total Cost Element Summary by CY (and GFY if required).</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_10.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 16, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>11. If more than one Contract Line Item Number (CLIN) or sub Contract Line Item Number (sub-CLIN) is proposed as required by the RFP, are there summary total amounts covering all line items for each element of cost and is it cross-referenced to the supporting cost or pricing data? (FAR 15.408, Table 15-2, Section I Paragraphs D and E)</p>', 4, 17, 1, 11, '11', 'FAR 15.408, Table 15-2, Section I Paragraphs D and E', 'If more than one Contract Line Item Number (CLIN) or sub Contract Line Item Number (sub-CLIN) is proposed as required by the RFP, are there summary total amounts covering all line items for each element of cost and is it cross-referenced to the supporting cost or pricing data?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Total Cost Summary by Element of Cost for each CLIN/sub-CLIN (if applicable), indexed to supporting documentation in the Cost Volume.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_11.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 17, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>12. Does the proposal identify any incurred costs for work performed before the submission of the proposal? (FAR 15.408, Table 15-2, Section I Paragraph F)</p>', 4, 18, 1, 11, '12', 'FAR 15.408, Table 15-2, Section I Paragraph F', 'Does the proposal identify any incurred costs for work performed before the submission of the proposal?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>There must be a separate cost report (priced with current pricing rates, for the year being priced) that identifies actual costs already performed. </p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_12.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 18, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>13. Is there a Government forward pricing rate agreement (FPRA)? If so, the offeror shall identify the official submittal of such rate and factor data. If not, does the proposal include all rates and factors by year that are utilized in the development of the proposal and the basis for those rates and factors? (FAR 15.408, Table 15-2, Section I Paragraph G)</p>', 4, 19, 1, 11, '13', 'FAR 15.408, Table 15-2, Section I Paragraph G', 'Is there a Government forward pricing rate agreement (FPRA)? If so, the offeror shall identify the official submittal of such rate and factor data. If not, does the proposal include all rates and factors by year that are utilized in the development of the proposal and the basis for those rates and factors?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Refer to Rates and Factor Section where the FPRA’s are referenced.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_13.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 19, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Cost Elements</b></p>', 2, 20, 1, 11, NULL, NULL, 'COST ELEMENTS');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 20, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Materials and Services</b></p>', 2, 21, 1, 11, NULL, NULL, 'MATERIALS AND SERVICES');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 21, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>14. Does the proposal include a consolidated summary of individual material and services, frequently referred to as a Consolidated Bill of Material (CBOM), to include the basis for pricing? The offeror’s consolidated summary shall include raw materials, parts, components, assemblies, subcontracts and services to be produced or performed by others, <u>identifying as a minimum the item, source, quantity, and price.</u> (FAR 15.408, Table 15-2, Section II Paragraph A)</p>', 4, 22, 1, 11, '14', 'FAR 15.408, Table 15-2, Section II Paragraph A', 'Does the proposal include a consolidated summary of individual material and services, frequently referred to as a Consolidated Bill of Material (CBOM), to include the basis for pricing? The offeror’s consolidated summary shall include raw materials, parts, components, assemblies, subcontracts and services to be produced or performed by others, identifying as a minimum the item, source, quantity, and price.');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>CBOM is required for all proposals that include Material and Subcontracts.  In either descending dollar or part number order.</p><br /><p>Separate CBOM may be required to identify CLIN pricing.</p><br /><p>IWTAs are not included on CBOM unless required by RFP.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_14.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 22, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Subcontracts</b></p>', 2, 23, 1, 11, NULL, NULL, 'SUBCONTRACTS (Purchased materials or services)');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 23, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>15. Has the offeror identified in the proposal those subcontractor proposals, for which the contracting officer has initiated or may need to request field pricing analysis? (DFARS 215.404-3)</p>', 4, 24, 1, 11, '15', 'DFARS 215.404-3', 'Has the offeror identified in the proposal those subcontractor proposals, for which the contracting officer has initiated or may need to request field pricing analysis?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Reference Subcontract Summary Table, if the cost volume includes subcontracts. </p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_15.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 24, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>16. Per the thresholds of FAR 15.404-3(c), Subcontract Pricing Considerations, does the proposal include a copy of the applicable subcontractor’s certified cost or pricing data? (FAR 15.404-3(c); FAR 52.244-2)</p>', 4, 25, 1, 11, '16', 'FAR 15.404-3(c); FAR 52.244-2', 'Per the thresholds of FAR 15.404-3(c), Subcontract Pricing Considerations, does the proposal include a copy of the applicable subcontractor’s certified cost or pricing data?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Subcontractor Proposals (If S/C proposal >= $13.5M or if S/C proposal > CCoPD threshold and 10% of the Prime Proposal price) Must be included with proposal or include statement how the subcontracts are submitted. </p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_16.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 25, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>17. Is there a price/cost analysis establishing the reasonableness of each of the proposed subcontracts included with the proposal? If the offeror’s price/cost analyses are not provided with the proposal, does the proposal include a matrix identifying dates for receipt of subcontractor proposal, completion of fact finding for purposes of price/cost analysis, and submission of the price/cost analysis? (FAR 15.408, Table 15-2, Note 1; Section II Paragraph A)</p>', 4, 26, 1, 11, '17', 'FAR 15.408, Table 15-2, Note 1; Section II Paragraph A', 'Is there a price/cost analysis establishing the reasonableness of each of the proposed subcontracts included with the proposal? If the offeror’s price/cost analyses are not provided with the proposal, does the proposal include a matrix identifying dates for receipt of subcontractor proposal, completion of fact finding for purposes of price/cost analysis, and submission of the price/cost analysis?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Reference to Subcontract summary with references to Price analysis, Cost Analysis, PBOEs.  PBOEs are required for Subcontracts and for any material items over the CCOPD threshold. </p><br /><p>Cost Analysis are required for all subcontracts over CCOPD unless you get written concurrence, from the customer, to provide at a later date.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_17.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 26, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Exceptions to Certified Cost or Pricing Data</b></p>', 2, 27, 1, 11, NULL, NULL, 'EXCEPTIONS TO CERTIFIED COST OR PRICING DATA');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 27, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>18. Has the offeror submitted an exception to the submission of certified cost or pricing data for commercial items proposed either at the prime or subcontractor level, in accordance with provision 52.215-20?<br />  a.  Has the offeror specifically identified the type of commercial item claim (FAR 2.101 commercial item definition, paragraphs (1) through (8)), and the basis on which the item meets the definition?<br />  b.  For modified commercial items (FAR 2.101 commercial item definition paragraph (3)); did the offeror classify the modification(s) as either—<br />  &nbsp;&nbsp;i.  A modification of a type customarily available in the commercial marketplace (paragraph (3)(i)); or<br />  &nbsp;&nbsp;ii.  A minor modification (paragraph (3)(ii)) of a type not customarily available in the commercial marketplace made to meet Federal Government requirements not exceeding the thresholds in FAR 15.403-1(c)(3)(iii)(B)?<br />  c.  For proposed commercial items "of a type", or "evolved" or modified (FAR 2.101 commercial item definition paragraphs (1) through (3)), did the contractor provide a technical description of the differences between the proposed item and the comparison item(s)?<br />  (FAR 52.215-20; FAR 2.101, "commercial item")</p>', 4, 28, 1, 11, '18', 'FAR 52.215-20; FAR 2.101, commercial item', 'Has the offeror submitted an exception to the submission of certified cost or pricing data for commercial items proposed either at the prime or subcontractor level, in accordance with provision 52.215-20?   a.  Has the offeror specifically identified the type of commercial item claim (FAR 2.101 commercial item definition, paragraphs (1) through (8)), and the basis on which the item meets the definition?  b.  For modified commercial items (FAR 2.101 commercial item definition paragraph (3));  did the offeror classify the modification(s) as either—      i.  A modification of a type customarily available in the commercial marketplace (paragraph (3)(i)); or       ii.  A minor modification (paragraph (3)(ii)) of a type not customarily available in the commercial marketplace made to meet Federal Government requirements not exceeding the thresholds in FAR 15.403-1(c)(3)(iii)(B)?  c.  For proposed commercial items “of a type”, or “evolved” or modified (FAR 2.101 commercial item definition paragraphs (1) through (3)), did the contractor provide a technical description of the differences between the proposed item and the comparison item(s)?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>If a supplier submits a proposal that includes a claim for commercial item exception for an item that is over the CCOPD threshold  include the following: LMAP Forms F 335, F 340; F 345 (as applicable).</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_18.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 28, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>19.<span>[Reserved]</span></p>', 4, 29, 1, 11, '19', 'Reserved', NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 4, 29, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>20. Does the proposal support the degree of competition and the basis for establishing the source and reasonableness of price for each subcontract or purchase order priced on a competitive basis exceeding the threshold for certified cost or pricing data? (FAR 15.408, Table 15-2, Section II Paragraph A(1))</p>', 4, 30, 1, 11, '20', 'FAR 15.408, Table 15-2, Section II Paragraph A(1)', 'Does the proposal support the degree of competition and the basis for establishing the source and reasonableness of price for each subcontract or purchase order priced on a competitive basis exceeding the threshold for certified cost or pricing data?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>PBOE, IBOE or CBOM must clearly state the degree of competition</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_20.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 30, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Interorganizational Transfers</b></p>', 2, 31, 1, 11, NULL, NULL, 'INTERORGANIZATIONAL TRANSFERS');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 31, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>21. For inter-organizational transfers proposed at cost, does the proposal include a complete cost proposal in compliance with Table 15-2? (FAR 15.408, Table 15-2, Section II Paragraph A.(2))</p>', 4, 32, 1, 11, '21', 'FAR 15.408, Table 15-2, Section II Paragraph A.(2)', 'For inter-organizational transfers proposed at cost, does the proposal include a complete cost proposal in compliance with Table 15-2?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Regardless of the IWTA value you must provide a FAR 15.408, Table 15-2, compliant IWTA Proposal</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_21.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 32, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>22. For inter-organizational transfers proposed at price in accordance with FAR 31.205-26(e), does the proposal provide an analysis by the prime that supports the exception from certified cost or pricing data in accordance with FAR 15.403-1? (FAR 15.408, Table 15-2, Section II Paragraph A(1))</p>', 4, 33, 1, 11, '22', 'FAR 15.408, Table 15-2, Section II Paragraph A(1)', 'For inter-organizational transfers proposed at price in accordance with FAR 31.205-26(e), does the proposal provide an analysis by the prime that supports the exception from certified cost or pricing data in accordance with FAR 15.403-1?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>IBOE must provide the analysis to support an IWTA "P" </p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_22.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 33, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Direct Labor</b></p>', 2, 34, 1, 11, NULL, NULL, 'DIRECT LABOR');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 34, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>23. Does the proposal include a time phased (i.e.; monthly, quarterly) breakdown of labor hours, rates and costs by category or skill level? If labor is the allocation base for indirect costs, the labor cost must be summarized in order that the applicable overhead rate can be applied. (FAR 15.408, Table 15-2, Section II Paragraph B)</p>', 4, 35, 1, 11, '23', 'FAR 15.408, Table 15-2, Section II Paragraph B', 'Does the proposal include a time phased (i.e.; monthly, quarterly) breakdown of labor hours, rates and costs by category or skill level? If labor is the allocation base for indirect costs, the labor cost must be summarized in order that the applicable overhead rate can be applied.');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Normal practice is to summarize cost at an annual level that ties back to the BOE.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_23.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 35, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>24. For labor Basis of Estimates (BOEs), does the proposal include labor categories, labor hours, and task descriptions, (e.g.; Statement of Work reference, applicable CLIN, Work Breakdown Structure, rationale for estimate, applicable history, and time-phasing)? (FAR 15.408, Table 15-2, Section II Paragraph B)</p>', 4, 36, 1, 11, '24', 'FAR 15.408, Table 15-2, Section II Paragraph B', 'For labor Basis of Estimates (BOEs), does the proposal include labor categories, labor hours, and task descriptions, (e.g.; Statement of Work reference, applicable CLIN, Work Breakdown Structure, rationale for estimate, applicable history, and time-phasing)?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_24.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 36, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>25. If covered by the Service Contract Labor Standards statute (41 U.S.C. chapter 67), are the rates in the proposal in compliance with the minimum rates specified in the statute? (FAR subpart 22.10)</p>', 4, 37, 1, 11, '25', 'FAR subpart 22.10', 'If covered by the Service Contract Labor Standards statute (41 U.S.C. chapter 67), are the rates in the proposal in compliance with the minimum rates specified in the statute?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_25.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 37, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Indirect Costs</b></p>', 2, 38, 1, 11, NULL, NULL, 'INDIRECT COSTS');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 38, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>26. Does the proposal indicate the basis of estimate for proposed indirect costs and how they are applied? (Support for the indirect rates could consist of cost breakdowns, trends, and budgetary data.) (FAR 15.408, Table 15-2, Section II Paragraph C)</p>', 4, 39, 1, 11, '26', 'FAR 15.408, Table 15-2, Section II Paragraph C', 'Does the proposal indicate the basis of estimate for proposed indirect costs and how they are applied? (Support for the indirect rates could consist of cost breakdowns, trends, and budgetary data.)');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Current status of all rates used in the proposal must be provided. Pricing must show application of rates.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_26.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 39, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Other Costs</b></p>', 2, 40, 1, 11, NULL, NULL, 'OTHER COSTS');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 40, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>27. Does the proposal include other direct costs and the basis for pricing? If travel is included does the proposal include number of trips, number of people, number of days per trip, locations, and rates (e.g. airfare, per diem, hotel, car rental, etc)? (FAR 15.408, Table 15-2, Section II Paragraph D)</p>', 4, 41, 1, 11, '27', 'FAR 15.408, Table 15-2, Section II Paragraph D', 'Does the proposal include other direct costs and the basis for pricing? If travel is included does the proposal include number of trips, number of people, number of days per trip, locations, and rates (e.g. airfare, per diem, hotel, car rental, etc)?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>All ODC BOE must be included in proposal.  If Travel is estimated with the use of a HEF the backup analysis must be provided.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_27.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 41, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>28. If royalties exceed $1,500 does the proposal provide the information/data identified by Table 15-2? (FAR 15.408, Table 15-2, Section II Paragraph E)</p>', 4, 42, 1, 11, '28', 'FAR 15.408, Table 15-2, Section II Paragraph E', ' If royalties exceed $1,500 does the proposal provide the information/data identified by Table 15-2? ');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_28.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 42, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>29. When facilities capital cost of money is proposed, does the proposal include submission of Form CASB-CMF or reference to an FPRA/FPRP and show the calculation of the proposed amount? (FAR 15.408, Table 15-2, Section II Paragraph F)</p>', 4, 43, 1, 11, '29', 'FAR 15.408, Table 15-2, Section II Paragraph F', ' When facilities capital cost of money is proposed, does the proposal include submission of Form CASB-CMF or reference to an FPRA/FPRP and show the calculation of the proposed amount?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Provide 1861 forms which show the calculations of the proposed FCCOM/CAS 414 cost. Ensure reference letter are included for the latest FCCOM/CAS 414 rates.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_29.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 43, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Formats For Submission of Line Item Summaries</b></p>', 2, 44, 1, 11, NULL, NULL, 'FORMATS FOR SUBMISSION OF LINE ITEM SUMMARIES');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 44, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>30. Are all cost element breakdowns provided using the applicable format prescribed in FAR 15.408, Table 15-2 III? (or alternative format if specified in the request for proposal) (FAR 15.408, Table 15-2, Section III)</p>', 4, 45, 1, 11, '30', 'FAR 15.408, Table 15-2, Section III', 'Are all cost element breakdowns provided using the applicable format prescribed in FAR 15.408, Table 15-2 III? (or alternative format if specified in the request for proposal)');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Ensure you have the correct FAR 15.408, 15-2 format</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_30.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 45, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>31. If the proposal is for a modification or change order, have cost of work deleted (credits) and cost of work added (debits) been provided in the format described in FAR 15.408, Table 15-2.III.B? (FAR 15.408, Table 15-2, Section III Paragraph B)</p>', 4, 46, 1, 11, '31', 'FAR 15.408, Table 15-2, Section III Paragraph B', 'If the proposal is for a modification or change order, have cost of work deleted (credits) and cost of work added (debits) been provided in the format described in FAR 15.408, Table 15-2.III.B?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_31.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 46, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>32. For price revisions/redeterminations, does the proposal follow the format in FAR 15.408, Table 15-2.III.C?</p>', 4, 47, 1, 11, '32', 'FAR 15.408, Table 15-2, Section III Paragraph C', 'For price revisions/redeterminations, does the proposal follow the format in FAR 15.408, Table 15-2.III.C?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_32.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 47, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Other</b></p>', 2, 48, 1, 11, NULL, NULL, 'OTHER');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 48, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>33. If an incentive contract type, does the proposal include offeror proposed target cost, target profit or fee, share ratio, and, when applicable, minimum/maximum fee, ceiling price? (FAR 16.4)</p>', 4, 49, 1, 11, '33', 'FAR 16.4', 'If an incentive contract type, does the proposal include offeror proposed target cost, target profit or fee, share ratio, and, when applicable, minimum/maximum fee, ceiling price?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Obtain from contracts if applicable</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_33.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 49, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>34. If Economic Price Adjustments are being proposed, does the proposal show the rationale and application for the economic price adjustment? (FAR 16.203-4 and FAR 15.408 Table 15-2, Section II, Paragraphs A, B, C, and D)</p>', 4, 50, 1, 11, '34', 'FAR 16.203-4 and FAR 15.408 Table 15-2, Section II, Paragraphs A, B, C, and D', 'If Economic Price Adjustments are being proposed, does the proposal show the rationale and application for the economic price adjustment?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_34.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 50, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>35. If the offeror is proposing Performance-Based Payments did the offeror comply with FAR 52.232-28? (FAR 52.232-28)</p>', 4, 51, 1, 11, '35', 'FAR 52.232-28', 'If the offeror is proposing Performance-Based Payments did the offeror comply with FAR 52.232-28?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_35.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 51, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>36. Excessive Pass-through Charges– Identification of Subcontract Effort: If the offeror intends to subcontract more than 70% of the total cost of work to be performed, does the proposal identify: (i) the amount of the offeror’s indirect costs and profit applicable to the work to be performed by the proposed subcontractor(s); and (ii) a description of the added value provided by the offeror as related to the work to be performed by the proposed subcontractor(s)? (FAR 15.408(n); FAR 52.215-22; FAR 52.215-23)</p>', 4, 52, 1, 11, '36', 'FAR 15.408(n); FAR 52.215-22; FAR 52.215-23', 'Excessive Pass-through Charges– Identification of Subcontract Effort: If the offeror intends to subcontract more than 70% of the total cost of work to be performed, does the proposal identify: (i) the amount of the offeror’s indirect costs and profit applicable to the work to be performed by the proposed subcontractor(s); and (ii) a description of the added value provided by the offeror as related to the work to be performed by the proposed subcontractor(s)?');
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>If Subcontract costs exceeds 70% of the total value of this proposal ensure you add a statement for "added value" that LM Space is providing.</p><p><a href=''https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_36.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 52, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><span style="font-weight:bold; font-style:italic;">Lead Estimator Comments (<span style="color: #0070c0">comments required for all "No" responses</span>)  <span style="color: red">All information must be unclassified and non-export controlled.</span></span></p>', 5, 53, 1, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 5, 53, 2, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><span style="font-weight:bold; font-style:italic;">Peer Reviewer Comments (<span style="color: #0070c0">comments required for all "No" responses</span>)  <span style="color: red">All information must be unclassified and non-export controlled.</span></span></p>', 6, 54, 1, 11, NULL, NULL, NULL);
        INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 6, 54, 2, 11, NULL, NULL, NULL);

        COMMIT TRANSACTION
    END TRY

    BEGIN CATCH
        ROLLBACK TRANSACTION
                
        DECLARE @ErrorMessage varchar (500)
        SELECT @ErrorMessage = ERROR_MESSAGE()
        RAISERROR (
            @ErrorMessage, -- Message text.
            11, -- Severity,/*Severity Changed to 11*/
            1 -- State,
            )
        RETURN
    END CATCH
END

/*
    4/02/2018 [brunworg] - BOEJ-3256 - Change SSC -> Space in PTM

    ## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '3', @AppVersion = '2018.3';
GO

/*
		## START ##
		4/2/2018     twilson3      BOEJ-3244 PTM BOE Data inconsistent
*/

ALTER TABLE [ProductLine] ALTER COLUMN [ProductLineName] VARCHAR(30) NOT NULL
GO

Update [ProductLine] SET [ProductLineName] = 'Advanced Technology Center' WHERE [ProductLineName] = 'ATC';
Update [ProductLine] SET [ProductLineName] = 'Strategic & Missile Defense' WHERE [ProductLineName] = 'SMD';
Update [ProductLine] SET [ProductLineName] = 'Special Programs' WHERE [ProductLineName] = 'SP';
Update [ProductLine] SET IsActive = 0 WHERE [ProductLineName] IN ('Comm Space', 'Civil Space', 'Commercial Launch', 'AWE');

IF NOT EXISTS (SELECT 1 FROM [dbo].[ProductLine] WHERE [ProductLineName] = 'Commercial/Civil Space')
BEGIN
	SET IDENTITY_INSERT [dbo].[ProductLine] ON
	INSERT INTO [ProductLine] ([ProductLineID],[ProductLineName],[ProductLineLongName],[ProductLineURL],[BusinessAreaID],[ForesightProductLineID],[IsActive]) VALUES (18, 'Commercial/Civil Space', 'Commercial/Civil Space', 'CC_Space', 2, -1, 1);
	SET IDENTITY_INSERT [dbo].[ProductLine] OFF
	SET IDENTITY_INSERT [dbo].[ContractTypeLU] ON
	INSERT INTO [ContractTypeLU] ([ContractTypeID],[ContractType],[IsActive]) VALUES (21, 'FPIS', 1);
	INSERT INTO [ContractTypeLU] ([ContractTypeID],[ContractType],[IsActive]) VALUES (22, 'CPAF/IF', 1);
	INSERT INTO [ContractTypeLU] ([ContractTypeID],[ContractType],[IsActive]) VALUES (23, 'CPLOE', 1);
	INSERT INTO [ContractTypeLU] ([ContractTypeID],[ContractType],[IsActive]) VALUES (24, 'CPNF', 1);
	SET IDENTITY_INSERT [dbo].[ContractTypeLU] OFF
END

UPDATE [ContractTypeLU] SET [IsActive] = 0 WHERE [ContractType] IN ('Time and Material Level of Effort', 'Other')
UPDATE [LineOfBusiness] SET [ProductLineID] = 18 WHERE [ProductLineID] IN (10, 11)
UPDATE [LineOfBusiness] SET [IsActive] = 0 WHERE [LineOfBusinessID] = 82
GO

/*
		4/2/2018     twilson3      BOEJ-3244 PTM BOE Data inconsistent

		## END ##
*/


/*
	## START ##
	11/29/17 [pattoncr] - BOEJ-2839 AD utils - NTID uniqueness - Step 2 (Remove Domain)
*/
-- THIS SOMEHOW DIDN'T MAKE IT INTO THE SCRIPTS FROM 2018.1, so we are rerunning it again.

-- Drop and recreate constraint (to remove NTDomain reference).
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[genTRACUser]') AND name = N'UK_genTRACUser')
ALTER TABLE [dbo].[genTRACUser] DROP CONSTRAINT [UK_genTRACUser]
GO

ALTER TABLE [dbo].[genTRACUser] ADD CONSTRAINT [UK_genTRACUser] UNIQUE NONCLUSTERED 
(
	[NTID] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO

-- Drop column from genTRACUser.
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'NTDomain' AND Object_ID = Object_ID(N'[dbo].[genTRACUser]'))
ALTER TABLE dbo.[genTRACUser] DROP COLUMN NTDomain
GO

/*
	11/29/17 [pattoncr] - BOEJ-2839 AD utils - NTID uniqueness - Step 2 (Remove Domain)
	## END ##
*/ 