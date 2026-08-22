namespace AssistantLogic.ViewModel
{
    public class ActualActivityBaseInfoVM
    {
        public ActualActivityBaseInfoVM() {
            Name = "";
            State = AstualActivityState.None;
        }
        
        public string Name { get; set; }

        public AstualActivityState State { get; set; }
    }
}
