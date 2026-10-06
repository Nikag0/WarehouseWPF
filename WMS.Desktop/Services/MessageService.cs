using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Desktop.Services
{
    public class MessageService(bool isUpdated) : ValueChangedMessage<bool>(isUpdated);
}
