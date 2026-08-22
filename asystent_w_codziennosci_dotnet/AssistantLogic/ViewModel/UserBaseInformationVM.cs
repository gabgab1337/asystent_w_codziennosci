using AssistantDatabase.Model;

namespace AssistantLogic.ViewModel
{
    public class UserBaseInformationVM
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public UserType Type { get; set; }
        public bool IsActive { get; set; }
        public string JoiningDate { get; set; }
        public bool CanDelete { get; set; }
    }
}
