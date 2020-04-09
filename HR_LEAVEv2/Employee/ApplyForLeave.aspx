<%@ Page Title="Apply for Leave" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ApplyForLeave.aspx.cs" Inherits="HR_LEAVEv2.Employee.ApplyForLeave" MaintainScrollPositionOnPostback="true" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ Register Src="~/UserControls/supervisorSelect.ascx" TagName="SupervisorSelectUserControl" TagPrefix="TWebControl"%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>

        #applyForLeaveContainer div.form-group{
            margin-top:10px;
        }

        .spaced-checkboxes input{
            margin-right:5px;
        }
    </style>
    
    <%--View mode, Edit mode: Allows user to return to last page they were on before --%>
    <asp:LinkButton ID="returnToPreviousBtn" runat="server" CssClass="btn btn-primary content-tooltipped" data-toggle="tooltip" data-placement="right" title="Return to previous page" OnClick="returnToPreviousBtn_Click">
        <i class="fa fa-arrow-left" aria-hidden="true"></i>
    </asp:LinkButton>

    <%--Apply mode--%>
    <asp:Panel ID="applyModeTitle" runat="server">
        <h1 style="margin: 0 auto;"><%: Title %></h1>
    </asp:Panel>

    <%--View mode--%>
    <asp:Panel ID="viewModeTitle" runat="server">
        <h1>View Leave Application</h1>
    </asp:Panel>

    <%--Edit mode--%>
    <asp:Panel ID="editModeTitle" runat="server">
        <h1>Edit Leave Application</h1>
    </asp:Panel>

    <%--Edit mode: id of employee used to fill audit log column, "affected_employee_id"--%>
    <asp:HiddenField ID="empIdHiddenTxt" runat="server"></asp:HiddenField>

    <%--View mode, Edit mode: shows name of employee who submitted the leave application--%>
    <asp:Panel ID="empNamePanel" runat="server">
        <h2 id="empNameHeader" runat="server"></h2>
    </asp:Panel>
    <div id="applyForLeaveContainer" class="container-fluid text-center">
        <%--View mode: Shows date and time leave application was submitted--%>
        <asp:Panel ID="submittedOnPanel" CssClass="row" runat="server">
            <asp:Label ID="submittedOnTxt" runat="server"></asp:Label>
        </asp:Panel>
        <%--View mode: Shows status of leave application--%> 
        <asp:Panel ID="statusPanel" Style="display:block;margin-top:35px; margin-bottom:10px;" runat="server">
            <label for="statustxt" style="font-size:1.2em">Status:</label>
            <asp:Label ID="statusTxt" runat="server" Style="display:inline; margin:0 auto; text-align:center; font-size:1.05em;"></asp:Label> 
        </asp:Panel>

         <%--Apply mode: Shows the current leave balance of employee --%>
        <asp:Panel ID="leaveCountPanel" runat="server" CssClass="row" Style="margin-top:15px;height:225px;" >
            <asp:UpdatePanel ID="UpdatePanelLeaveCount" runat="server">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="LeaveCountUserControl" />
                </Triggers>
                <ContentTemplate>
                    <TWebControl:LeaveCountUserControlBS4 ID="LeaveCountUserControl" runat="server"></TWebControl:LeaveCountUserControlBS4>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:Panel>

        <%--View mode: Shows the start and end date of leave application--%>
        <%--Apply mode: Allows user to enter start and end date of application--%>
        <div class="row">

            <%--Start Date--%>
            <div style="display:inline-block; margin-right:15%;">
                <label for="txtFrom" style="font-size:1.2em">From:</label>

                <asp:Panel ID="startDateInfoPanel" runat="server" Style="display:inline;">
                    <asp:Label ID="startDateInfoTxt" runat="server" Style="display:inline;font-size:1.05em"></asp:Label> 
                </asp:Panel>

                <asp:Panel ID="startDateApplyPanel" runat="server">
                    <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" Style="width: 150px; height: auto; display: inline;" AutoPostBack="true" OnTextChanged="datesEntered"></asp:TextBox>
                    <i id="fromCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                    <ajaxToolkit:CalendarExtender ID="fromCalendarExtender" TargetControlID="txtFrom" PopupButtonID="fromCalendar" runat="server" Format="d/MM/yyyy"></ajaxToolkit:CalendarExtender>
                </asp:Panel>
                
            </div>
            <%--End Date--%>
            <div style="display: inline-block;">
                <label for="txtTo" style="font-size: 1.2em">To:</label>

                <asp:Panel ID="endDateInfoPanel" runat="server" Style="display:inline;">
                    <asp:Label ID="endDateInfoTxt" runat="server" Style="display: inline;font-size:1.05em"></asp:Label>
                </asp:Panel>

                <asp:Panel ID="endDateApplyPanel" runat="server">
                    <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" Style="width: 150px; height: auto; display: inline;" AutoPostBack="true" OnTextChanged="datesEntered"></asp:TextBox>
                    <i id="toCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                    <ajaxToolkit:CalendarExtender ID="toCalendarExtender" TargetControlID="txtTo" PopupButtonID="toCalendar" runat="server" Format="d/MM/yyyy" />
                </asp:Panel>

            </div>
        </div>

        <%--Apply mode, View mode: Shows the number of days being applied for--%>
        <%--Edit mode: allows a HR 1 or HR 2 to edit the number of days applied for--%> 
        <asp:Panel ID="numDaysAppliedForPanel" CssClass="row form-group" runat="server">

            <%--apply mode, view mode--%>
            <label for="numDaysAppliedFor" style="font-size: 1.2em">Days applied for:</label>

            <asp:Label ID="numDaysAppliedFor" runat="server" Text="0" Style="font-size: 1.05em"></asp:Label>

            <%--edit mode--%>
            <asp:TextBox ID="numDaysAppliedForEditTxt" runat="server" Width ="35px" Visible="false" ></asp:TextBox>
            <asp:RegularExpressionValidator ValidationGroup="editGroup"
                ID="RegularExpressionValidator11" runat="server"
                ControlToValidate="numDaysAppliedForEditTxt"
                ErrorMessage="Please enter a valid number" ForeColor="Red"
                ValidationExpression="^[0-9]*$"
                Display="Dynamic">  
            </asp:RegularExpressionValidator>

        </asp:Panel>

         <%--View mode: Shows if leave application is qualified--%> 
        <asp:Panel ID="qualifiedPanel" Style="display:block;" runat="server">
            <label for="qualifiedTxt" style="font-size:1.2em">Qualified for Leave:</label>
            <asp:Label ID="qualifiedTxt" runat="server" Style="display:inline; margin:0 auto; text-align:center;font-size:1.05em;"></asp:Label> 
        </asp:Panel>

        <%--View mode: Shows the type of leave applied for--%>
        <%--Apply mode: Allows user to enter type of leave to apply for--%>
        <div class="row form-group" >
            <label for="typeOfLeave" style="font-size:1.2em;display:inline;">Type:</label>

            <%--apply mode--%>
            <asp:Panel ID="typeOfLeaveDropdownPanel" runat="server" Style="display:inline;">
                <asp:DropDownList ID="typeOfLeave" runat="server" CssClass="form-control" Width="150px" Height="35px" Style="display: inline;" AutoPostBack ="true" OnSelectedIndexChanged="typeOfLeave_SelectedIndexChanged">
                </asp:DropDownList>
            </asp:Panel>

            <%--view mode, edit mode--%>
            <asp:Panel ID="typeOfLeavePanel" runat="server" Style="display:inline;">
                <asp:Label ID="typeOfLeaveTxt" runat="server" Style="text-align:center;font-size:1.05em;"></asp:Label> 
            </asp:Panel>
        </div>

        <%--View mode: Shows the supervisor the leave application was submitted to--%>
        <%--Apply mode: Allows user to enter supervisor to send leave application to--%>
        <div class="row form-group">

            <label for="supervisor_select" style="font-size:1.2em; display:inline;">Supervisor:</label>

            <%--apply mode--%>
            <asp:Panel ID="supervisorSelectUserControlPanel" runat="server" Style="display: inline;">
                <ajaxToolkit:ComboBox ID="supervisorSelect"
                    runat="server" AutoPostBack="true" DropDownStyle="DropDownList"
                    AutoCompleteMode="SuggestAppend"
                    DataSourceID="supervisorDataSource"
                    DataTextField="Supervisor Name"
                    DataValueField="employee_id"
                    MaxLength="0" Height="27px"
                    OnSelectedIndexChanged="ComboBox1_SelectedIndexChanged">
                </ajaxToolkit:ComboBox>
                <asp:SqlDataSource ID="supervisorDataSource"
                    runat="server"
                    ConnectionString="<%$ ConnectionStrings:dbConnectionString %>"
                    ProviderName="System.Data.SqlClient"
                    SelectCommand="getSupervisors"
                    SelectCommandType="StoredProcedure"></asp:SqlDataSource>
            </asp:Panel>

            <%--view mode--%>
            <asp:Panel ID="supervisorPanel" runat="server"  Style="display:inline;">
                <asp:Label ID="supervisorNameTxt" runat="server" Style="text-align:center;font-size:1.05em;"></asp:Label>
            </asp:Panel>

        </div>

        <%--Apply mode: Allows user to upload docs--%>
        <asp:Panel ID="fileUploadPanel" runat="server" Style="margin:0 auto; text-align:center" CssClass="row form-group">

            <label for="FileUpload1" style="font-size:1.2em; display:inline;">Upload Files:</label>

            <asp:FileUpload ID="FileUpload1" runat="server" Width="475px" Style="margin:0 auto; display: inline-block; background-color: lightgrey" AllowMultiple="true"/> 
            
            <asp:LinkButton ID="uploadFilesBtn" runat="server" OnClick="uploadBtn_Click" CssClass="btn btn-sm btn-primary content-tooltipped" data-toggle="tooltip" data-placement="top" title="Upload files">
                <i class="fa fa-upload" aria-hidden="true"></i>
            </asp:LinkButton>

            <asp:LinkButton ID="clearAllFilesBtn" runat="server" OnClick="clearUploadedFiles_Click" OnClientClick="return confirm('Clear all files?');" CssClass="btn btn-sm btn-danger content-tooltipped" data-toggle="tooltip" data-placement="top" title="Clear all uploaded files">
                <i class="fa fa-times" aria-hidden="true"></i>
            </asp:LinkButton>

            <br />

            <%--Shows the files uploaded to Session--%>
            <asp:Panel ID="filesUploadedPanel" runat="server" Style="text-align: left; margin: 0 auto; display: inline-block;">
                Files Uploaded:
                <asp:ListView ID="filesUploadedListView" runat="server" GroupItemCount="10">
                    <GroupTemplate>
                        <div id="itemPlaceholderContainer" runat="server">
                            <div id="itemPlaceholder" runat="server"></div>
                        </div>
                    </GroupTemplate>

                    <ItemTemplate>
                        <div style="margin-top:5px;">
                            <span>
                                <asp:LinkButton ID="clearIndividualFileBtn" OnClick="clearIndividualFileBtn_Click" data-id='<%#Eval("file_name") %>' runat="server" CssClass="btn btn-danger btn-sm content-tooltipped" data-toggle="tooltip" data-placement="left" title="Clear file">
                                    <i class="fa fa-times" aria-hidden="true"></i>
                                </asp:LinkButton>
                            </span>
                            <span> <%#Eval("file_name") %></span>
                        </div>
                    </ItemTemplate>

                    <AlternatingItemTemplate>
                        <div style="margin-top:5px;">
                            <span>
                                <asp:LinkButton ID="clearIndividualFileBtn" OnClick="clearIndividualFileBtn_Click" data-id='<%#Eval("file_name") %>' runat="server" CssClass="btn btn-danger btn-sm content-tooltipped" data-toggle="tooltip" data-placement="left" title="Clear file">
                                    <i class="fa fa-times" aria-hidden="true"></i>
                                </asp:LinkButton>
                            </span>
                            <span> <%#Eval("file_name") %></span>
                        </div>
                    </AlternatingItemTemplate>


                    <LayoutTemplate>
                        <div id="groupPlaceholderContainer" runat="server">
                            <div id="groupPlaceholder" runat="server"></div>
                        </div>
                    </LayoutTemplate>

                </asp:ListView>
            </asp:Panel>

            <br />       
                                 
        </asp:Panel>

        <%--View mode, Edit mode: shows a downloadable list of all the files associated with a LA--%>
        <asp:Panel ID="filesToDownloadPanel" runat="server">

            <label for="filesToDownloadList" style="font-size:1.2em; display:inline;">Previously uploaded files:</label>

            <asp:DropDownList ID="filesToDownloadList" runat="server"></asp:DropDownList>

            <asp:LinkButton ID="btnDownloadFiles" runat="server" CssClass="btn btn-primary content-tooltipped" data-toggle="tooltip" data-placement="right" title="Download file" OnClick="btnDownloadFiles_Click" Style="display:inline-block; margin-left:5px;">
                <i class="fa fa-download" aria-hidden="true"></i>
            </asp:LinkButton>

        </asp:Panel>
        

        <%--View mode: Shows the comments made by employee when submitting the leave application --%>
        <%--Apply mode: Allows employee to enter comments to explain anything necessary as to why they need the leave etc.--%>
        <asp:Panel ID="empCommentsPanel" runat="server" CssClass="row form-group">
            <label for="empCommentsTxt" style="font-size:1.2em">Employee Comments</label>
            <textarea runat="server" class="form-control" id="empCommentsTxt" rows="4" style="width:45%; margin:0 auto; font-size:1.05em;"></textarea>
        </asp:Panel>

        <%--View mode: Shows the comments made by supervisor as to why they recommended or did not recommend the leave application --%>
        <asp:Panel ID="supCommentsPanel" runat="server" CssClass="row form-group">
            <label for="supCommentsTxt" style="font-size:1.2em">Supervisor Comments</label>
            <textarea runat="server" class="form-control" id="supCommentsTxt" rows="4" style="width:45%; margin:0 auto;font-size:1.05em;"></textarea>
        </asp:Panel>

        <%--View mode: Shows the comments made by hr as to why they approved or did not approve the leave application --%>
        <asp:Panel ID="hrCommentsPanel" runat="server" CssClass="row form-group">
            <label for="hrCommentsTxt" style="font-size:1.2em">HR Comments</label>
            <textarea runat="server" class="form-control" id="hrCommentsTxt" rows="4" style="width:45%; margin:0 auto;font-size:1.05em;"></textarea>
        </asp:Panel>

        <%--Shows info relevant to the employee but which does not stop them from completing a LA--%>
        <div class="row" id="infoRow" style="margin-bottom:5px;">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>

                    <asp:Panel ID="daysTakenDisclaimerPanel" runat="server" CssClass="row alert alert-info" Style="display:none; margin:0px 5px; width:450px;" role="alert">
                        <i class="fa fa-info-circle" aria-hidden="true"></i>
                        <span id="Span10" runat="server">Disclaimer: days applied for may not be completely accurate and is subject to change. Consult HR for further information</span>
                    </asp:Panel>

                    <asp:Panel ID="holidayInAppliedTimePeriodPanel" runat="server" CssClass="row alert alert-info" Style="display: none; margin:0px 5px;width:450px;" role="alert">
                        <i class="fa fa-info-circle" aria-hidden="true"></i>
                        <span id="holidayInAppliedTimeTxt" runat="server"></span>
                    </asp:Panel>

                    <asp:Panel ID="submitHardCopyOfMedicalDisclaimerPanel" runat="server" CssClass="row alert alert-info" Style="display: none; margin:0px 5px;width:450px;" role="alert">
                        <i class="fa fa-info-circle" aria-hidden="true"></i>
                        <span id="Span13" runat="server">A hard copy of your medical leave must also be submitted to HR</span>
                    </asp:Panel>

                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <%--Apply mode: Shows any necessary validation messages to user --%>
        <div class="row" id="validationRow">
            <asp:UpdatePanel ID="applyModeFeedbackUpdatePanel" runat="server">
                <ContentTemplate>
                    <asp:Panel ID="startDateIsHoliday" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="startDateIsHolidayTxt" runat="server"></span>
                    </asp:Panel>

                    <asp:Panel ID="endDateIsHoliday" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="endDateIsHolidayTxt" runat="server"></span>
                    </asp:Panel>

                    <asp:Panel ID="startDateIsWeekend" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span4" runat="server">Start date is on the weekend</span>
                    </asp:Panel>

                    <asp:Panel ID="endDateIsWeekend" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span5" runat="server">End date is on the weekend</span>
                    </asp:Panel>

                    <asp:Panel ID="invalidStartDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="invalidStartDateValidationMsg" runat="server">Start date is not valid</span>
                    </asp:Panel>

                    <asp:Panel ID="startDateBeforeTodayValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="startDateBeforeTodayValidationMsg" runat="server">Start date cannot be before today</span>
                    </asp:Panel>

                    <asp:Panel ID="invalidEndDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="invalidEndDateValidationMsg" runat="server">End date is not valid</span>
                    </asp:Panel>

                    <asp:Panel ID="dateComparisonValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="dateComparisonValidationMsg" runat="server">End date cannot precede start date</span>
                    </asp:Panel>

                    <asp:Panel ID="fileUploadNeededPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span15" runat="server">No files currently uploaded. Upload supporting documentation to submit your application</span>
                    </asp:Panel>

                    <asp:Panel ID="moreThan2DaysConsecutiveSickLeave" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span6" runat="server">No files currently uploaded. More than 2 days of consecutive sick leave requires a medical leave of absence.</span>
                    </asp:Panel>

                    <asp:Panel ID="invalidVacationStartDateMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 5px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="invalidVacationStartDateMsg" runat="server">You must request vacation leave at least a month before the start date</span>
                    </asp:Panel>

                    <asp:Panel ID="invalidSickLeaveStartDate" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span1" runat="server">Sick leave cannot be taken in advance</span>
                    </asp:Panel>

                    <asp:Panel ID="invalidLeaveTypePanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="invalidLeaveTypeTxt" runat="server"></span>
                    </asp:Panel>

                    <asp:Panel ID="invalidFileTypePanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 5px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="invalidFileTypeErrorTxt" runat="server">Invalid file type</span>
                    </asp:Panel>

                    <asp:Panel ID="fileUploadedTooLargePanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="fileUploadTooLargeTxt" runat="server">File upload too large</span>
                    </asp:Panel>

                    <asp:Panel ID="invalidSupervisor" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 5px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span3" runat="server">Could not verify supervisor</span>
                    </asp:Panel>

                    <asp:Panel ID="errorInsertingFilesToDbPanel" runat="server" CssClass="row alert alert-danger" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span7" runat="server">Error inserting files into database</span>
                    </asp:Panel>

                    <asp:Panel ID="errorSendingEmailNotifications" runat="server" CssClass="row alert alert-danger" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span11" runat="server">Error sending email notifications</span>
                    </asp:Panel>

                    <asp:Panel ID="errorSendingInHouseNotifications" runat="server" CssClass="row alert alert-danger" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span12" runat="server">Error sending in-application notifications</span>
                    </asp:Panel>

                    <asp:Panel ID="errorSubmittingLeaveApplicationPanel" runat="server" CssClass="row alert alert-danger" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span8" runat="server">Error submitting leave application</span>
                    </asp:Panel>

                    <asp:Panel ID="successMsgPanel" runat="server" CssClass="row alert alert-success" Style="display: none; margin: 0 5px;" role="alert">
                        <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                        <span id="successMsg" runat="server">Application successfully submitted</span>
                        <asp:Button ID="submitAnotherLA" runat="server" Text="Submit another" CssClass="btn btn-success" Style="display: inline; margin-left: 10px" OnClick="refreshForm" />
                    </asp:Panel>

                    <asp:Panel ID="submitButtonPanel" runat="server" CssClass="row form-group">
                        <asp:LinkButton ID="cancelBtn" runat="server" Text="Cancel" Style="margin-right: 35px;" CssClass="btn btn-danger" OnClick="refreshForm" CausesValidation="False">
                                 <i class="fa fa-times" aria-hidden="true"></i>
                                 Cancel
                        </asp:LinkButton>

                        <asp:LinkButton ID="submitBtn" runat="server" Text="Submit" CssClass="btn btn-success" OnClick="submitLeaveApplication_Click">
                                 <i class="fa fa-send" aria-hidden="true"></i>
                                 Submit
                        </asp:LinkButton>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <%--Edit mode: contains submit button and validaation messages for edits made--%>
        <asp:Panel ID="submitEditsPanel" runat="server" CssClass="row" Style="margin-top: 15px;">
            <asp:LinkButton ID="submitEditsBtn" runat="server" CssClass="btn btn-success" OnClick="submitEditsBtn_Click" ValidationGroup="editGroup">
                <i class="fa fa-floppy-o" aria-hidden="true"></i>
                Save edit(s)
            </asp:LinkButton>

            <asp:Panel ID="noEditsMadePanel" runat="server" CssClass="row alert alert-info" Style="display: inline-block;" role="alert" Visible="false">
                <i class="fa fa-info-circle" aria-hidden="true"></i>
                <span id="Span9" runat="server">No edits made</span>
                <asp:Button ID="Button2" runat="server" Text="Go back" CssClass="btn btn-primary" Style="margin-left:3px;" OnClick="returnToPreviousBtn_Click"/>
            </asp:Panel>

            <asp:Panel ID="successfulSubmitEditsMsgPanel" runat="server" CssClass="row alert alert-success" Style="display: inline-block;" role="alert" Visible="false">
                <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                <span id="Span2" runat="server">Edits successfully made</span>
                <asp:Button ID="Button1" runat="server" Text="Go back" CssClass="btn btn-primary" Style="margin-left:3px;" OnClick="returnToPreviousBtn_Click"/>
            </asp:Panel>

            <asp:Panel ID="errorEditingApplicationPanel" runat="server" CssClass="row alert alert-danger" Style="display: inline-block;" role="alert" Visible="false">
                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                <span id="Span14" runat="server">Error editing application</span>
                <asp:Button ID="Button3" runat="server" Text="Go back" CssClass="btn btn-primary" Style="margin-left:3px;" OnClick="returnToPreviousBtn_Click"/>
            </asp:Panel>

        </asp:Panel>

    </div>
</asp:Content>


