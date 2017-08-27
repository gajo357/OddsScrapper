namespace OddsWebsite
{
    public interface IEmailService
    {
        void SendEmail(string name, string email, string subject, string body);
    }
}
