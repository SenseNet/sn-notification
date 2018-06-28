using System;
using SenseNet.ContentRepository;

namespace SenseNet.Notification
{
    public class NotificationComponent : SnComponent
    {
        public override string ComponentId => "SenseNet.Notification";
        public override Version SupportedVersion { get; } = new Version(7, 1, 0);
    }
}
