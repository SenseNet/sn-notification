using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SN = SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.ContentRepository.Schema;
using SenseNet.Notification;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.Portal.Portlets
{
    public class NotificationListPortlet : ContextBoundPortlet
    {
        [WebBrowsable(true), Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public string ContentViewPath { get; set; } = "/Root/System/SystemPlugins/Notifications/NotificationList.ascx";

        private User _user;
        public User User => _user ?? (_user = this.ContextNode as User ?? User.Current as User);

        public bool HasPermission => this.User.Id == User.Current.Id ||
                                     this.User.Security.HasPermission(PermissionType.Save);

        public NotificationListPortlet()
        {
            Name = "$NotificationListPortlet:PortletDisplayName";
            Description = "$NotificationListPortlet:PortletDescription";
            this.Category = new PortletCategory("$PortletFramework:Category_Notification", "$PortletFramework:Category_Notification_Description");

            this.HiddenProperties.Add("Renderer");
        }

        protected override object GetModel()
        {
            string subscriptionCtd;
            using (new SystemAccount())
            {
                var ctdFile = Node.LoadNode("/Root/System/Schema/ContentTypes/GenericContent/Subscription") as ContentType;
                if (ctdFile != null)
                {
                    using (var ctdStream = ctdFile.Binary.GetStream())
                    {
                        subscriptionCtd = RepositoryTools.GetStreamString(ctdStream);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Subscription CTD not found.");
                }
            }

            return (from subscripton in Subscription.GetSubscriptionsByUser(this.User.Path)
                    where !string.IsNullOrEmpty(subscripton.ContentPath) && Node.Exists(subscripton.ContentPath)
                    select SN.Content.Create(subscripton, subscriptionCtd)).ToList();
        }

        protected override void CreateChildControls()
        {
            if (Cacheable && CanCache && IsInCache)
                return;

            if (!this.HasPermission)
            {
                Controls.Add(new LiteralControl(HttpContext.GetGlobalResourceObject("Notification", "NotEnoughPermissions") as string));
                return;
            }

            try
            {
                var modelData = GetModel() as IEnumerable<SN.Content>;

                var viewControl = Page.LoadControl(ContentViewPath);
                if (viewControl != null)
                {
                    var contentList = viewControl.FindControl("ContentList");
                    if (contentList != null)
                    {
                        ContentQueryPresenterPortlet.DataBindingHelper.SetDataSourceAndBind(contentList, modelData);
                    }

                    Controls.Add(viewControl);

                    ChildControlsCreated = true;
                }
            }
            catch (Exception ex)
            {
                SnLog.WriteException(ex);
                Controls.Clear();
                Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
            }
        }
    }
}
