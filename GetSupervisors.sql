select e.employee_id, e.first_name + ' ' + e.last_name as 'Supervisor Name'
from [HRLeaveTestDb].[dbo].[employee] e
left join [HRLeaveTestDb].[dbo].[employeerole] er
on e.employee_id = er.employee_id
where er.role_id = 'sup';