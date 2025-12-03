EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.18';
GO

-- 12/01/2025 Carlos (e426263) PROPH-3379 LOB Changes for RMS

-- BEGIN
-- 	UPDATE dbo.LineOfBusiness
-- 	SET IsActive = 0
-- 	WHERE LineOfBusinessName IN ('C6ISR', 'Integrated Warfare Systems and Sensors');

-- 	IF NOT EXISTS (SELECT 1 
--                FROM dbo.LineOfBusiness
--                WHERE LineOfBusinessName IN ('Mission Integrated Command and Control (MIC2)', 
--                                             'Sensors, Effectors & Mission Systems (SEMS)'))
-- 	BEGIN
-- 		INSERT INTO dbo.LineOfBusiness (LineOfBusinessName, IsActive)
-- 		VALUES ('Mission Integrated Command and Control (MIC2)', 1),
-- 			   ('Sensors, Effectors & Mission Systems (SEMS)', 1);
-- 	END
-- END



