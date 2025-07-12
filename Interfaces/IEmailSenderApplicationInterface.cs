using Microsoft.AspNetCore.Identity;
using VMart.Models;

namespace VMart.Interfaces
{
    public interface IEmailSenderApplicationInterface
    {
        Task SendContactMessageToAdminAsync(Contact contact);
    }
}
