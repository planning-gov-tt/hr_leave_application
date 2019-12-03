select e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email
from [HRLeaveTestDb].[dbo].[employee] e
join [HRLeaveTestDb].[dbo].assignment a
on e.employee_id = a.supervisee_id
where (a.supervisor_id = 1) 
AND
 ((e.employee_id LIKE 'searchTerm%') OR (e.ihris_id LIKE 'searchTerm%') OR (e.first_name LIKE 'searchTerm%') OR (e.last_name LIKE 'searchTerm%') OR (e.email LIKE 'searchTerm%') );