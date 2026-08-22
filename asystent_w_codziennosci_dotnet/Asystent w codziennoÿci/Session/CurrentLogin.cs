using AssistantLogic.Model;
using Microsoft.AspNetCore.Mvc;

namespace AsystentView.Session
{
    public class CurrentLogin : Controller
    {
        SessionManager sessionManager;

        public string ActualLogin;
        public CurrentLogin(SessionManager sessionManager)
        {
            this.sessionManager = sessionManager;
            if (sessionManager == null)
            {
                ActualLogin = "Nie jesteś zalogowany";
            }
            else
            {
                UserSessionModel user = sessionManager.User;
                if (user != null)
                {
                    ActualLogin = "Aktualny login: " + user.UserName;
                }
            }
        }
    }
}
