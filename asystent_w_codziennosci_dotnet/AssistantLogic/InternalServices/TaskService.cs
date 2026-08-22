using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;
using AssistantLogic.Triggers.TriggersTime;
using AssistantLogic.ViewModel;

namespace AssistantLogic.InternalServices
{
    public class TaskService : ITaskService
    {
        private ITaskRepository taskRepository;
        private IUserRepository userRepository;
        private ITriggerService triggerService;
        private IPointService pointService;

        public TaskService(ITaskRepository taskRepository, IUserRepository userRepository, ITriggerService triggerService, IPointService pointService)
        {
            this.taskRepository = taskRepository;
            this.userRepository = userRepository;
            this.triggerService = triggerService;
            this.pointService = pointService;
        }

        public ListTaskBaseInformationVM ListOfTaskBaseInfoVMForAsdPerson(int asdPersonId, string asdPersonName, TriggerWeekDayType? weekDay)
        {
            ListTaskBaseInformationVM model = new ListTaskBaseInformationVM();

            List<TaskDM> listDM = ReadAllForAsdPersonAndWeekDay(asdPersonId, weekDay);
            model.Tasks = ToListOfTaskBaseInformationVM(listDM);
            model.actualProtegeName = asdPersonName;
            foreach (TaskBaseInformationVM vm in model.Tasks)
            {
                vm.Triggers = triggerService.ShortNameTriggersForTask(vm.Id);
            }

            return model;
        }
        private List<TaskDM> ReadAllForAsdPersonAndWeekDay(int asdPersonId, TriggerWeekDayType? weekDay)
        {
            List<TaskDM> listDM = taskRepository.ReadAllForAsdPerson(asdPersonId);
            for (int i = 0; i < listDM.Count; i++)
            {
                if (!triggerService.WorkingForWeekDay(weekDay, listDM[i].Id))
                {
                    listDM.RemoveAt(i);
                    i--;
                }
            }
            return listDM;
        }


        public TaskDetailsVM GetTaskDetailsVM(int id, int caregiverId)
        {
            TaskDM taskDM = taskRepository.GetByIdWithDetails(id);
            TaskDetailsVM task = ToTaskDetailsVM(taskDM);
            task.TriggersToAdd = triggerService.GetTriggersBaseInfoToAddForTask(id, caregiverId);
            task.Triggers = triggerService.GetTriggersBaseInfoForTask(id);
            task.Points = pointService.ListOfPointBaseInfoVMForTask(id);
            return task;
        }

        public void Add(TaskAddEditVM viewModel, int caregiverId, int asdPersonID)
        {
            taskRepository.Add(ToTaskDM(viewModel), caregiverId, asdPersonID);
        }

        public void Update(TaskAddEditVM model)
        {
            TaskDM task = taskRepository.GetById(model.Id);
            task = ToTaskDM(model, task);
            taskRepository.Update(task);
        }
        public void Delete(int id)
        {
            taskRepository.Delete(id);
        }
        public void Up(int asdPersonId, int taskId, TriggerWeekDayType? weekDay)
        {
            if(weekDay==null || weekDay.Value == TriggerWeekDayType.None)
            {
                taskRepository.Up(asdPersonId, taskId);
                return;
            }

            List<TaskDM> listDM = ReadAllForAsdPersonAndWeekDay(asdPersonId, weekDay);
            TaskDM? task = listDM.Where(t => t.Id == taskId).FirstOrDefault();
            if(task == null)
            {
                return;
            }
            int index = listDM.IndexOf(task);
            if(index == 0)
            {
                return;
            }

            while (taskRepository.UpOverNumber(asdPersonId, taskId, listDM[index - 1].Number));
        }
        public void Down(int asdPersonId, int taskId, TriggerWeekDayType? weekDay)
        {
            if (weekDay == null || weekDay.Value == TriggerWeekDayType.None)
            {
                taskRepository.Down(asdPersonId, taskId);
            }

            List<TaskDM> listDM = ReadAllForAsdPersonAndWeekDay(asdPersonId, weekDay);
            TaskDM? task = listDM.Where(t => t.Id == taskId).FirstOrDefault();
            if (task == null)
            {
                return;
            }
            int index = listDM.IndexOf(task);
            if (index == listDM.Count-1)
            {
                return;
            }

            while (taskRepository.DownUnderNumber(asdPersonId, taskId, listDM[index + 1].Number)) ;
        }
        public TaskAddEditVM GetTaskAddEditVM(int id)
        {
            TaskDM task = taskRepository.GetById(id);
            return ToTaskAddEditVM(task);
        }
        public void RemoveTrigger(int taskId, int triggerId)
        {
            taskRepository.RemoveTrigger(taskId, triggerId);
        }
        public void AddTrigger(int taskId, int triggerId, bool negation)
        {
            taskRepository.AddTrigger(taskId, triggerId, negation);
        }

        private List<TaskBaseInformationVM> ToListOfTaskBaseInformationVM(List<TaskDM> listDM)
        {
            List<TaskBaseInformationVM> listVM = new List<TaskBaseInformationVM>();

            foreach (TaskDM dm in listDM)
            {
                listVM.Add(ToUserBaseInformationVM(dm));
            }
            return listVM;
        }

        private TaskBaseInformationVM ToUserBaseInformationVM(TaskDM dm)
        {
            TaskBaseInformationVM vm = new TaskBaseInformationVM();
            vm.Id = dm.Id;
            vm.Name = dm.Name;
            vm.AnchorBegin = dm.AnchorBegin;
            vm.AnchorEnd = dm.AnchorEnd;
            vm.Time = FromMinutesToTimeInWords(dm.Time);
            vm.TimeBegin = FromMinutesToTime(dm.TimeBegin);
            vm.TimeEnd = FromMinutesToTime(dm.TimeEnd);
            return vm;
        }


        private TaskAddEditVM ToTaskAddEditVM(TaskDM dm)
        {
            TaskAddEditVM vm = new TaskAddEditVM();

            vm.Id = dm.Id;
            vm.Name = dm.Name;
            vm.Description = dm.Description;
            vm.AnchorBegin = dm.AnchorBegin;
            vm.AnchorEnd = dm.AnchorEnd;
            vm.Time = FromMinutesToTimeInWords(dm.Time);
            vm.Hours = ReturnHour(dm.Time);
            vm.Minutes = ReturnMinute(dm.Time);
            vm.TimeBegin = FromMinutesToTime(dm.TimeBegin);
            vm.TimeEnd = FromMinutesToTime(dm.TimeEnd);
            return vm;
        }
        private TaskDM ToTaskDM(TaskAddEditVM vm)
        {
            TaskDM dm = new TaskDM();
            return ToTaskDM(vm, dm);
        }
        private TaskDM ToTaskDM(TaskAddEditVM vm, TaskDM dm)
        {
            dm.Id = vm.Id;
            dm.Name = vm.Name;
            dm.Description = vm.Description;
            dm.AnchorBegin = vm.AnchorBegin;
            dm.AnchorEnd = vm.AnchorEnd;

            dm.Time = FromTimeToMinutesInWords(vm.Hours, vm.Minutes).Value;
            dm.TimeBegin = FromTimeToMinutes(vm.TimeBegin);
            dm.TimeEnd = FromTimeToMinutes(vm.TimeEnd);

            return dm;
        }

        private int ReturnHour(int time)
        {
            return time /= 60;
        }
        private int ReturnMinute(int time)
        {
            return time %= 60;
        }
        private string? FromMinutesToTimeInWords(int? minutes)
        {
            if (!minutes.HasValue)
            {
                return null;
            }

            var min = minutes.Value % 60;
            var hours = minutes.Value / 60;
            if ((hours == 0) && (minutes == 0))
            {
                return "";
            }
            else if (hours == 0)
            {
                return $"{min} min";
            }
            else if (minutes == 0)
            {
                return $"{hours} godz";
            }
            else
            {
                return $"{hours} godz {min} min";
            }
        }
        private int? FromTimeToMinutesInWords(int? hours, int? minutes)
        {
            return 60 * hours + minutes;
        }
        private string? FromMinutesToTime(int? minutes)
        {
            if(!minutes.HasValue)
            {
                return null;
            }

            var min = minutes.Value % 60;
            var hours = minutes.Value / 60;

            return $"{hours:D2}:{min:D2}";
        }

        private int? FromTimeToMinutes(string? time)
        {
            if (time == null || time.Length == 0)
                return null;

            string[] parts = time.Split(':');
            return 60 * int.Parse(parts[0]) + int.Parse(parts[1]);
        }

        private TaskDetailsVM ToTaskDetailsVM(TaskDM dm)
        {
            TaskDetailsVM vm = new TaskDetailsVM();

            vm.Id = dm.Id;
            vm.Name = dm.Name;
            vm.Description = dm.Description;
            //  vm.Points = new List<PointBaseInformationVM>();
            //  TaskPointVM point1 = new TaskPointVM();
            //  point1.Id = 1;
            //  point1.AvgTime = 3;
            //  point1.MinTime = 2;
            //  point1.Name = "Ubieranie sie";
            //  point1.Description = "Ubierz czsta bielizne, czyste skarpetki i koszulke";
            //  vm.Points.Add(point1);

            return vm;
        }
    }
}
