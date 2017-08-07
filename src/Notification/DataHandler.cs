using System.Configuration;

namespace SenseNet.Notification
{
    // WARNING: Do not reorder this
    public enum NotificationFrequency { Immediately, Daily, Weekly, Monthly }

    internal class DataHandler : NotificationsDataContext
    {
        public DataHandler() : base(ConfigurationManager.ConnectionStrings["SnCrMsSql"].ConnectionString) { }
    }
}
