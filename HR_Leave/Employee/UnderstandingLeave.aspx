<%@ Page Title="Understanding Leave" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UnderstandingLeave.aspx.cs" Inherits="HR_Leave.UnderstandingLeave" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        div.row{
            margin-top:25px;
        }

        hr{
            width:75%;
        }
    </style>
    <h1><%: Title %></h1>
    <div class="row text-center">
        <h2>Types of Leave</h2>
        <hr />
        <ul class="list-group" style="width:40%; margin: auto;">
          <li class="list-group-item">
              <h4>Sick Leave</h4>
              <p>Lorem Ipsum</p>
          </li>
          <li class="list-group-item">
              <h4>Personal/Casual Leave</h4>
              <p>Lorem Ipsum</p>
          </li>
          <li class="list-group-item">
              <h4>Vacation Leave</h4>
              <p>Lorem Ipsum</p>
          </li>
        </ul>
    </div>
    <div class="row text-center">
        <h2>Contract Workers</h2>
        <hr/>
        <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc maximus, nulla ut commodo sagittis, sapien dui mattis dui, non pulvinar lorem felis nec erat</p>
    </div>
    <div class="row text-center">
        <h2>Public Service Workers</h2>
        <hr/>
        <div class="alert alert-danger" style="width:35%; margin:auto;"><h4>Disclaimer regarding Public Service Workers</h4></div>
    </div>
</asp:Content>
