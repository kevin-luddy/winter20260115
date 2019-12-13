SELECT NTID, count(*) as cnt
  FROM [dbo].[ETIuser]
  group by NTID
  order by cnt desc
