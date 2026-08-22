using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.Triggers;
using AssistantLogic.Triggers.Time;
using AssistantLogic.Triggers.TriggersTime;
using AssistantLogic.ViewModel;

namespace AssistantLogic.InternalServices
{
    public class TriggerService : ITriggerService
    {
        private ITriggerRepository triggerRepository;

        public TriggerService(ITriggerRepository triggerRepository)
        {
            this.triggerRepository = triggerRepository;
        }

        public TriggerDM? ToTriggerDM(TriggerAddEditVM vm)
        {
            return TriggerCreator.ConvertToDM(vm);
        }

        public void Add(TriggerDM triggerDM, int caregiverId)
        {
            triggerRepository.Add(triggerDM, caregiverId);
        }

        public void Update(TriggerAddEditVM model)
        {
            //TriggerDM trigger = triggerRepository.GetById(model.Id);
            TriggerDM trigger = TriggerCreator.ConvertToDM(model);
            triggerRepository.Update(trigger);
        }

        public void Delete(int id)
        {
            triggerRepository.Delete(id);
        }

        public TriggerAddEditVM GetTriggerAddEditVM(int id)
        {
            TriggerDM triggerDM = triggerRepository.GetById(id);
            ITrigger trigger = TriggerCreator.CreateTrigger(triggerDM);
            TriggerAddEditVM model = trigger.VM;
            model.Id = id;
            return model;
        }
        public List<TriggerBaseInformationVM> GetTriggersBaseInfoToAddForTask(int taskId, int caregiverId)
        {
            return ToListOfTriggerBaseInformationVM(triggerRepository.ReadAllForTaskToAdd(taskId, caregiverId));
        }
        public List<TriggerBaseInformationVM> GetTriggersBaseInfoForTask(int taskId)
        {
            return ToListOfTriggerBaseInformationVM(triggerRepository.ReadAllForTask(taskId));
        }
        

        /**
         * Zwracamy liste zadań dla których:
         * nie mamy zadnych wyzwalaczy zwiazanych z czasem
         * lub
         * mamy wyzwalacze z czasem, ale wtedy którykolwiek spełnia zadany dzień tygodnia
         */ 
        public bool WorkingForWeekDay(TriggerWeekDayType? weekDay, int taskId)
        {
            if (weekDay == null)
            {
                return true;
            }
            if (weekDay == TriggerWeekDayType.None)
            {
                return true;
            }

            List<TriggerDM> triggersDM = triggerRepository.ReadAllForTask(taskId);
            if(triggersDM.Count == 0)
            {
                return true;
            }

            bool existTimeTrigger = false;
            foreach(TriggerDM triggerDM in triggersDM)
            {
                if (triggerDM.Type == TriggerType.Time && !triggerDM.Negation)
                {

                    TriggerTime triggerTime = new TriggerTime(triggerDM);
                    existTimeTrigger = true;
                    if(triggerTime.IsWorkinForDayWeek(weekDay.Value))
                    {
                        return true;
                    }
                }
            }
            return !existTimeTrigger;
        }

        public List<TriggerBaseInformationVM> ToListOfTriggerBaseInformationVM(List<TriggerDM> listDM)
        {
            List<TriggerBaseInformationVM> listVM = new List<TriggerBaseInformationVM>();

            foreach (TriggerDM dm in listDM)
            {
                listVM.Add(ToTriggerBaseInformationVM(dm));
            }
            return listVM;
        }
        public ListTriggerBaseInformationVM ToListOfTriggerBaseInformationVMWithProtageName(int caregiverId, string ProtageName)
        {
            List<TriggerDM> listDM = triggerRepository.ReadAllForCaregiver(caregiverId);
            List<TriggerBaseInformationVM> listVM = new List<TriggerBaseInformationVM>();
            ListTriggerBaseInformationVM model = new ListTriggerBaseInformationVM();
            foreach (TriggerDM dm in listDM)
            {
                listVM.Add(ToTriggerBaseInformationVM(dm));
            }
            model.actualProtegeName = ProtageName;
            model.Triggers = listVM;
            return model;
        }
        public List<string> ShortNameTriggersForPoint(int pointId)
        {
            return ToListOfTriggerShortName(triggerRepository.ReadAllForPoint(pointId));
        }
        public List<string> ShortNameTriggersForTask(int taskId)
        {
            return ToListOfTriggerShortName(triggerRepository.ReadAllForTask(taskId));
        }


        public List<string> ToListOfTriggerShortName(List<TriggerDM> listDM)
        {
            List<TriggerBaseInformationVM> listVM = new List<TriggerBaseInformationVM>();
            List<string> list = new List<string>();
            foreach (TriggerDM dm in listDM)
            {
                ITrigger trigger = TriggerCreator.CreateTrigger(dm);
                list.Add(trigger.ShortDescription);
            }
            return list;
        }

        private TriggerBaseInformationVM ToTriggerBaseInformationVM(TriggerDM dm)
        {
            TriggerBaseInformationVM vm = new TriggerBaseInformationVM();
            vm.Id = dm.Id;
            vm.Name = dm.Name;
            vm.Type = dm.Type;
            ITrigger trigger = TriggerCreator.CreateTrigger(dm);
            vm.SubType = trigger.SubTypeName;
            vm.Description = trigger.Description;
            vm.ShortDescription = trigger.ShortDescription;
            vm.CanDelete = triggerRepository.AllowDeleteTrigger(dm.Id);
            return vm;
        }

        public List<ITrigger> GetTriggersForTask(int taskId)
        {
            return GetTriggers(triggerRepository.ReadAllForTask(taskId));
        }

        public List<ITrigger> GetTriggersForPoint(int pointId)
        {
            return GetTriggers(triggerRepository.ReadAllForPoint(pointId));
        }

        private List<ITrigger> GetTriggers(List<TriggerDM> listDM)
        {
            List<ITrigger> list = new List<ITrigger>();
            foreach (TriggerDM dm in listDM)
            {
                list.Add(TriggerCreator.CreateTrigger(dm));
            }
            return list;
        }


        public List<TaskDM> CheckTasks(List<TaskDM> listOfTasks, PlanDayParameters parameters)
        {
            List<TaskDM> resultTasks = new List<TaskDM>();

            foreach (TaskDM taskDM in listOfTasks)
            {
                List<ITrigger> triggers = GetTriggersForTask(taskDM.Id);
                if (CheskTriggers(triggers, parameters))
                {
                    resultTasks.Add(taskDM);
                }
            }
            return resultTasks;
        }

        public List<TaskPointDM> CheckPoints(List<TaskPointDM> listOfPoint, PlanDayParameters parameters)
        {
            List<TaskPointDM> resultPoints = new List<TaskPointDM>();
           
            foreach (TaskPointDM pointDM in listOfPoint)
            {
                List<ITrigger> triggers = GetTriggersForPoint(pointDM.Id);
                if (CheskTriggers(triggers, parameters))
                {
                    resultPoints.Add(pointDM);
                }
            }
            return resultPoints;
        }

        public bool CheckTask(TaskDM task, PlanDayParameters parameters)
        {
            List<ITrigger> triggers = GetTriggersForTask(task.Id);
            if (!CheskTriggers(triggers, parameters))
            {
                return false;
            }

            return true;
        }

        private bool CheskTriggers(List<ITrigger> triggers, PlanDayParameters parameters)
        {
            if (triggers.Count == 0)
            {
                return true;
            }
            bool response = false;
            foreach (ITrigger trigger in triggers)
            {
                if (trigger.IsRun(parameters))
                {
                    if(trigger.Negation)
                    {
                        return false;
                    }
                    response = true;
                }
            }
            return response;
        }
    }
}
