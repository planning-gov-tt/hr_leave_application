<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainGridView.ascx.cs" Inherits="HR_LEAVEv2.UserControls.MainGridView" %>


<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<style>
    #gridViewContainer th, td{
        text-align:center;
    }

    .inactive-employee-LA{
        background-color: #e0e0eb;
    }
</style>

<%--filter fields--%>
<div id="filterContainer" class="container" style="padding-bottom: 30px">

    <div class="row" style="text-align: right">

        <%--Submitted From--%>
        <div class="col-xs-3 form-group">
            <asp:TextBox ID="tbSubmittedFrom" runat="server" CssClass="form-control content-tooltipped" data-toggle="tooltip" data-placement="top" title="Enter submitted from date" Style="width: 75%; display: inline;" placeholder="Submitted From"></asp:TextBox>
            <i id="fromCalendarSubmitted" class="fa fa-calendar fa-lg calendar-icon"></i>
            <ajaxToolkit:CalendarExtender ID="CalendarExtender3" TargetControlID="tbSubmittedFrom" PopupButtonID="fromCalendarSubmitted" runat="server" Format="d/MM/yyyy"></ajaxToolkit:CalendarExtender>
        </div>

        <%--Submitted to--%>
        <div class="col-xs-3 form-group">
            <asp:TextBox ID="tbSubmittedTo" runat="server" CssClass="form-control content-tooltipped" data-toggle="tooltip" data-placement="top" title="Enter submitted to date" Style="width: 75%; display: inline;" placeholder="Submitted To"></asp:TextBox>
            <i id="toCalendarSubmitted" class="fa fa-calendar fa-lg calendar-icon"></i>
            <ajaxToolkit:CalendarExtender ID="CalendarExtender4" TargetControlID="tbSubmittedTo" PopupButtonID="toCalendarSubmitted" runat="server" Format="d/MM/yyyy"/>
        </div>

        <%--Start Date--%>
        <div class="col-xs-3 form-group">
            <asp:TextBox ID="tbStartDate" runat="server" CssClass="form-control content-tooltipped" data-toggle="tooltip" data-placement="top" title="Enter start date" Style="width: 75%; display: inline;" placeholder="Start Date"></asp:TextBox>
            <i id="fromCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
            <ajaxToolkit:CalendarExtender ID="CalendarExtender1" TargetControlID="tbStartDate" PopupButtonID="fromCalendar" runat="server" Format="d/MM/yyyy"></ajaxToolkit:CalendarExtender>
        </div>

        <%--End Date--%>
        <div class="col-xs-3 form-group">
            <asp:TextBox ID="tbEndDate" runat="server" CssClass="form-control content-tooltipped" data-toggle="tooltip" data-placement="top" title="Enter end date" Style="width: 75%; display: inline;" placeholder="End Date"></asp:TextBox>
            <i id="toCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
            <ajaxToolkit:CalendarExtender ID="CalendarExtender2" TargetControlID="tbEndDate" PopupButtonID="toCalendar" runat="server" Format="d/MM/yyyy"/>
        </div>

    </div>

    <%--Supervisor Name/ID search string--%>
    <div id="divTbSupervisor" class="row" runat="server">
        <div class="col-sm-12 form-group" style="text-align: center">
            <asp:TextBox ID="tbSupervisor" runat="server" CssClass="form-control content-tooltipped" data-toggle="tooltip" data-placement="right" title="Enter supervisor name/ID/Email" placeholder="Supervisor Name/ID/Email" Style="margin:0 auto; text-align:center" Width="50%"></asp:TextBox>
        </div>
    </div>

    <%--Employee Name/ID search string--%>
    <div id="divTbEmployee" class="row" runat="server">
        <div class="col-sm-12 form-group" style="text-align: center">
            <asp:TextBox ID="tbEmployee" runat="server" CssClass="form-control content-tooltipped" data-toggle="tooltip" data-placement="right" title="Enter employee name/ID/Email" placeholder="Employee Name/ID/Email" Style="margin:0 auto; text-align:center" Width="50%"></asp:TextBox>
        </div>
    </div>

    <div class="row">

        <%--Leave Types--%>
        <div class="col-sm-3">
            <asp:DropDownList CssClass="form-control content-tooltipped" data-toggle="tooltip" data-placement="top" title="Enter type of leave" runat="server" ID="ddlType" DataSourceID="SqlDataSource1" DataTextField="type_id" DataValueField="type_id" AppendDataBoundItems="true">
                <asp:ListItem Text="Leave Types (All)" Value=""></asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" SelectCommand="SELECT [type_id] FROM [leavetype] ORDER BY [type_id] DESC"></asp:SqlDataSource>
        </div>

        <%--Status of LA--%>
        <div class="col-sm-3">
            <asp:DropDownList CssClass="form-control content-tooltipped" data-toggle="tooltip" data-placement="top" title="Enter status of leave" runat="server" ID="ddlStatus">
                <asp:ListItem Text="Status (All)" Value=""></asp:ListItem>
                <asp:ListItem Text="Pending" Value="Pending"></asp:ListItem>
                <asp:ListItem Text="Recommended" Value="Recommended"></asp:ListItem>
                <asp:ListItem Text="Not Recommended" Value="Not Recommended"></asp:ListItem>
                <asp:ListItem Text="Approved" Value="Approved"></asp:ListItem>
                <asp:ListItem Text="Not Approved" Value="Not Approved"></asp:ListItem>
                <asp:ListItem Text="Date Change Requested" Value="Date Change Requested"></asp:ListItem>
            </asp:DropDownList>
        </div>

        <%--Qualified--%>
        <div class="col-sm-3">
            <asp:DropDownList CssClass="form-control content-tooltipped" data-toggle="tooltip" data-placement="top" title="Enter value for if user is qualified" runat="server" ID="ddlQualified">
                <asp:ListItem Text="Qualified (All)" Value=""></asp:ListItem>
                <asp:ListItem Text="Yes" Value="Yes"></asp:ListItem>
                <asp:ListItem Text="No" Value="No"></asp:ListItem>
            </asp:DropDownList>
        </div>

        <%--Show LAs from --%>
        <div class="col-sm-3">
            <asp:DropDownList CssClass="form-control content-tooltipped" data-toggle="tooltip" data-placement="top" title="Enter which type of LA to view" runat="server" ID="ddlLAfromActiveOrInactive">
                <asp:ListItem Text="All Leave Applications" Value="ActiveInactive"></asp:ListItem>
                <asp:ListItem Text="Active Leave Applications" Value="Active"></asp:ListItem>
                <asp:ListItem Text="Inactive Leave Applications" Value="Inactive"></asp:ListItem>
            </asp:DropDownList>
        </div>

    </div>

</div>

<%--filter submit or clear buttons--%>
<div class="container" style="padding-bottom: 12px; text-align: center">
    <asp:LinkButton ID="clearFilterBtn" runat="server" CssClass="btn btn-primary" Style="margin-left: 5px;" OnClick="clearFilterBtn_Click">
         <i class="fa fa-times"></i>
         Clear Filter
    </asp:LinkButton>

    <asp:LinkButton ID="filterBtn" runat="server" OnClick="btnFilter_Click" CssClass="btn btn-primary">
         <i class="fa fa-filter" aria-hidden="true"></i>
         Filter
    </asp:LinkButton>

    <i class="fa fa-info-circle content-tooltipped" aria-hidden="true" style="margin-left: 5px;  cursor:pointer"  
        data-toggle="tooltip" 
        data-placement="right" 
        title="Filter finds records which fulfill all the entered constraints">
    </i>
</div>


<%--gridview container--%>
<div id="gridViewContainer" class="container">

    <%--if grid view is empty then show notification panel--%>
    <div class="row text-center" id="validationRow" style="margin-bottom:25px;">

        <%--Validation Panel--%>
        <asp:UpdatePanel ID="UpdatePanelMsgs" runat="server">

            <ContentTemplate>

                <asp:Panel ID="emptyGridViewMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; width: 500px; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="validationMsg" runat="server">No leave data available</span>
                </asp:Panel>

                <asp:Panel ID="invalidSubmittedFromDate" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="invalidSubmittedFromDateMsg" runat="server">Submitted from date is not valid</span>
                </asp:Panel>

                <asp:Panel ID="invalidSubmittedToDate" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="invalidSubmittedToDateMsg" runat="server">Submitted to date is not valid</span>
                </asp:Panel>

                <asp:Panel ID="submittedDateComparisonValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="submittedDateComparisonValidationMsg" runat="server">Submitted to date cannot precede submitted from</span>
                </asp:Panel>
                <asp:Panel ID="invalidStartDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="invalidStartDateValidationMsg" runat="server">Start date is not valid</span>
                </asp:Panel>

                <asp:Panel ID="invalidEndDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="invalidEndDateValidationMsg" runat="server">End date is not valid</span>
                </asp:Panel>

                <asp:Panel ID="appliedForDateComparisonValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="dateComparisonValidationMsg" runat="server">End date cannot precede start date</span>
                </asp:Panel> 
                              
            </ContentTemplate>

        </asp:UpdatePanel>

    </div>
    
    
    <%--actual gridview--%>
    <asp:GridView
        ID="GridView"
        BorderStyle="None" CssClass="table" GridLines="Horizontal"
        AutoGenerateColumns="false"
        AllowSorting="true" OnSorting="GridView_Sorting"
        AllowPaging="true" PageSize="5" OnPageIndexChanging="GridView_PageIndexChanging"
        OnRowCommand="GridView_RowCommand"
        OnRowDataBound="GridView_RowDataBound"
        DataKeyNames="transaction_id, employee_id, supervisor_id, hr_manager_id"
        runat="server">

        <Columns>           
            <asp:BoundField HeaderText="Date Submitted" DataField="date_submitted" SortExpression="date_submitted" DataFormatString="{0:d/MM/yyyy h:mm tt}" />
            <asp:BoundField HeaderText="Supervisor" DataField="supervisor_name" SortExpression="supervisor_name" />
            <asp:BoundField HeaderText="Employee" DataField="employee_name" SortExpression="employee_name" />
            <asp:BoundField HeaderText="Start Date" DataField="start_date" SortExpression="start_date" DataFormatString="{0:d/MM/yyyy}" />
            <asp:BoundField HeaderText="End Date" DataField="end_date" SortExpression="end_date" DataFormatString="{0:d/MM/yyyy}" />
            <asp:BoundField HeaderText="Days" DataField="days_taken" SortExpression="days_taken" />
            <asp:BoundField HeaderText="Leave Type" DataField="leave_type" SortExpression="leave_type" />
            <asp:BoundField HeaderText="Status" DataField="status" SortExpression="status" />
            <asp:BoundField HeaderText="Qualified" DataField="qualified" SortExpression="qualified" />
            <%--action buttons--%>
            <asp:TemplateField HeaderText="Action" Visible="true">
                <ItemTemplate>
                    <%--details button--%>
                    <asp:LinkButton ID="btnDetails" runat="server" CssClass="btn btn-primary content-tooltipped" data-toggle="tooltip" data-placement="top" title="Open Details"
                        CommandName="details"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>">
                    <i class="fa fa-external-link" aria-hidden="true"></i>
                    </asp:LinkButton>

                    <%--employee buttons--%>
                    <asp:LinkButton ID="btnCancelLeave" CssClass="btn btn-danger content-tooltipped" data-toggle="tooltip" data-placement="top" title="Delete Leave Request" Visible="false" runat="server"
                        OnClientClick="return confirm('Delete leave application?');"
                        CommandName="cancelLeave"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>">
                    <i class="fa fa-trash-o" aria-hidden="true"></i>
                    </asp:LinkButton>


                    <%--supervisor buttons--%>
                    <asp:LinkButton ID="btnNotRecommended" CssClass="btn btn-danger content-tooltipped" data-toggle="tooltip" data-placement="top" title="Not Recommended" Visible="false" runat="server"
                        CommandName="notRecommended"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>">
                    <i class="fa fa-times" aria-hidden="true"></i>
                    </asp:LinkButton>

                    <asp:LinkButton ID="btnRecommended" CssClass="btn btn-success content-tooltipped" data-toggle="tooltip" data-placement="top" title="Recommended" Visible="false" runat="server"
                        CommandName="recommended"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>">
                    <i class="fa fa-check" aria-hidden="true"></i>
                    </asp:LinkButton>

                    <%--hr buttons--%>
                    <asp:LinkButton ID="btnNotApproved" CssClass="btn btn-danger content-tooltipped" data-toggle="tooltip" data-placement="top" title="Not Approved"  Visible="false" runat="server"
                        CommandName="notApproved"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>">
                    <i class="fa fa-times" aria-hidden="true"></i>
                    </asp:LinkButton>

                    <asp:LinkButton ID="btnApproved" CssClass="btn btn-success content-tooltipped" data-toggle="tooltip" data-placement="top" title="Approved"  Visible="false" runat="server"
                        CommandName="approved"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>">
                    <i class="fa fa-check" aria-hidden="true"></i>
                    </asp:LinkButton>

                    <asp:LinkButton ID="btnEditLeaveRequest"  CssClass="btn btn-warning content-tooltipped" data-toggle="tooltip" data-placement="top" title="Edit Leave Request"  Visible="false" runat="server"
                        CommandName="editLeaveRequest"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>">
                    <i class="fa fa-pencil" aria-hidden="true"></i>
                    </asp:LinkButton>

                    <asp:LinkButton ID="btnUndoApprove" CssClass="btn btn-warning content-tooltipped" data-toggle="tooltip" data-placement="top" title="Undo Approve"  Visible="false" runat="server"
                        CommandName="undoApprove"
                        CommandArgument="<%# ((GridViewRow) Container).RowIndex %>">
                    <i class="fa fa-undo" aria-hidden="true"></i>
                    </asp:LinkButton>
                </ItemTemplate>

            </asp:TemplateField>

            <asp:BoundField HeaderText="isLAfromActiveUser" DataField="isLAfromActiveUser" SortExpression="isLAfromActiveUser" /> 

        </Columns>

    </asp:GridView>

    <%--Modal to enter comments when Not Recommending or Not Approving a LA--%>
    <div class="modal fade" id="commentsModal" tabindex="-1" role="dialog" aria-labelledby="commentsModalTitle" aria-hidden="true">

        <div class="modal-dialog" role="document" style="width: 50%;">

            <div class="modal-content">

                <div class="modal-header text-center">

                    <h2 class="modal-title" id="commentsModalTitle" style="display: inline; width: 150px;">Comments</h2>

                    <button type="button" class="close" runat="server" onserverclick="closeCommentModal_Click" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>

                <asp:UpdatePanel ID="commentsUpdatePanel" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>

                        <div class="modal-body text-center">

                            <asp:HiddenField ID="previousCommentsHiddenField" runat="server" />
                            <asp:HiddenField ID="commentModalTransactionIdHiddenField" runat="server" />
                            <asp:HiddenField ID="commentModalEmployeeIdHiddenField" runat="server" />

                            <asp:Label ID="Label1" runat="server" Text="Enter comment:" Font-Size="Medium"></asp:Label>

                            <textarea id="commentsTxtArea" runat="server" cols="20" rows="4" class="form-control" style="width: 45%; margin: 0 auto; font-size: 1.05em;"></textarea>

                            <%--Error submitting email--%>
                            <asp:Panel ID="errorSubmittingEmailMsgPanel" runat="server" Style="display: inline-block; margin-top: 30px;" role="alert" Visible="false">
                                <span class="alert alert-danger">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span id="Span0">Error sending email notification</span>
                                </span>
                            </asp:Panel>

                            <%--No Comment added--%>
                            <asp:Panel ID="noEditsMadePanel" runat="server" Style="display: inline-block; margin-top: 30px;" role="alert" Visible="false">
                                <span class="alert alert-info">
                                    <i class="fa fa-info-circle" aria-hidden="true"></i>
                                    <span id="Span1">No changes made to comments</span>
                                </span>
                            </asp:Panel>

                            <%--Successful addition of comment--%>
                            <asp:Panel ID="addCommentSuccessPanel" runat="server" Style="display: inline-block; margin-top: 30px;" Visible="false" role="alert">
                                <span class="alert alert-success">
                                    <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                                    <span id="Span9">Comments successfully added to leave application</span>
                                </span>
                            </asp:Panel>

                            <%--Successful edit of comment--%>
                            <asp:Panel ID="editCommentSuccessPanel" runat="server" Style="display: inline-block; margin-top: 30px;" Visible="false" role="alert">
                                <span class="alert alert-success">
                                    <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                                    <span id="Span2">Comments successfully edited</span>
                                </span>
                            </asp:Panel>

                        </div>

                        <div class="modal-footer">

                            <asp:LinkButton runat="server" ID="submitCommentBtn" class="btn btn-primary" CausesValidation="false" OnClick="submitCommentBtn_Click">
                                        <i class="fa fa-send" aria-hidden="true"></i>
                                        Submit
                            </asp:LinkButton>

                            <asp:Button ID="closeCommentModal" runat="server" Text="Close" CssClass="btn btn-secondary" OnClick="closeCommentModal_Click" />
                        
                        </div>

                    </ContentTemplate>

                </asp:UpdatePanel>

            </div>

        </div>

    </div>

</div>
