<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainGridView.ascx.cs" Inherits="HR_LEAVEv2.UserControls.MainGridView" %>


<div>

    <center>    

        <%--Paging also triggers the OnRowCommand fn--%>
<%--           "          --%>

        <asp:GridView ID="GridView" 
            BorderStyle="None" 
 CssClass="table"
            GridLines="Horizontal"
            AutoGenerateColumns="false" 
            OnRowCommand="GridView_RowCommand" 
            OnRowDataBound="GridView_RowDataBound"
            DataKeyNames="transaction_id, employee_id, supervisor_id, hr_manager_id" 
            
            
         
            runat="server">

            <Columns>                       

                <asp:BoundField HeaderText="Date Submitted" DataField="date_submitted" />
                <asp:BoundField HeaderText="Supervisor" DataField="supervisor_name" />
                <asp:BoundField HeaderText="Employee" DataField="employee_name" />
                <asp:BoundField HeaderText="Leave Type" DataField="leave_type" />
                <asp:BoundField HeaderText="Start Date" DataField="start_date" />
                <asp:BoundField HeaderText="End Date" DataField="end_date" />
                <asp:BoundField HeaderText="Status" DataField="status" />
                <%--comments--%>

                <%--action buttons--%>
                <asp:TemplateField  HeaderText="">
                    <ItemTemplate>   

                        <%--employee buttons--%>
                        <asp:Button ID="btnChangeDate" class="btn btn-primary" Visible=<%# btnEmpVisible %> runat="server"
                            CommandName="Date Change Requested"
                            CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                            Text="✎"
                            ToolTip="Change Date" />
                        
                        <%--supervisor buttons--%>           
                        <asp:Button ID="btnNotRecommended" class="btn btn-danger" Visible=<%# btnSupVisible %> runat="server"
                            CommandName="notRecommended"
                            CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                            Text="✘"
                            ToolTip="Not Recommneded" />
                        <asp:Button ID="btnRecommended" class="btn btn-success" Visible=<%# btnSupVisible %> runat="server"
                            CommandName="recommended"
                            CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                            Text="✔"
                            ToolTip="Recommended" />

                        <%--hr buttons--%>
                        <asp:Button ID="btnNotApproved" class="btn btn-danger" Visible=<%# btnHrVisible %> runat="server"
                            CommandName="notApproved"
                            CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                            Text="✘"
                            ToolTip="Not Approved" />
                        <asp:Button ID="btnApproved" class="btn btn-success" Visible=<%# btnHrVisible %> runat="server"
                            CommandName="approved"
                            CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                            Text="✔"
                            ToolTip="Approved" />

                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
        </asp:GridView>       

    </center>
</div>