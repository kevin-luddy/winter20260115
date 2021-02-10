/*
	## START ##

	10/5/2016 [twilson3] -- BOEJ-1466 Roles for BOE Forms
*/

IF NOT EXISTS (SELECT * FROM [dbo].[RoleLU] WHERE [RoleName] = 'Subcontract Administrator')
BEGIN
	INSERT INTO [dbo].[RoleLU] ([RoleID],[RoleName]) VALUES (10, 'Subcontract Administrator')
END
GO
/*
	10/5/2016 [twilson3] -- BOEJ-1466 Roles for BOE Forms

	## END ##
*/

/*
	## START ##

	10/17/2016 [twilson3] -- BOEJ-1477 DB tables for IBOE
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BOEFormIBOE]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[BOEFormIBOE](
		[IBOEFormID] [int] IDENTITY(1,1) NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] [int] NOT NULL,
		[FormName] [varchar](200) NOT NULL,
		[Description] [varchar](max) NOT NULL,
		[BasisAndRationale] [varchar](max) NOT NULL,
		[ProposalTitle] [varchar](200) NOT NULL,
		[ProposalDate] [varchar](10) NOT NULL,
		[Poc] [varchar](50) NOT NULL,
		[PocPhone] [varchar](20) NOT NULL,
		[Approver] [varchar](50) NOT NULL,
		[ApproverPhone] [varchar](20) NOT NULL,
		[BusinessArea] [varchar](50) NOT NULL,
		[Revision] [int] NOT NULL,
		[FormVersion] [int] NOT NULL
		PRIMARY KEY CLUSTERED 
		(
			[IBOEFormID] ASC
			)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
		) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[BOEFormIBOE]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[BOEFormIBOE](
		[IBOEFormID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] [int] NOT NULL,
		[FormName] [varchar](200) NOT NULL,
		[Description] [varchar](max) NOT NULL,
		[BasisAndRationale] [varchar](max) NOT NULL,
		[ProposalTitle] [varchar](200) NOT NULL,
		[ProposalDate] [varchar](10) NOT NULL,
		[Poc] [varchar](50) NOT NULL,
		[PocPhone] [varchar](20) NOT NULL,
		[Approver] [varchar](50) NOT NULL,
		[ApproverPhone] [varchar](20) NOT NULL,
		[BusinessArea] [varchar](50) NOT NULL,
		[Revision] [int] NOT NULL,
		[FormVersion] [int] NOT NULL,
		VersionId	INT				NOT NULL
		) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BOEFormIBOEResourcesXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[BOEFormIBOEResourcesXREF](
		[IBOEFormID] [int] NOT NULL,
		[ResourceID] [int] NOT NULL
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[BOEFormIBOEResourcesXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[BOEFormIBOEResourcesXREF](
		[IBOEFormID] [int] NOT NULL,
		[ResourceID] [int] NOT NULL,
		VersionId	INT				NOT NULL
) ON [PRIMARY]
END
GO

/*
	10/17/2016 [twilson3] -- BOEJ-1477 DB tables for IBOE

	## END ##
*/

/*
	## START ##

	10/17/2016 [twilson3] -- BOEJ-1479 DB tables for PBOE
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BOEFormPBOE]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[BOEFormPBOE](
		[PBOEFormID] [int] IDENTITY(1,1) NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] [int] NOT NULL,
		[FormName] [varchar](200) NOT NULL,
		[Description] [varchar](max) NOT NULL,
		[BasisAndRationale] [varchar](max) NOT NULL,
		[ProposalTitle] [varchar](200) NOT NULL,
		[ProposalDate] [varchar](10) NOT NULL,
		[Poc] [varchar](50) NOT NULL,
		[PocPhone] [varchar](20) NOT NULL,
		[Approver] [varchar](50) NOT NULL,
		[ApproverPhone] [varchar](20) NOT NULL,
		[Revision] [int] NOT NULL,
		[FormVersion] [int] NOT NULL,
		[DegreeOfCompetition] [int] NOT NULL,
		[CCoPD] [int] NOT NULL,
		[CCoPDOtherText] [varchar] (100) NULL,
		[RFP] [varchar](50) NOT NULL,
		[ProposalNumber] [varchar](50) NOT NULL,
		[SupplierName] [varchar](50) NOT NULL,
		[ValidityDate] [varchar](10) NOT NULL,
		[SupplierProposalSupportingDataIncluded] int NOT NULL,
		[PriceAnalysisIncluded] int NOT NULL,
		[CommercialItemDocIncluded] int NOT NULL,
		[CostAnalysisIncluded] int NOT NULL,
		[ShouldCostEstimate] int NOT NULL,
		[ShouldCostEstimateDate] date NULL,
		[ShouldCostEstimateText] varchar(50) NULL,
		[SowWritten] int NOT NULL,
		[SowWrittenDate] date NULL,
		[SowWrittenText] varchar(50) NULL,
		[RFPRelease] int NOT NULL,
		[RFPReleaseDate] date NULL,
		[RFPReleaseText] varchar(50) NULL,
		[FirmSupplierReceipt] int NOT NULL,
		[FirmSupplierReceiptDate] date NULL,
		[FirmSupplierReceiptText] varchar(50) NULL,
		[SourceSelection] int NOT NULL,
		[SourceSelectionDate] date NULL,
		[SourceSelectionText] varchar(50) NULL,
		[CID] int NOT NULL,
		[CIDDate] date NULL,
		[CIDText] varchar(50) NULL,
		[GovtReview] int NOT NULL,
		[GovtReviewDate] date NULL,
		[GovtReviewText] varchar(50) NULL,
		[PriceAnalysis] int NOT NULL,
		[PriceAnalysisDate] date NULL,
		[PriceAnalysisText] varchar(50) NULL,
		[TechnicalEvaluation] int NOT NULL,
		[TechnicalEvaluationDate] date NULL,
		[TechnicalEvaluationText] varchar(50) NULL,
		[FactFinding] int NOT NULL,
		[FactFindingDate] date NULL,
		[FactFindingText] varchar(50) NULL,
		[CostAnalysis] int NOT NULL,
		[CostAnalysisDate] date NULL,
		[CostAnalysisText] varchar(50) NULL,
		[GovtPricing] int NOT NULL,
		[GovtPricingDate] date NULL,
		[GovtPricingText] varchar(50) NULL,
		[SupplierNegotiations] int NOT NULL,
		[SupplierNegotiationsDate] date NULL,
		[SupplierNegotiationsText] varchar(50) NULL,
		[MOU] int NOT NULL,
		[MOUDate] date NULL,
		[MOUText] varchar(50) NULL,
		[Procurement] int NOT NULL,
		[ProcurementDate] date NULL,
		[ProcurementText] varchar(50) NULL,
		[PlannedDate_WrittenApproval] date NULL,
		[PlannedDate_ApprovedSubmission] date NULL
		PRIMARY KEY CLUSTERED 
		(
			[PBOEFormID] ASC
			)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
		) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[BOEFormPBOE]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[BOEFormPBOE](
		[PBOEFormID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] [int] NOT NULL,
		[FormName] [varchar](200) NOT NULL,
		[Description] [varchar](max) NOT NULL,
		[BasisAndRationale] [varchar](max) NOT NULL,
		[ProposalTitle] [varchar](200) NOT NULL,
		[ProposalDate] [varchar](10) NOT NULL,
		[Poc] [varchar](50) NOT NULL,
		[PocPhone] [varchar](20) NOT NULL,
		[Approver] [varchar](50) NOT NULL,
		[ApproverPhone] [varchar](20) NOT NULL,
		[Revision] [int] NOT NULL,
		[FormVersion] [int] NOT NULL,
		[DegreeOfCompetition] [int] NOT NULL,
		[CCoPD] [int] NOT NULL,
		[CCoPDOtherText] [varchar] (100) NULL,
		[RFP] [varchar](50) NOT NULL,
		[ProposalNumber] [varchar](50) NOT NULL,
		[SupplierName] [varchar](50) NOT NULL,
		[ValidityDate] [varchar](10) NOT NULL,
		[SupplierProposalSupportingDataIncluded] int NOT NULL,
		[PriceAnalysisIncluded] int NOT NULL,
		[CommercialItemDocIncluded] int NOT NULL,
		[CostAnalysisIncluded] int NOT NULL,
		[ShouldCostEstimate] int NOT NULL,
		[ShouldCostEstimateDate] date NULL,
		[ShouldCostEstimateText] varchar(50) NULL,
		[SowWritten] int NOT NULL,
		[SowWrittenDate] date NULL,
		[SowWrittenText] varchar(50) NULL,
		[RFPRelease] int NOT NULL,
		[RFPReleaseDate] date NULL,
		[RFPReleaseText] varchar(50) NULL,
		[FirmSupplierReceipt] int NOT NULL,
		[FirmSupplierReceiptDate] date NULL,
		[FirmSupplierReceiptText] varchar(50) NULL,
		[SourceSelection] int NOT NULL,
		[SourceSelectionDate] date NULL,
		[SourceSelectionText] varchar(50) NULL,
		[CID] int NOT NULL,
		[CIDDate] date NULL,
		[CIDText] varchar(50) NULL,
		[GovtReview] int NOT NULL,
		[GovtReviewDate] date NULL,
		[GovtReviewText] varchar(50) NULL,
		[PriceAnalysis] int NOT NULL,
		[PriceAnalysisDate] date NULL,
		[PriceAnalysisText] varchar(50) NULL,
		[TechnicalEvaluation] int NOT NULL,
		[TechnicalEvaluationDate] date NULL,
		[TechnicalEvaluationText] varchar(50) NULL,
		[FactFinding] int NOT NULL,
		[FactFindingDate] date NULL,
		[FactFindingText] varchar(50) NULL,
		[CostAnalysis] int NOT NULL,
		[CostAnalysisDate] date NULL,
		[CostAnalysisText] varchar(50) NULL,
		[GovtPricing] int NOT NULL,
		[GovtPricingDate] date NULL,
		[GovtPricingText] varchar(50) NULL,
		[SupplierNegotiations] int NOT NULL,
		[SupplierNegotiationsDate] date NULL,
		[SupplierNegotiationsText] varchar(50) NULL,
		[MOU] int NOT NULL,
		[MOUDate] date NULL,
		[MOUText] varchar(50) NULL,
		[Procurement] int NOT NULL,
		[ProcurementDate] date NULL,
		[ProcurementText] varchar(50) NULL,
		[PlannedDate_WrittenApproval] date NULL,
		[PlannedDate_ApprovedSubmission] date NULL,
		VersionId	INT				NOT NULL
		) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BOEFormPBOEResourcesXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[BOEFormPBOEResourcesXREF](
		[PBOEFormID] [int] NOT NULL,
		[ResourceID] [int] NOT NULL
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[BOEFormPBOEResourcesXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[BOEFormPBOEResourcesXREF](
		[PBOEFormID] [int] NOT NULL,
		[ResourceID] [int] NOT NULL,
		VersionId	INT				NOT NULL
) ON [PRIMARY]
END
GO

/*
	10/17/2016 [twilson3] -- BOEJ-1479 DB tables for PBOE

	## END ##
*/

/*
	## START ##

	10/27/2016 [twilson3] -- BOEJ-1531 CLIN DB tables for INL Forms
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BOEFormPBOECLINsXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[BOEFormPBOECLINsXREF](
		[PBOEFormID] [int] NOT NULL,
		[ClinID] [int] NOT NULL,
		[ContractType] [int] NOT NULL
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[BOEFormPBOECLINsXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[BOEFormPBOECLINsXREF](
		[PBOEFormID] [int] NOT NULL,
		[ClinID] [int] NOT NULL,
		[ContractType] [int] NOT NULL,
		VersionId	INT	NOT NULL
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BOEFormIBOECLINsXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[BOEFormIBOECLINsXREF](
		[IBOEFormID] [int] NOT NULL,
		[ClinID] [int] NOT NULL,
		[ContractType] [int] NOT NULL
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[BOEFormIBOECLINsXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[BOEFormIBOECLINsXREF](
		[IBOEFormID] [int] NOT NULL,
		[ClinID] [int] NOT NULL,
		[ContractType] [int] NOT NULL,
		VersionId	INT	NOT NULL
) ON [PRIMARY]
END
GO

/*
	10/27/2016 [twilson3] -- BOEJ-1531 CLIN DB tables for INL Forms

	## END ##
*/

/*
	## START ##

	10/19/2016 [pattoncr] -- BOEJ-1322 Rebrand MST -> RMS
*/

-- These changes are to be executed in MST only. The way we can tell the environments apart is that SSC has LOBs in the range of 1000's. MST is 2000+ and ISGS is 0-999
IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusinessLU] WHERE LineOfBusinessId > 2000 AND LineOfBusinessId < 2999)
BEGIN
	update [dbo].[SegmentLU] set Segment = 'RMS' where Segment = 'MST';
	update [dbo].[LineOfBusiness] set LineOfBusinessName = 'Cyber, Ships and Advanced Technologies', LineOfBusinessLongName = 'Cyber, Ships and Advanced Technologies', LineOfBusinessURL = 'Cyber, Shi' where LineOfBusinessName = 'Ship and Aviation Systems';
	update [dbo].[LineOfBusiness] set LineOfBusinessName = 'C4ISR & Undersea Systems', LineOfBusinessLongName = 'C4ISR & Undersea Systems', LineOfBusinessURL = 'C4ISR & Un' where LineOfBusinessName = 'Undersea Systems';
	update [dbo].[LineOfBusiness] set IsActive = 0 where LineOfBusinessName = 'New Ventures';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'Master Template - MST - Portrait';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'MST - Deepwater - Portrait';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'MST - Portrait - 12 point 1 inch margins';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'MST - Portrait - 12 point 1 inch margins - Custom Fields';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'MST - Portrait - 12 point 1 inch margins with genBOE BOE, Task, and Resource IDs';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'MST - Portrait - 12 point 1 inch margins with genBOE IDs and CLIN';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'MST - Portrait - 12 point 1 inch margins with genBOE Task IDs';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'MST - Space Fence - Portrait';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'MST - Standard - Portrait';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'MST Portrait - 12 point 1 inch margins - Custom Fields';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'MST-Master-Test-ver';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'Sanitized - MST - Portrait - 12 point 1 inch margins';
	update [dbo].[OutputFormatTemplate] set Template = REPLACE(Template,'MST','RMS'), TemplateDescription = REPLACE(TemplateDescription,'MST','RMS') where template = 'Updated - MST Portrait with genBOE IDs';
END
GO

/*
	10/19/2016 [pattoncr] -- BOEJ-1322 Rebrand MST -> RMS

	## END ##
*/

/*
	## START ##

	10/26/2016 [Joe] -- iBoe/pBoe Reports. SSC only
*/

-- These changes are to be executed in SSC only. The way we can tell the environments apart is that SSC has LOBs in the range of 1000's. RMS is 2000+ and ISGS is 0-999
IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusinessLU] WHERE LineOfBusinessId > 1000 AND LineOfBusinessId < 1999) AND
	(NOT EXISTS (SELECT 1 FROM [dbo].[ReportLU] WHERE ReportId = 17))
	INSERT INTO [dbo].[ReportLU] VALUES (17, 'BOE Forms', 'Export IBOE/PBOE Forms');
GO

/*
	10/26/2016 [Joe] -- iBoe/pBoe Reports. SSC only

	## END ##
*/

/*
	## START ##

	10/26/2016 [Tom] -- Zone travel stuff
*/

-- Since this feature was not at use yet, it was easier to drop a table and then recreate it.

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_MSTTravelTripCustomFieldValueXREF_MSTTravelTrip')
BEGIN
	ALTER TABLE [dbo].[MSTTravelTripCustomFieldValueXREF] DROP CONSTRAINT [FK_MSTTravelTripCustomFieldValueXREF_MSTTravelTrip];
END
GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_MSTTravelTripCustomFieldValueXREF_MSTCustomFieldValue')
BEGIN
	ALTER TABLE [dbo].[MSTTravelTripCustomFieldValueXREF] DROP CONSTRAINT [FK_MSTTravelTripCustomFieldValueXREF_MSTCustomFieldValue];
END
GO

DROP TABLE [dbo].[MSTTravelTrip];

GO

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[MSTTravelTrip](
	[MSTTravelTripID] [int] IDENTITY(1,1) NOT NULL,
	[ModeID] [int] NOT NULL,
	[TravelTripTaskElementID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[GroupID] [int] NULL,
	[SegmentID] [int] NOT NULL,
	[Purpose] [varchar](35) NULL,
	[PerformingOrganizationID] [int] NOT NULL,
	[TripDate] [date] NOT NULL,
	[EstimateDate] [date] NULL,
	[NumPeople] [int] NOT NULL,
	[NumDays] [int] NOT NULL,
	[ZoneOriginID] [int] NULL,
	[ZoneDestCity] [varchar](35) NULL,
	[ZoneDestinationID] [int] NULL,
	[ZoneResourceID] [int] NULL,
	[NonZoneFrom] [varchar](150) NULL,
	[NonZoneTo] [varchar](150) NULL,
	[NonZoneAirFareEstimate] [money] NULL,
	[NonZonePerDiemDaily] [money] NULL,
	[NonZoneCarRentalTrans] [money] NULL,
	[NonZoneNumCars] [int] NULL,
 CONSTRAINT [PK_dbo.MSTTravelTrip] PRIMARY KEY CLUSTERED 
(
	[MSTTravelTripID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[MSTTravelTrip] WITH CHECK ADD  CONSTRAINT [FK_MSTTravelTrip_MSTTravelModeLU] 
	FOREIGN KEY([ModeID]) REFERENCES [dbo].[MSTTravelModeLU] ([MSTTravelModeID]);
ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_MSTTravelModeLU];

ALTER TABLE [dbo].[MSTTravelTrip]  WITH CHECK ADD  CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelDestination] 
	FOREIGN KEY([ZoneDestinationID]) REFERENCES [dbo].[MSTZoneTravelDestination] ([DestinationID]);
ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelDestination];

ALTER TABLE [dbo].[MSTTravelTrip]  WITH CHECK ADD  CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelOrigin] 
	FOREIGN KEY([ZoneOriginID]) REFERENCES [dbo].[MSTZoneTravelOrigin] ([OriginID]);
ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelOrigin];

ALTER TABLE [dbo].[MSTTravelTrip]  WITH CHECK ADD  CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelResource] 
	FOREIGN KEY([ZoneResourceID]) REFERENCES [dbo].[MSTZoneTravelResource] ([ResourceID]);
ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelResource];

ALTER TABLE [dbo].[MSTTravelTrip]  WITH NOCHECK ADD  CONSTRAINT [FK_MSTTravelTrip_PerformingOrganization] 
	FOREIGN KEY([PerformingOrganizationID]) REFERENCES [dbo].[PerformingOrganization] ([PerformingOrganizationID]);
ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_PerformingOrganization];

ALTER TABLE [dbo].[MSTTravelTrip]  WITH NOCHECK ADD  CONSTRAINT [FK_MSTTravelTrip_SegmentLU] 
	FOREIGN KEY([SegmentID]) REFERENCES [dbo].[SegmentLU] ([SegmentID]);
ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_SegmentLU];

ALTER TABLE [dbo].[MSTTravelTrip]  WITH NOCHECK ADD  CONSTRAINT [FK_MSTTravelTrip_TravelTripTaskElement] 
	FOREIGN KEY([TravelTripTaskElementID]) REFERENCES [dbo].[TravelTripTaskElement] ([TravelTripTaskElementID]);
ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_TravelTripTaskElement];

GO

-- RECREATE MSTTravelTripCustomFieldValueXREF FK CONSTRAINTS

ALTER TABLE [dbo].[MSTTravelTripCustomFieldValueXREF]  WITH NOCHECK ADD  CONSTRAINT [FK_MSTTravelTripCustomFieldValueXREF_MSTCustomFieldValue] 
	FOREIGN KEY([MSTCustomFieldValueID]) REFERENCES [dbo].[CustomFieldValue] ([CustomFieldValueID]);
ALTER TABLE [dbo].[MSTTravelTripCustomFieldValueXREF] CHECK CONSTRAINT [FK_MSTTravelTripCustomFieldValueXREF_MSTCustomFieldValue];

ALTER TABLE [dbo].[MSTTravelTripCustomFieldValueXREF]  WITH NOCHECK ADD  CONSTRAINT [FK_MSTTravelTripCustomFieldValueXREF_MSTTravelTrip] 
	FOREIGN KEY([MSTTravelTripID]) REFERENCES [dbo].[MSTTravelTrip] ([MSTTravelTripID]);
ALTER TABLE [dbo].[MSTTravelTripCustomFieldValueXREF] CHECK CONSTRAINT [FK_MSTTravelTripCustomFieldValueXREF_MSTTravelTrip];
GO

/*
	10/26/2016 [Tom] -- Zone travel stuff

	## END ##
*/

/*
	## START ##

	10/26/2016 [RJ] - Fees and Costs
*/


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MSTTravelNonzoneFeesAndCosts]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[MSTTravelNonzoneFeesAndCosts]
	(
	FeesAndCostsID int IDENTITY(1,1) NOT NULL,
	ModeID int NOT NULL UNIQUE,
	TravelAgencyFee money NOT NULL,
	MiscOther money NOT NULL,
	PRIMARY KEY (FeesAndCostsID),
	FOREIGN KEY (ModeID) REFERENCES [dbo].[MSTTravelModeLU](MSTTravelModeID)
	);
END
GO

-- 11/23/2016 [buckwalj] -- BOEJ-1544 Travel Agency Fee/Misc/Other - Add UpdateDT
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'UpdateDT' AND Object_ID = Object_ID(N'[dbo].[MSTTravelNonzoneFeesAndCosts]'))
	BEGIN
		--If Column does not exist create it
		ALTER TABLE dbo.MSTTravelNonzoneFeesAndCosts
			ADD UpdateDT datetime2(7) 
			NOT NULL CONSTRAINT DF_MSTTravelNonzoneFeesAndCosts_UpdateDT 
			DEFAULT GETDATE()
	END
GO

-- These changes are to be executed in RMS only. The way we can tell the environments apart is that SSC has LOBs in the range of 1000's. RMS is 2000+ and ISGS is 0-999
IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusinessLU] WHERE LineOfBusinessId > 2000 AND LineOfBusinessId < 2999) AND
	NOT EXISTS(SELECT 1 FROM [dbo].[MSTTravelNonzoneFeesAndCosts] WHERE ModeID IN (3, 4))
	INSERT INTO [dbo].[MSTTravelNonzoneFeesAndCosts] VALUES (3, 0, 0, GetDate()), (4, 0, 0, GetDate());

/*
	10/26/2016 [RJ] - Fees and Costs

	## END ##
*/

/*
	## START ##

	10/26/2016 [Dusan] - Adding Zone Tables into Version
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[MSTTravelTrip]') AND type in (N'U'))
BEGIN
CREATE TABLE [version].[MSTTravelTrip](
	[MSTTravelTripID] [int] NOT NULL,
	[ModeID] [int] NOT NULL,
	[TravelTripTaskElementID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[GroupID] [int] NULL,
	[SegmentID] [int] NOT NULL,
	[Purpose] [varchar](35) NULL,
	[PerformingOrganizationID] [int] NOT NULL,
	[TripDate] [date] NOT NULL,
	[EstimateDate] [date] NULL,
	[NumPeople] [int] NOT NULL,
	[NumDays] [int] NOT NULL,
	[ZoneOriginID] [int] NULL,
	[ZoneDestCity] [varchar](35) NULL,
	[ZoneDestinationID] [int] NULL,
	[ZoneResourceID] [int] NULL,
	[NonZoneFrom] [varchar](150) NULL,
	[NonZoneTo] [varchar](150) NULL,
	[NonZoneAirFareEstimate] [money] NULL,
	[NonZonePerDiemDaily] [money] NULL,
	[NonZoneCarRentalTrans] [money] NULL,
	[NonZoneNumCars] [int] NULL,
	VersionId int NOT NULL
	);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[MSTTravelTripCustomFieldValueXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[MSTTravelTripCustomFieldValueXREF](
		[MSTTCFVID] [int] NOT NULL,
		[MSTTravelTripID] [int] NOT NULL,
		[MSTCustomFieldValueID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		VersionId int NOT NULL
		);
END
GO

/*
	10/26/2016 [Dusan] - Adding Zone Tables into Version

	## END ##
*/

/*
	## START ##

	10/27/2016 [buckwalj] -- BOEJ-1484 Adding ContractType column to CLINs
*/
	IF NOT EXISTS(
		SELECT *
		FROM sys.columns 
		WHERE Name      = N'ContractTypeID'
		  AND Object_ID = Object_ID(N'[dbo].[CLIN]'))
	BEGIN
		--If Column does not exist create it
		ALTER TABLE dbo.CLIN ADD ContractTypeID int NULL

		ALTER TABLE dbo.CLIN ADD CONSTRAINT
			FK_CLIN_ContractTypeLU FOREIGN KEY
			(
				ContractTypeID
			) REFERENCES dbo.ContractTypeLU
			(
				ContractTypeID
			) ON UPDATE  NO ACTION 
			 ON DELETE  NO ACTION 
	END

	IF NOT EXISTS(
		SELECT *
		FROM sys.columns 
		WHERE Name      = N'ContractTypeID'
		  AND Object_ID = Object_ID(N'[version].[CLIN]'))
	BEGIN
		--If Column does not exist create it
		ALTER TABLE version.CLIN ADD ContractTypeID int NULL

	END
/*
	10/27/2016 [buckwalj] -- BOEJ-1484 Adding ContractType column to CLINs

	## END ##
*/
/*
	## START ##

	10/31/2016 [twilson3] -- BOEJ-1504 Adding Workspace Rates
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts]
	(
		FeesAndCostsID int IDENTITY(1,1) NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] int NOT NULL,
		ModeID int NOT NULL,
		TravelAgencyFee money NOT NULL,
		MiscOther money NOT NULL,
		PRIMARY KEY (FeesAndCostsID),
		FOREIGN KEY (ModeID) REFERENCES [dbo].[MSTTravelModeLU](MSTTravelModeID),
		FOREIGN KEY (WorkspaceID) REFERENCES [dbo].[Workspace](WorkspaceID)
	);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[WorkspaceRMSTravelNonzoneFeesAndCosts]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[WorkspaceRMSTravelNonzoneFeesAndCosts]
	(
		FeesAndCostsID int NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] int NOT NULL,
		ModeID int NOT NULL,
		TravelAgencyFee money NOT NULL,
		MiscOther money NOT NULL,
		VersionId int NOT NULL
	);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceRMSTravelEscalationRate]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[WorkspaceRMSTravelEscalationRate](
		[TravelEscalationRateID] [int] IDENTITY(1,1) NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] int NOT NULL,
		[Year] [int] NOT NULL,
		[Escalation] [decimal](7, 5) NOT NULL,
		PRIMARY KEY (TravelEscalationRateID),
		FOREIGN KEY (WorkspaceID) REFERENCES [dbo].[Workspace](WorkspaceID)
		);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[WorkspaceRMSTravelEscalationRate]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[WorkspaceRMSTravelEscalationRate](
		[TravelEscalationRateID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] int NOT NULL,
		[Year] [int] NOT NULL,
		[Escalation] [decimal](7, 5) NOT NULL,
		VersionId int NOT NULL
		);
END
GO
/*
	10/31/2016 [twilson3] -- BOEJ-1484 Adding Workspace Rates

	## END ##
*/
/*
	## START ##

	11/1/2016 [Dusan] - BOEJ-1535 Making Number of Days/People/Cars decimal
*/

ALTER TABLE [dbo].[MSTTravelTrip] ALTER COLUMN 	[NumPeople] DECIMAL(10,6) NOT NULL;
ALTER TABLE [dbo].[MSTTravelTrip] ALTER COLUMN 	[NumDays] DECIMAL(10,6) NOT NULL;
ALTER TABLE [dbo].[MSTTravelTrip] ALTER COLUMN 	[NonZoneNumCars] DECIMAL(10,6);
GO

ALTER TABLE [version].[MSTTravelTrip] ALTER COLUMN 	[NumPeople] DECIMAL(10,6) NOT NULL;
ALTER TABLE [version].[MSTTravelTrip] ALTER COLUMN 	[NumDays] DECIMAL(10,6) NOT NULL;
ALTER TABLE [version].[MSTTravelTrip] ALTER COLUMN 	[NonZoneNumCars] DECIMAL(10,6);
GO

/*
	11/1/2016 [Dusan] - BOEJ-1535 Making Number of Days/People/Cars decimal
	
	## END ##
*/
/*
	## START ##

	11/2/2016 [tglick] - Adding Non Zone Resource Id
*/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'NonZoneResourceID' AND Object_ID = Object_ID('[dbo].[MSTTravelTrip]'))
BEGIN
	ALTER TABLE dbo.MSTTravelTrip ADD NonZoneResourceID int NULL

	ALTER TABLE dbo.MSTTravelTrip ADD CONSTRAINT
		 FK_MSTTravelTrip_Resource FOREIGN KEY (NonZoneResourceID) REFERENCES dbo.Resource (ResourceID) 
			ON UPDATE NO ACTION ON DELETE NO ACTION 
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'NonZoneResourceID' AND Object_ID = Object_ID('[version].[MSTTravelTrip]'))
BEGIN
	ALTER TABLE version.MSTTravelTrip ADD NonZoneResourceID int NULL
END
GO
/*
	11/2/2016 [tglick] - Adding Non Zone Resource Id
	
	## END ##
*/

/*
	## START ##

	11/7/2016 [Dusan] - Adding Clin/Wbs into RMS Zone Trip, to support multi CLIN/WBS
*/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ClinId' AND Object_ID = Object_ID('[dbo].[MSTTravelTrip]'))
BEGIN
	ALTER TABLE [dbo].[MSTTravelTrip] ADD ClinId int NULL

	ALTER TABLE [dbo].[MSTTravelTrip] ADD CONSTRAINT
		FK_MSTTravelTrip_Clins FOREIGN KEY (ClinId) REFERENCES [dbo].[Clin](ClinID) 
		ON UPDATE NO ACTION ON DELETE NO ACTION 
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ClinId' AND Object_ID = Object_ID('[version].[MSTTravelTrip]'))
BEGIN
	ALTER TABLE [version].[MSTTravelTrip] ADD ClinId int NULL
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'WbsId' AND Object_ID = Object_ID('[dbo].[MSTTravelTrip]'))
BEGIN
	ALTER TABLE [dbo].[MSTTravelTrip] ADD WbsId int NULL

	ALTER TABLE [dbo].[MSTTravelTrip] ADD CONSTRAINT
		FK_MSTTravelTrip_Wbses FOREIGN KEY (WbsId) REFERENCES [dbo].[WorkBreakdownStructure](WbsId) 
		ON UPDATE NO ACTION ON DELETE NO ACTION 
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'WbsId' AND Object_ID = Object_ID('[version].[MSTTravelTrip]'))
BEGIN
	ALTER TABLE [version].[MSTTravelTrip] ADD WbsId int NULL
END
GO
/*
	11/7/2016 [Dusan] - Adding Clin/Wbs into RMS Zone Trip, to support multi CLIN/WBS
	
	## END ##
*/

/*
	## START ##

	11/8/2016 [RJ] - Making EstimatedDate Not Null
*/
UPDATE [dbo].[MSTTravelTrip]
	SET EstimateDate = GETDATE()
	WHERE EstimateDate IS NULL;

ALTER TABLE [dbo].[MSTTravelTrip]
	ALTER COLUMN EstimateDate DATE NOT NULL;

GO
/*
	11/8/2016 [RJ] - Making EstimatedDate Not Null

	## END ##
*/

/*
	## START ##

	11/21/2016 [Dusan] - Adding a table that will track our DB updates/versioning (mostly to be used by UAT and such)
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BoeDatabaseVersion]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[BoeDatabaseVersion] (
		Id				INT					IDENTITY(1,1)		PRIMARY KEY,
		DBVersion		VARCHAR(100)		NOT NULL,
		AppVersion		VARCHAR(100)		NOT NULL,
		UpdateDate		DATETIME			NOT NULL
	);

END

GO

-- Due to the order in which the files get combined, we need to put this SP in here.. It does have a separate file, 
-- including all of the comments in there, this is a quick and dirty version of it

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateDbVersion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[UpdateDbVersion];

GO

CREATE PROCEDURE [dbo].[UpdateDbVersion](@DbVersion VARCHAR(100), @AppVersion VARCHAR(100)) 
AS
	IF(NOT EXISTS(SELECT 1 FROM [dbo].[BoeDatabaseVersion] WHERE DBVersion = @DbVersion AND AppVersion = @AppVersion))
	BEGIN
		INSERT INTO [dbo].[BoeDatabaseVersion] (DBVersion, AppVersion, UpdateDate)
		VALUES (@DbVersion, @AppVersion, GETDATE())
	END

GO

EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2.13';
GO

/*
	11/21/2016 [Dusan] - Adding a table that will track our DB updates/versioning (mostly to be used by UAT and such)

	## END ##
*/


/*
	## START ##

	11/23/2016 [buckwalj] -- BOEJ-1559 Rename MST -> RMS in resources, fields etc
*/

update [dbo].[MSTZoneTravelOrigin] set [Origin]  = 'RMS Services Dahlgren' where [Origin] = 'MST Services Dahlgren';
update [dbo].[MSTZoneTravelOrigin] set [Origin]  = 'RMS Services Newport' where [Origin] = 'MST Services Newport';
update [dbo].[MSTZoneTravelOrigin] set [Origin]  = 'RMS Services Pax River' where [Origin] = 'MST Services Pax River';
update [dbo].[MSTZoneTravelOrigin] set [Origin]  = 'RMS Services San Diego' where [Origin] = 'MST Services San Diego';
update [dbo].[MSTZoneTravelOrigin] set [Origin]  = 'RMS Services Virginia Beach' where [Origin] = 'MST Services Virginia Beach';
update [dbo].[MSTZoneTravelOrigin] set [Origin]  = 'RMS Services Albuquerque' where [Origin] = 'MST Services Albuquerque';
update [dbo].[MSTZoneTravelOrigin] set [Origin]  = 'RMS Services Ft Walton Beach' where [Origin] = 'MST Services Ft Walton Beach';
update [dbo].[MSTZoneTravelOrigin] set [Origin]  = 'RMS Services Little Rock' where [Origin] = 'MST Services Little Rock';
update [dbo].[MSTZoneTravelOrigin] set [Origin]  = 'San Diego-RMS' where [Origin] = 'San Diego-MST';
update [dbo].[MSTZoneTravelResource] set [Description]  = 'San Diego-RMS Per Diem/Misc Zone1' where [Description] = 'San Diego-MST Per Diem/Misc Zone1';
update [dbo].[MSTZoneTravelResource] set [Description]  = 'San Diego-RMS Per Diem/Misc Zone2' where [Description] = 'San Diego-MST Per Diem/Misc Zone2';
update [dbo].[MSTZoneTravelResource] set [Description]  = 'San Diego-RMS Per Diem/Misc Zone3' where [Description] = 'San Diego-MST Per Diem/Misc Zone3';
update [dbo].[MSTZoneTravelResource] set [Description]  = 'San Diego-RMS Per Diem/Misc Zone4' where [Description] = 'San Diego-MST Per Diem/Misc Zone4';
update [dbo].[MSTZoneTravelResource] set [Description]  = 'San Diego-RMS Per Diem/Misc Zone5' where [Description] = 'San Diego-MST Per Diem/Misc Zone5';
update [dbo].[MSTZoneTravelResource] set [Description]  = 'San Diego-RMS Per Diem/Misc Zone6' where [Description] = 'San Diego-MST Per Diem/Misc Zone6';
update [dbo].[MSTZoneTravelResource] set [Description]  = 'San Diego-RMS Airfare to Zone1' where [Description] = 'San Diego-MST Airfare to Zone1';
update [dbo].[MSTZoneTravelResource] set [Description]  = 'San Diego-RMS Airfare to Zone2' where [Description] = 'San Diego-MST Airfare to Zone2';
update [dbo].[MSTZoneTravelResource] set [Description]  = 'San Diego-RMS Airfare to Zone3' where [Description] = 'San Diego-MST Airfare to Zone3';
update [dbo].[MSTZoneTravelResource] set [Description]  = 'San Diego-RMS Airfare to Zone4' where [Description] = 'San Diego-MST Airfare to Zone4';
update [dbo].[MSTZoneTravelResource] set [Description]  = 'San Diego-RMS Airfare to Zone5' where [Description] = 'San Diego-MST Airfare to Zone5';
update [dbo].[MSTZoneTravelResource] set [Description]  = 'San Diego-RMS Airfare to Zone6' where [Description] = 'San Diego-MST Airfare to Zone6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services DahlgrenPR1' where [LookupValue] = 'MST Services DahlgrenPR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services DahlgrenPR2' where [LookupValue] = 'MST Services DahlgrenPR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services DahlgrenPR3' where [LookupValue] = 'MST Services DahlgrenPR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services DahlgrenPR4' where [LookupValue] = 'MST Services DahlgrenPR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services DahlgrenPR5' where [LookupValue] = 'MST Services DahlgrenPR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services DahlgrenPR6' where [LookupValue] = 'MST Services DahlgrenPR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services DahlgrenTR1' where [LookupValue] = 'MST Services DahlgrenTR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services DahlgrenTR2' where [LookupValue] = 'MST Services DahlgrenTR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services DahlgrenTR3' where [LookupValue] = 'MST Services DahlgrenTR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services DahlgrenTR4' where [LookupValue] = 'MST Services DahlgrenTR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services DahlgrenTR5' where [LookupValue] = 'MST Services DahlgrenTR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services DahlgrenTR6' where [LookupValue] = 'MST Services DahlgrenTR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services AlbuquerquePR1' where [LookupValue] = 'MST Services AlbuquerquePR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services AlbuquerquePR2' where [LookupValue] = 'MST Services AlbuquerquePR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services AlbuquerquePR3' where [LookupValue] = 'MST Services AlbuquerquePR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services AlbuquerquePR4' where [LookupValue] = 'MST Services AlbuquerquePR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services AlbuquerquePR5' where [LookupValue] = 'MST Services AlbuquerquePR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services AlbuquerquePR6' where [LookupValue] = 'MST Services AlbuquerquePR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services AlbuquerqueTR1' where [LookupValue] = 'MST Services AlbuquerqueTR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services AlbuquerqueTR2' where [LookupValue] = 'MST Services AlbuquerqueTR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services AlbuquerqueTR3' where [LookupValue] = 'MST Services AlbuquerqueTR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services AlbuquerqueTR4' where [LookupValue] = 'MST Services AlbuquerqueTR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services AlbuquerqueTR5' where [LookupValue] = 'MST Services AlbuquerqueTR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services AlbuquerqueTR6' where [LookupValue] = 'MST Services AlbuquerqueTR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Ft Walton BeachPR1' where [LookupValue] = 'MST Services Ft Walton BeachPR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Ft Walton BeachPR2' where [LookupValue] = 'MST Services Ft Walton BeachPR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Ft Walton BeachPR3' where [LookupValue] = 'MST Services Ft Walton BeachPR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Ft Walton BeachPR4' where [LookupValue] = 'MST Services Ft Walton BeachPR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Ft Walton BeachPR5' where [LookupValue] = 'MST Services Ft Walton BeachPR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Ft Walton BeachPR6' where [LookupValue] = 'MST Services Ft Walton BeachPR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Ft Walton BeachTR1' where [LookupValue] = 'MST Services Ft Walton BeachTR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Ft Walton BeachTR2' where [LookupValue] = 'MST Services Ft Walton BeachTR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Ft Walton BeachTR3' where [LookupValue] = 'MST Services Ft Walton BeachTR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Ft Walton BeachTR4' where [LookupValue] = 'MST Services Ft Walton BeachTR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Ft Walton BeachTR5' where [LookupValue] = 'MST Services Ft Walton BeachTR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Ft Walton BeachTR6' where [LookupValue] = 'MST Services Ft Walton BeachTR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Little RockPR1' where [LookupValue] = 'MST Services Little RockPR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Little RockPR2' where [LookupValue] = 'MST Services Little RockPR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Little RockPR3' where [LookupValue] = 'MST Services Little RockPR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Little RockPR4' where [LookupValue] = 'MST Services Little RockPR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Little RockPR5' where [LookupValue] = 'MST Services Little RockPR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Little RockPR6' where [LookupValue] = 'MST Services Little RockPR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Little RockTR1' where [LookupValue] = 'MST Services Little RockTR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Little RockTR2' where [LookupValue] = 'MST Services Little RockTR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Little RockTR3' where [LookupValue] = 'MST Services Little RockTR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Little RockTR4' where [LookupValue] = 'MST Services Little RockTR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Little RockTR5' where [LookupValue] = 'MST Services Little RockTR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Little RockTR6' where [LookupValue] = 'MST Services Little RockTR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services NewportPR1' where [LookupValue] = 'MST Services NewportPR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services NewportPR2' where [LookupValue] = 'MST Services NewportPR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services NewportPR3' where [LookupValue] = 'MST Services NewportPR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services NewportPR4' where [LookupValue] = 'MST Services NewportPR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services NewportPR5' where [LookupValue] = 'MST Services NewportPR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services NewportPR6' where [LookupValue] = 'MST Services NewportPR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services NewportTR1' where [LookupValue] = 'MST Services NewportTR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services NewportTR2' where [LookupValue] = 'MST Services NewportTR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services NewportTR3' where [LookupValue] = 'MST Services NewportTR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services NewportTR4' where [LookupValue] = 'MST Services NewportTR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services NewportTR5' where [LookupValue] = 'MST Services NewportTR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services NewportTR6' where [LookupValue] = 'MST Services NewportTR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Pax RiverPR1' where [LookupValue] = 'MST Services Pax RiverPR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Pax RiverPR2' where [LookupValue] = 'MST Services Pax RiverPR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Pax RiverPR3' where [LookupValue] = 'MST Services Pax RiverPR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Pax RiverPR4' where [LookupValue] = 'MST Services Pax RiverPR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Pax RiverPR5' where [LookupValue] = 'MST Services Pax RiverPR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Pax RiverPR6' where [LookupValue] = 'MST Services Pax RiverPR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Pax RiverTR1' where [LookupValue] = 'MST Services Pax RiverTR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Pax RiverTR2' where [LookupValue] = 'MST Services Pax RiverTR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Pax RiverTR3' where [LookupValue] = 'MST Services Pax RiverTR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Pax RiverTR4' where [LookupValue] = 'MST Services Pax RiverTR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Pax RiverTR5' where [LookupValue] = 'MST Services Pax RiverTR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Pax RiverTR6' where [LookupValue] = 'MST Services Pax RiverTR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services San DiegoPR1' where [LookupValue] = 'MST Services San DiegoPR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services San DiegoPR2' where [LookupValue] = 'MST Services San DiegoPR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services San DiegoPR3' where [LookupValue] = 'MST Services San DiegoPR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services San DiegoPR4' where [LookupValue] = 'MST Services San DiegoPR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services San DiegoPR5' where [LookupValue] = 'MST Services San DiegoPR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services San DiegoPR6' where [LookupValue] = 'MST Services San DiegoPR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services San DiegoTR1' where [LookupValue] = 'MST Services San DiegoTR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services San DiegoTR2' where [LookupValue] = 'MST Services San DiegoTR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services San DiegoTR3' where [LookupValue] = 'MST Services San DiegoTR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services San DiegoTR4' where [LookupValue] = 'MST Services San DiegoTR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services San DiegoTR5' where [LookupValue] = 'MST Services San DiegoTR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services San DiegoTR6' where [LookupValue] = 'MST Services San DiegoTR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'San Diego-RMSPR1' where [LookupValue] = 'San Diego-MSTPR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'San Diego-RMSPR2' where [LookupValue] = 'San Diego-MSTPR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'San Diego-RMSPR3' where [LookupValue] = 'San Diego-MSTPR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'San Diego-RMSPR4' where [LookupValue] = 'San Diego-MSTPR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'San Diego-RMSPR5' where [LookupValue] = 'San Diego-MSTPR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'San Diego-RMSPR6' where [LookupValue] = 'San Diego-MSTPR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'San Diego-RMSTR1' where [LookupValue] = 'San Diego-MSTTR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'San Diego-RMSTR2' where [LookupValue] = 'San Diego-MSTTR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'San Diego-RMSTR3' where [LookupValue] = 'San Diego-MSTTR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'San Diego-RMSTR4' where [LookupValue] = 'San Diego-MSTTR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'San Diego-RMSTR5' where [LookupValue] = 'San Diego-MSTTR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'San Diego-RMSTR6' where [LookupValue] = 'San Diego-MSTTR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Virginia BeachPR1' where [LookupValue] = 'MST Services Virginia BeachPR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Virginia BeachPR2' where [LookupValue] = 'MST Services Virginia BeachPR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Virginia BeachPR3' where [LookupValue] = 'MST Services Virginia BeachPR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Virginia BeachPR4' where [LookupValue] = 'MST Services Virginia BeachPR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Virginia BeachPR5' where [LookupValue] = 'MST Services Virginia BeachPR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Virginia BeachPR6' where [LookupValue] = 'MST Services Virginia BeachPR6';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Virginia BeachTR1' where [LookupValue] = 'MST Services Virginia BeachTR1';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Virginia BeachTR2' where [LookupValue] = 'MST Services Virginia BeachTR2';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Virginia BeachTR3' where [LookupValue] = 'MST Services Virginia BeachTR3';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Virginia BeachTR4' where [LookupValue] = 'MST Services Virginia BeachTR4';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Virginia BeachTR5' where [LookupValue] = 'MST Services Virginia BeachTR5';
update [dbo].[MSTZoneTravelResource] set [LookupValue]  = 'RMS Services Virginia BeachTR6' where [LookupValue] = 'MST Services Virginia BeachTR6';

GO

/*
	11/23/2016 [buckwalj] -- BOEJ-1559 Rename MST -> RMS in resources, fields etc

	## END ##
*/

/*
	## START ##

	11/28/2016 [Tom] -- Adding ProPricer company column
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProPricerCompanyLU]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[ProPricerCompanyLU](
		[ProPricerCompanyID] [int] NOT NULL,
		[ProPricerCompany] [varchar](50) NOT NULL,
		CONSTRAINT [PK_ProPricerCompanyLU] PRIMARY KEY CLUSTERED 
		(
			[ProPricerCompanyID] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
	) ON [PRIMARY]

	--------------------------------------------------------------------------------------------------------
	-- INSERT STATEMENTS FOR propricercompanylu table
	--------------------------------------------------------------------------------------------------------
	insert into propricercompanylu values(0, 'N/A')
	insert into propricercompanylu values(1, 'IS&GS')
	insert into propricercompanylu values(2, 'Space')
	insert into propricercompanylu values(3, 'RMS')
	--------------------------------------------------------------------------------------------------------

	--------------------------------------------------------------------------------------------------------
	-- ADD ProPricerCompanyID COLUMN TO dbo.ProPricerFieldLU TABLE
	-- Defaults all to 0, which sets them to 'N/A' (any company, rms/Space/etc.).
	--------------------------------------------------------------------------------------------------------
	ALTER TABLE dbo.ProPricerFieldLU ADD ProPricerCompanyID int NOT NULL CONSTRAINT DF_ProPricerFieldLU_ProPricerCompanyID DEFAULT 0;

	--------------------------------------------------------------------------------------------------------
	-- ADD relationship constraint FOR [dbo].[ProPricerFieldLU] -- [dbo].[ProPricerCompanyLU]
	-- Defaults all to 0, which sets them to 'N/A' (any company, rms/Space/etc.).
	--------------------------------------------------------------------------------------------------------
	ALTER TABLE [dbo].[ProPricerFieldLU]  WITH CHECK ADD  CONSTRAINT [FK_ProPricerFieldLU_ProPricerCompanyLU] 
		FOREIGN KEY([ProPricerCompanyID]) REFERENCES [dbo].[ProPricerCompanyLU] ([ProPricerCompanyID]);

	ALTER TABLE [dbo].[ProPricerFieldLU] CHECK CONSTRAINT [FK_ProPricerFieldLU_ProPricerCompanyLU];

END
GO
--------------------------------------------------------------------------------------------------------
-- Delete all newly added propricer travel for rms travel, then re-add with appropriate companyid
-- set COMPANYID for rms travel fields to companyid '3' (rms)
--------------------------------------------------------------------------------------------------------

DELETE FROM dbo.ProPricerFieldXREF where ProPricerFieldID > 36;
DELETE FROM dbo.ProPricerFieldLU where ProPricerFieldID > 36;

--insert new propricer fields with task/resource & company designator 
INSERT INTO dbo.ProPricerFieldLU VALUES(37,'Trip_NumPeople',1,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(38,'Trip_NumDays',1,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(39,'Trip_Origin',1,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(40,'Trip_Destination',1,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(41,'Trip_AirEst',1,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(42,'Trip_PerDiem',1,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(43,'Trip_CarRental',1,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(44,'Trip_NumCars',1,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(45,'Trip_GroupID',2,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(46,'Trip_TripDate',2,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(47,'Trip_Origin',2,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(48,'Trip_Destination',2,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(49,'Trip_NumCars',2,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(50,'Trip_TotalCost',2,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(51,'Trip_ID',1,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(52,'Trip_GroupID',1,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(53,'Trip_Purpose',1,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(54,'Trip_ResouceID',1,3);
INSERT INTO dbo.ProPricerFieldLU VALUES(55,'Trip_ResourceID',2,3);

GO

/*
	11/28/2016 [Tom] -- Adding ProPricer company column

	## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '2', @AppVersion = '2.13';
GO

/*
	## START ##
	
	11/22/2016 [twilson3] - BOEJ:1601 Adding a table for Elmah
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_Error]') AND type in (N'U'))
BEGIN

	CREATE TABLE [dbo].[ELMAH_Error]
	(
		[ErrorId]     UNIQUEIDENTIFIER NOT NULL,
		[Application] NVARCHAR(60)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[Host]        NVARCHAR(50)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[Type]        NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[Source]      NVARCHAR(60)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[Message]     NVARCHAR(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[User]        NVARCHAR(50)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[StatusCode]  INT NOT NULL,
		[TimeUtc]     DATETIME NOT NULL,
		[Sequence]    INT IDENTITY (1, 1) NOT NULL,
		[AllXml]      NTEXT COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
	) 
	ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

	ALTER TABLE [dbo].[ELMAH_Error] WITH NOCHECK ADD 
		CONSTRAINT [PK_ELMAH_Error] PRIMARY KEY NONCLUSTERED ([ErrorId]) ON [PRIMARY] 
	
	ALTER TABLE [dbo].[ELMAH_Error] ADD 
		CONSTRAINT [DF_ELMAH_Error_ErrorId] DEFAULT (NEWID()) FOR [ErrorId]
	
	CREATE NONCLUSTERED INDEX [IX_ELMAH_Error_App_Time_Seq] ON [dbo].[ELMAH_Error] 
	(
		[Application]   ASC,
		[TimeUtc]       DESC,
		[Sequence]      DESC
	) 
	ON [PRIMARY]

END

GO
/*
	11/22/2016 [twilson3] - BOEJ:1601 Adding a table for Elmah

	## END ##
*/

/*
	## START ##

	12/10/2016 [Dusan] - Adding 2 additional rates to the Travel Escalation Rate table
*/

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'MiscRate' AND Object_ID = Object_ID('[dbo].[TravelEscalationRate]'))
BEGIN
	ALTER TABLE [dbo].[TravelEscalationRate] ADD MiscRate DECIMAL(7,5);
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'MiscRate' AND Object_ID = Object_ID('[dbo].[WorkspaceLockedTravelEscalationRate]'))
BEGIN
	ALTER TABLE [dbo].[WorkspaceLockedTravelEscalationRate] ADD MiscRate DECIMAL(7,5);
	ALTER TABLE [version].[WorkspaceLockedTravelEscalationRate] ADD MiscRate DECIMAL(7,5);
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'MiscRate' AND Object_ID = Object_ID('[dbo].[WorkspaceRMSTravelEscalationRate]'))
BEGIN
	ALTER TABLE [dbo].[WorkspaceRMSTravelEscalationRate] ADD MiscRate DECIMAL(7,5);
	ALTER TABLE [version].[WorkspaceRMSTravelEscalationRate] ADD MiscRate DECIMAL(7,5);

	ALTER TABLE [dbo].[WorkspaceRMSTravelEscalationRate] ADD PerDiemRate DECIMAL(7,5);
	ALTER TABLE [version].[WorkspaceRMSTravelEscalationRate] ADD PerDiemRate DECIMAL(7,5);
END
GO

EXEC [dbo].[UpdateDbVersion] @DbVersion = '3', @AppVersion = '2.13';
GO

/*
	12/10/2016 [Dusan] - Adding 2 additional rates to the Travel Escalation Rate table
	
	## END ##
*/

/*
	## START ##

	12/13/2016 [Tom] - delete trip_totalcost field from existing propricer export templates and then from propricerfield lookup table
*/

DELETE FROM dbo.propricerFieldXREF WHERE propricerfieldid = 50;
DELETE FROM dbo.propricerfieldlu WHERE propricerfieldid = 50;
GO

EXEC [dbo].[UpdateDbVersion] @DbVersion = '4', @AppVersion = '2.13';
GO

/*
	12/13/2016 [Tom] - delete trip_totalcost field from existing propricer export templates and then from propricerfield lookup table
	
	## END ##
*/
