select e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email
from [dbo].[employee] e
join [dbo].assignment a
on e.employee_id = a.supervisee_id
where a.supervisor_id = 1;