using System.Text.Json;
using System.Text;
using AssistantDatabase.Model;
using AssistantLogic.Model;
using AssistantLogic.ViewModel;


namespace AsystentView.Session
{
    public class SessionManager
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext.Session;

        //public SessionManager(ISession session)
        // {
        // this.session = session;
        // }

        public ActualActivityVM ActualActivity
        {
            get
            {
                ActualActivityVM user = null;
                byte[] bytes = null;
                if (Session.TryGetValue("ActualActivity", out bytes))
                {
                    string jsonString = Encoding.UTF8.GetString(bytes);
                    user = JsonSerializer.Deserialize<ActualActivityVM>(jsonString);
                }

                return user;
            }
            set
            {
                string jsonString = JsonSerializer.Serialize(value);
                byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
                Session.Set("ActualActivity", bytes);
            }
        }

        public UserSessionModel User
        {
            get
            {
                UserSessionModel user = null;
                byte[] bytes = null;
                if (Session.TryGetValue("user", out bytes))
                {
                    string jsonString = Encoding.UTF8.GetString(bytes);
                    user = JsonSerializer.Deserialize<UserSessionModel>(jsonString);
                }

                return user;
            }
            set
            {
                string jsonString = JsonSerializer.Serialize(value);
                byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
                Session.Set("user", bytes);
            }
        }
       

        public bool AdminPermisions()
        {
            UserSessionModel user = User;
            if (user != null && (user.Type == UserType.administrator))
            {
                return true;
            }
            return false;
        }

        public bool CaregiverPermisions()
        {
            UserSessionModel user = User;
            if (user != null && (user.Type == UserType.caregiver))
            {
                return true;
            }
            return false;
        }

        public bool AsdPersonPermisions()
        {
            UserSessionModel user = User;
            if (user != null && (user.Type == UserType.asdPerson))
            {
                return true;
            }
            return false;
        }
    }
}
