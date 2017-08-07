using SenseNet.ContentRepository;
using SenseNet.ApplicationModel;

namespace SenseNet.Notification.ApplicationModel
{
    public class SetNotificationAction : UrlAction
    {
        public override void Initialize(Content context, string backUri, Application application, object parameters)
        {
            base.Initialize(context, backUri, application, parameters);

            var enabled = NotificationConfig.NotificationEnabled;
            this.Forbidden = !enabled;
        }
    }
}
