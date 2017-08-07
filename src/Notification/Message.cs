using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace SenseNet.Notification
{
    public partial class Message
    {
        internal MailMessage GenerateMailMessage()
        {
            var sender = string.IsNullOrEmpty(SenderAddress) ? Configuration.SenderAddress : SenderAddress;
            var message = new MailMessage(sender, Address, Subject, Body)
            {
                IsBodyHtml = true,
                SubjectEncoding = Configuration.MessageEncoding,
                HeadersEncoding = Configuration.MessageEncoding,
                BodyEncoding = Configuration.MessageEncoding
            };

            return message;
        }

        internal static void DeleteAllMessages()
        {
            using (var context = new DataHandler())
            {
                context.ExecuteCommand("DELETE FROM [Notification.Messages]");
            }
        }

        internal static IEnumerable<Message> GetAllMessages()
        {
            using (var context = new DataHandler())
            {
                return context.Messages.ToArray();
            }
        }

        private static readonly Message[] _emptyMessages = new Message[0];
        public static IEnumerable<Message> EmptyMessages => _emptyMessages;
    }
}
