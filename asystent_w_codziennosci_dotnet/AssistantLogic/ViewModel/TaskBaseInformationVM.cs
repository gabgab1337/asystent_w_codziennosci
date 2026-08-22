namespace AssistantLogic.ViewModel
{
    public class TaskBaseInformationVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TimeBegin { get; set; }
        public string Time { get; set; }
        public string TimeEnd { get; set; }
        public bool AnchorBegin { get; set; }
        public bool AnchorEnd { get; set; }
        public List<string> Triggers { get; set; }
    }
}
