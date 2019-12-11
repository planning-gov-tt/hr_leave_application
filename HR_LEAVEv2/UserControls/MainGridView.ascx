<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainGridView.ascx.cs" Inherits="HR_LEAVEv2.UserControls.MainGridView" %>

<%--
    <%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<div id="filterContainer" class="container">
    <form>
        <div class="row">            
            <div class="col-xs-6 form-group">
                <label for="txtFrom">Start Date:</label>
                <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                <i id="fromCalendar" class="fa fa-calendar fa-lg"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender1" TargetControlID="txtFrom" PopupButtonID="fromCalendar" runat="server"></ajaxToolkit:CalendarExtender>
            </div>
            <div class="col-xs-6 form-group">
                <label for="txtTo">End Date:</label>
                <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                <i id="toCalendar" class="fa fa-calendar fa-lg"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender2" TargetControlID="txtTo" PopupButtonID="toCalendar" runat="server" />
            </div> 
        </div> 
        <div class="row">
            <div class="col-xs-6 form-group">
                <label for="txtFrom">Submitted From:</label>
                <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                <i id="fromCalendarSubmitted" class="fa fa-calendar fa-lg"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender3" TargetControlID="txtFrom" PopupButtonID="fromCalendar" runat="server"></ajaxToolkit:CalendarExtender>
            </div>
            <div class="col-xs-6 form-group">
                <label for="txtTo">Submitted To:</label>
                <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                <i id="toCalendarSubmitted" class="fa fa-calendar fa-lg"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender4" TargetControlID="txtTo" PopupButtonID="toCalendar" runat="server" />
            </div> 
        </div>
        <div class="row">
            <div class="col-sm-3">                
                <asp:dropdownlist CssClass="form-control" runat="server" id="ddlType" OnSelectedIndexChanged="ddlType_SelectedIndexChanged"> 
                        <asp:listitem text="Type" value=""></asp:listitem>
                        <asp:listitem text="Sick" value="Sick"></asp:listitem>
                        <asp:listitem text="Vacation" value="Vacation"></asp:listitem>
                        <asp:listitem text="Personal" value="Personal"></asp:listitem>
                        <asp:listitem text="Casual" value="Casual"></asp:listitem>
                        <asp:listitem text="Bereavement" value="Bereavement"></asp:listitem>
                        <asp:listitem text="Leave Renewal" value="Leave Renewal"></asp:listitem>
                        <asp:listitem text="Maternity" value="Maternity"></asp:listitem>
                        <asp:listitem text="No Pay" value="No Pay"></asp:listitem>
                </asp:dropdownlist>
            </div>
            <div class="col-sm-3">                
                <asp:dropdownlist CssClass="form-control" runat="server" id="ddlStatus" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged"> 
                        <asp:listitem text="Status" value=""></asp:listitem>
                        <asp:listitem text="Pending" value="Pending"></asp:listitem>
                        <asp:listitem text="Recommended" value="Recommended"></asp:listitem>
                        <asp:listitem text="Not Recommended" value="Not Recommended"></asp:listitem>
                        <asp:listitem text="Approved" value="Approved"></asp:listitem>
                        <asp:listitem text="Not Approved" value="Not Approved"></asp:listitem>
                        <asp:listitem text="Date Change Requested" value="Date Change Requested"></asp:listitem>
                </asp:dropdownlist>
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
     --%>


<div id="gridViewContainer" class="container">
    <center>    

        <%-- FIXME: Paging also triggers the OnRowCommand fn--%>


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

                <%--undo button only--%>
                <asp:TemplateField  HeaderText="">
                    <ItemTemplate>   
                        <asp:Button ID="btnUndo" class="btn btn-warning" Visible=<%# btnHrVisible %> runat="server"
                            CommandName="undo"
                            CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                            Text="⮌"
                            ToolTip="Undo Approve" />                             
                    </ItemTemplate>
                </asp:TemplateField>
                
                                
                <%--action buttons--%>
                <asp:TemplateField  HeaderText="">
                    <ItemTemplate>   

                        <%--employee buttons--%>
                        <asp:Button ID="btnCancelLeave" class="btn btn-danger" Visible=<%# btnEmpVisible %> runat="server"
                            CommandName="Cancel Leave Request"
                            CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                            Text="🗑"
                            ToolTip="Cancel Leave Request" />
                        
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