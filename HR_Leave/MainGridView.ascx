<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainGridView.ascx.cs" Inherits="HR_Leave.MainGridView" %>


<%--<asp:Label Text='<%# gridViewType %>' runat="server"></asp:Label>--%>


<asp:GridView ID="GridView" 
    AutoGenerateColumns="false"  
    DataKeyNames="transaction_id, employee_id, supervisor_id, hr_manager_id" 
    runat="server">
    <Columns>
                        
        <%--<asp:TemplateField HeaderText="Name" SortExpression="LastName">
        <ItemTemplate>
           <asp:Label ID="lblName" runat="server" Text='<%# Eval("first_name").ToString()[0] +". "+ Eval("last_name")%>' ></asp:Label>
        </ItemTemplate>
        </asp:TemplateField>--%>

        <asp:BoundField HeaderText="Date Submitted" DataField="date_submitted" />
        <asp:BoundField HeaderText="Supervisor" DataField="supervisor_name" />
        <asp:BoundField HeaderText="Employee" DataField="employee_name" />
        <asp:BoundField HeaderText="Leave Type" DataField="leave_type" />
        <asp:BoundField HeaderText="Start Date" DataField="start_date" />
        <asp:BoundField HeaderText="End Date" DataField="end_date" />
        <asp:BoundField HeaderText="Status" DataField="status" />
        <%--comments--%>


    </Columns>
</asp:GridView>