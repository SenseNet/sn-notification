using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.Notification
{
    public enum NotificationType
    {
        Created, MajorVersionModified, MinorVersionModified, CopiedFrom, MovedFrom, MovedTo, RenamedFrom, RenamedTo, Deleted, Restored
    }

    public partial class Event
    {
        public NotificationType NotificationType
        {
            get { return (NotificationType)this.NotificationTypeId; }
            set { this.NotificationTypeId = (int)value; }
        }

        partial void OnCreated()
        {
            When = DateTime.UtcNow;
        }

        // caller: NotificationObserver
        public static void CreateAndSave(Node node, NotificationType type, string who)
        {
            if (type != NotificationType.MovedFrom && type != NotificationType.MovedTo)
                if (!IsSubscriptionExist(node.Path))
                    return;

            var @event = new Event
            {
                ContentPath = node.Path,
                NotificationType = type,
                Who = who,
                CreatorId = node.CreatedById,
                LastModifierId = node.ModifiedById
            };
            @event.Save();
        }
        public static void CreateAndSave(string contentPath, int creatorId, int lastModifierId, NotificationType type, string who)
        {
            CreateAndSave(contentPath, creatorId, lastModifierId, type, who, null);
        }
        internal static void CreateAndSave(string contentPath, int creatorId, int lastModifierId, NotificationType type, string who, DateTime? when)
        {
            if (type != NotificationType.MovedFrom && type != NotificationType.MovedTo)
                if (!IsSubscriptionExist(contentPath))
                    return;

            var @event = new Event
            {
                ContentPath = contentPath,
                NotificationType = type,
                Who = who,
                CreatorId = creatorId,
                LastModifierId = lastModifierId,
            };
            if (when.HasValue)
                @event.When = when.Value;
            @event.Save();
        }

        private static bool IsSubscriptionExist(string contentPath)
        {
            var sql = @"DECLARE @PathCollection AS TABLE([Path] nvarchar(450))
INSERT @PathCollection
    SELECT [Path].value('.', 'nvarchar(900)')
    FROM @ProbeCollection .nodes('/r/p') AS Identifiers([Path])
SELECT COUNT(*) FROM @PathCollection C
    INNER JOIN [Notification.Subscriptions] S ON C.[Path] = S.[ContentPath]
";

            var xml = new StringBuilder();
            xml.Append("<r>");
            var path = contentPath;
            while (path.Length > 1)
            {
                xml.Append("<p>").Append(path).Append("</p>");
                var p = path.LastIndexOf('/');
                path = path.Substring(0, p);
            }
            xml.Append("</r>");

            using (var proc = DataProvider.Instance.CreateDataProcedure(sql)
                .AddParameter("@ProbeCollection", xml.ToString(), DbType.Xml))
            {
                proc.CommandType = CommandType.Text;

                var result = proc.ExecuteScalar();
                var count = (int)result;
                return count > 0;
            }
        }

        private void Save()
        {
            using (var context = new DataHandler())
            {
                context.Events.InsertOnSubmit(this);
                context.SubmitChanges();
            }
        }

        internal static void DeleteAllEvents()
        {
            using (var context = new DataHandler())
            {
                context.ExecuteCommand("DELETE FROM [Notification.Events]");
            }
        }
        internal static void DeleteOldEvents(DateTime now)
        {
            using (var context = new DataHandler())
            {
                context.ExecuteCommand(String.Format("DELETE FROM [Notification.Events] WHERE [When] < '{0}'", now.AddMonths(-2).ToString("yyyy-MM-dd HH:mm:ss")));
            }
        }

        internal static IEnumerable<Event> GetAllEvents()
        {
            using (var context = new DataHandler())
            {
                return context.Events.ToArray();
            }
        }
        internal static int GetCountOfEvents()
        {
            using (var context = new DataHandler())
            {
                return context.Events.Count();
            }
        }
    }
}
