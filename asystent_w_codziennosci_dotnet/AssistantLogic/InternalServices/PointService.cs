using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;
using AssistantLogic.ViewModel;

namespace AssistantLogic.InternalServices
{
    public class PointService : IPointService
    {
        private IPointRepository pointRepository;
        private ITriggerService triggerService;

        public PointService(IPointRepository pointRepository, ITriggerService triggerService)
        {
            this.pointRepository = pointRepository;
            this.triggerService = triggerService;
        }

        public PointDetailsVM GetPointDetailsVM(int id)
        {
            TaskPointDM pointDM = pointRepository.GetById(id);
            return ToPointDetailsVM(pointDM);
        }

        public List<PointBaseInformationVM> ListOfPointBaseInfoVMForTask(int taskId)
        {
            return ToListOfPointBaseInformationVM(pointRepository.ReadAllForTask(taskId));
        }

        public void Add(PointAddEditVM viewModel)
        {
            TaskPointDM dm = new TaskPointDM();
            dm = ToTaskDM(viewModel, dm);

            pointRepository.Add(dm);
        }
        public void Update(PointAddEditVM viewModel)
        {
            TaskPointDM dm = pointRepository.GetById(viewModel.Id);
            dm = ToTaskDM(viewModel, dm);
            pointRepository.Update(dm);
        }
        public void Delete(int pointId)
        {
            pointRepository.Delete(pointId);
        }

        public void Up(int taskId, int pointId)
        {
            pointRepository.Up(taskId, pointId);
        }
        public void Down(int taskId, int pointId)
        {
            pointRepository.Down(taskId, pointId);
        }
        public void AddTrigger(int pointId, int triggerId, bool negation)
        {
            pointRepository.AddTrigger(pointId, triggerId, negation);
        }
        private List<PointBaseInformationVM> ToListOfPointBaseInformationVM(List<TaskPointDM> listDM)
        {
            List<PointBaseInformationVM> listVM = new List<PointBaseInformationVM>();

            foreach (TaskPointDM dm in listDM)
            {
                listVM.Add(ToPointBaseInformationVM(dm));
            }
            return listVM;
        }
        
        private PointBaseInformationVM ToPointBaseInformationVM(TaskPointDM dm)
        {
            PointBaseInformationVM vm = new PointBaseInformationVM();
            vm.Id = dm.Id;
            vm.Name = dm.Name;
            vm.Description = dm.Description;
            vm.PhotoUrl = dm.PhotoUrl;
            vm.Triggers = triggerService.ShortNameTriggersForPoint(vm.Id);
            return vm;
        }

        public PointAddEditVM ToTaskAddEditVM(TaskPointDM dm)
        {
            PointAddEditVM vm = new PointAddEditVM();

            vm.Id = dm.Id;
            vm.TaskId = dm.TaskId;
            vm.Name = dm.Name;
            vm.Description = dm.Description;
            vm.PhotoUrl = dm.PhotoUrl;
            return vm;
        }
        private TaskPointDM ToTaskDM(PointAddEditVM vm, TaskPointDM dm)
        {
            dm.Id = vm.Id;
            dm.TaskId = vm.TaskId;
            dm.Name = vm.Name;
            dm.Description = vm.Description;
            dm.PhotoUrl = vm.PhotoUrl;
            return dm;
        }

        private PointDetailsVM ToPointDetailsVM(TaskPointDM dm)
        {
            PointDetailsVM vm = new PointDetailsVM();

            vm.Id = dm.Id;
            vm.TaskId = dm.TaskId;
            vm.Name = dm.Name;
            vm.Description = dm.Description;
            vm.Number = dm.Number;
            
            
            if(!string.IsNullOrEmpty(dm.PhotoUrl))
            {
                vm.Image = dm.PhotoUrl;
            }
            else
            {
                vm.Image = "Podzadanie nie zawiera zdjęcia";
            }
            return vm;
        }

    }
}
