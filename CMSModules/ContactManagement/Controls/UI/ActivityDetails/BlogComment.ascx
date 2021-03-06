<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlogComment.ascx.cs" Inherits="CMSModules_ContactManagement_Controls_UI_ActivityDetails_BlogComment" %>
<table>
    <tr>
        <td class="ActivityDetailsLabel">
            <cms:LocalizedLabel runat="server" ID="lblDocID" ResourceString="om.activitydetails.blogdocument"
                EnableViewState="false" DisplayColon="true" />
        </td>
        <td>
            <asp:Label runat="server" ID="lblDocIDVal" />
        </td>
    </tr>
    <asp:PlaceHolder runat="server" ID="plcComment" Visible="false" EnableViewState="false">
        <tr>
            <td class="ActivityDetailsLabel" style="vertical-align: top;">
                <cms:LocalizedLabel runat="server" ID="lblComment" ResourceString="om.activitydetails.blogcomment"
                    EnableViewState="false" DisplayColon="true" />
            </td>
            <td>
                <cms:CMSTextBox runat="server" ID="txtComment" TextMode="MultiLine" ReadOnly="true"
                    CssClass="ActivityDetailsCommentBox" Wrap="false" />
            </td>
        </tr>
    </asp:PlaceHolder>
</table>
