/*
	   ## START ##
	   1/16/18 [twilson3] - BOEJ-2932 Remove Storage of Spreads
*/

DELETE ls
FROM dbo.BOELaborSpread ls
INNER JOIN dbo.BOELaborType lt
ON lt.BOELaborTypeID = ls.BOELaborTypeID
WHERE lt.SpreadCurveID <> 0 AND lt.SpreadCurveID <> 1  -- Only save Discrete Labor Spreads

-- or run this one 
--WHILE EXISTS(SELECT 1
--FROM dbo.BOELaborSpread ls
--INNER JOIN dbo.BOELaborType lt
--ON lt.BOELaborTypeID = ls.BOELaborTypeID
--WHERE lt.SpreadCurveID <> 0 AND lt.SpreadCurveID <> 1)
--BEGIN
--	DELETE 
--	FROM dbo.BOELaborSpread where BOELaborTypeID in (
--	select distinct top 100000 lt.BOELaborTypeID FROM
--	dbo.boelaborspread ls
--	INNER JOIN dbo.BOELaborType lt
--	ON lt.BOELaborTypeID = ls.BOELaborTypeID
--	WHERE lt.SpreadCurveID <> 0 AND lt.SpreadCurveID <> 1  -- Only save Discrete Labor Spreads
--	)
--END
GO

DELETE ls
FROM version.BOELaborSpread ls
INNER JOIN version.BOELaborType lt
ON lt.BOELaborTypeID = ls.BOELaborTypeID AND lt.VersionID = ls.VersionID
WHERE lt.SpreadCurveID <> 0 AND lt.SpreadCurveID <> 1  -- Only save Discrete Labor Spreads
GO
-- or run this one many times
--DELETE 
--FROM version.BOELaborSpread where BOELaborTypeID in (
--select distinct top 100000 lt.BOELaborTypeID FROM
--version.boelaborspread ls
--INNER JOIN version.BOELaborType lt
--ON lt.BOELaborTypeID = ls.BOELaborTypeID AND lt.VersionID = ls.VersionID
--WHERE lt.SpreadCurveID <> 0 AND lt.SpreadCurveID <> 1  -- Only save Discrete Labor Spreads
--)

-- We should run a db shrink on the DB after these scripts, take note the DB name is in the command
--DBCC SHRINKDATABASE (GenBOESpace, 0);
--GO
/*
	   1/16/18 [twilson3] - BOEJ-2887 Remove Storage of Spreads
	   ## END ##
*/