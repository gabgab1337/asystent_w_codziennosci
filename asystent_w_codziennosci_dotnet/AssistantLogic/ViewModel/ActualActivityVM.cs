namespace AssistantLogic.ViewModel
{
    public class ActualActivityVM
    {
        public string? TaskNameBefore {  get; set; }
        public ActualTaskVM? TaskVM { get; set; } = new ActualTaskVM();
        public bool IsBegin { get; set; }
        public bool IsEnd { get; set; }
        public string PhotoUrl { get; set; }
        public string? TaskNameAfter { get; set; }
    }
}
