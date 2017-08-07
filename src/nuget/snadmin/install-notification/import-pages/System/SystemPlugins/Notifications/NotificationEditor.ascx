<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>

<% var targetContent = SenseNet.ContentRepository.Content.Load(this.Content["ContentPath"] as string); %>

<div class="sn-content-inlineview-header ui-helper-clearfix">
    <%= targetContent == null ? string.Empty : SenseNet.Portal.UI.IconHelper.RenderIconTag(targetContent.Icon, null, 32)%>
    <div class="sn-content-info sn-notification-content">
        <h2 class="sn-view-title"><%= targetContent == null ? (HttpContext.GetGlobalResourceObject("Notifications", "UnknownContent") as string) : HttpUtility.HtmlEncode(targetContent.DisplayName) %>
        <% if ( !(bool)(ContextBoundPortlet.GetContainingContextBoundPortlet(this) as NotificationEditorPortlet).IsSubscriptionNew ) {  %>
            <i><%=GetGlobalResourceObject("Notification", "EditExisting")%></i>
        <% } %></h2>
        <%= this.Content["ContentPath"] %>
    </div>
</div>

<% if (!string.IsNullOrEmpty(this.Content["UserEmail"] as string)) {  %>
    <sn:ShortText ID="UserEmail" runat="server" FieldName="UserEmail" ControlMode="Browse" />
<% } %>

<sn:RadioButtonGroup ID="DrpDwnFrequency" runat="server" FieldName="Frequency" />

<% if ( !(bool)(ContextBoundPortlet.GetContainingContextBoundPortlet(this) as NotificationEditorPortlet).IsSubscriptionNew ) {  %>
    <sn:Boolean ID="BoolIsActive" runat="server" FieldName="IsActive" />
<% } %>

<sn:DropDown ID="DrpDwnLang" runat="server" FieldName="Language" />

<div class="sn-panel sn-buttons">
    <% if (!string.IsNullOrEmpty(this.Content["UserEmail"] as string)) {  %>
        <asp:Button ID="BtnSave" runat="server" CssClass="sn-submit" Text="<%$ Resources:Content,Save %>" />
    <% } else { %>    
        <span class="sn-error"><%= GetGlobalResourceObject("Notification", "MissingEmailAddressError")%></span>
    <% } %>

    <sn:BackButton ID="BackButton" runat="server" class="sn-submit" Text="<%$ Resources:Content,Cancel %>" />  
</div>​