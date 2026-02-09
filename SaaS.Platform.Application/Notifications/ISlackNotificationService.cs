using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Application.Notifications
{
    public interface ISlackNotificationService
    {
        Task SendAsync(string message);
    }
}
