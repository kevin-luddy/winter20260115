-- Clean up Error Logs older than 30 days
DELETE FROM [ELMAH_Error] WHERE TimeUtc < DATEADD(d, -30, getdate());
GO

-- Clean up Error Logs older than 30 days
DELETE FROM [_Logs] WHERE [TimeStamp] < DATEADD(d, -30, getdate());
GO