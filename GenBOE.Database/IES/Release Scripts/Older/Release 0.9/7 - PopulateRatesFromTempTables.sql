DECLARE 
	@year int, 
	@sql_stmt varchar(4000),
	@BECursor CURSOR,
	@BEID int,
	@BurdenElementID varchar(4),
	@BurdenElement varchar(50),
	@UpdateDate datetime2(7),
	@VersionNumber int = 173,
	@Revision varchar(50),	
	@Id int;
BEGIN
	WHILE (@VersionNumber <= 174)	 -- LOOP for versions 173 and 174
	BEGIN
		SET @Revision = CAST(@VersionNumber as varchar(50));
		SET @VersionNumber = @VersionNumber + 1;
		SELECT @Id = ID FROM [dbo].[Revision] WHERE Revision = @Revision;
		PRINT 'RevisionID = ' + CAST(@Id as VARCHAR(10));
		DELETE FROM [dbo].[ProPricerBurdenRateMap] 
		WHERE EXISTS
			(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
		DELETE FROM [dbo].[ProPricerRateCodeXref] 
		WHERE EXISTS 
			(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
		DELETE FROM [dbo].[RateCodeYear] 
		WHERE EXISTS 
			(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
		DELETE FROM [dbo].[RateCode] WHERE RevisionID = @Id
		DELETE FROM [dbo].[BurdenPoolLU] where RevisionID = @Id	

		--------------------------------------------------------------------------------------------------------------------------------------
		-- Populate RateCode table
		-- 1) Add Rate Codes from Rate worksheet
		-- 2) Add "ProPricer Only" Rate Codes (Resources) from Direct Rates worksheet that don't exist in RateCode table
		--    Ignore the following codes: XADD, XMLM, XCZD, XCZP (in favor of equivalent codes from rates worksheet)
		--    	XXDD => XADD
		--    	XXLM => XMLM
		--    	XXZD => XCZD
		--    	XXZP => XCZP
		--------------------------------------------------------------------------------------------------------------------------------------
		INSERT into [dbo].[RateCode] (UpdateDate, RevisionID, CategoryID, Description, SectionID, RateCode, CobraRateSet, CobraCode1ID)
		SELECT GETDATE(), rev.ID as RevisionID, tr.CategoryID,
			   tr.[RateDescription] as Description,
			   s.id as SectionID, tr.[RateCode], 
				CASE CobraRateSet WHEN '' then null ELSE CobraRateSet END as CobraRateSet,
				CASE Code1 WHEN '' then null ELSE cc.ID END as CobraCode1ID
		  FROM [dbo].[TempRates] tr
		  JOIN [dbo].[Revision] rev on rev.[Revision] = @Revision
		  LEFT OUTER JOIN [dbo].[Section] s on tr.[SectionTitle] = s.[Title] AND s.[Title] <> '' AND s.RevisionID = rev.ID
		  LEFT OUTER JOIN [dbo].[CobraCode1LU] cc on tr.[Code1] = cc.[Description]
		  WHERE tr.Revision = @Revision;

		INSERT INTO [dbo].[RateCode]
			   ([UpdateDate],[RevisionID],[CategoryID],[Description],[RateCode]
			   ,[ResourceTypeID],[RateTypeID])
		SELECT GETDATE(), rev.ID as RevisionID, clu.ID as CategoryID, d.Description, 
			   d.[Resource] as RateCode, reslu.ID as ResourceTypeID, rtlu.ID as RateTypeID			
		FROM (SELECT [ResourceType], [Resource], [Description], [RateType],
					  Max([Step]) as Step, Max([Factor]) as Factor
			  FROM [dbo].[TempPPDirectRates]
			  GROUP BY [ResourceType], [Resource], [Description], [RateType]) d
			JOIN [dbo].[Revision] rev on rev.[Revision] = @Revision
			JOIN [dbo].[CategoryLU] clu on clu.Description = 'Direct Labor'
			JOIN [dbo].[ResourceTypeLU] reslu on d.ResourceType = reslu.Description
			JOIN [dbo].[RateTypeLU] rtlu on d.RateType = rtlu.Description
			left outer join [dbo].[RateCode] r1 on d.[Resource] = r1.[RateCode] and rev.ID = r1.RevisionID
			WHERE r1.[RateCode] IS NULL AND 
					((LEN(d.[Resource]) = 7 AND (RIGHT(d.[Resource],1) < '1' OR RIGHT(d.[Resource],1) > '6')) OR LEN(d.[Resource]) <> 7);	-- Rate codes without extensions

		INSERT INTO [dbo].[RateCode]
			   ([UpdateDate],[RevisionID],[CategoryID],[Description],[RateCode]
			   ,[ResourceTypeID],[RateTypeID])
		SELECT GETDATE(), RevisionID, CategoryID, MAX(Description) as Description,
				RateCode, ResourceTypeID, RateTypeID
		FROM (
			SELECT rev.ID as RevisionID, clu.ID as CategoryID, d.Description, 
					LEFT(d.[Resource],6) as RateCode,
					reslu.ID as ResourceTypeID,
					rtlu.ID as RateTypeID			
			FROM (SELECT [ResourceType], [Resource], [Description], [RateType],
						 Max([Step]) as Step, Max([Factor]) as Factor
				  FROM [dbo].[TempPPDirectRates]
				  GROUP BY [ResourceType], [Resource], [Description], [RateType]) d
				JOIN [dbo].[Revision] rev on rev.[Revision] = @Revision
				JOIN [dbo].[CategoryLU] clu on clu.Description = 'Direct Labor'
				JOIN [dbo].[ResourceTypeLU] reslu on d.ResourceType = reslu.Description
				JOIN [dbo].[RateTypeLU] rtlu on d.RateType = rtlu.Description
				left outer join [dbo].[RateCode] r1 on LEFT(d.[Resource],6) = r1.[RateCode] and rev.ID = r1.RevisionID
				WHERE r1.[RateCode] IS NULL AND 
					  (LEN(d.[Resource]) = 7 AND RIGHT(d.[Resource],1) >= '1' AND RIGHT(d.[Resource],1) <= '6')) src	-- Rate codes with extensions
		GROUP BY RevisionID, CategoryID, RateCode, ResourceTypeID, RateTypeID;

		--------------------------------------------------------------------------------------------------------------------------------------
		-- Populate RateCodeYear table
		--------------------------------------------------------------------------------------------------------------------------------------
		SET @year = 2002;
		WHILE @year <= 2040
		BEGIN
			SET @sql_stmt = 'INSERT INTO [dbo].[RateCodeYear] ([UpdateDate],[RateCodeID],[Year],[Rate])
				SELECT GETDATE() as UpdateDate, rc.ID as RateCodeID, ' + CAST(@year as varchar(4)) + ' as Year, tr.[RY' + CAST(@year as varchar(4)) + '] as Rate
				FROM [dbo].[TempRates] tr
				JOIN [dbo].[Revision] rev on rev.[Revision] = ''' + @Revision + '''
				JOIN [dbo].[RateCode] rc on tr.[RateCode] = rc.[RateCode] AND tr.[CategoryID] = rc.[CategoryID] AND rc.RevisionID = rev.ID AND tr.[RY' + CAST(@year as varchar(4)) + '] is not null
				WHERE tr.[Revision] = ''' + @Revision + '''';
			EXEC (@sql_stmt);
			PRINT 'Rate Code Year = ' + CAST(@year as varchar(10));
			PRINT @sql_stmt;

			SET @year = @year + 1;
		END;

		INSERT INTO [dbo].[RateCodeYear] ([UpdateDate],[RateCodeID],[Year],[Rate])
		SELECT GETDATE(), dstnct.*
		FROM (
			SELECT DISTINCT rc.ID as RateCodeID, RIGHT(EndDate,4) as Year, ISNULL(tr.BaseRate, 0) as Rate
			FROM [dbo].[TempPPDirectRates] tr
			JOIN [dbo].[Revision] rev on rev.[Revision] = @Revision
			JOIN [dbo].[RateCode] rc on rev.ID = rc.RevisionID and rc.RateCode = tr.Resource
			JOIN [dbo].[CategoryLU] clu on rc.CategoryID = clu.ID and clu.Description = 'Direct Labor') as dstnct
		LEFT OUTER JOIN [dbo].[RateCodeYear] rcy on dstnct.RateCodeID = rcy.RateCodeID AND dstnct.Year = rcy.Year
		WHERE rcy.RateCodeID is NULL;

		INSERT INTO [dbo].[RateCodeYear] ([UpdateDate],[RateCodeID],[Year],[Rate])
		SELECT GETDATE(), dstnct.*
		FROM ( 
			SELECT DISTINCT rc.ID as RateCodeID, RIGHT(EndDate,4) as Year, ISNULL(tr.BaseRate, 0) as Rate
			FROM [dbo].[TempPPDirectRates] tr
			JOIN [dbo].[Revision] rev on rev.[Revision] = @Revision
			JOIN [dbo].[RateCode] rc on rev.ID = rc.RevisionID AND 
				(rc.RateCode = tr.Resource OR (LEN(tr.Resource) = 7 AND LEFT(tr.Resource,6) = rc.RateCode and RIGHT(tr.[Resource],1) >= '1' AND RIGHT(tr.[Resource],1) <= '6'))
			JOIN [dbo].[CategoryLU] clu on rc.CategoryID = clu.ID and clu.Description = 'Direct Labor') as dstnct
		LEFT OUTER JOIN [dbo].[RateCodeYear] rcy on dstnct.RateCodeID = rcy.RateCodeID AND dstnct.Year = rcy.Year
		WHERE rcy.RateCodeID is NULL;

		--------------------------------------------------------------------------------------------------------------------------------------
		-- Populate [BurdenPoolLU] table
		--------------------------------------------------------------------------------------------------------------------------------------
		INSERT INTO [dbo].[BurdenPoolLU] ([UpdateDate], [BurdenPool], [Description], [IsGaT2ApplicableForMissionSolutions], [RevisionId])
		  SELECT GETDATE(), dstnct.*, rev.ID as RevisionID
		  FROM (
			  SELECT DISTINCT
				  [BurdenPool], [Description], 0 as [IsGaT2ApplicableForMissionSolutions]
			  FROM [dbo].[TempPPBurdenRates]) as dstnct
		  			JOIN [dbo].[Revision] rev on rev.[Revision] = @Revision;

		--------------------------------------------------------------------------------------------------------------------------------------
		-- Populate [ProPricerBurdenRateMap]
		-- 1) Add rows from the BurdenRate spreadsheet (InputSource=2).  (For now, ignore the rows from BurdenRate (InputSource=0) and 
		--    BurdenRateCommercial (InputSource=1)).
		-- 2) Update (set the [IsGaT2ApplicableForMissionSolutions]=1) for Burden Pools where the [RC_G&A T2] column is only populated
		--    in the BurdenRateServices spreadsheet (InputSource=2), but not InputSource=0 or 1.
		--------------------------------------------------------------------------------------------------------------------------------------
		SET @BECursor = CURSOR FOR
			SELECT [ID],[BurdenElement]
			FROM [dbo].[BurdenElementLU]

		OPEN @BECursor
		FETCH NEXT FROM @BECursor
		INTO @BEID, @BurdenElement;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @BurdenElementID = @BEID;
			SET @sql_stmt = 'INSERT INTO [dbo].[ProPricerBurdenRateMap] ([UpdateDate],[BurdenPoolID],[BurdenElementID],[RateCodeID])
	SELECT GETDATE() as UpdateDate, bplu.[id] as BurdenPoolID, ' + @BurdenElementID + ' as BurdenElementID, rc.[id] as RateCodeID
	  FROM (SELECT DISTINCT [BurdenPool], [RC_' + @BurdenElement + '] as RateCode 
			FROM [dbo].[TempPPBurdenRates]  
			WHERE [R_' + @BurdenElement + '] !='''' AND InputSource = 2) tr
	  JOIN [dbo].[Revision] rev on rev.[Revision] = ''' + @Revision + '''
	  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu on tr.[BurdenPool] = bplu.[BurdenPool] AND bplu.[RevisionID] = rev.ID
	  LEFT OUTER JOIN [dbo].[RateCode] rc on tr.[RateCode] = rc.[RateCode] AND rc.[RevisionID] = rev.ID';

			PRINT 'BurdenElement = ' + @BurdenElement;
			PRINT @sql_stmt;

			EXEC (@sql_stmt);

			FETCH NEXT FROM @BECursor
			INTO @BEID, @BurdenElement;
				
		END;

		UPDATE [dbo].[BurdenPoolLU]
		   SET [IsGaT2ApplicableForMissionSolutions] = 1
		  FROM [dbo].[BurdenPoolLU] bplu  
		  JOIN [dbo].[Revision] rev on rev.[Revision] = @Revision and bplu.RevisionID = rev.ID
		 WHERE BurdenPool in 
				(SELECT BurdenPool
				   FROM (
	   					SELECT BurdenPool, [RC_G&A T2], InputSource
						  FROM dbo.TempPPBurdenRates
						 WHERE [RC_G&A T2] != ''
						GROUP BY BurdenPool, [RC_G&A T2], InputSource) grp
				 GROUP BY BurdenPool, [RC_G&A T2]
				 HAVING COUNT(*) = 1); -- [RC_G&A T2] is populated in BurdenRateServices spreadsheet, but not the others

		--------------------------------------------------------------------------------------------------------------------------------------
		-- Update RateCode table by populating Pro Pricer fields.
		-- This takes multiple steps: 
		--   1) Update entries where the Resource code exactly matches the Rate Code (i.e. no extension). Populate Description for these entries.
		--   2) Update entries where the first 6 characters of the Resource code match the Rate Code (i.e. ignore the extension 1,2,3, etc.). Leave Description Null.
		--------------------------------------------------------------------------------------------------------------------------------------
		UPDATE [dbo].[RateCode]
		SET ResourceTypeID = rt.ID,
			GovernmentBurdenPoolID = bpluGov.ID,
			CommercialBurdenPoolID = bpluComm.ID,
			RateTypeID = rtlu.ID
		FROM [dbo].[RateCode] rc  
		JOIN [dbo].[Revision] rev on rev.[Revision] = @Revision and rc.RevisionID = rev.ID
		JOIN [dbo].[TempPPDirectRates] tr on rc.RateCode = tr.Resource
		LEFT OUTER JOIN [dbo].[ResourceTypeLU] rt on tr.[ResourceType] = rt.[Description]
		LEFT OUTER JOIN [dbo].[BurdenPoolLU] bpluGov on bpluGov.RevisionID = rev.ID AND tr.[BurdenPool] = bpluGov.[BurdenPool] AND tr.[InputSource] = 0
		LEFT OUTER JOIN [dbo].[BurdenPoolLU] bpluComm on bpluComm.RevisionID = rev.ID AND tr.[BurdenPool] = bpluComm.[BurdenPool] AND tr.[InputSource] = 1
		LEFT OUTER JOIN [dbo].[RateTypeLU] rtlu on tr.[RateType] = rtlu.[Description]
		;

		UPDATE [dbo].[RateCode]
		SET ResourceTypeID = rt.ID,
			GovernmentBurdenPoolID = bpluGov.ID,
			CommercialBurdenPoolID = bpluComm.ID,
			RateTypeID = rtlu.ID
		FROM [dbo].[RateCode] rc  
		JOIN [dbo].[Revision] rev on rev.[Revision] = @Revision and rc.RevisionID = rev.ID
		JOIN [dbo].[TempPPDirectRates] tr on LEFT(tr.[Resource],6) = rc.[RateCode]
		LEFT OUTER JOIN [dbo].[ResourceTypeLU] rt on tr.[ResourceType] = rt.[Description]
		LEFT OUTER JOIN [dbo].[BurdenPoolLU] bpluGov on bpluGov.RevisionID = rev.ID AND tr.[BurdenPool] = bpluGov.[BurdenPool] AND tr.[InputSource] = 0
		LEFT OUTER JOIN [dbo].[BurdenPoolLU] bpluComm on bpluComm.RevisionID = rev.ID AND tr.[BurdenPool] = bpluComm.[BurdenPool] AND tr.[InputSource] = 1
		LEFT OUTER JOIN [dbo].[RateTypeLU] rtlu on tr.[RateType] = rtlu.[Description]
		;

		/**
		 * Query to find anomalies...
		SELECT * 
		FROM (
			SELECT r1.[RateCode] as rc1, r2.[RateCode] as rc2, d.*
			FROM (SELECT DISTINCT [ResourceType]
				  ,[Resource]
				  ,[Description]
				  ,[BurdenPool]
				  ,[RateType]
				  ,[Step]
				  ,[Factor]
			  FROM [dbo].[TempPPDirectRates]) d
			  left outer join [dbo].[RateCode] r1 on d.[Resource] = r1.[RateCode]
			  left outer join [dbo].[RateCode] r2 on LEFT(d.[Resource],6) = r2.[RateCode]) as j
		WHERE j.rc1 is null and j.rc2 is null;
		*/

		--------------------------------------------------------------------------------------------------------------------------------------
		-- Insert Pro Pricer Rate Code Xref entries.
		--------------------------------------------------------------------------------------------------------------------------------------
		INSERT INTO [dbo].[ProPricerRateCodeXref] (UpdateDate, RateCodeID, RateCodeExtensionID, Description)
		SELECT GETDATE() as UpdateDate, RateCodeID, RateCodeExtensionID, Description
		FROM (
			SELECT DISTINCT rev.ID as RevisionID, rc.ID as RateCodeID, rc.RateCode, tr.Description, rcelu.ID as RateCodeExtensionID  
			FROM (SELECT DISTINCT [ResourceType]
							,[Resource]
							,[Description]
							,[BurdenPool]
							,[RateType]
							,[Step]
							,[Factor]
						  FROM [dbo].[TempPPDirectRates]) as tr
			JOIN [dbo].[Revision] rev on rev.[Revision] = @Revision
			JOIN [dbo].RateCode rc on rc.RevisionID = rev.ID AND (LEFT(tr.[Resource],6) = rc.RateCode OR tr.[Resource] = rc.RateCode)
			LEFT OUTER JOIN [dbo].RateCodeExtensionLU rcelu on LEN(tr.[Resource]) = 7 AND RIGHT(tr.[Resource],1) = rcelu.RateCodeExtension) as g
		ORDER BY RateCode, RateCodeExtensionID;
	END; -- WHILE

	--------------------------------------------------------------------------------------------------------------------------------------
	-- Make final Category adjustments as needed
	--------------------------------------------------------------------------------------------------------------------------------------
	UPDATE dbo.[RateCode]
		SET CategoryId = 1	-- Direct Labor
	WHERE RateCode in ('541750LB', '541940CLB', '541940SLB', 'Bus Ops Factor 1');

	UPDATE dbo.[RateCode]
		SET CategoryId = 7	-- Non-Labor Escalation Factor
	WHERE RateCode in ('Material Escalation', 'MATESC', 'NLBESCCHSERV', 'Serv Material Esc', 'Travel Escalation', 'Travel Factor', 'Travel Factor HEF', 'Travel SERV Esc', 'Travel SERV Factor');

	UPDATE dbo.[RateCode]
		SET CategoryId = 11	-- Service Center
	WHERE RateCode in ('541750NL', '541940CNL', '541940SNL');

	UPDATE dbo.[RateCode]
		SET CategoryId = 13	-- Travel Mlge
	WHERE RateCode in ('Mileage', 'Mileage Serv');

	-- Note: CategoryId values for 'NLBESCCH', 'SALRYESC', and 'TRAVLESC' were set by the InsertTempRates.sql script.

	--------------------------------------------------------------------------------------------------------------------------------------
	-- Clear out previous year rates as needed
	--------------------------------------------------------------------------------------------------------------------------------------
	-- 1) Delete RateCodeYear rows for RateCodes like 'XXK%'
	delete dbo.RateCodeYear
	from dbo.RateCode rc
	join dbo.RateCodeYear rcy on rc.ID = rcy.RateCodeId
	where rc.RateCode like 'XXK%' and rc.CategoryID = 1;

	-- 2) Delete RateCodeYear rows for Category=Direct Rate, RateCode starts with alpha character, years 2014-2016.
	delete dbo.RateCodeYear
	from dbo.RateCode rc
	join dbo.RateCodeYear rcy on rc.ID = rcy.RateCodeId
	where (rc.RateCode like 'C%' OR rc.RateCode like 'F%' OR rc.RateCode like 'M%' OR rc.RateCode like 'P%' OR rc.RateCode like 'S%' OR rc.RateCode like 'X%' OR rc.RateCode like 'Z%') and rc.CategoryID = 1 and rcy.year in (2014,2015,2016) and (rcy.Rate = 0.00 OR rcy.Rate IS NULL)

END;
