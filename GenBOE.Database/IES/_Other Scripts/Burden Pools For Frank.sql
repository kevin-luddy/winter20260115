DECLARE @revision INT;
SELECT @revision = MAX(Id) FROM Revision;

--SELECT * FROM BurdenElementLU;
--SELECT * FROM BurdenPoolLU WHERE RevisionID = @revision;
--SELECT Id, RateCode FROM RateCode WHERE RevisionID = @revision;

SELECT Description
			,[OHDEVJOBSH]
			,[OHDEVJOBSH-G]
			,[OHDEVNET]
			,[OHDEVNET-G]
			,[OHFBMJOB]
			,[OHFBMJOB-G]
			,[OHFBMNET]
			,[OHFBMNET-G]
			,[OHHNTJOB]
			,[OHHNTJOB-G]
			,[OHHNTNET]
			,[OHHNTNET-G]
			,[OHMICJOB]
			,[OHMICJOB-G]
			,[OHMICNET]
			,[OHMICNET-G]
			,[OHOFFNET]
			,[OHOFFNET-G]
			,[OHOFJOBSH]
			,[OHOFJOBSH-G]
			,[OHPRDNET]
			,[OHPRDNET-G]
			,[OHPRODJOBSH]
			,[OHPRODJOBSH-G]
			,[OHPRONET]
			,[OHPRONET-G]
			,[OHPRUNET]
			,[OHPRUNET-G]
			,[OHSERV1OFFNET]
			,[OHSERV1OFFNET-G]
			,[OHSERV1ONNET]
			,[OHSERV1ONNET-G]
			,[OHSERV2OFFNET]
			,[OHSERV2OFFNET-G]
			,[OHSERV2ONNET]
			,[OHSERV2ONNET-G]
			,[OHSERVOFFJOB]
			,[OHSERVOFFJOB-G]
			,[OHSERVONJOB]
			,[OHSERVONJOB-G]
			,[OHSERVPRONET]
			,[OHSERVPRONET-G]
			,[SERVNLG&APOOL]
			,[SERVNLG&APOOL-G]
FROM(
	SELECT bp.BurdenPool, bE.Description, rC.RateCode
		FROM ProPricerBurdenRateMap x, BurdenPoolLU bp, RateCode rC, BurdenElementLU bE
		WHERE
			rC.RevisionId = @revision AND x.RateCodeId = rC.Id
			AND x.BurdenPoolId = bp.Id AND bp.RevisionId = rC.RevisionId
			AND x.BurdenElementId = bE.Id
--		AND BurdenPool = 'OHDEVJOBSH'
		) AS temp
PIVOT
	(
	MAX(RateCode)
	FOR BurdenPool IN ([OHDEVJOBSH]
						,[OHDEVJOBSH-G]
						,[OHDEVNET]
						,[OHDEVNET-G]
						,[OHFBMJOB]
						,[OHFBMJOB-G]
						,[OHFBMNET]
						,[OHFBMNET-G]
						,[OHHNTJOB]
						,[OHHNTJOB-G]
						,[OHHNTNET]
						,[OHHNTNET-G]
						,[OHMICJOB]
						,[OHMICJOB-G]
						,[OHMICNET]
						,[OHMICNET-G]
						,[OHOFFNET]
						,[OHOFFNET-G]
						,[OHOFJOBSH]
						,[OHOFJOBSH-G]
						,[OHPRDNET]
						,[OHPRDNET-G]
						,[OHPRODJOBSH]
						,[OHPRODJOBSH-G]
						,[OHPRONET]
						,[OHPRONET-G]
						,[OHPRUNET]
						,[OHPRUNET-G]
						,[OHSERV1OFFNET]
						,[OHSERV1OFFNET-G]
						,[OHSERV1ONNET]
						,[OHSERV1ONNET-G]
						,[OHSERV2OFFNET]
						,[OHSERV2OFFNET-G]
						,[OHSERV2ONNET]
						,[OHSERV2ONNET-G]
						,[OHSERVOFFJOB]
						,[OHSERVOFFJOB-G]
						,[OHSERVONJOB]
						,[OHSERVONJOB-G]
						,[OHSERVPRONET]
						,[OHSERVPRONET-G]
						,[SERVNLG&APOOL]
						,[SERVNLG&APOOL-G])) AS PivotTable
	ORDER BY Description;
