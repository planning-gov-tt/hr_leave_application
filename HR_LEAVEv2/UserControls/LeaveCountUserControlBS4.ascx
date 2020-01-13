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
</style>


<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>


<div class="container">
    <div class="row">
        <br />
        <div class="col text-center">
            <h4 id="leaveCountHeader">Leave Remaining</h4>
        </div>
    </div>
    <div class="row text-center">

        <%--sick--%>
        <div class="col-sm-4">
            <div class="counter">
                <i class="fa fa-plus-square fa-2x"></i>
                <h2 id="h2Sick" class="timer count-title count-number" data-to="<%= ViewState["sick"]%>" data-speed="600"><%= ViewState["sick"]%></h2>
                <p class="count-text ">Sick</p>
            </div>
        </div>

        <%--vacation--%>
        <div class="col-sm-4">
            <div class="counter">
                <i class="fa fa-plane fa-2x"></i>
                <h2 id="h2Vacation" class="timer count-title count-number" data-to="<%= ViewState["vacation"]%>" data-speed="600"><%= ViewState["vacation"]%></h2>
                <p class="count-text ">Vacation</p>
            </div>
        </div>

        <%--personal--%>
        <div class="col-sm-4">
            <div class="counter">
                <i class="fa fa-user fa-2x"></i>
                <h2 id="h2Personal" class="timer count-title count-number" data-to="<%= ViewState["personal"]%>" data-speed="600"><%= ViewState["personal"]%></h2>
                <p class="count-text ">Personal</p>
            </div>
        </div>
    </div>
</div>

