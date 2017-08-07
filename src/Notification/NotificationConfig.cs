using System.Linq;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using System.Text.RegularExpressions;

namespace SenseNet.Notification
{
    [ContentHandler]
    public class NotificationConfig : GenericContent
    {
        // =================================================================================================== Constructors
        public NotificationConfig(Node parent) : this(parent, null) { }
        public NotificationConfig(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected NotificationConfig(NodeToken nt) : base(nt) { }


        // =================================================================================================== Public consts
        public static readonly string CONFIGFOLDERNAME = "(config)";
        public static readonly string NOTIFICATIONCONFIGTYPENAME = "NotificationConfig";
        public static readonly string NOTIFICATIONCONFIGCONTENTNAME = "NotificationConfig";

        
        // =================================================================================================== Properties
        private const string SUBJECTPROPERTYNAME = "Subject";
        [RepositoryProperty(SUBJECTPROPERTYNAME, RepositoryDataType.String)]
        public virtual string Subject
        {
            get { return base.GetProperty<string>(SUBJECTPROPERTYNAME); }
            set { base.SetProperty(SUBJECTPROPERTYNAME, value); }
        }

        private const string BODYPROPERTYNAME = "Body";
        [RepositoryProperty(BODYPROPERTYNAME, RepositoryDataType.Text)]
        public virtual string Body
        {
            get { return base.GetProperty<string>(BODYPROPERTYNAME); }
            set { base.SetProperty(BODYPROPERTYNAME, value); }
        }

        private const string SENDERADDRESSPROPERTYNAME = "SenderAddress";
        [RepositoryProperty(SENDERADDRESSPROPERTYNAME, RepositoryDataType.String)]
        public virtual string SenderAddress
        {
            get { return base.GetProperty<string>(SENDERADDRESSPROPERTYNAME); }
            set { base.SetProperty(SENDERADDRESSPROPERTYNAME, value); }
        }

        public static bool NotificationEnabled
        {
            get { return Configuration.Enabled; }
        }

        // =================================================================================================== Public methods
        /// <summary>
        /// Returns the subject for the emails controlled by this config. {FieldName} entities are replaced with referenced Field value.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual string GetSubject(Node context)
        {
            return this.ReplacePropertyValues(context, this.Subject);
        }
        /// <summary>
        /// Returns the subject for the emails controlled by this config. {FieldName} entities are replaced with referenced Field value.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual string GetBody(Node context)
        {
            return this.ReplacePropertyValues(context, this.Body);
        }
        /// <summary>
        /// Returns the sender address for the emails controlled by this config. {FieldName} entities are replaced with referenced Field value.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual string GetSenderAddress(Node context)
        {
            return this.ReplacePropertyValues(context, this.SenderAddress);
        }
        /// <summary>
        /// Returns false if notification email should not be sent for current context. The default implementation only returns false when the notification would be about this config.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool IsNotificationAllowedForContent(Node context)
        {
            return context.Id != this.Id;
        }


        // =================================================================================================== Helpers
        protected string ReplacePropertyValues(Node context, string text)
        {
            var newText = context.PropertyTypes.Aggregate(text, (current, propertyType) => current.Replace("{" + propertyType.Name + "}", context[propertyType] as string));

            newText = newText.Replace("{Id}", context.Name).Replace("{Name}", context.Name).Replace("{Path}", context.Path).Replace("{DisplayName}", context.DisplayName);

            var regex = new Regex("{.*}", RegexOptions.IgnoreCase);
            var matches = regex.Match(newText);

            foreach (Capture capture in matches.Captures)
            {
                var template = capture.Value;
                var splitterPos = template.IndexOf('.');
                if (splitterPos == -1)
                    continue;

                var reference = template.Substring(1, splitterPos - 1);
                var prop = template.Substring(splitterPos + 1, template.Length - splitterPos - 2);

                var referenceNode = context[reference] as Node;

                newText = newText.Replace(template, referenceNode?[prop] as string ?? string.Empty);
            }

            return newText;
        }


        // =================================================================================================== Property routing
        public override object GetProperty(string name)
        {
            switch (name)
            {
                case SUBJECTPROPERTYNAME:
                    return this.Subject;
                case BODYPROPERTYNAME:
                    return this.Body;
                case SENDERADDRESSPROPERTYNAME:
                    return this.SenderAddress;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case SUBJECTPROPERTYNAME:
                    this.Subject = (string)value;
                    break;
                case BODYPROPERTYNAME:
                    this.Body = (string)value;
                    break;
                case SENDERADDRESSPROPERTYNAME:
                    this.SenderAddress = (string)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }
    }
}