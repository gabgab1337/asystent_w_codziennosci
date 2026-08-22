namespace AssistantLogic.ViewModel
{
    public class ListTriggerBaseInformationVM
    {
        public string actualProtegeName { get; set; }

        public List<TriggerBaseInformationVM> Triggers { get; set; } = new List<TriggerBaseInformationVM>();
    }
}
