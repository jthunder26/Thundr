using SendGrid.Helpers.Mail;
using SendGrid;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Thunder.Services
{
    public interface IMailService
    {

        Task SendEmailAsync(string toEmail, string subject, string content);
        Task SendEmailWithAttachmentAsync(string toEmail, string subject, string content, byte[] attachmentBytes, string attachmentFileName);
        Task testEmail();
    }




    public class MailService : IMailService
    {
        private IConfiguration _configuration;
        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task testEmail()
        {
            var client = new SendGridClient(_configuration["SendGridAPIKey"]);
            var templateId = "d-2b9042dfb827468ab181ebfb5a862a54";

            var response = await client.RequestAsync(
                method: SendGridClient.Method.GET,
                urlPath: $"templates/{templateId}"
            );

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var jsonResponse = JObject.Parse(await response.Body.ReadAsStringAsync());

                // If the template has versions, get HTML content of the first version
                if (jsonResponse["versions"] != null && jsonResponse["versions"].HasValues)
                {
                    string htmlContent = jsonResponse["versions"][0]["html_content"].ToString();

                    // Now you have the HTML content of the first version
                    Console.WriteLine(htmlContent);
                }
            }

        }


        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string content, byte[] attachmentBytes, string attachmentFileName)
        {
            var client = new SendGridClient(_configuration["SendGridAPIKey"]);
            var from = new EmailAddress("support@thundrship.com", "Thundr Support");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            var fileContent = Convert.ToBase64String(attachmentBytes);
            msg.AddAttachment(attachmentFileName, fileContent, "application/pdf");
            var response = await client.SendEmailAsync(msg);
            //Console.Write(response.ToString());
        }
        public async Task SendEmailAsync(string toEmail, string subject, string content)
        {
            var apiKey = _configuration["SendGridAPIKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("support@thundrship.com", "Thundr Support");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            var response = await client.SendEmailAsync(msg);
           // Console.Write(response.ToString());
        }
    }
}
