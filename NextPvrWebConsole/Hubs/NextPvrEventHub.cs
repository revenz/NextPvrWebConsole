using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;

namespace NextPvrWebConsole.Hubs
{
    [HubName("nextPvrEvent")]
    public class NextPvrEventHub : Hub
    {
        #region client methods
        public void Send(string Message, string Title = null)
        {
            Clients.showInfoMessage(Message);
        }
        #endregion

        #region Static Methods for Server calling Clients
        private static IHubContext GetHubContext()
        {
            return SignalR.GlobalHost.ConnectionManager.GetHubContext<NextPvrEventHub>();
        }

        public static void Clients_ShowInfoMessage(string Message, string Title = null)
        {
            GetHubContext().Clients.showInfoMessage(Message, Title);
        }
        public static void Clients_ShowWarningMessage(string Message, string Title = null)
        {
            GetHubContext().Clients.showWarningMessage(Message, Title);
        }
        public static void Clients_ShowSuccessMessage(string Message, string Title = null)
        {
            GetHubContext().Clients.showSuccessMessage(Message, Title);
        }
        public static void Clients_ShowErrorMessage(string Message, string Title = null)
        {
            GetHubContext().Clients.showErrorMessage(Message, Title);
        }
        #endregion
    }

    class NextPvrEventListener : NUtility.IEventNotification
    {
        public NextPvrEventListener()
        {
            NUtility.EventBus.GetInstance().AddListener(this);
        }

        public void Notify(string eventName, object eventArg)
        {
            NextPvrEventHub.Clients_ShowInfoMessage("NextPVR Event: " + eventName);
        }
    }
}