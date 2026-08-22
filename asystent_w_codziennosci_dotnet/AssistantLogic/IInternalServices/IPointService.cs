using AssistantLogic.ViewModel;

namespace AssistantLogic.IInternalServices
{
    public interface IPointService
    {
        PointDetailsVM GetPointDetailsVM(int id);
        List<PointBaseInformationVM> ListOfPointBaseInfoVMForTask(int taskId);
        void Add(PointAddEditVM viewModel);
        void Update(PointAddEditVM viewModel);
        void Delete(int pointId);
        void AddTrigger(int taskId, int triggerId, bool negation);
        void Up(int taskId, int pointId);
        void Down(int taskId, int pointId);
    }
}
