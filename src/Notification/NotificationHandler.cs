using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage.AppModel;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.Notification
{
    /// <summary>
    /// Implements a service that can start when the Repository is starting.
    /// </summary>
    public class NotificationService : ISnService
    {
        /// <summary>
        /// Starts the service if it is enabled in the configuration.
        /// </summary>
        /// <returns>True if the service has started.</returns>
        public bool Start()
        {
            return NotificationHandler.StartNotificationSystem();
        }

        /// <summary>
        /// Shuts down the service. Called when the Repository is finishing.
        /// </summary>
        public void Shutdown()
        {
        }
    }

    internal class NotificationHandler
    {
        private const double MINPOLLINTERVAL = 2000.0;
        private const string NOTIFICATIONLOCKNAME = "ProcessNotificationLock";
        private const double TimeoutMultiplier = 1.6;
        private static double LockingTimeframe;
        private static string AppDomainId;
        private const string insertSql = @"
UPDATE [dbo].[Notification.Synchronization]
   SET [Locked] = 1
      ,[LockedUntil] = @lockedUntil
      ,[ComputerName] = @computerName
      ,[LockId] = @lockId
 WHERE [LockName] = @lockName AND [LockedUntil] = @oldLockedUntil";

        private static bool _started;
        internal static bool StartNotificationSystem()
        {
            if (Configuration.Enabled && !_started)
            {
                var pollInterval = Configuration.TimerInterval * 60.0 * 1000.0;
                LockingTimeframe = TimeoutMultiplier * Configuration.TimerInterval;
                AppDomainId = Guid.NewGuid().ToString();

                _notifTimer = new System.Timers.Timer(pollInterval);
                _notifTimer.Elapsed += new System.Timers.ElapsedEventHandler(NotifTimerElapsed);
                _notifTimer.Disposed += new EventHandler(NotifTimerDisposed);
                _notifTimer.Enabled = true;

                _started = true;
            }
            return _started;
        }

        // =========================================================================================================== Polling

        private static System.Timers.Timer _notifTimer;
        private static bool _processing;

        private static void NotifTimerDisposed(object sender, EventArgs e)
        {
            _notifTimer.Elapsed -= new System.Timers.ElapsedEventHandler(NotifTimerElapsed);
            _notifTimer.Disposed -= new EventHandler(NotifTimerDisposed);
        }
        private static void NotifTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (!Configuration.Enabled)
                    return;
                if (_processing)
                    RefreshLock(LockingTimeframe);
                else
                    TimerTick(DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                SnLog.WriteException(ex);
            }
        }
        internal static void TimerTick(DateTime now)
        {
            if (AcquireLock(LockingTimeframe))
            {
                try
                {
                    Debug.WriteLine("#Notification> ** lock recieved");
                    _processing = true;
                    TimerTick(now, NotificationFrequency.Immediately);
                    TimerTick(now, NotificationFrequency.Daily);
                    TimerTick(now, NotificationFrequency.Weekly);
                    TimerTick(now, NotificationFrequency.Monthly);
                }
                catch (Exception ex)
                {
                    SnLog.WriteException(ex);
                }
                finally
                {
                    _processing = false;
                    ReleaseLock();
                    NotificationSender.StartMessageProcessing();
                }
            }
            else
            {
                Debug.WriteLine("#Notification> ** couldn't get lock");
            }
        }
        internal static void TimerTick(DateTime now, NotificationFrequency freq)
        {
            switch (freq)
            {
                case NotificationFrequency.Immediately: if (!Configuration.ImmediatelyEnabled) return; break;
                case NotificationFrequency.Daily: if (!Configuration.DailyEnabled) return; break;
                case NotificationFrequency.Weekly: if (!Configuration.WeeklyEnabled) return; break;
                case NotificationFrequency.Monthly: if (!Configuration.MonthlyEnabled) return; break;
                default: throw GetUnknownFrequencyException(freq);
            }

            if (now >= LastProcessTime.GetNextTime(freq, now))
                GenerateMessages(freq, now);
        }

        // -------------------------------------------------

        //TODO: AcquireLock, RefreshLock, Release lock csak a teszteles miatt internal, private jobb lenne
        internal static bool AcquireLock(double timeframe)
        {
            Synchronization notificationLock;

            using (var context = new DataHandler())
            {
                notificationLock = context.Synchronizations.SingleOrDefault(lockItem => lockItem.LockName == NOTIFICATIONLOCKNAME);
                if (notificationLock == null)
                    return InsertLock(timeframe);
            }

            if (notificationLock.Locked && (notificationLock.LockId != AppDomainId) && notificationLock.LockedUntil > DateTime.UtcNow)
                return false;

            return UpdateLock(notificationLock, timeframe);
        }
        internal static void RefreshLock(double timeframe)
        {
            using (var context = new DataHandler())
            {
                var notificationLock = context.Synchronizations.SingleOrDefault(lockItem => lockItem.LockName == NOTIFICATIONLOCKNAME);
                if (notificationLock == null)
                    throw new InvalidOperationException("Could not update the Notification lock, because it doesn't exist. (the lock must be present when RefreshLock is called.)");

                notificationLock.LockedUntil = DateTime.UtcNow.AddMinutes(timeframe);
                context.SubmitChanges();
            }
            Debug.WriteLine("#Notification> ** lock refreshed");
        }
        internal static void ReleaseLock()
        {
            using (var context = new DataHandler())
            {
                var notificationLock = context.Synchronizations.SingleOrDefault(lockItem => lockItem.LockName == NOTIFICATIONLOCKNAME);
                if (notificationLock == null)
                    //TODO: #notifB: ?? what is this?
                    throw new Exception();

                notificationLock.Locked = false;
                context.SubmitChanges();
            }
            Debug.WriteLine("#Notification> ** lock released");
        }

        private static bool InsertLock(double timeframe)
        {
            using (var context = new DataHandler())
            {
                var success = false;
                var notifLock = new Synchronization
                                    {
                                        LockName = NOTIFICATIONLOCKNAME,
                                        Locked = true,
                                        ComputerName = Environment.MachineName,
                                        LockedUntil =
                                            DateTime.UtcNow.AddMinutes(timeframe),
                                        LockId = AppDomainId
                                    };
                try
                {
                    context.Synchronizations.InsertOnSubmit(notifLock);
                    context.SubmitChanges();
                    success = true;
                }
                catch (Exception ex)
                {
                    SnLog.WriteException(ex);
                }

                return success;
            }
        }
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        private static bool UpdateLock(Synchronization oldLock, double timeframe)
        {
            var db = (RelationalDataProviderBase) DataStore.DataProvider;

            try
            {
                using (var ctx = db.CreateDataContext(CancellationToken.None))
                {
                    var rows = ctx.ExecuteNonQueryAsync(insertSql,
                            cmd =>
                            {
                                cmd.Parameters.Add(
                                    ctx.CreateParameter("@lockName", DbType.String, NOTIFICATIONLOCKNAME));
                                cmd.Parameters.Add(ctx.CreateParameter("@lockedUntil", DbType.DateTime2,
                                    DateTime.UtcNow.AddMinutes(timeframe)));
                                cmd.Parameters.Add(ctx.CreateParameter("@computerName", DbType.String,
                                    Environment.MachineName));
                                cmd.Parameters.Add(ctx.CreateParameter("@lockId", DbType.String,
                                    Guid.NewGuid().ToString()));
                                cmd.Parameters.Add(ctx.CreateParameter("@oldLockedUntil", DbType.DateTime2,
                                    oldLock.LockedUntil));
                            })
                        .ConfigureAwait(false).GetAwaiter().GetResult();

                    return rows == 1;
                }
            }
            catch (Exception ex)
            {
                SnLog.WriteException(ex);
                return false;
            }
        }

        // ===========================================================================================================

        public static void GenerateMessages(NotificationFrequency freq, DateTime now)
        {
            if (Configuration.GroupNotificationsByUser)
            {
                var groupedSubscriptions = CollectEventsPerSubscriptionByUser(freq, now);
                if (groupedSubscriptions != null)
                    GenerateMessages(groupedSubscriptions);
            }
            else
            {
                var subscriptions = CollectEventsPerSubscription(freq, now);
                if (subscriptions != null)
                    GenerateMessages(subscriptions);
            }


            Cleanup(freq, now);
        }

        private static Dictionary<int, List<Subscription>> CollectEventsPerSubscriptionByUser(NotificationFrequency freq, DateTime now)
        {
            var subscriptions = CollectEventsPerSubscription(freq, now);

            if (subscriptions != null)
                return (from sub in subscriptions
                        where sub != null
                        group sub by sub.UserId
                        into subs
                        select subs).ToDictionary(subs => subs.Key, subs => subs.ToList());
            
            return null;
        }

        private static IEnumerable<Subscription> CollectEventsPerSubscription(NotificationFrequency freq, DateTime now)
        {
            var lastTime = now;
            var subscriptions = Subscription.GetActiveSubscriptionsByFrequency(freq);
            if (subscriptions.Count() == 0)
                return null;

            var time = LastProcessTime.GetLastProcessTime(freq);

            using (var context = new DataHandler())
            {
                var events = (time == DateTime.MinValue) ?
                    context.Events.Where(x => x.When <= lastTime) :
                    context.Events.Where(x => x.When > time && x.When <= lastTime);
                foreach (var @event in events.OrderBy(x => x.When))
                {
                    foreach (var subscription in subscriptions.Where(subscription => IsRelatedPath(@event.ContentPath, subscription.ContentPath) && HasPermission(subscription, @event)))
                    {
                        subscription.AddRelatedEvent(@event);
                    }
                }
            }

            LastProcessTime.SetLastProcessTime(freq, lastTime);

            return subscriptions;
        }

        private static bool IsRelatedPath(string eventPath, string subscriptionPath)
        {
            if (eventPath.Equals(subscriptionPath, StringComparison.OrdinalIgnoreCase))
                return true;
            if (!eventPath.StartsWith(subscriptionPath, StringComparison.OrdinalIgnoreCase))
                return false;
            return eventPath[subscriptionPath.Length] == '/';
        }
        private static bool HasPermission(Subscription subscription, Event @event)
        {
            if (@event == null || string.IsNullOrEmpty(@event.ContentPath) || subscription == null || subscription.User == null)
                return false;

            var head = NodeHead.Get(@event.ContentPath);
            if (head == null)
                return false;

            if (@event.NotificationType == NotificationType.MinorVersionModified)
                return SecurityHandler.HasPermission(subscription.User, head.Id, PermissionType.OpenMinor);
            else
                return SecurityHandler.HasPermission(subscription.User, head.Id, PermissionType.Open);
        }

        private static void GenerateMessages(Dictionary<int, List<Subscription>> groupedSubscriptions)
        {
            var configs = new Dictionary<string, NotificationConfig>();

            using (var context = new DataHandler())
            {
                foreach (var item in groupedSubscriptions)
                {
                    foreach (var subscription in item.Value)
                    {
                        // gather notification configs (handler: NotificationConfig.cs) for all related content of subscriptions
                        GatherConfigsForSubscription(subscription, configs);
                    }

                    var msg = GenerateMessage(item.Value, configs);
                    if (msg != null)
                        context.Messages.InsertOnSubmit(msg);

                    context.SubmitChanges();
                }
            }
        }

        private static void GenerateMessages(IEnumerable<Subscription> subscriptions)
        {
            var configs = new Dictionary<string, NotificationConfig>();

            using (var context = new DataHandler())
            {
                foreach (var subscription in subscriptions)
                {
                    // gather notification configs (handler: NotificationConfig.cs) for all related content of subscriptions
                    GatherConfigsForSubscription(subscription, configs);

                    // generate messges for content whose notification are controlled by a notification config
                    var msgs = GenerateMessagesForConfig(subscription, configs);
                    foreach (var message in msgs)
                    {
                        context.Messages.InsertOnSubmit(message);
                    }

                    // generate messages for content whose notification are NOT controlled by a notification config (send a generic notification message)
                    var msg = GenerateMessage(subscription, configs);
                    if (msg != null)
                        context.Messages.InsertOnSubmit(msg);
                }

                context.SubmitChanges();
            }
        }

        private static void GatherConfigsForSubscription(Subscription subscription, Dictionary<string, NotificationConfig> configs)
        {
            // go through related events, and gather available configs
            foreach (var @event in subscription.RelatedEvents)
            {
                // config already resolved for this contentpath
                if (configs.ContainsKey(@event.ContentPath))
                    continue;

                // check custom config
                var query = new ApplicationQuery(NotificationConfig.CONFIGFOLDERNAME, false, false, HierarchyOption.Path);
                var configHeads = query.ResolveApplications(NotificationConfig.NOTIFICATIONCONFIGCONTENTNAME, @event.ContentPath, NotificationConfig.NOTIFICATIONCONFIGTYPENAME);
                var targetConfigHead = configHeads.FirstOrDefault();

                NotificationConfig configNode = null;
                if (targetConfigHead != null)
                {
                    configNode = Node.LoadNode(targetConfigHead.Id) as NotificationConfig;
                }
                configs.Add(@event.ContentPath, configNode);
            }
        }
        private static IEnumerable<Message> GenerateMessagesForConfig(Subscription subscription, Dictionary<string, NotificationConfig> configs)
        {
            if (subscription.RelatedEvents.Count == 0)
                yield break;

            foreach (var @event in subscription.RelatedEvents)
            {
                // we will process those content only for which config is available.
                var config = configs[@event.ContentPath];
                if (config == null)
                    continue;

                var contextNode = Node.LoadNode(@event.ContentPath);
                if (contextNode == null)
                    continue;

                var allowed = config.IsNotificationAllowedForContent(contextNode);
                if (!allowed)
                    continue;

                var subject = config.GetSubject(contextNode);
                var body = config.GetBody(contextNode);
                var sender = config.GetSenderAddress(contextNode);

                yield return new Message
                {
                    Address = subscription.UserEmail,
                    Subject = subject,
                    Body = body.ToString(),
                    SenderAddress = sender
                };
            }
        }
        
        private static Message GenerateMessage(IEnumerable<Subscription> subscriptions, IDictionary<string, NotificationConfig> configs)
        {
            if (subscriptions.Where(s => s.RelatedEvents.Count == 0).Count() == subscriptions.Count())
                return null;
            
            var multipleSubs = subscriptions.Count() > 1;
            
            // the most used subscription language
            var mainLanguage = (from s in subscriptions
                                   group s by s.Language into lg
                                   let count = lg.Count()
                                   orderby count descending
                                   select lg).First().Key;
                                  
            // all of the subscriptions have the same frequency and user
            var frequency = subscriptions.First().Frequency;
            var email = subscriptions.First().UserEmail;

            var template = new MessageTemplate(mainLanguage);
            var cultureInfo = CultureInfo.CreateSpecificCulture(mainLanguage);

            var subjectTemplate = GetSubjectTemplate(template, frequency);

            //TODO: Currently the following method uses only User information from 
            // the Subscription, so it does not matter which subscription is passed. If that
            // changes, we need to refactor this.
            var subject = ReplaceParameters(subjectTemplate, subscriptions.First()); 

            var body = new StringBuilder();
            var headTemplate = GetHeaderTemplate(template, frequency);
            string sender = null;

            //TODO: Currently the following method uses only User information from 
            // the Subscription, so it does not matter which subscription is passed. If that
            // changes, we need to refactor this.
            var head = ReplaceParameters(headTemplate, subscriptions.First());

            body.Append(head);

            var hasConfig = false;
            foreach (var subscription in subscriptions)
            {
                foreach (var @event in subscription.RelatedEvents)
                {
                    if (configs[@event.ContentPath] != null)
                    {
                        var conf = configs[@event.ContentPath];
                        var contextNode = Node.LoadNode(@event.ContentPath);
                        if (contextNode != null)
                        {
                            hasConfig = true;
                            if (subscription.Frequency == NotificationFrequency.Immediately && !multipleSubs)
                            {
                                subject = ReplaceParameters(conf.GetSubject(contextNode), subscription);
                                body.Clear();
                            }
                            body.Append(ReplaceParameters(conf.GetBody(contextNode), subscription));
                            sender = conf.GetSenderAddress(contextNode);
                        }
                    }
                    else
                    {
                        body.Append(ReplaceParameters(GetEventTemplate(template, @event.NotificationType), subscription.SitePath, subscription.SiteUrl, @event, cultureInfo));
                    }
                }
            }

            string footTemplate;
            switch (frequency)
            {
                case NotificationFrequency.Immediately:
                    if (hasConfig && !multipleSubs) footTemplate = string.Empty;
                    else footTemplate = template.ImmediatelyFooter;
                    break;
                default: footTemplate = GetFooterTemplate(template, frequency); 
                    break;
            }

            //TODO: Currently the following method uses only User information from 
            // the Subscription, so it does not matter which subscription is passed. If that
            // changes, we need to refactor this.
            var footer = ReplaceParameters(footTemplate, subscriptions.First());
            body.Append(footer);

            return new Message
            {
                Address = email,
                Subject = subject,
                Body = body.ToString(),
                SenderAddress = sender
            };

        }
        private static Message GenerateMessage(Subscription subscription, IDictionary<string, NotificationConfig> configs)
        {
            if (subscription.RelatedEvents.Count == 0)
                return null;

            // we will process those content only for which no config is available. if there is no such content, we are done.
            if (!subscription.RelatedEvents.Any(e => configs[e.ContentPath] == null))
                return null;

            var template = new MessageTemplate(subscription.Language);
            var cultureInfo = CultureInfo.CreateSpecificCulture(subscription.Language);

            var subjectTemplate = GetSubjectTemplate(template, subscription.Frequency);
            var subject = ReplaceParameters(subjectTemplate, subscription);

            var body = new StringBuilder();
            var headTemplate = GetHeaderTemplate(template, subscription.Frequency);

            body.Append(ReplaceParameters(headTemplate, subscription));

            foreach (var @event in subscription.RelatedEvents)
            {
                // a custom config exists for this contentpath, email for that has already been processed previously
                if (configs[@event.ContentPath] != null)
                    continue;

                body.Append(ReplaceParameters(GetEventTemplate(template, @event.NotificationType), subscription.SitePath, subscription.SiteUrl, @event, cultureInfo));
            }

            var footTemplate = GetFooterTemplate(template, subscription.Frequency);

            body.Append(ReplaceParameters(footTemplate, subscription));

            return new Message
            {
                Address = subscription.UserEmail,
                Subject = subject,
                Body = body.ToString()
            };
        }

        private static string GetSubjectTemplate(MessageTemplate template, NotificationFrequency frequency)
        {
            switch (frequency)
            {
                case NotificationFrequency.Immediately: return template.ImmediatelySubject;
                case NotificationFrequency.Daily: return template.DailySubject;
                case NotificationFrequency.Weekly: return template.WeeklySubject;
                case NotificationFrequency.Monthly: return template.MonthlySubject;
                default: throw GetUnknownFrequencyException(frequency);
            }
        }

        private static string GetHeaderTemplate(MessageTemplate template, NotificationFrequency frequency)
        {
            switch (frequency)
            {
                case NotificationFrequency.Immediately: return template.ImmediatelyHeader;
                case NotificationFrequency.Daily: return template.DailyHeader;
                case NotificationFrequency.Weekly: return template.WeeklyHeader;
                case NotificationFrequency.Monthly: return template.MonthlyHeader;
                default: throw GetUnknownFrequencyException(frequency);
            }
        }

        private static string GetFooterTemplate(MessageTemplate template, NotificationFrequency frequency)
        {
            switch (frequency)
            {
                case NotificationFrequency.Immediately: return template.ImmediatelyFooter;
                case NotificationFrequency.Daily: return template.DailyFooter; 
                case NotificationFrequency.Weekly: return template.WeeklyFooter;
                case NotificationFrequency.Monthly: return template.MonthlyFooter;
                default: throw GetUnknownFrequencyException(frequency);
            }
        }

        private static string GetEventTemplate(MessageTemplate template, NotificationType notificationType)
        {
            switch (notificationType)
            {
                case NotificationType.Created: return template.CreatedTemplate;
                case NotificationType.MajorVersionModified: return template.MajorVersionModifiedTemplate;
                case NotificationType.MinorVersionModified: return template.MinorVersionModifiedTemplate;
                case NotificationType.CopiedFrom: return template.CopiedFromTemplate;
                case NotificationType.MovedFrom: return template.MovedFromTemplate;
                case NotificationType.MovedTo: return template.MovedToTemplate;
                case NotificationType.RenamedFrom: return template.RenamedFromTemplate;
                case NotificationType.RenamedTo: return template.RenamedToTemplate;
                case NotificationType.Deleted: return template.DeletedTemplate;
                case NotificationType.Restored: return template.RestoredTemplate;
                default: throw new SnNotSupportedException("Unknown NotificationType: " + notificationType);
            }
        }
        private static string ReplaceParameters(string template, Subscription subscription)
        {
            return template
                .Replace("{Address}", subscription.UserEmail)
                .Replace("{Addressee}", subscription.UserName);
        }
        private static string ReplaceParameters(string template, string sitePath, string siteUrl, Event @event, CultureInfo cultureInfo)
        {
            return template
                .Replace("{ContentPath}", @event.ContentPath)
                .Replace("{ContentUrl}", String.Concat("http://", GetContentUrl(sitePath, siteUrl, @event.ContentPath)))
                .Replace("{When}", @event.When.ToString(cultureInfo))
                .Replace("{Who}", @event.Who);
        }

        private static string GetContentUrl(string sitePath, string siteUrl, string contentPath)
        {
            if (contentPath == sitePath)
                return siteUrl;
            if (!contentPath.StartsWith(sitePath))
                return contentPath;
            if (contentPath[sitePath.Length] != '/')
                return contentPath;
            return contentPath.Replace(sitePath, siteUrl);
        }

        private static void Cleanup(NotificationFrequency freq, DateTime now)
        {
            switch (freq)
            {
                case NotificationFrequency.Immediately:
                    if (Configuration.DailyEnabled || Configuration.WeeklyEnabled || Configuration.MonthlyEnabled)
                        return;
                    break;
                case NotificationFrequency.Daily:
                    if (Configuration.WeeklyEnabled || Configuration.MonthlyEnabled)
                        return;
                    break;
                case NotificationFrequency.Weekly:
                    if (Configuration.MonthlyEnabled)
                        return;
                    break;
                case NotificationFrequency.Monthly:
                    break;
                default:
                    throw GetUnknownFrequencyException(freq);
            }
            Event.DeleteOldEvents(now);
        }

        internal static Exception GetUnknownFrequencyException(NotificationFrequency freq)
        {
            return new SnNotSupportedException("Unknown NotificationFrequency: " + freq);
        }

    }
}
