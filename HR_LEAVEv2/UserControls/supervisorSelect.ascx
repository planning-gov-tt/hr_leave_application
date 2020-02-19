﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="supervisorSelect.ascx.cs" Inherits="HR_LEAVEv2.UserControls.supervisorSelect" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>


<ajaxToolkit:ComboBox ID="ComboBox1" 
    runat="server" AutoPostBack="true" DropDownStyle="DropDownList"
    AutoCompleteMode="SuggestAppend" 
    DataSourceID="SqlDataSource1" 
    DataTextField="Supervisor Name" 
    DataValueField="employee_id" 
    MaxLength="0"  Height="27px"
    OnSelectedIndexChanged="ComboBox1_SelectedIndexChanged">
</ajaxToolkit:ComboBox>
<asp:SqlDataSource ID="SqlDataSource1" 
    runat="server" 
    ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" 
    ProviderName="System.Data.SqlClient" 
    SelectCommand="getSupervisors" 
    SelectCommandType="StoredProcedure">
</asp:SqlDataSource>
