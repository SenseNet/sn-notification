using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository;
using SenseNet.Notification;
using SN = SenseNet.ContentRepository;
using SenseNet.Portal.UI;
using SenseNet.ContentRepository.Schema;
using Content = SenseNet.ContentRepository.Content;

namespace SenseNet.Portal.Portlets
{
    public class NotificationEditorPortlet : ContextBoundPortlet
    {
        private const string NotificationEditorPortletClass = "NotificationEditorPortlet";

        private ContentView _contentView;

        [WebBrowsable(true), Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public string ContentViewPath { get; set; } = "/Root/System/SystemPlugins/Notifications/NotificationEditor.ascx";

        [WebBrowsable(true), Personalizable(true)]
        [LocalizedWebDisplayName(NotificationEditorPortletClass, "Prop_IsSubscriptionNew_DisplayName")]
        [LocalizedWebDescription(NotificationEditorPortletClass, "Prop_IsSubscriptionNew_Description")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(110)]
        public bool IsSubscriptionNew { get; set; }

        private IButtonControl _button;
        protected IButtonControl SaveButton
        {
            get { return _button ?? (_button = this.FindControlRecursive("BtnSave") as IButtonControl); }
        }

        private string _contentPath;
        public string ContentPath
        {
            get
            {
                if (_contentPath == null)
                {
                    _contentPath = HttpContext.Current.Request["ContentPath"];
                    if (string.IsNullOrEmpty(_contentPath))
                    {
                        // subscription for the current content
                        _contentPath = this.ContextNode.Path;
                    }
                }

                return _contentPath;
            }
        }

        private User _user;
        public User User
        {
            get
            {
                if (_user == null)
                {
                    var userPath = HttpContext.Current.Request["UserPath"];
                    _user = string.IsNullOrEmpty(userPath) ? User.Current as User : Node.Load<User>(userPath);
                }

                return _user;
            }
        }

        public bool HasPermission => this.User.Id == User.Current.Id ||
                                     this.User.Security.HasPermission(PermissionType.Save);

        public NotificationEditorPortlet()
        {
            Name = "$NotificationEditorPortlet:PortletDisplayName";
            Description = "$NotificationEditorPortlet:PortletDescription";
            this.Category = new PortletCategory("$PortletFramework:Category_Notification", "$PortletFramework:Category_Notification_Description");

            this.HiddenPropertyCategories = new List<string>() { EditorCategory.Cache };
            this.HiddenProperties.Add("Renderer");
            Cacheable = false;
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
            var currentSite = PortalContext.Current.Site;
            var siteUrl = PortalContext.Current.SiteUrl;

            if (this.User == null)
                return null;

            // edit subscription or create new if not exists
            var subscription = Subscription.GetSubscriptionByUser(this.User.Path, this.ContentPath);
            if (subscription != null){
                IsSubscriptionNew = false;
            }
            else
            {
                subscription = new Subscription()
                                    {
                                        ContentPath = this.ContentPath,
                                        Frequency = NotificationFrequency.Immediately,
                                        IsActive = true,
                                        UserEmail = this.User.Email,
                                        UserPath = this.User.Path,
                                        UserId = this.User.Id,
                                        UserName = this.User.Name,
                                        Language = "en",
                                        SitePath = currentSite?.Path,
                                        SiteUrl = siteUrl
                                    };

                IsSubscriptionNew = true;
            }

            var sct = Content.Create(subscription, subscriptionCtd);
            var rch = sct.ContentHandler as Content.RuntimeContentHandler;
            rch?.SetIsNew(IsSubscriptionNew);

            return sct;
        }

        protected override void CreateChildControls()
        {
            try
            {
                var modelData = GetModel() as SN.Content;
                if (modelData == null)
                    return;

                if (!this.HasPermission)
                {
                    Controls.Add(new LiteralControl(HttpContext.GetGlobalResourceObject("Notification", "NotEnoughPermissions") as string));
                    return;
                }

                _contentView = ContentView.Create(modelData, Page, ViewMode.Edit, ContentViewPath);
                this.Controls.Add(_contentView);

                // add event handler
                if (this.SaveButton != null)
                {
                    this.SaveButton.Click += BtnSave_Click;
                }
            }
            catch (Exception ex)
            {
                SnLog.WriteException(ex);
                Controls.Clear();
                Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
            }
        }

        protected void BtnSave_Click(object sender, EventArgs args)
        {
            _contentView.UpdateContent();
            var content = _contentView.Content;

            if (!_contentView.IsUserInputValid || !content.IsValid)
                return;

            if (!this.HasPermission)
            {
                _contentView.ContentException = new SenseNetSecurityException(HttpContext.GetGlobalResourceObject("Notification", "NotEnoughPermissions") as string);
                return;
            }

            Subscription subscription;
            if (IsSubscriptionNew){
                subscription = new Subscription
                {
                    IsActive = true,
                    ContentPath = (string)content["ContentPath"],
                    Frequency = (NotificationFrequency)Enum.Parse(typeof(NotificationFrequency),
                                                                  (content["Frequency"] as List<String>)[0]),
                    UserEmail = (string)content["UserEmail"],
                    UserId = (int)(decimal)content["UserId"],
                    UserName = (string)content["UserName"],
                    UserPath = (string)content["UserPath"],
                    Language = (content["Language"] as List<String>)[0],
                    SitePath = PortalContext.Current.Site.Path,
                    SiteUrl = PortalContext.Current.SiteUrl
                };
            } else
            {
                subscription = new Subscription
                {
                    IsActive = (bool)content["IsActive"],
                    ContentPath = (string)content["ContentPath"],
                    Frequency = (NotificationFrequency)Enum.Parse(typeof(NotificationFrequency),
                                                                  (content["Frequency"] as List<String>)[0]),
                    UserEmail = (string)content["UserEmail"],
                    UserId = (int)(decimal)content["UserId"],
                    UserName = (string)content["UserName"],
                    UserPath = (string)content["UserPath"],
                    Language = (content["Language"] as List<String>)[0],
                    SitePath = PortalContext.Current.Site.Path,
                    SiteUrl = PortalContext.Current.SiteUrl
                };
            }

            try
            {
                subscription.Save();

                CallDone(false);
            }
            catch (Exception ex) // logged
            {
                SnLog.WriteException(ex);
                _contentView.ContentException = ex;
            }
        }
    }
}