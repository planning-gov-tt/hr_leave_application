<%@ Page Title="Leave Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="LeaveSettings.aspx.cs" Inherits="HR_LEAVEv2.HR.LeaveSettings" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .space-label-from-input-element{
            margin-right:5px;
        }

        #currentAccSettingsDiv th, td {
            text-align: center;
        }
    </style>

    <h1><%: Title %></h1>

    <div class="container" style="text-align:center;">
        <div id="currentAccSettingsDiv" class="row">

            <asp:UpdatePanel ID="UpdatePanel" runat="server">

                <ContentTemplate>

                    <%--Panel showing gridview containing accumulations data--%>
                    <asp:Panel ID="accExistsPanel" runat="server">

                        <asp:LinkButton ID="addNewAccBtn" runat="server" CssClass="btn btn-primary" OnClick="addAccumulationBtn_Click" Style="margin-top: 15px; margin-bottom: 15px;">
                            <i class="fa fa-plus"></i>
                            Add new Accumulation
                        </asp:LinkButton>

                        <asp:GridView ID="GridView1"
                            runat="server"
                            BorderStyle="None" CssClass="table" Style="margin: 0 auto;"
                            GridLines="Horizontal"
                            AllowSorting="true" AllowPaging="true"
                            PageSize="5" OnPageIndexChanging="GridView1_PageIndexChanging" AutoGenerateColumns="False"
                            OnRowDeleting="GridView1_RowDeleting">

                            <Columns>

                                <asp:TemplateField HeaderText="Action" Visible="true">
                                    <ItemTemplate>
                                        <%--delete button--%>
                                        <asp:LinkButton ID="deleteBtn"
                                            runat="server"
                                            CssClass="btn btn-danger content-tooltipped"
                                            data-toggle="tooltip"
                                            data-placement="bottom"
                                            title="Delete accumulation"
                                            OnClientClick="return confirm('Delete accumulation?');"
                                            CommandName="delete"
                                            CausesValidation="false">
                                        <i class="fa fa-trash-o" aria-hidden="true"></i>
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="id" HeaderText="ID" SortExpression="id" HeaderStyle-CssClass="hidden" ItemStyle-CssClass="hidden"></asp:BoundField>
                                <asp:BoundField DataField="employment_type" HeaderText="Employment Type" SortExpression="employment_type"></asp:BoundField>
                                <asp:BoundField DataField="leave_type" HeaderText="Leave Type" SortExpression="leave_type"></asp:BoundField>
                                <asp:BoundField DataField="accumulation_period_text" HeaderText="Accumulation Period" SortExpression="accumulation_period_text"></asp:BoundField>
                                <asp:BoundField DataField="accumulation_type" HeaderText="Accumulation Type" SortExpression="accumulation_type"></asp:BoundField>
                                <asp:BoundField DataField="num_days" HeaderText="Number of Days" SortExpression="num_days"></asp:BoundField>
                            
                            </Columns>

                        </asp:GridView>

                    </asp:Panel>

                    <%--Panel showing message if no accumulations data is available--%>
                    <asp:Panel ID="noAccumulationsPanel" runat="server" CssClass="alert alert-info text-center" role="alert" Style="display: inline-block; margin: 0 auto; margin-top:25px" Visible="false">
                        
                        <i class="fa fa-info-circle"></i>
                        No data on accumulations available
                        <asp:LinkButton ID="addAccumulationBtn" runat="server" CssClass="btn btn-primary" OnClick="addAccumulationBtn_Click" Style="margin-left: 10px;">
                             <i class="fa fa-plus"></i>
                            Add
                        </asp:LinkButton>

                    </asp:Panel>

                </ContentTemplate>

            </asp:UpdatePanel>

        </div>

       

        <%-- Add new Accumulation Modal--%>
        <div class="modal fade" id="addAccumulationModal" tabindex="-1" role="dialog" aria-labelledby="addAccumulationTitle" aria-hidden="true">

            <div class="modal-dialog" role="document" style="width: 50%;">

                <div class="modal-content">

                    <div class="modal-header text-center">

                        <h2 class="modal-title" id="addAccumulationTitle" style="display: inline; width: 150px;">
                            Adjust Settings for Accumulation of Leave on a Yearly Basis
                        </h2>

                        <button type="button" class="close" runat="server" onserverclick="closeAddAccumulationBtn_Click" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>

                    </div>

                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">

                        <Triggers>
                            <asp:PostBackTrigger ControlID="saveAccumulation" />
                        </Triggers>

                        <ContentTemplate>

                            <div class="modal-body text-center">

                                <div class="row" style="margin-bottom: 20px">
                                    <%--Employment Type--%>
                                    <label for="empTypeList" class="space-label-from-input-element">Employment Type:</label>
                                    <asp:DropDownList ID="empTypeList" runat="server" DataSourceID="empTypeDataSource" DataTextField="type_id" AutoPostBack="true" ValidationGroup="submitAccumulation"></asp:DropDownList>
                                    <asp:SqlDataSource ID="empTypeDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" ProviderName="System.Data.SqlClient" SelectCommand="SELECT [type_id] FROM [employmenttype] ORDER BY [type_id]"></asp:SqlDataSource>
                                </div>

                                <div class="row" style="margin-bottom: 20px">
                                    <%-- Leave Type--%>
                                    <label for="leaveTypeList" class="space-label-from-input-element">Leave Type:</label>
                                    <asp:DropDownList ID="leaveTypeList" runat="server" ValidationGroup="submitAccumulation"></asp:DropDownList>
                                </div>

                                <div class="row" style="margin-bottom: 20px">
                                    <%--Accumulation Period--%>
                                    <label for="accumulationPeriodList" class="space-label-from-input-element">Accumulation Period:</label>
                                    <asp:DropDownList ID="accumulationPeriodList" runat="server" ValidationGroup="submitAccumulation"></asp:DropDownList>
                                </div>

                                <div class="row" style="margin-bottom: 20px">
                                    <%--Type of Accumulation Period--%>
                                    <label for="accumulationTypeList" class="space-label-from-input-element">What to do with leave balance:</label>
                                    <asp:DropDownList ID="accumulationTypeList" runat="server" ValidationGroup="submitAccumulation">
                                        <asp:ListItem>Add</asp:ListItem>
                                        <asp:ListItem>Replace</asp:ListItem>
                                    </asp:DropDownList>
                                </div>

                                <div class="row">
                                    <%--Num Days--%>
                                    <label for="numDaysForAccumulation" class="space-label-from-input-element">Number of days:</label>
                                    <asp:TextBox ID="numDaysForAccumulation" runat="server" Width="100px"></asp:TextBox>
                                    <asp:RegularExpressionValidator ValidationGroup="submitAccumulation" ID="RegularExpressionValidator7" runat="server"
                                        ControlToValidate="numDaysForAccumulation" ErrorMessage="Enter valid number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic">  
                                    </asp:RegularExpressionValidator>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ValidationGroup="submitAccumulation" ControlToValidate="numDaysForAccumulation" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>
                                </div>

                            </div>

                            <div class="modal-footer">

                                <asp:Button ID="closeAddAccumulationBtn" runat="server" Text="Close" CssClass="btn btn-secondary" OnClick="closeAddAccumulationBtn_Click" />
                                <asp:LinkButton ID="saveAccumulation" runat="server" OnClick="saveAccumulation_Click" ValidationGroup="submitAccumulation" CssClass="btn btn-primary">
                                    <i class="fa fa-floppy-o"></i>
                                    Save
                                </asp:LinkButton>

                            </div>

                        </ContentTemplate>

                    </asp:UpdatePanel>

                </div>

            </div>

        </div>

    </div>
</asp:Content>
