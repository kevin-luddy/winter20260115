
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProposalClassLU]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[ProposalClassLU](
		[ProposalClassID] [int] IDENTITY(1,1) NOT NULL,
		[ProposalClass] [varchar](50) NOT NULL,
		[IsActive] [bit] NOT NULL,
		 CONSTRAINT [PK_ProposalClassLU] PRIMARY KEY CLUSTERED 
		(
			[ProposalClassID] ASC
		)WITH (PAD_INDEX  = ON, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 100) ON [PRIMARY]
		) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[ProposalClassLU] WHERE ProposalClassID = 1)
BEGIN
	-- Regular values.
	INSERT INTO [dbo].[ProposalClassLU] VALUES
		('Firm', 1),
		('NTE', 1),
		('ROM', 1);

	-- 'Not Set' value.
	SET identity_insert [dbo].[ProposalClassLU] ON

	INSERT [dbo].[ProposalClassLU](ProposalClassID, ProposalClass, IsActive)
	VALUES (0, 'Not Set', 1)

	SET identity_insert [dbo].[ProposalClassLU] OFF
END

GO

IF NOT EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'Proposal' AND
					C.name = 'ProposalClassID' AND
					S.name = 'dbo'
				)
BEGIN
	-- Add column and set default value (of 'Not Set').
	ALTER TABLE [dbo].[Proposal]
		ADD [ProposalClassID] [int] NOT NULL DEFAULT 0

		-- Create Foreign Key.
	ALTER TABLE [dbo].[Proposal]  WITH CHECK 
		ADD  CONSTRAINT [FK_Proposal_ProposalClassLU] FOREIGN KEY([ProposalClassID])
		REFERENCES [dbo].[ProposalClassLU] ([ProposalClassID])
END
GO

ALTER TABLE dbo.Proposal
	ALTER COLUMN [EstimatedProposalValue] BIGINT;
	
SET IDENTITY_INSERT [dbo].[PricingToolLU] ON;
GO
IF NOT EXISTS ( SELECT 1 FROM [dbo].[PricingToolLU] WHERE PricingToolId = 0)
BEGIN
	INSERT INTO [dbo].[PricingToolLU] (PricingToolId, PricingTool, IsActive) VALUES (0, 'None', 1);
END
SET IDENTITY_INSERT [dbo].[PricingToolLU] OFF;
GO

SET IDENTITY_INSERT [dbo].[BoeToolLU] ON;
GO
IF NOT EXISTS ( SELECT 1 FROM [dbo].[BoeToolLU] WHERE BoeToolId = 0)
BEGIN
	INSERT INTO [dbo].[BoeToolLU] (BoeToolId, BoeTool, IsActive) VALUES (0, 'None', 1);
END
SET IDENTITY_INSERT [dbo].[BoeToolLU] OFF;


SET IDENTITY_INSERT [dbo].[ProposalLocationLU] ON;
GO
IF NOT EXISTS ( SELECT 1 FROM [dbo].[ProposalLocationLU] WHERE ProposalLocationId = 0)
BEGIN
	INSERT INTO [dbo].[ProposalLocationLU] (ProposalLocationId, ProposalLocation, IsActive) VALUES (0, 'None', 1);

END
SET IDENTITY_INSERT [dbo].[ProposalLocationLU] OFF;
GO

Update [dbo].[PARChecklistContent] set [ChecklistText] = REPLACE([ChecklistText], 'Pricer Comm', 'Estimator Comm') where [ChecklistText] like '%Pricer Comm%'
Update [dbo].[PARChecklistContent] set [ChecklistText] = REPLACE([ChecklistText], 'leted by the Pricer ', 'leted by the Estimator ') where [ChecklistText] like '%leted by the Pricer %'

/*   ***   Start BOEJ-1130 *** */
Update [dbo].CostElementLU set CostElement = 'TDY/Relocation' where CostElement = 'TDY'
/*   ***   END BOEJ-1130 *** */

/*   ***   Start BOEJ-1168 *** */
Update [dbo].[PARChecklistContent] set [ChecklistText] = REPLACE([ChecklistText], 'Pricer Comm', 'Estimator Comm') where [ChecklistText] like '%Pricer Comm%'
Update [dbo].[PARChecklistContent] set [ChecklistText] = REPLACE([ChecklistText], 'leted by the Pricer ', 'leted by the Estimator ') where [ChecklistText] like '%leted by the Pricer %'
/*   ***   END BOEJ-1168 *** */

GO

/*   ***   START BOEJ-1132, BOEJ-1133  *** */
IF NOT EXISTS (SELECT 1 FROM dbo.ProductLine WHERE ProductLineName = 'Civil Space')
BEGIN

	BEGIN TRANSACTION

	BEGIN TRY
		UPDATE dbo.[ProductLine] set IsActive = 0
		UPDATE dbo.[LineOfBusiness] set IsActive = 0

		DECLARE @BAID int
		DECLARE @PLID int 
		DECLARE @LOBID int 

		IF NOT EXISTS (SELECT 1 FROM dbo.[BusinessArea] WHERE [BusinessAreaName] = 'Space')
		BEGIN
			UPDATE dbo.[BusinessArea] set IsActive = 0
			SET @BAID = (SELECT MAX([BusinessAreaID]) + 1 FROM dbo.[BusinessArea])
			-- set identity insert off first since if it's already on, it errors when setting to On
			SET IDENTITY_INSERT [dbo].[BusinessArea] OFF
			SET IDENTITY_INSERT [dbo].[BusinessArea] ON
			INSERT [dbo].[BusinessArea] ([BusinessAreaID], [BusinessAreaName], [BusinessAreaLongName], [BusinessAreaURL], [ForesightBusinessAreaID], [IsActive]) VALUES (@BAID, N'Space', N'Space', N'Space', -1, 1)
			SET IDENTITY_INSERT [dbo].[BusinessArea] OFF
		END

		SET @BAID = (SELECT [BusinessAreaID] FROM dbo.[BusinessArea] WHERE [BusinessAreaName] = 'Space')
		SET @PLID = (SELECT MAX([ProductLineID]) + 1 FROM dbo.[ProductLine])
	
		SET IDENTITY_INSERT [dbo].[ProductLine] OFF
		SET IDENTITY_INSERT [dbo].[ProductLine] ON
		INSERT [dbo].[ProductLine] ([ProductLineID], [ProductLineName], [ProductLineLongName], [ProductLineURL], [BusinessAreaID], [ForesightProductLineID], [IsActive]) VALUES (@PLID, N'ATC', N'Advanced Technology Center', N'ATC', @BAID, -1, 1)
		SET @PLID = @PLID + 1

		INSERT [dbo].[ProductLine] ([ProductLineID], [ProductLineName], [ProductLineLongName], [ProductLineURL], [BusinessAreaID], [ForesightProductLineID], [IsActive]) VALUES (@PLID, N'Civil Space', N'Civil Space', N'CivilSpace', @BAID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Advanced Programs', N'Advanced Programs', N'AP', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'GOES-R', N'GOES-R', N'GOES_R', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Orion', N'Orion', N'Orion', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Space Exploration Systems', N'Space Exploration Systems', N'SES', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Space Science & Energy Systems', N'Space Science & Energy Systems', N'SS_ES', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Space Transportation Systems', N'Space Transportation Systems', N'STS', @PLID, -1, 1)
		SET @PLID = @PLID + 1
	
		INSERT [dbo].[ProductLine] ([ProductLineID], [ProductLineName], [ProductLineLongName], [ProductLineURL], [BusinessAreaID], [ForesightProductLineID], [IsActive]) VALUES (@PLID, N'Comm Space', N'Commercial Space', N'Comm_Space', @BAID, -1, 1)
		SET @PLID = @PLID + 1
	
		INSERT [dbo].[ProductLine] ([ProductLineID], [ProductLineName], [ProductLineLongName], [ProductLineURL], [BusinessAreaID], [ForesightProductLineID], [IsActive]) VALUES (@PLID, N'Mission Solutions', N'Mission Solutions', N'M_S', @BAID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Mission Operations', N'Mission Operations', N'M_O', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'MSS&S', N'Mission Services, Systems & Solutions', N'MSSS', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Special Programs', N'Special Programs', N'S_P', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Enterprise Ground', N'Enterprise Ground', N'E_G', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Range & Depot', N'Range & Depot', N'RangeDepot', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Mission Sys Capabilities', N'Mission Sys Capabilities', N'MSC', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Civil and New Commercial/Intl', N'Civil and New Commercial/Intl', N'CNCI', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'AAD&S', N'Advance Application Development & Solutions', N'AADS', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'TBD', N'TO BE DETERMINED', N'TBD', @PLID, -1, 1)
		SET @PLID = @PLID + 1
	
		INSERT [dbo].[ProductLine] ([ProductLineID], [ProductLineName], [ProductLineLongName], [ProductLineURL], [BusinessAreaID], [ForesightProductLineID], [IsActive]) VALUES (@PLID, N'Military Space', N'Military Space', N'M_S', @BAID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'SBIRS', N'SBIRS', N'SBIRS', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'GPS', N'GPS', N'GPS', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'AEHF', N'AEHF', N'AEHF', @PLID, -1, 1)
		SET @PLID = @PLID + 1
	
		INSERT [dbo].[ProductLine] ([ProductLineID], [ProductLineName], [ProductLineLongName], [ProductLineURL], [BusinessAreaID], [ForesightProductLineID], [IsActive]) VALUES (@PLID, N'SP', N'Special Programs', N'SP', @BAID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'APME', N'APME', N'APME', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'MSP', N'MSP', N'MSP', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'RSP', N'RSP', N'RSP', @PLID, -1, 1)
		SET @PLID = @PLID + 1
	
		INSERT [dbo].[ProductLine] ([ProductLineID], [ProductLineName], [ProductLineLongName], [ProductLineURL], [BusinessAreaID], [ForesightProductLineID], [IsActive]) VALUES (@PLID, N'SMD', N'Strategic & Missile Defense Systems', N'SMD', @BAID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Advanced Program', N'Advanced Program', N'A_P', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'FBM', N'FBM', N'FBM', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'THAAD', N'THAAD', N'THAAD', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Targets', N'Targets', N'Targets', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Re-Entry', N'Re-Entry', N'Re_Entry', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Northern AL Ops', N'Northern AL Ops', N'N_A_O', @PLID, -1, 1)
		SET @PLID = @PLID + 1
	
		INSERT [dbo].[ProductLine] ([ProductLineID], [ProductLineName], [ProductLineLongName], [ProductLineURL], [BusinessAreaID], [ForesightProductLineID], [IsActive]) VALUES (@PLID, N'Commercial Launch', N'Commercial Launch', N'COMM_L', @BAID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'ArabSat', N'ArabSat', N'ArabSat', @PLID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'JCSAT', N'JCSAT', N'JCSAT', @PLID, -1, 1)
		SET @PLID = @PLID + 1
	
		INSERT [dbo].[ProductLine] ([ProductLineID], [ProductLineName], [ProductLineLongName], [ProductLineURL], [BusinessAreaID], [ForesightProductLineID], [IsActive]) VALUES (@PLID, N'AWE', N'AWE', N'AWE', @BAID, -1, 1)
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'TBD', N'TBD', N'TBD', @PLID, -1, 1)
	
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

	SET IDENTITY_INSERT [dbo].[ProductLine] OFF
    																																		
END

GO
/*   ***   END BOEJ-1132, BOEJ-1133  *** */

/*   ***   START BOEJ-1311  *** */
/* WARNING!!!!!!!  This will take some time, over 5 minutes to delete the 6K plus rows in each table */
DECLARE @LMRetainedSSC int
SET @LMRetainedSSC = (select [ProgramProposalStatusID] from dbo.[ProgramProposalStatusLU] where [ProgramProposalStatus] = 'LM Retained – SSC')

IF EXISTS (SELECT 1 FROM dbo.Proposal where ISNull(ProgramProposalStatusID, 0) <> @LMRetainedSSC)
BEGIN
	BEGIN TRANSACTION

	BEGIN TRY
		---- Create a temporary table that has the proposal id from Proposal table with proposals to be deleted
		DECLARE @DeleteProposal TABLE
		(
			[ProposalID] [int]
		)

		INSERT INTO @DeleteProposal
		SELECT [ProposalID]
		FROM [dbo].Proposal
		where ISNULL(ProgramProposalStatusID, 0) <> @LMRetainedSSC

	    DELETE p FROM dbo.ProposalUserRole p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
        DELETE p FROM dbo.ProposalContractTypeXREF p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.ProposalCostElementXREF p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.ProposalPARChecklistXREF p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.ProposalPPRChecklistXREF p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.ProposalChecklist p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.ProposalChecklistComplete p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.ProposalEmail p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.Proposal p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID

		-- Now delete some records that slipped through
		DELETE FROM @DeleteProposal
		INSERT INTO @DeleteProposal (ProposalID) VALUES (6217), 
			(6376), 
			(6395), 
			(6428), 
			(6480), 
			(6492), 
			(6494), 
			(6557), 
			(6576), 
			(6578), 
			(6582), 
			(6585), 
			(6596), 
			(6617), 
			(6619), 
			(6629), 
			(6776), 
			(6777), 
			(6807), 
			(6831), 
			(6885), 
			(6934), 
			(6947), 
			(6956), 
			(7157), 
			(7185), 
			(7237), 
			(7238), 
			(7266), 
			(7326), 
			(7377), 
			(7470), 
			(7497), 
			(7508), 
			(7543), 
			(7560), 
			(7605), 
			(7674), 
			(7675), 
			(7717), 
			(7952), 
			(8010), 
			(8064), 
			(8065), 
			(8103), 
			(8104), 
			(8105), 
			(8141), 
			(8170), 
			(8213), 
			(8256), 
			(8257), 
			(8258), 
			(8267), 
			(8269), 
			(8293), 
			(8316), 
			(8356), 
			(8422), 
			(8449), 
			(8455), 
			(8456), 
			(8506), 
			(8658), 
			(8668), 
			(8690), 
			(8721), 
			(8772), 
			(8779), 
			(8804), 
			(8806), 
			(8807), 
			(9997), 
			(10027), 
			(10028), 
			(10043)

		DELETE p FROM dbo.ProposalUserRole p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
        DELETE p FROM dbo.ProposalContractTypeXREF p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.ProposalCostElementXREF p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.ProposalPARChecklistXREF p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.ProposalPPRChecklistXREF p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.ProposalChecklist p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.ProposalChecklistComplete p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.ProposalEmail p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID
		DELETE p FROM dbo.Proposal p INNER JOIN @DeleteProposal d ON p.ProposalID = d.ProposalID


		-- set all of the proposals to LOB Mission Solutions
		DECLARE @MSID int 
		SET @MSID = (SELECT [ProductLineID] FROM dbo.[ProductLine] WHERE [ProductLineName] = N'Mission Solutions')
		-- set all of the proposals to Program Area TBD, then pick and choose some of them into the other PAs
		DECLARE @TBD int
		SET @TBD = (SELECT [LineOfBusinessID] from dbo.[LineOfBusiness] WHERE LineOfBusinessLongName = N'TO BE DETERMINED')
		UPDATE dbo.Proposal SET [ProductLineID] = @MSID, [LineOfBusinessID] = @TBD
		
		DECLARE @CivilNew int
		DECLARE @EnterpriseGround int
		DECLARE @MissionOperations int
		DECLARE @MissionSysCapabilities int
		DECLARE @MSSS int
		DECLARE @RangeDepot int
		DECLARE @SpecialPrograms int
		DECLARE @AADS int
		SET @AADS = (SELECT [LineOfBusinessID] from dbo.[LineOfBusiness] WHERE [LineOfBusinessName] = N'AAD&S')
		SET @CivilNew = (SELECT [LineOfBusinessID] from dbo.[LineOfBusiness] WHERE [LineOfBusinessName] = N'Civil and New Commercial/Intl')
		SET @EnterpriseGround = (SELECT [LineOfBusinessID] from dbo.[LineOfBusiness] WHERE [LineOfBusinessName] = N'Enterprise Ground')
		SET @MissionOperations = (SELECT [LineOfBusinessID] from dbo.[LineOfBusiness] WHERE [LineOfBusinessName] = N'Mission Operations')
		SET @MissionSysCapabilities = (SELECT [LineOfBusinessID] from dbo.[LineOfBusiness] WHERE [LineOfBusinessName] = N'Mission Sys Capabilities')
		SET @MSSS = (SELECT [LineOfBusinessID] from dbo.[LineOfBusiness] WHERE [LineOfBusinessName] = N'MSS&S')
		SET @RangeDepot = (SELECT [LineOfBusinessID] from dbo.[LineOfBusiness] WHERE [LineOfBusinessName] = N'Range & Depot')
		SET @SpecialPrograms = (SELECT [LineOfBusinessID] from dbo.[LineOfBusiness] WHERE [LineOfBusinessName] = N'Special Programs')
		
		Update Proposal SET LineOfBusinessID = @AADS WHERE ProposalID IN (5765, 
			6355, 
			6356, 
			6457, 
			6496, 
			6522, 
			6523, 
			6539, 
			6541, 
			6543, 
			6573, 
			6575, 
			6589, 
			6641, 
			6665, 
			6748, 
			6761, 
			6762, 
			6772, 
			6786, 
			6787, 
			6834, 
			6859, 
			6876, 
			6922, 
			6993, 
			6994, 
			7008, 
			7027, 
			7036, 
			7075, 
			7080, 
			7205, 
			7333, 
			7379, 
			7534, 
			7536, 
			7563, 
			7590, 
			7811, 
			7927, 
			7957, 
			7972, 
			7977, 
			8003, 
			8004, 
			8014, 
			8020, 
			8021, 
			8028, 
			8037, 
			8043, 
			8044, 
			8049, 
			8056, 
			8073, 
			8106, 
			8158, 
			8166, 
			8167, 
			8264, 
			8312, 
			8313, 
			8327, 
			8329, 
			8331, 
			8336, 
			8340, 
			8409, 
			8459, 
			8511, 
			8555, 
			8559, 
			8569, 
			8603, 
			8614, 
			8630, 
			8666, 
			8679, 
			8740, 
			8748, 
			8749, 
			8755, 
			8799, 
			8811, 
			8892, 
			8912, 
			8930, 
			8931, 
			8937, 
			8938, 
			8945, 
			8948, 
			9966, 
			9970, 
			9993, 
			10033, 
			10034, 
			10035, 
			10050, 
			10054, 
			10058, 
			11120, 
			12118, 
			12133, 
			12138, 
			12140, 
			12181, 
			12270, 
			12302, 
			12323, 
			12374, 
			12394, 
			13432, 
			13433, 
			13446, 
			13470, 
			14602, 
			14730, 
			14815, 
			14816, 
			14821, 
			14829, 
			14831, 
			14836, 
			14837, 
			14839, 
			14845, 
			14848, 
			14872, 
			14888, 
			14891, 
			14893, 
			14895)

		Update Proposal SET LineOfBusinessID = @CivilNew WHERE ProposalID IN (6879, 
			6925, 
			8271, 
			8442, 
			8560, 
			8567, 
			8778, 
			13426, 
			13558)

		Update Proposal SET LineOfBusinessID = @EnterpriseGround WHERE ProposalID IN (5918, 
			6062, 
			6180, 
			6289, 
			6290, 
			6389, 
			6390, 
			6396, 
			6430, 
			6466, 
			6467, 
			6544, 
			6588, 
			6593, 
			6746, 
			6793, 
			6803, 
			6828, 
			6843, 
			6867, 
			6913, 
			6916, 
			6958, 
			7090, 
			7172, 
			7173, 
			7190, 
			7222, 
			7300, 
			7302, 
			7303, 
			7308, 
			7415, 
			7416, 
			7433, 
			7434, 
			7466, 
			7474, 
			7475, 
			7561, 
			7646, 
			7647, 
			7648, 
			7658, 
			7659, 
			7737, 
			7836, 
			7842, 
			7864, 
			7868, 
			7906, 
			7907, 
			7908, 
			7917, 
			7979, 
			8032, 
			8034, 
			8045, 
			8078, 
			8086, 
			8092, 
			8160, 
			8161, 
			8162, 
			8197, 
			8212, 
			8231, 
			8234, 
			8248, 
			8284, 
			8285, 
			8332, 
			8357, 
			8362, 
			8374, 
			8402, 
			8414, 
			8425, 
			8436, 
			8474, 
			8487, 
			8536, 
			8599, 
			8606, 
			8607, 
			8615, 
			8655, 
			8669, 
			8670, 
			8705, 
			8752, 
			8783, 
			8784, 
			8785, 
			8818, 
			8842, 
			8843, 
			8897, 
			8900, 
			8929, 
			9988, 
			10014, 
			10036, 
			10041, 
			10057, 
			11094, 
			11108, 
			12146, 
			12167, 
			12168, 
			12180, 
			12257, 
			12265, 
			12295, 
			12296, 
			12378, 
			12398, 
			13555, 
			14618, 
			14639, 
			14644, 
			14711, 
			14731, 
			14826, 
			14827, 
			14828, 
			14832, 
			14843, 
			14864, 
14885 )

		Update Proposal SET LineOfBusinessID = @MissionOperations WHERE ProposalID IN (6234, 
			6371, 
			6415, 
			6416, 
			6500, 
			6507, 
			6830, 
			6917, 
			7054, 
			7113, 
			7138, 
			7153, 
			7154, 
			7336, 
			7544, 
			7606, 
			7607, 
			7608, 
			7622, 
			7645, 
			7654, 
			7693, 
			7705, 
			7749, 
			7782, 
			7849, 
			7851, 
			7856, 
			7893, 
			7937, 
			7989, 
			7995, 
			8094, 
			8150, 
			8180, 
			8472, 
			8473, 
			8646, 
			8661, 
			8692, 
			8699, 
			8760, 
			8835, 
			8870, 
			8890, 
			8903, 
			9972, 
			9998, 
			12126, 
			12155, 
			12245, 
			12248, 
			12280, 
			12285, 
			13541, 
			14693, 
			14729, 
			14776, 
			14834, 
			14844, 
			14853, 
			14854, 
			14856, 
			14857, 
			14868, 
			14879, 
			14880, 
			14881, 
			14882, 
			14883, 
			14892, 
			14894, 
			14897 )

		Update Proposal SET LineOfBusinessID = @MissionSysCapabilities WHERE ProposalID IN (6273, 
			6443, 
			6465, 
			6471, 
			6508, 
			6524, 
			6536, 
			6574, 
			6598, 
			6601, 
			6646, 
			6688, 
			6689, 
			6796, 
			6797, 
			6826, 
			6827, 
			6892, 
			6893, 
			6914, 
			6915, 
			7013, 
			7014, 
			7015, 
			7016, 
			7040, 
			7051, 
			7052, 
			7053, 
			7055, 
			7063, 
			7127, 
			7129, 
			7142, 
			7158, 
			7159, 
			7160, 
			7161, 
			7179, 
			7180, 
			7214, 
			7215, 
			7232, 
			7288, 
			7317, 
			7318, 
			7346, 
			7347, 
			7371, 
			7372, 
			7399, 
			7408, 
			7409, 
			7464, 
			7487, 
			7513, 
			7514, 
			7517, 
			7518, 
			7556, 
			7557, 
			7558, 
			7609, 
			7610, 
			7612, 
			7613, 
			7672, 
			7682, 
			7702, 
			7703, 
			7739, 
			7740, 
			7751, 
			7752, 
			7780, 
			7781, 
			7783, 
			7784, 
			7788, 
			7789, 
			7799, 
			7800, 
			7812, 
			7813, 
			7816, 
			7818, 
			7830, 
			7839, 
			7840, 
			7857, 
			7858, 
			7891, 
			8047, 
			8048, 
			8058, 
			8068, 
			8091, 
			8168, 
			8226, 
			8253, 
			8259, 
			8311, 
			8361, 
			8363, 
			8387, 
			8475, 
			8596, 
			8647, 
			8703, 
			8704, 
			8711, 
			8712, 
			8720, 
			8732, 
			8733, 
			8747, 
			8764, 
			8817, 
			8830, 
			8865, 
			8868, 
			8872, 
			8873, 
			8875, 
			8891, 
			8901, 
			8932, 
			8933, 
			8934, 
			8942, 
			8949, 
			9951, 
			9964, 
			9969, 
			9984, 
			9991, 
			10016, 
			10040, 
			10061, 
			10085, 
			11095, 
			12125, 
			12142, 
			12143, 
			12198, 
			12205, 
			12214, 
			12230, 
			12232, 
			12233, 
			12236, 
			12256, 
			12307, 
			12329, 
			12361, 
			12370, 
			13416, 
			13425, 
			13430, 
			13435, 
			13460, 
			13502, 
			13503, 
			13506, 
			13511, 
			13529, 
			13554, 
			13560, 
			13561, 
			13566, 
			13567, 
			13568, 
			13575, 
			14608, 
			14662, 
			14671, 
			14672, 
			14673, 
			14689, 
			14690, 
			14734, 
			14778, 
			14780, 
			14814, 
			14818, 
			14819, 
			14820, 
			14822, 
			14823, 
			14824, 
			14825, 
			14833, 
			14835, 
			14838, 
			14846, 
			14850, 
			14865, 
			14866, 
			14867, 
			14869, 
			14871, 
			14878, 
			14884, 
			14886, 
			14890, 
			14896 )

		Update Proposal SET LineOfBusinessID = @MSSS WHERE ProposalID IN (6314, 
			6315, 
			6426, 
			6427, 
			6479, 
			6495, 
			6501, 
			6517, 
			6518, 
			6605, 
			6771, 
			7059, 
			7196, 
			7230, 
			7239, 
			7240, 
			7259, 
			7260, 
			7263, 
			7264, 
			7281, 
			7292, 
			7340, 
			7363, 
			7405, 
			7406, 
			7457, 
			7458, 
			7460, 
			7461, 
			7619, 
			7620, 
			7634, 
			7635, 
			7883, 
			7920, 
			8075, 
			8140, 
			8172, 
			8237, 
			8301, 
			8465, 
			8466, 
			8505, 
			8507, 
			8508, 
			8520, 
			8539, 
			8576, 
			8598, 
			8725, 
			8847, 
			8889, 
			9990, 
			10077, 
			10078, 
			12159, 
			12173, 
			12223, 
			12224, 
			12301, 
			12330, 
			13423, 
			13590, 
			14720, 
			14782, 
			14847, 
			14849, 
			14858, 
			14859, 
			14860, 
			14861, 
			14862, 
			14875, 
			14889, 
			14898, 
14899 )

		Update Proposal SET LineOfBusinessID = @RangeDepot WHERE ProposalID IN (4435, 
			4436, 
			6178, 
			6225, 
			6379, 
			6468, 
			6469, 
			6470, 
			6493, 
			6511, 
			6512, 
			6525, 
			6558, 
			6572, 
			6591, 
			6608, 
			6679, 
			6693, 
			6713, 
			6799, 
			6857, 
			6861, 
			6864, 
			6883, 
			6894, 
			6908, 
			6928, 
			6932, 
			6939, 
			6941, 
			6942, 
			7034, 
			7048, 
			7050, 
			7093, 
			7114, 
			7124, 
			7132, 
			7137, 
			7144, 
			7152, 
			7216, 
			7224, 
			7225, 
			7226, 
			7272, 
			7310, 
			7316, 
			7366, 
			7380, 
			7385, 
			7386, 
			7387, 
			7410, 
			7437, 
			7440, 
			7445, 
			7446, 
			7472, 
			7473, 
			7480, 
			7485, 
			7486, 
			7527, 
			7530, 
			7546, 
			7580, 
			7626, 
			7650, 
			7711, 
			7722, 
			7723, 
			7724, 
			7776, 
			7777, 
			7785, 
			7824, 
			7844, 
			7855, 
			7861, 
			7862, 
			7870, 
			7871, 
			7875, 
			7881, 
			7885, 
			7914, 
			7932, 
			7949, 
			7996, 
			7998, 
			8013, 
			8029, 
			8031, 
			8072, 
			8096, 
			8114, 
			8117, 
			8121, 
			8152, 
			8210, 
			8211, 
			8255, 
			8282, 
			8334, 
			8358, 
			8359, 
			8369, 
			8373, 
			8377, 
			8379, 
			8420, 
			8463, 
			8570, 
			8595, 
			8640, 
			8645, 
			8648, 
			8731, 
			8759, 
			8766, 
			8768, 
			8769, 
			8812, 
			8822, 
			8838, 
			8859, 
			8862, 
			8902, 
			10015, 
			10031, 
			10032, 
			12144, 
			12175, 
			12191, 
			12206, 
			12255, 
			12335, 
			12351, 
			12392, 
			12393, 
			12399, 
			12404, 
			13464, 
			13465, 
			13466, 
			13467, 
			13508, 
			13532, 
			13556, 
			13557, 
			13563, 
			13565, 
			13570, 
			13593, 
			14623, 
			14628, 
			14753, 
			14763, 
			14767, 
			14777, 
			14798, 
			14799, 
			14840, 
			14841, 
			14842, 
			14851, 
			14852, 
			14855, 
			14873, 
			14874, 
			14876, 
			14877, 
			14887 )

		Update Proposal SET LineOfBusinessID = @SpecialPrograms WHERE ProposalID IN (6446, 
			6454, 
			6455, 
			6456, 
			6547, 
			6606, 
			6973, 
			6974, 
			6975, 
			6977, 
			6982, 
			6983, 
			6984, 
			7002, 
			7251, 
			7643, 
			7696, 
			7709, 
			8027, 
			8069, 
			8070, 
			8071, 
			8360, 
			8590, 
			8591, 
			8608, 
			8741, 
			8743, 
			8854, 
			8879, 
			8880, 
			8883, 
			8884, 
			13456, 
			14817, 
			14870 )

		COMMIT TRANSACTION

	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
	

		DECLARE @ErrorMessage2 varchar (500)
		SELECT @ErrorMessage2 = ERROR_MESSAGE()
		RAISERROR (
				@ErrorMessage2, -- Message text.
				11, -- Severity,
				1 -- State,
				)
		RETURN
	
	END CATCH

	DELETE FROM [dbo].[LineOfBusiness] WHERE IsActive = 0
	DELETE px FROM dbo.ProductLineRoleXREF px Inner Join dbo.ProductLine p on px.ProductLineID = p.ProductLineID WHERE p.IsActive = 0
	DELETE FROM dbo.ProductLine WHERE IsActive = 0
END
/*   ***   END BOEJ-1311  *** */
GO

DROP VIEW [genBOE].[genTracData]
GO
CREATE VIEW [genBOE].[genTracData] AS 
SELECT DISTINCT
	P.ProposalID AS [genTracProposalID],
	P.ProposalTrackingID AS [TrackingNumber],
	P.ProposalTitle AS [ProposalTitle],
	/*
	Workspace Name is the concatenation of 
	Tracking Number and Proposal Title 
	separated by space hyphen space (‘ – ‘):  <Tracking Number> - <Proposal Title>
	AS WorkspaceName
	*/
	P.ProposalTrackingID + '-' +  P.ProposalTitle AS [WorkspaceName],
	
	--Product Line has been renamed Line of Business
	PL.ProductLineLongName AS [LineOfBusinessName],
	
	--Line of Business has been renamed Program Area
	LOB.LineOfBusinessLongName AS [ProgramAreaName],
	
	PT.ProposalType AS [ProposalType],		
	PS.ProposalStatus AS [ProposalStatus],	
	
	P.EstimatedProposalValue AS [EstimatedValue],
	PC.ISGSTotalPrice AS [SubmittedValue],
		
	CAST (P.DateCreated AS DATE) AS [ProposalStartDate],
	CAST (PC.UpdateDate AS DATE) AS [ProposalEndDate],
	CAST (P.DateCreated AS DATE) AS [CreatedDate],
	CAST(PC.ProposalSubmittalDate AS DATE) AS [SubmittalDate],
	
	P.RevisionID AS [RevisionID],
	CAST (P.DateCreated AS DATE) AS [RevisionDate],
	
	'' AS [Segment],	
	P.ProgramName AS [ProgramName],
		
	PRICER.DisplayName AS [LeadPricer],		
	Pricer.RoleType AS [PricerType],
	APR1.AdditionalPricingResource1 AS [AdditionalPricingResource1],
	APR2.AdditionalPricingResource2 AS [AdditionalPricingResource2],
	
	P.Customer AS [Customer],
	CT.CustomerType AS [CustomerType],
	
	CL.ContractLeader AS [ContractLeader],
		
	CVL.DisplayName AS [CostVolumeLead],
	
	Mngr.Manager AS Manager,
	
	PrcT.PricingTool AS [PricingTool],
	
	BT.BOETool AS [BOETool],
	
	dbo.udfCreateCommaSeparatedList (P.ProposalID, 1) AS [ContractType],
	dbo.udfCreateCommaSeparatedList (P.ProposalID, 2) AS [ElementsOfCost],
	
	P.IsIWTA AS [IWTA],
	P.IsLMIS AS [LMIS],
	
	CASE I.ISGSRole
		WHEN 'IWTA' THEN ''
		ELSE I.ISGSRole
	END AS [PrimeOrSub],
		
	P.RFPNumber AS [RFPNumber]

	
FROM [dbo].[Proposal] P
	INNER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName,
				U.NTDomain,
				U.NTID,
				RT.RoleType

			  FROM
			[dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[RoleTypeLU] RT ON PUR.RoleTypeID = RT.RoleTypeID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.RoleID = 3 /*Pricer*/  
		) PRICER ON P.ProposalID = PRICER.ProposalID
		
	INNER JOIN [dbo].[ProductLine] PL ON P.ProductLineID = PL.ProductLineID
	INNER JOIN [dbo].[LineOfBusiness] LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
	INNER JOIN [dbo].[ProposalTypeLU] PT ON P.ProposalTypeID = PT.ProposalTypeID

	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName

			  FROM
			[dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.RoleID = 2 /*Cost Volume Lead*/
		) CVL ON P.ProposalID = CVL.ProposalID
		
	LEFT OUTER JOIN [dbo].[ProposalChecklist] PC ON P.ProposalID = PC.ProposalID
	INNER JOIN [dbo].[BOEToolLU] BT ON BT.BOEToolID = P.BOEToolID
	INNER JOIN [dbo].[PricingToolLU] PrcT ON PrcT.PricingToolID = P.PricingToolID
	INNER JOIN [dbo].[ProposalStatusLU] PS ON PS.ProposalStatusID = P.ProposalStatusID
	INNER JOIN [dbo].[CustomerTypeLU] CT ON CT.CustomerTypeID = P.CustomerTypeID
	INNER JOIN [dbo].[ISGSRoleLU] I ON I.ISGSRoleID = P.ISGSRoleID
	
	LEFT OUTER JOIN
	(
		SELECT 
			ProposalID,
			MAX(SubmitDate) AS MaxSubmitDate
		FROM dbo.ProposalChecklistComplete
		GROUP BY ProposalID
	) PCE ON P.ProposalID = PCE.ProposalID
	
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS ContractLeader,
			U.UserID AS ContractLeaderUserID,
			U.NTID AS ContractLeaderNTID
		FROM dbo.ProposalUserRole PUR
			INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 8 /*Contracts POC*/
	) CL ON P.ProposalID = CL.ProposalID
	
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS AdditionalPricingResource1,
			U.UserID AS AdditionalPricingResource1UserID,
			U.NTID AS AdditionalPricingResource1NTID
		FROM dbo.ProposalUserRole PUR
			INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 4	/*Additional Pricing Resource 1*/
	) APR1 ON P.ProposalID = APR1.ProposalID	
	
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS AdditionalPricingResource2,
			U.UserID AS AdditionalPricingResource2UserID,
			U.NTID AS AdditionalPricingResource2NTID
			
		FROM dbo.ProposalUserRole PUR
			INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 5	/*Additional Pricing Resource 2*/
	) APR2 ON P.ProposalID = APR2.ProposalID

	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS Manager,
			U.UserID AS ManagerUserID,
			U.NTID AS ManagerNTID
			
		FROM dbo.ProposalUserRole PUR
			INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 1 /*Capture Manager*/
	) Mngr ON P.ProposalID = Mngr.ProposalID
GO
