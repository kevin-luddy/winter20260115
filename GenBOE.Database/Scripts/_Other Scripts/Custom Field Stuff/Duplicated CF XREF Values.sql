select BOETaskElementID, CustomFieldValueID, count(*) as cnt from
BOETaskElementCustomFieldValueXREF
group by BOETaskElementID, CustomFieldValueID
HAVING COUNT(*) > 1
order by cnt desc

select BOELaborTypeID, CustomFieldValueID, count(*) as cnt from
BOELaborTypeCustomFieldValueXREF
group by BOELaborTypeID, CustomFieldValueID
HAVING COUNT(*) > 1
order by cnt desc

select BOEID, CustomFieldValueID, count(*) as cnt from
BOECustomFieldValueXREF
group by BOEID, CustomFieldValueID
HAVING COUNT(*) > 1
order by cnt desc

select BOEID, CustomFieldID, MAX(x.UpdateDT), count(*) as cnt 
	from BOECustomFieldValueXREF x
		INNER JOIN CustomFieldValue cFv ON x.CustomFieldValueId = cFv.CustomFieldValueID
	group by BOEID, CustomFieldID
	HAVING COUNT(*) > 1
	order by cnt desc;

select BOETaskElementID, CustomFieldID, MAX(x.UpdateDT), count(*) as cnt from
BOETaskElementCustomFieldValueXREF x
		INNER JOIN CustomFieldValue cFv ON x.CustomFieldValueId = cFv.CustomFieldValueID
group by BOETaskElementID, CustomFieldID
HAVING COUNT(*) > 1
order by cnt desc;

select BOELaborTypeID, CustomFieldID, MAX(x.UpdateDT), count(*) as cnt from
BOELaborTypeCustomFieldValueXREF x
		INNER JOIN CustomFieldValue cFv ON x.CustomFieldValueId = cFv.CustomFieldValueID
group by BOELaborTypeID, CustomFieldID
HAVING COUNT(*) > 1
order by cnt desc;
