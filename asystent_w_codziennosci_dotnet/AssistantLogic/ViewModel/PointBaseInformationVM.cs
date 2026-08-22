namespace AssistantLogic.ViewModel
{
    public class PointBaseInformationVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PhotoUrl { get; set; }
        public List<string> Triggers { get; set; }
    }
}
