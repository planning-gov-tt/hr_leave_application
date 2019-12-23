<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainGridView.ascx.cs" Inherits="HR_LEAVEv2.UserControls.MainGridView" %>


<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>


<div id="filterContainer" class="container" style="padding-bottom: 30px">
    <form>
        <div class="row">
            <div class="col-xs-6 form-group">
                <label for="txtFrom">Start Date:</label>
                <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                <i id="fromCalendar" class="fa fa-calendar fa-lg"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender1" TargetControlID="txtFrom" PopupButtonID="fromCalendar" runat="server"></ajaxToolkit:CalendarExtender>
            </div>
            <div class="col-xs-6 form-group">
                <label for="txtTo">End Date:</label>
                <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                <i id="toCalendar" class="fa fa-calendar fa-lg"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender2" TargetControlID="txtTo" PopupButtonID="toCalendar" runat="server" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-6 form-group">
                <label for="txtFrom">Submitted From:</label>
                <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                <i id="fromCalendarSubmitted" class="fa fa-calendar fa-lg"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender3" TargetControlID="txtFrom" PopupButtonID="fromCalendar" runat="server"></ajaxToolkit:CalendarExtender>
            </div>
            <div class="col-xs-6 form-group">
                <label for="txtTo">Submitted To:</label>
                <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                <i id="toCalendarSubmitted" class="fa fa-calendar fa-lg"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender4" TargetControlID="txtTo" PopupButtonID="toCalendar" runat="server" />
            </div>
        </div>
        <div class="row">
            <div class="col-sm-3">
                <asp:DropDownList CssClass="form-control" runat="server" ID="ddlType" OnSelectedIndexChanged="ddlType_SelectedIndexChanged">
                    <asp:ListItem Text="Type" Value=""></asp:ListItem>
                    <asp:ListItem Text="Sick" Value="Sick"></asp:ListItem>
                    <asp:ListItem Text="Vacation" Value="Vacation"></asp:ListItem>
                    <asp:ListItem Text="Personal" Value="Personal"></asp:ListItem>
                    <asp:ListItem Text="Casual" Value="Casual"></asp:ListItem>
                    <asp:ListItem Text="Bereavement" Value="Bereavement"></asp:ListItem>
                    <asp:ListItem Text="Leave Renewal" Value="Leave Renewal"></asp:ListItem>
                    <asp:ListItem Text="Maternity" Value="Maternity"></asp:ListItem>
                    <asp:ListItem Text="No Pay" Value="No Pay"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-sm-3">
                <asp:DropDownList CssClass="form-control" runat="server" ID="ddlStatus" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">
                    <asp:ListItem Text="Status" Value=""></asp:ListItem>
                    <asp:ListItem Text="Pending" Value="Pending"></asp:ListItem>
                    <asp:ListItem Text="Recommended" Value="Recommended"></asp:ListItem>
                    <asp:ListItem Text="Not Recommended" Value="Not Recommended"></asp:ListItem>
                    <asp:ListItem Text="Approved" Value="Approved"></asp:ListItem>
                    <asp:ListItem Text="Not Approved" Value="Not Approved"></asp:ListItem>
                    <asp:ListItem Text="Date Change Requested" Value="Date Change Requested"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-sm-3">
                <asp:TextBox ID="tbSearch" runat="server" CssClass="form-control" placeholder="Search"></asp:TextBox>
            </div>
            <div class="col-sm-3">
                <asp:Button CssClass="btn btn-primary" Text="Filter" ID="btnFilter" runat="server" OnClick="btnFilter_Click" />
            </div>
        </div>
    </form>
</div>




<div id="gridViewContainer" class="container">

    <%--if grid view is empty then show notification panel--%>
    <div class="row" id="validationRow">
        <asp:UpdatePanel ID="UpdatePanelEmptyGridView" runat="server">
            <ContentTemplate>
                <asp:Panel ID="emptyGridViewMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; width: 500px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="validationMsg" runat="server">No leave data available</span>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>


    <%-- FIXME: Paging also triggers the OnRowCommand fn--%>


    <asp:GridView
        ID="GridView"
        BorderStyle="None" CssClass="table" GridLines="Horizontal"
        AutoGenerateColumns="false"
        AllowSorting="true" OnSorting="GridView_Sorting"
        AllowPaging="true" PageSize="3" OnPageIndexChanging="GridView_PageIndexChanging"
        OnRowCommand="GridView_RowCommand"
        OnRowDataBound="GridView_RowDataBound"
        DataKeyNames="transaction_id, employee_id, supervisor_id, hr_manager_id"
        runat="server">

        <Columns>

            <asp:BoundField HeaderText="Date Submitted" DataField="date_submitted" SortExpression="date_submitted" />
            <asp:BoundField HeaderText="Supervisor" DataField="supervisor_name" SortExpression="supervisor_name" />
            <asp:BoundField HeaderText="Employee" DataField="employee_name" SortExpression="employee_name" />
            <asp:BoundField HeaderText="Leave Type" DataField="leave_type" SortExpression="leave_type" />
            <asp:BoundField HeaderText="Start Date" DataField="start_date" SortExpression="start_date" />
            <asp:BoundField HeaderText="End Date" DataField="end_date" SortExpression="end_date" />
            <asp:BoundField HeaderText="Status" DataField="status" SortExpression="status" />
            <%--comments--%>


            <%--action buttons--%>
            <asp:TemplateField HeaderText="Action" Visible="true">
                <ItemTemplate>

                    <%--employee buttons--%>
                    <asp:Button ID="btnCancelLeave" class="btn btn-danger" Visible="<%# btnEmpVisible %>" runat="server"
                        CommandName="cancelLeave"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                        Text="🗑"
                        ToolTip="Cancel Leave Request" />

                    <%--supervisor buttons--%>
                    <asp:Button ID="btnNotRecommended" class="btn btn-danger" Visible="<%# btnSupVisible %>" runat="server"
                        CommandName="notRecommended"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                        Text="✘"
                        ToolTip="Not Recommneded" />
                    <asp:Button ID="btnRecommended" class="btn btn-success" Visible="<%# btnSupVisible %>" runat="server"
                        CommandName="recommended"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                        Text="✔"
                        ToolTip="Recommended" />

                    <%--hr buttons--%>
                    <asp:Button ID="btnNotApproved" class="btn btn-danger" Visible="<%# btnHrVisible %>" runat="server"
                        CommandName="notApproved"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                        Text="✘"
                        ToolTip="Not Approved" />
                    <asp:Button ID="btnApproved" class="btn btn-success" Visible="<%# btnHrVisible %>" runat="server"
                        CommandName="approved"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                        Text="✔"
                        ToolTip="Approved" />
                    <asp:Button ID="btnEditLeaveRequest" class="btn btn-primary" Visible="<%# btnHrVisible %>" runat="server"
                        CommandName="editLeaveRequest"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                        Text="✎"
                        ToolTip="Edit Leave Request" />
                    <asp:Button ID="btnUndoApprove" class="btn btn-warning" Visible="<%# btnHrVisible %>" runat="server"
                        CommandName="undoApprove"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                        Text="⮌"
                        ToolTip="Undo Approve" />

                </ItemTemplate>
            </asp:TemplateField>

        </Columns>
    </asp:GridView>

</div>
