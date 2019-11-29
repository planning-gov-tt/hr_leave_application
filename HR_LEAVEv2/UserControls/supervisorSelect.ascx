<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="supervisorSelect.ascx.cs" Inherits="HR_LEAVEv2.UserControls.supervisorSelect" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>


<ajaxToolkit:ComboBox ID="ComboBox1" 
    runat="server" 
    AutoCompleteMode="SuggestAppend" 
    DataSourceID="SqlDataSource1" 
    DataTextField="Supervisor Name" 
    DataValueField="employee_id" 
    MaxLength="0" 
    OnSelectedIndexChanged="ComboBox1_SelectedIndexChanged">
</ajaxToolkit:ComboBox>
<asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" ProviderName="System.Data.SqlClient" SelectCommand="select e.employee_id, e.first_name + ' ' + e.last_name as 'Supervisor Name'
from [HRLeaveTestDb].[dbo].[employee] e
left join [HRLeaveTestDb].[dbo].[employeerole] er
on e.employee_id = er.employee_id
where er.role_id = 'sup';"></asp:SqlDataSource>
<asp:RequiredFieldValidator runat="server" ControlToValidate="ComboBox1" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>