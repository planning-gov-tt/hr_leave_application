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

        #deleteSuccessfulMsgCancelBtn, #deleteUnsuccessfulMsgCancelBtn{
            cursor:pointer;
            outline:none;
        }
    </style>
    <h1><%: Title %></h1>
    <div style="margin: 0 auto; margin-top: 30px; width:90%;text-align:center">
        <div class="row">
            <asp:DropDownList ID="DropDownList1" runat="server" AutoPostBack="true" OnSelectedIndexChanged="DropDownList1_SelectedIndexChanged">
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

        <asp:Panel ID="addPanel" Visible ="false" runat="server" class="row" style="margin-top:15px; margin-bottom:25px; background-color:#e0e0eb;">
            <asp:Panel ID="headerForFormPanel" CssClass="row" runat="server" Style="margin-bottom: -20px;" >
                <h2 id="headerForForm" runat="server"></h2>
                <%--<div>
                    <asp:LinkButton ID="resetForm" runat="server" CssClass="btn btn-primary">
                        <i class="fa fa-refresh"></i>
                    </asp:LinkButton>
                </div>--%>
            </asp:Panel>
            <div class="row">
                <table id="tblDynamicForm" runat="server" style="margin: 0 auto;">
                    <tr>
                        <td>
                            <asp:PlaceHolder ID="formPlaceholder" runat="server"></asp:PlaceHolder>
                        </td>
                    </tr>
                </table>
            </div>
            
            <asp:UpdatePanel ID="validationPanel" runat="server" Style="margin-bottom:15px;">
                <ContentTemplate>
                    <asp:Panel ID="createSuccessfulPanel" runat="server" CssClass="row alert alert-success" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                        <span>Insert Successful</span>
                    </asp:Panel>

                    <asp:Panel ID="createUnsuccessfulPanel" runat="server" class="alert alert-danger text-center" role="alert" style="display:none; margin:0 5px">
                        <i class="fa fa-exclamation-triangle"></i>
                        Insert could not be completed
                    </asp:Panel>

                    <asp:Panel ID="clashingRecordsPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span>Employment record being inserted clashes with another employment record</span>
                    </asp:Panel>

                    <asp:Panel ID="multipleActiveRecordsPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span>Employment record being inserted would result in multiple active records</span>
                    </asp:Panel>

                    <asp:Panel ID="Panel2" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span>Start date is not valid</span>
                    </asp:Panel>

                    <asp:Panel ID="invalidStartDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span>Start date is not valid</span>
                    </asp:Panel>
                    <asp:Panel ID="invalidExpectedEndDatePanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span>Expected end date is not valid</span>
                    </asp:Panel>
                    <asp:Panel ID="dateComparisonExpectedValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span>Expected end date cannot precede start date</span>
                    </asp:Panel>
                    <asp:Panel ID="dateComparisonActualValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span>Actual end date cannot precede start date</span>
                    </asp:Panel>
                    <asp:Panel ID="startDateIsWeekendPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span>Start date is on the weekend</span>
                    </asp:Panel>
                    <asp:Panel ID="expectedEndDateIsWeekendPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span>Expected end date is on the weekend</span>
                    </asp:Panel>
                    <asp:Panel ID="invalidActualEndDatePanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span>Actual end date is not valid</span>
                    </asp:Panel>
                    <asp:Panel ID="actualEndDateOnWeekend" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span>Actual end date is on the weekend</span>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            
            <asp:Panel ID="submitBtnPanel" CssClass="row" runat="server" Style="margin-bottom: 25px; margin-top:15px;">
                <asp:LinkButton ID="createBtn" CssClass="btn btn-primary" runat="server" Visible="false" OnClick="createBtn_Click" ValidationGroup="CU_validation">
                    <i class="fa fa-save"></i>
                    Save
                </asp:LinkButton>
                <asp:LinkButton ID="EditBtn" CssClass="btn btn-primary" runat="server" Visible="false" ValidationGroup="CU_validation">
                    <i class="fa fa-save"></i>
                    Save
                </asp:LinkButton>
            </asp:Panel>
        </asp:Panel>

        <div id="gridViewContainer" class="row">
            <%--Search bar--%>
            <asp:Panel ID="searchPanel" runat="server" DefaultButton="searchBtn" CssClass="input-group" Width="510px" Style="margin:0 auto; margin-bottom: 10px;" Visible="false">
                <asp:TextBox ID="searchTxtbox" runat="server" CssClass="form-control" placeholder="Search" aria-label="Search" aria-describedby="basic-addon2" AutoPostBack="true" OnTextChanged="searchTxtbox_TextChanged"></asp:TextBox>
                <div class="input-group-addon">
                    <asp:LinkButton ID="searchBtn" runat="server" OnClick="searchBtn_Click">
                            <span class="input-group-text" id="basic-addon2">
                                <i class="fa fa-search"></i>
                            </span>
                    </asp:LinkButton>
                </div>
            </asp:Panel>

            <asp:LinkButton ID="clearSearchBtn" runat="server" Visible="false" CssClass="btn btn-primary" OnClick="clearSearchBtn_Click">
                <i class="fa fa-times"></i>
                Clear Search
            </asp:LinkButton>

            <div class="row">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server" Style="margin-bottom: 10px; margin-top:10px;">
                    <ContentTemplate>
                        <asp:Panel ID="noDataPanel" runat="server" class="alert alert-info text-center" role="alert" Style="display: none; margin: 0px 5px">
                            <i class="fa fa-info-circle"></i>
                            No Data available
                        </asp:Panel>

                        <asp:Panel ID="noTableSelectedPanel" runat="server" class="alert alert-info text-center" role="alert" Style="display: none; margin: 0px 5px">
                            <i class="fa fa-info-circle"></i>
                            No Table selected
                        </asp:Panel>

                        <asp:Panel ID="deleteUnsuccessfulPanel" runat="server" class="alert alert-danger text-center" role="alert" Style="display: none; margin: 0px 5px">
                            <i class="fa fa-exclamation-triangle"></i>
                            Delete could not be completed
                            <i class="fa fa-times-circle" id="deleteUnsuccessfulMsgCancelBtn" style="margin-left: 11px; color: #484848;"></i>
                        </asp:Panel>

                        <asp:Panel ID="deleteSuccessfulPanel" runat="server" class="alert alert-success text-center" role="alert" Style="display: none; margin: 0px 5px">
                            <i class="fa fa-thumbs-up"></i>
                            Delete successful
                            <i class="fa fa-times-circle" id="deleteSuccessfulMsgCancelBtn" style="margin-left: 11px; color: #484848;"></i>
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>

            <asp:GridView ID="GridView1" runat="server"
                AutoGenerateColumns="true"
                AllowPaging="true" PageSize="5" OnPageIndexChanging="GridView1_PageIndexChanging"  Style="margin:0 auto;" OnRowCommand="GridView1_RowCommand">
                <Columns>
                    <%--action buttons--%>
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <div style="display: flex; justify-content: center;">
                                <%--details button--%>
                                <asp:LinkButton ID="editBtn" runat="server" CssClass="btn btn-primary content-tooltipped" data-toggle="tooltip" data-placement="top" title="Edit" Style="display: inline-block; margin-right: 5px"
                                    CommandName="editRow"
                                    CommandArgument="<%# ((GridViewRow) Container).RowIndex %>">
                        <i class="fa fa-pencil" aria-hidden="true"></i>
                                </asp:LinkButton>

                                <%--employee buttons--%>
                                <asp:LinkButton ID="deleteBtn" CssClass="btn btn-danger content-tooltipped" data-toggle="tooltip" data-placement="top" title="Delete" runat="server" Style="display: inline-block;"
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
        </div>
        

        

    </div>
    
    <script>
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {

            $('#deleteSuccessfulMsgCancelBtn').click(function () {
                $('#<%= deleteSuccessfulPanel.ClientID %>').hide();
            });
           
            $('#deleteUnsuccessfulMsgCancelBtn').click(function () {
                $('#<%= deleteUnsuccessfulPanel.ClientID %>').hide();
            });
        });
    </script>

</asp:Content>
