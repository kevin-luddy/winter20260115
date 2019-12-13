-- Clean up Error Logs older than 30 days
DELETE FROM [ELMAH_Error] WHERE TimeUtc < DATEADD(d, -30, getdate());
GO