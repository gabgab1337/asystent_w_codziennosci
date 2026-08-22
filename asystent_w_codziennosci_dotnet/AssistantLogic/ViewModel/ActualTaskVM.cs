namespace AssistantLogic.ViewModel
{
    public class ActualTaskVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Number { get; set; }
        public bool AnchorBegin { get; set; }
        public string TimeBegin { get; set; } = string.Empty;
        public int TimeBeginValue { get; set; }
        public string Time { get; set; } = string.Empty;
        public int TimeValue { get; set; }
        public bool AnchorEnd { get; set; }
        public string TimeEnd { get; set; } = string.Empty;

        public int TimeEndValue { get; set; }
        public List<ActualPointVM> ActualPoints { get; set; } = new List<ActualPointVM>();

        public int? CurrentPointId { get; set; }

        public bool Done;
    }
}
