using System;
using System.Collections.Generic;
using System.Configuration.Abstractions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Common;
using NLog.Targets;
using SendGrid;
using SendGrid.Helpers.Mail;
using AsyncHelpers = Akrual.DDD.Utils.Internal.Async.AsyncHelpers;

namespace Akrual.DDD.Utils.WebApi.Logging
{
    [Target("SendGrid")]
    public class MailerSendGrid : TargetWithLayout
	{
		public string SendGridApiKey { get; set; }
		public List<string> DebugEmails { get; set; }
		public string FomEmail { get; set; }
		public string FromText { get; set; }
		public string FooterHtml { get; set; }

		public MailerSendGrid()
		{
			SendGridApiKey = "";
			FooterHtml = "";
			FomEmail = "";
			FromText = "";
			DebugEmails = new List<string>();
		}

	    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public void ComposeMailTask(List<string> Receivers, string Title, string Body, bool isHtml = true)
		{
		    var client = new SendGridClient(SendGridApiKey);

		    var mail =
		        new SendGrid.Helpers.Mail.SendGridMessage
		        {
		            From = new EmailAddress(FomEmail, FromText),
		            Subject = Title
		        };

		    Personalization personalization = new Personalization();
            var emailsTo = new List<EmailAddress>();
            foreach (var receiver in Receivers)
            {
                var emailTo = new EmailAddress(receiver);
                emailsTo.Add(emailTo);
            }
		    personalization.Tos = emailsTo;
		    mail.Personalizations = new List<Personalization>() {personalization};

            Content content = isHtml ? new Content("text/html", Body) : new Content("text/plain", Body);
            mail.Contents = new List<Content>(){content};

            MailSettings mailSettings = new MailSettings();

		    FooterSettings footerSettings = new FooterSettings
		    {
		        Enable = true,
		        Html = FooterHtml
		    };

		    mailSettings.FooterSettings = footerSettings;

            mail.MailSettings = mailSettings;

		    var final = AsyncHelpers.RunSync<Response>(() => SendEmail(client, mail));

            if (final.StatusCode != HttpStatusCode.Accepted)
            {
                throw new ApplicationException("SendGrid had some problem sending Email: " + final.Body.ReadAsStringAsync().Result);
            }
		}

	    private async Task<Response> SendEmail(SendGridClient sg, SendGrid.Helpers.Mail.SendGridMessage mail)
	    {
	        var response = await sg.SendEmailAsync(mail);

	        return response;
	    }

	    protected override void Write(IList<AsyncLogEventInfo> logEvents)
	    {
            string logMessage = "";
	        for (int index = 0; index < logEvents.Count; ++index)
	        {
	            var logEvent = logEvents[index];
                try
                {
                    this.MergeEventProperties(logEvent.LogEvent);
                    logMessage += "<p>" + this.Layout.Render(logEvent.LogEvent) + "</p><br>";
                    logEvent.Continuation((Exception)null);
                }
                catch (Exception ex)
                {
                    if (MustBeRethrown(ex))
                        throw;
                    else
                        logEvent.Continuation(ex);
                }
            }
            
	        if (!String.IsNullOrEmpty(logMessage))
	        {
	            var client = ConfigurationManager.Instance.AppSettings["client"] ?? "akrual";
	            var slot = ConfigurationManager.Instance.AppSettings["slot"] ?? "prod";
                ComposeMailTask(DebugEmails, $"[Log_{client}_{slot}] NLog", logMessage, true);
	        }
        }

	    private bool MustBeRethrown(Exception ex)
	    {
	        if (ex is StackOverflowException || ex is ThreadAbortException || ex is OutOfMemoryException)
	            return true;
	        bool flag = ex is NLogConfigurationException;
	        if (!IsLoggedToInternalLogger(ex))
	        {
	            NLog.LogLevel level = flag ? NLog.LogLevel.Warn : NLog.LogLevel.Error;
	            InternalLogger.Log(ex, level, "Error has been raised.");
	        }
	        int num;
	        if (!flag)
	        {
	            num = LogManager.ThrowExceptions ? 1 : 0;
	        }
	        else
	        {
	            bool? configExceptions = LogManager.ThrowConfigExceptions;
	            num = configExceptions.HasValue ? (configExceptions.GetValueOrDefault() ? 1 : 0) : (LogManager.ThrowExceptions ? 1 : 0);
	        }
	        return num != 0;
        }

	    private bool IsLoggedToInternalLogger(Exception ex)
	    {
	        if (ex != null)
	            return ex.Data[(object)"NLog.ExceptionLoggedToInternalLogger"] as bool? ?? false;
	        return false;
        }
	}
}