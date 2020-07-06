EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.7';
GO

/*
		## START ##
		8/27/18 RJ - BOEJ-3661: GenBOE Workspace Creator Role
*/

IF NOT EXISTS (SELECT 1 FROM [dbo].[RoleLU] WHERE [RoleID] = 21)
BEGIN

SET IDENTITY_INSERT [dbo].[RoleLU] ON;

INSERT INTO [dbo].[RoleLU] (RoleID, Role)
VALUES (21, 'GenBOE Workspace Creator');

SET IDENTITY_INSERT [dbo].[RoleLU] OFF;
END
GO

/*
		8/27/18 RJ - BOEJ-3661: GenBOE Workspace Creator Role
		## END ##
*/

/*
		## START ##
		8/29/18 twilson3 - BOEJ-3756: Post Proposal Info
*/
IF EXISTS (
                           SELECT * FROM sys.all_columns C
                                  INNER JOIN sys.tables T on C.object_id = T.object_id
                                  INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
                           WHERE
                                  T.name = 'Proposal' AND
                                  C.name = 'NegotiationStartDate' AND
                                  S.name = 'dbo'
                           )
BEGIN
       ALTER TABLE [dbo].[Proposal] DROP COLUMN [NegotiationStartDate]
	   ALTER TABLE [dbo].[Proposal] DROP COLUMN [NegotiationEndDate]
	   ALTER TABLE [dbo].[Proposal] DROP COLUMN [AwardDate]
	   ALTER TABLE [dbo].[Proposal] DROP COLUMN [UndefinitizedContractActions]
	   ALTER TABLE [dbo].[Proposal] DROP COLUMN [AuditEntranceConferenceDate]
	   ALTER TABLE [dbo].[Proposal] DROP COLUMN [AuditReportDate]
	   ALTER TABLE [dbo].[Proposal] DROP COLUMN [ProposalDeemedInadequate]
	   UPDATE [dbo].[Proposal] SET [Comments] = NULL
END
GO;

IF NOT EXISTS (
                           SELECT * FROM sys.all_columns C
                                  INNER JOIN sys.tables T on C.object_id = T.object_id
                                  INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
                           WHERE
                                  T.name = 'Proposal' AND
                                  C.name = 'AgreementDate' AND
                                  S.name = 'dbo'
                           )
BEGIN
       ALTER TABLE [dbo].[Proposal] ADD [AgreementDate] date NULL
	   ALTER TABLE [dbo].[Proposal] ADD [CertificationDate] date NULL
	   ALTER TABLE [dbo].[Proposal] ADD [CutOffDateUtilization] int NULL
	   ALTER TABLE [dbo].[Proposal] ADD [CertificationTimelineCompleted] datetime2 NULL
	   ALTER TABLE [dbo].[Proposal] ADD [CertificationLastEmailed] datetime2 NULL

	   CREATE TABLE [dbo].[CutOffDateUtilizationLU] (
			[ID] [int] NOT NULL,
			[Name] [varchar](100) NOT NULL,
			CONSTRAINT [PK_CutOffDateUtilizationLU] PRIMARY KEY CLUSTERED 
		(
			[ID] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]

		INSERT INTO [CutOffDateUtilizationLU] ([ID], [Name]) VALUES (0, 'Yes');
		INSERT INTO [CutOffDateUtilizationLU] ([ID], [Name]) VALUES (1, 'No, LM did not request');
		INSERT INTO [CutOffDateUtilizationLU] ([ID], [Name]) VALUES (2, 'No, LM request denied');

		ALTER TABLE [dbo].[Proposal]  WITH CHECK ADD  CONSTRAINT [FK_CutOffDateUtilization] FOREIGN KEY([CutOffDateUtilization])
			REFERENCES [dbo].[CutOffDateUtilizationLU] ([ID])
END
GO
/*
		8/29/18 twilson3 - BOEJ-3756: Post Proposal Info
		## END ##
*/

/*
		## START ##
		8/31/18 twilson3 - BOEJ-3761: New Submitted Proposal Status
*/
IF NOT EXISTS (
                           SELECT * FROM [dbo].[ProposalStatusLU] 
                           WHERE
                                  [ProposalStatus] = 'Submitted'
                           )
BEGIN
	SET IDENTITY_INSERT [dbo].[ProposalStatusLU] ON;
	INSERT INTO [dbo].[ProposalStatusLU] ([ProposalStatusID], [ProposalStatus]) VALUES (6, 'Submitted');
	SET IDENTITY_INSERT [dbo].[ProposalStatusLU] OFF;
END
GO
/*
		8/31/18 twilson3 - BOEJ-3761: New Submitted Proposal Status
		## END ##
*/