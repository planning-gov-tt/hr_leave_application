<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="HR_LEAVEv2.Admin.Settings" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        #gridViewContainer th, td{
            text-align:center;
            padding:10px;
        }

        .gvPager span{
            background-color: #DEE1E7;
        }

        .gvPager td{
            padding-left: 5px;
            padding-right: 5px;
        }

        #tblDynamicForm td {
            text-align:left;
        }
    </style>
    <h1><%: Title %></h1>
    <div style="margin: 0 auto; margin-top: 30px; width:85%;text-align:center">
        <div class="row">
            <asp:DropDownList ID="DropDownList1" runat="server" AutoPostBack="true">
                <asp:ListItem Value="-">Choose a Table</asp:ListItem>
                <asp:ListItem Value="assignment">Assignment</asp:ListItem>
                <asp:ListItem Value="accumulations">Accumulations</asp:ListItem>
                <asp:ListItem Value="department">Department</asp:ListItem>
                <asp:ListItem Value="employee">Employee</asp:ListItem>
                <asp:ListItem Value="employeeposition">EmployeePosition</asp:ListItem>
                <asp:ListItem Value="employeerole">EmployeeRole</asp:ListItem>
                <asp:ListItem Value="employmenttype">EmploymentType</asp:ListItem>
                <asp:ListItem Value="emptypeleavetype">EmpTypeLeaveType</asp:ListItem>
                <asp:ListItem Value="leavetransaction">LeaveTransaction</asp:ListItem>
                <asp:ListItem Value="leavetype">LeaveType</asp:ListItem>
                <asp:ListItem Value="permission">Permission</asp:ListItem>
                <asp:ListItem Value="position">Position</asp:ListItem>
                <asp:ListItem Value="role">Role</asp:ListItem>
                <asp:ListItem Value="rolepermission">RolePermission</asp:ListItem>
            </asp:DropDownList>
        </div>

        <asp:Panel ID="addPanel" Visible ="false" runat="server" class="row" style="margin-top:15px;">
            <table id="tblDynamicForm" runat="server" style="margin: 0 auto;">
                <tr>
                    <td>
                        <asp:PlaceHolder ID="formPlaceholder" runat="server"></asp:PlaceHolder>
                    </td>
                </tr>
            </table>
        </asp:Panel>

        <div id="gridViewContainer" class="row" style="margin-top: 15px;">
            <asp:GridView ID="GridView1" runat="server"
            AutoGenerateColumns="true"
            AllowPaging="true" PageSize="5" OnPageIndexChanging="GridView1_PageIndexChanging" Style="margin:0 auto;" OnRowCommand="GridView1_RowCommand"
            >
                <Columns>
                    <%--action buttons--%>
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <div style="display:flex; justify-content:center;">
                                <%--details button--%>
                            <asp:LinkButton ID="editBtn" runat="server" CssClass="btn btn-primary content-tooltipped" data-toggle="tooltip" data-placement="top" title="Edit" Style="display:inline-block; margin-right:5px"
                                CommandName="editRow"
                                CommandArgument="<%# ((GridViewRow) Container).RowIndex %>">
                        <i class="fa fa-pencil" aria-hidden="true"></i>
                            </asp:LinkButton>

                            <%--employee buttons--%>
                            <asp:LinkButton ID="deleteBtn" CssClass="btn btn-danger content-tooltipped" data-toggle="tooltip" data-placement="top" title="Delete" runat="server" Style="display:inline-block;"
                                OnClientClick="return confirm('Delete row?');"
                                CommandName="deleteRow"
                                CommandArgument="<%# ((GridViewRow) Container).RowIndex %>">
                        <i class="fa fa-trash-o" aria-hidden="true"></i>
                            </asp:LinkButton>
                            </div>
                            
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
               

                <PagerStyle CssClass="gvPager" />

            </asp:GridView>

            <asp:UpdatePanel ID="UpdatePanel1" runat="server" Style="margin-top: 10px;">
                <ContentTemplate>
                    <asp:Panel ID="noDataPanel" runat="server" class="alert alert-info text-center" role="alert" style="display:none; margin:0 5px">
                        <i class="fa fa-info-circle"></i>
                        No Data available
                    </asp:Panel>

                    <asp:Panel ID="noTableSelectedPanel" runat="server" class="alert alert-info text-center" role="alert" style="display:none; margin:0 5px">
                        <i class="fa fa-info-circle"></i>
                        No Table selected
                    </asp:Panel>

                    <asp:Panel ID="deleteUnsuccessfulPanel" runat="server" class="alert alert-danger text-center" role="alert" style="display:none; margin:0 5px">
                        <i class="fa fa-exclamation-triangle"></i>
                        Delete could not be completed
                    </asp:Panel>

                    <asp:Panel ID="deleteSuccessfulPanel" runat="server" class="alert alert-success text-center" role="alert" style="display:none; margin:0 5px">
                        <i class="fa fa-thumbs-up"></i>
                        Delete successful
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        

    </div>
    


</asp:Content>
