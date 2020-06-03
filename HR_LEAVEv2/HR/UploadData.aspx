<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UploadData.aspx.cs" Inherits="HR_LEAVEv2.HR.UploadData" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        #deleteSuccessfulMsgCancelBtn{
            cursor:pointer;
            outline:none;
        }
    </style>
    <h1><%: Title %></h1>
    <div class="container-fluid" style="margin-top:35px;">
        <asp:Panel ID="fileUploadPanel" runat="server" Style="margin: 0 auto; text-align: center" CssClass="row">
            <label for="FileUpload1" style="font-size: 1.2em; display: inline;">Upload Files:</label>
            <asp:FileUpload ID="FileUpload1" runat="server" Width="475px" Style="margin: 0 auto; display: inline-block; background-color: lightgrey" AllowMultiple="false" />
            <asp:LinkButton ID="uploadFilesBtn" runat="server" OnClick="uploadFilesBtn_Click" CssClass="btn btn-sm btn-primary content-tooltipped" data-toggle="tooltip" data-placement="top" title="Upload file">
                <i class="fa fa-upload" aria-hidden="true"></i>
            </asp:LinkButton>
            <asp:LinkButton ID="clearFileBtn" runat="server" OnClick="clearFileBtn_Click" OnClientClick="return confirm('Clear uploaded file?');" CssClass="btn btn-sm btn-danger content-tooltipped" data-toggle="tooltip" data-placement="top" title="Clear uploaded file">
                <i class="fa fa-times" aria-hidden="true"></i>
            </asp:LinkButton>
            <br />
            <asp:Label ID="uploadedFile" runat="server" Text="" Visible ="false"></asp:Label>
        </asp:Panel>
        <asp:Panel ID="chooseTablePanel" runat="server" Style="display:flex; justify-content:center; margin-top:30px;" Visible="false">
<%--            <asp:Label ID="Label1" runat="server" Text="Choose a table to upload data to:"></asp:Label>--%>
            <asp:DropDownList ID="tableSelectDdl" runat="server" AutoPostBack="true" OnSelectedIndexChanged="tableSelectDdl_SelectedIndexChanged">
                <asp:ListItem Value="-">Choose a Table</asp:ListItem>
                <asp:ListItem Value="assignment">Assignment</asp:ListItem>
                <asp:ListItem Value="department">Department</asp:ListItem>
                <asp:ListItem Value="employee">Employee</asp:ListItem>
                <asp:ListItem Value="employeeposition">EmployeePosition</asp:ListItem>
                <asp:ListItem Value="employmenttype">EmploymentType</asp:ListItem>
                <asp:ListItem Value="leavetype">LeaveType</asp:ListItem>
                <asp:ListItem Value="position">Position</asp:ListItem>
            </asp:DropDownList>
        </asp:Panel>
        <asp:Panel ID="uploadDataBtnPanel" runat="server" Visible=" false" Style="text-align:center; margin-top:15px;">
            <asp:LinkButton ID="uploadDataBtn" runat="server" CssClass="btn btn-primary" OnClick="uploadDataBtn_Click">
                <i class="fa fa-upload navbar-link-icon"></i>
                Upload data
            </asp:LinkButton>
        </asp:Panel>
        <asp:UpdatePanel ID="applyModeFeedbackUpdatePanel" runat="server" Style="margin-top:15px;text-align:center;">
            <ContentTemplate>
                <asp:Panel ID="invalidFileTypePanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span>Invalid file type</span>
                </asp:Panel>

                <asp:Panel ID="fileUploadedTooLargePanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span>File upload too large</span>
                </asp:Panel>

                <asp:Panel ID="noFileUploaded" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span>No file was uploaded</span>
                </asp:Panel>

                <asp:Panel ID="successfulDataInsertPanel" runat="server" CssClass="row alert alert-success" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                    <span>Data was successfully uploaded to database</span>
                    <i class="fa fa-times-circle" id="deleteSuccessfulMsgCancelBtn" style="margin-left: 11px; color: #484848;"></i>
                </asp:Panel>

                <asp:Panel ID="typeErrorPanel" runat="server" CssClass="row alert alert-danger" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="typeErrorTxt" runat="server"></span>
                </asp:Panel>

                <asp:Panel ID="maxLengthErrorPanel" runat="server" CssClass="row alert alert-danger" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="maxLengthErrorTxt" runat="server"></span>
                </asp:Panel>

                <asp:Panel ID="nullableErrorPanel" runat="server" CssClass="row alert alert-danger" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="nullableErrorTxt" runat="server"></span>
                </asp:Panel>

                <asp:Panel ID="clashingRecordsPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="clashingRecordsTxt" runat="server">Employment record being inserted clashes with another employment record</span>
                </asp:Panel>

                <asp:Panel ID="multipleActiveRecordsPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="multipleActiveRecordsTxt" runat="server">Employment record being inserted would result in multiple active records</span>
                </asp:Panel>

                <asp:Panel ID="invalidStartDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="invalidStartDateTxt" runat="server">Start date is not valid</span>
                </asp:Panel>

                <asp:Panel ID="invalidExpectedEndDatePanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="invalidExpectedEndDateTxt" runat="server">Expected end date is not valid</span>
                </asp:Panel>

                <asp:Panel ID="dateComparisonExpectedValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="dateComparisonExpectedTxt" runat="server">Expected end date cannot precede start date</span>
                </asp:Panel>

                <asp:Panel ID="dateComparisonActualValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="dateComparisionActualTxt" runat="server">Actual end date cannot precede start date</span>
                </asp:Panel>

                <asp:Panel ID="startDateIsWeekendPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="startDateIsWeekendTxt" runat="server">Start date is on the weekend</span>
                </asp:Panel>

                <asp:Panel ID="expectedEndDateIsWeekendPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="expectedEndDateIsWeekendTxt" runat="server">Expected end date is on the weekend</span>
                </asp:Panel>

                <asp:Panel ID="invalidActualEndDatePanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="invalidActualEndDateTxt" runat="server">Actual end date is not valid</span>
                </asp:Panel>

                <asp:Panel ID="actualEndDateOnWeekendPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="actualEndDateIsWeekendTxt" runat="server">Actual end date is on the weekend</span>
                </asp:Panel>

                <asp:Panel ID="wrongTablePanel" runat="server" CssClass="row alert alert-danger" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span>Table structure in Excel file does not match table structure for selected table</span>
                </asp:Panel>

                <asp:Panel ID="invalidAnnualOrMaximumVacationLeaveAmtPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="invalidAnnualOrMaximumVacationLeaveAmtTxt" runat="server"></span>
                </asp:Panel>

                <asp:Panel ID="unsuccessfulInsertPanel" runat="server" CssClass="row alert alert-danger" Style="display: none; margin: 0px 5px;" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span>Data was not uploaded to database</span>
                </asp:Panel>

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <script>
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {

            $('#deleteSuccessfulMsgCancelBtn').click(function () {
                $('#<%= successfulDataInsertPanel.ClientID %>').hide();
            });
        });
    </script>

</asp:Content>
