--This data should be deleted as soon as it's retrieved, so clean out any data older than a day

DELETE
  FROM [dbo].[ReportXmlData]
  WHERE UpdateDT <= DATEADD(DAY, -1, GETDATE())

GO