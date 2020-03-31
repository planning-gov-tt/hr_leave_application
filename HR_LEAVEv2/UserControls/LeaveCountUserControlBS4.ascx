<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LeaveCountUserControlBS4.ascx.cs" Inherits="HR_LEAVEv2.UserControls.LeaveCountUserControlBS4" %>

<style>
    .counter {
        background-color: #f5f5f5;
        padding: 20px 0;
        border-radius: 5px;
    }

    .count-title {
        font-size: 40px;
        font-weight: normal;
        margin-top: 10px;
        margin-bottom: 0;
        text-align: center;
    }

    .count-text {
        font-size: 13px;
        font-weight: normal;
        margin-top: 10px;
        margin-bottom: 0;
        text-align: center;
    }

    .fa-2x {
        margin: 0 auto;
        float: none;
        display: table;
        color: #4ad1e5;
    }

    .count-block{
        width:300px;
        height:250px;
        margin:0px 5px;
    }
</style>


<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>


<div class="container" style =" height:250px">

    <div class="row" style="margin-bottom:10px;">
        <div class="col text-center">
            <h4 id="leaveCountHeader">Current Leave Balance</h4>
        </div>
    </div>

    <div class="row text-center">


        <asp:Panel ID="inactiveEmpPanel" runat="server" Visible ="false" Style="display:inline-block;">
            <div class="alert alert-info">
                <i class="fa fa-info-circle" aria-hidden="true"></i>
                <span>Leave Balances unavailable since this employee is inactive</span>
            </div>
        </asp:Panel>

        <%--sick--%>
        <asp:Panel ID="sickPanel" runat="server" CssClass="col count-block" Style="display:inline-block;">
            <div class="counter">
                <i class="fa fa-ambulance fa-2x"></i>
                <h2 id="h2Sick" class="count-title count-number"><%= ViewState["sick"]%></h2>
                <p class="count-text ">Sick</p>
            </div>
        </asp:Panel>

        <%--vacation--%>
        <asp:Panel ID="vacationPanel" runat="server" CssClass="col count-block" Style="display:inline-block;">
            <div class="counter">
                <i class="fa fa-plane fa-2x"></i>
                <h2 id="h2Vacation" class="timer count-title count-number"><%= ViewState["vacation"]%></h2>
                <p class="count-text ">Vacation</p>
            </div>
        </asp:Panel>

        <%--personal--%>
        <asp:Panel ID="personalPanel" runat="server" CssClass="col count-block" Style="display:inline-block;">
            <div class="counter">
                <i class="fa fa-user fa-2x"></i>
                <h2 id="h2Personal" class="timer count-title count-number"><%= ViewState["personal"]%></h2>
                <p class="count-text ">Personal</p>
            </div>
        </asp:Panel>
       
            

         <%--casual--%>
        <asp:Panel ID="casualPanel" runat="server" CssClass="col count-block" Style="display:inline-block;">
            <div class="counter">
                <i class="fa fa-user fa-2x"></i>
                <h2 id="h2Casual" class="timer count-title count-number"><%= ViewState["casual"]%></h2>
                <p class="count-text ">Casual</p>
            </div>
        </asp:Panel>
    </div>
</div>

