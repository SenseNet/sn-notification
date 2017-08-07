using SenseNet.ContentRepository.i18n;
using System.Globalization;

namespace SenseNet.Notification
{
    public class MessageTemplate
    {
        private readonly CultureInfo _cultureInfo;

        public MessageTemplate(string langCode)
        {
            _cultureInfo = CultureInfo.CreateSpecificCulture(langCode);
        }

        private const string CLASSNAME = "MessageTemplate";

        public const string IMMEDIATELYSUBJECT = "ImmediatelySubject";
        public const string DAILYSUBJECT = "DailySubject";
        public const string WEEKLYSUBJECT = "WeeklySubject";
        public const string MONTHLYSUBJECT = "MonthlySubject";

        public const string IMMEDIATELYHEADER = "ImmediatelyHeader";
        public const string DAILYHEADER = "DailyHeader";
        public const string WEEKLYHEADER = "WeeklyHeader";
        public const string MONTHLYHEADER = "MonthlyHeader";

        public const string IMMEDIATELYFOOTER = "ImmediatelyFooter";
        public const string DAILYFOOTER = "DailyFooter";
        public const string WEEKLYFOOTER = "WeeklyFooter";
        public const string MONTHLYFOOTER = "MonthlyFooter";

        public const string CREATEDTEMPLATE = "DocumentCreated";
        public const string MAJORVERSIONMODIFIEDTEMPLATE = "DocumentMajorVersionModified";
        public const string MINORVERSIONMODIFIEDTEMPLATE = "DocumentMinorVersionModified";
        public const string COPIEDFROMTEMPLATE = "DocumentCopiedFrom";
        public const string MOVEDFROMTEMPLATE = "DocumentMovedFrom";
        public const string MOVEDTOTEMPLATE = "DocumentMovedTo";
        public const string RENAMEDFROMTEMPLATE = "DocumentRenamedFrom";
        public const string RENAMEDTOTEMPLATE = "DocumentRenamedTo";
        public const string DELETEDTEMPLATE = "DocumentDeleted";
        public const string RESTOREDTEMPLATE = "DocumentRestored";

        public string ImmediatelySubject => SenseNetResourceManager.Current.GetString(CLASSNAME, IMMEDIATELYSUBJECT, _cultureInfo);
        public string DailySubject => SenseNetResourceManager.Current.GetString(CLASSNAME, DAILYSUBJECT, _cultureInfo);
        public string WeeklySubject => SenseNetResourceManager.Current.GetString(CLASSNAME, WEEKLYSUBJECT, _cultureInfo);
        public string MonthlySubject => SenseNetResourceManager.Current.GetString(CLASSNAME, MONTHLYSUBJECT, _cultureInfo);

        public string ImmediatelyHeader => SenseNetResourceManager.Current.GetString(CLASSNAME, IMMEDIATELYHEADER, _cultureInfo);
        public string DailyHeader => SenseNetResourceManager.Current.GetString(CLASSNAME, DAILYHEADER, _cultureInfo);
        public string WeeklyHeader => SenseNetResourceManager.Current.GetString(CLASSNAME, WEEKLYHEADER, _cultureInfo);
        public string MonthlyHeader => SenseNetResourceManager.Current.GetString(CLASSNAME, MONTHLYHEADER, _cultureInfo);

        public string ImmediatelyFooter => SenseNetResourceManager.Current.GetString(CLASSNAME, IMMEDIATELYFOOTER, _cultureInfo);
        public string DailyFooter => SenseNetResourceManager.Current.GetString(CLASSNAME, DAILYFOOTER, _cultureInfo);
        public string WeeklyFooter => SenseNetResourceManager.Current.GetString(CLASSNAME, WEEKLYFOOTER, _cultureInfo);
        public string MonthlyFooter => SenseNetResourceManager.Current.GetString(CLASSNAME, MONTHLYFOOTER, _cultureInfo);

        public string CreatedTemplate => SenseNetResourceManager.Current.GetString(CLASSNAME, CREATEDTEMPLATE, _cultureInfo);
        public string MajorVersionModifiedTemplate => SenseNetResourceManager.Current.GetString(CLASSNAME, MAJORVERSIONMODIFIEDTEMPLATE, _cultureInfo);
        public string MinorVersionModifiedTemplate => SenseNetResourceManager.Current.GetString(CLASSNAME, MINORVERSIONMODIFIEDTEMPLATE, _cultureInfo);
        public string CopiedFromTemplate => SenseNetResourceManager.Current.GetString(CLASSNAME, COPIEDFROMTEMPLATE, _cultureInfo);
        public string MovedFromTemplate => SenseNetResourceManager.Current.GetString(CLASSNAME, MOVEDFROMTEMPLATE, _cultureInfo);
        public string MovedToTemplate => SenseNetResourceManager.Current.GetString(CLASSNAME, MOVEDTOTEMPLATE, _cultureInfo);
        public string RenamedFromTemplate => SenseNetResourceManager.Current.GetString(CLASSNAME, RENAMEDFROMTEMPLATE, _cultureInfo);
        public string RenamedToTemplate => SenseNetResourceManager.Current.GetString(CLASSNAME, RENAMEDTOTEMPLATE, _cultureInfo);
        public string DeletedTemplate => SenseNetResourceManager.Current.GetString(CLASSNAME, DELETEDTEMPLATE, _cultureInfo);
        public string RestoredTemplate => SenseNetResourceManager.Current.GetString(CLASSNAME, RESTOREDTEMPLATE, _cultureInfo);
    }
}
