using AssistantDatabase.Model;

namespace AssistantLogic.ViewModel
{
    public class ActualUserVM
    {
        public List<UserBaseInformationVM> Users { get; set; }
        public List<AsdUserBaseInformationVM> Protages { get; set; }
        public string Message { get; set; }
        public UserType ActualType { get; set; }
    }
}
