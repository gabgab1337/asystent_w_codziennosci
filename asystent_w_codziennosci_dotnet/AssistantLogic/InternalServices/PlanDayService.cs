using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.Common;
using AssistantLogic.IExternalServices;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.Triggers.Weather;
using AssistantLogic.ViewModel;

namespace AssistantLogic.InternalServices
{
    public class PlanDayService : IPlanDayService
    {

        private ITaskRepository taskRepository;
        private IPointRepository pointRepository;
        private ITriggerService triggerService;
        private ITimeService timeService;
        private IWeatherService weatherService;

        public PlanDayService(
            ITaskRepository taskRepository, 
            ITriggerService triggerService, 
            IPointRepository pointRepository, 
            ITimeService timeService,
            IWeatherService weatherService
            )
        {
            this.taskRepository = taskRepository;
            this.triggerService = triggerService;
            this.pointRepository = pointRepository;
            this.timeService = timeService;
            this.weatherService = weatherService;
        }

        public PlanDayParameters UpdatePlanDayParameters(PlanDayParameters planDayParameters)
        {
            if (planDayParameters == null)
            {
                planDayParameters = new PlanDayParameters();
            }
            planDayParameters.CurrentDateTime = DateTime.Now;

           
            if (planDayParameters.Weather == null)
            {
                planDayParameters.Weather = weatherService.GetCurrentWeather().GetAwaiter().GetResult(); 
            }
            else if((DateTime.Now-planDayParameters.Weather.LastUpdate).TotalMinutes>120)
            {
                planDayParameters.Weather = weatherService.GetCurrentWeather().GetAwaiter().GetResult();
            }

            return planDayParameters;
        }

        public PlanDayVM CreateModelPlanDay(PlanDayParameters parameters, int asdPersonId)
        {
            PlanDayVM planDayVM = new PlanDayVM();
            planDayVM.Parameters = UpdatePlanDayParameters(parameters);
            planDayVM.Tasks = GeneratePlan(asdPersonId, parameters);
            return planDayVM;
        }


        public PlanDayPreviewVM CreateModelPlanDayPreview(PlanDayParameters parameters, int asdPersonId, string asdPersonName)
        {
            PlanDayPreviewVM planDayPreviewVM = new PlanDayPreviewVM();
            planDayPreviewVM.actualProtegeName = asdPersonName;
            if (parameters == null)
            {
                parameters = UpdatePlanDayParameters(null);
            }
            planDayPreviewVM.Parameters = parameters;

            planDayPreviewVM.WindLevels = ScaleWind.Calm.GetTypesList();
            planDayPreviewVM.WeatherTypes = WeatherType.Rain.GetTypesList();
            planDayPreviewVM.RainLevels = RainLevel.LightRain.GetTypesList();
            planDayPreviewVM.SnowLevels = SnowLevel.LightSnow.GetTypesList();

            planDayPreviewVM.Tasks = GeneratePlan(asdPersonId, parameters);

            return planDayPreviewVM;
        }

        public List<PlanDayTaskVM> GeneratePlan(int asdPersonId, PlanDayParameters planDayParameters)
        {
            
            List<PlanDayTaskVM> planDay = new List<PlanDayTaskVM>();

            DateTime currentDate = planDayParameters.CurrentDate;
            ///todo
            WeatherModel weather = planDayParameters.Weather;
            TaskDM? lastDoneTask;
            PlanDayDone(planDay, asdPersonId, currentDate.Date, out lastDoneTask);
            PlanDayToDo(planDay, asdPersonId, planDayParameters, currentDate, lastDoneTask);
            return planDay;
        }
        private void PlanDayDone(List<PlanDayTaskVM> planDay, int asdPersonId, DateTime currentDate, out TaskDM? lastDoneTask)
        {
            List<TaskDM> listDoneTaskDM = taskRepository.ReadAllDoneTasksForAsdPerson(asdPersonId, currentDate);

            if (listDoneTaskDM.Count > 0)
            {
                lastDoneTask = listDoneTaskDM[listDoneTaskDM.Count-1];
                int time = listDoneTaskDM[0].TimeBegin.Value;
                foreach (TaskDM taskDM in listDoneTaskDM)
                {
                    PlanDayTaskVM task = new PlanDayTaskVM();
                    task.Id = taskDM.Id;
                    task.Name = taskDM.Name;
                    task.Description = taskDM.Description?.ToHTML();
                    task.AnchorBegin = taskDM.AnchorBegin;
                    task.AnchorEnd = taskDM.AnchorEnd;
                    task.Time = taskDM.Time;

                    task.State = RealizationState.Done;
                    task.TimeBegin = timeService.ConvertToTimeHHMM(time);
                    DateTime finishTime = taskRepository.ReadLastDoneTaskTime(task.Id, currentDate);
                    int timeEnd = finishTime.Hour * 60 + finishTime.Minute;
                    task.TimeEnd = timeService.ConvertToTimeHHMM(timeEnd);
                    int realTime = timeEnd - time;
                    int delay = task.Time - realTime;
                    if (delay < 0)
                    {
                        task.Message = "Zadanie zrealizowane wolniej o " + timeService.ConvertToTimeHourMinutes(-delay);
                        task.MessagePositiv = false;
                    }
                    else
                    {
                        task.Message = "Zadanie zrealizowane szybciej o " + timeService.ConvertToTimeHourMinutes(delay);
                        task.MessagePositiv = true;
                    }
                    time = timeEnd;
                    taskDM.Points = pointRepository.ReadAllDonePointsForTask(task.Id);
                    //ReadPointsToTask(taskDM, planDayParameters);
                    task.Points = GeneratePlanPoints(taskDM.Points, RealizationState.Done);
                    
                    if (task.Points.Count == 0)
                    {
                        PlanDayPointVM point = GetDefaultPoint(taskDM, RealizationState.Done);
                        task.Points.Add(point);
                    }

                    planDay.Add(task);
                }
            }
            else
            {
                lastDoneTask = null;
            }
        }

        private void PlanDayToDo(List<PlanDayTaskVM> planDay, int asdPersonId, PlanDayParameters planDayParameters, DateTime currentDateTime, TaskDM? lastDoneTask)
        {
            List<TaskDM> listToDoTaskDM = taskRepository.ReadAllToDoTasksForAsdPerson(asdPersonId, currentDateTime.Date);
            listToDoTaskDM = triggerService.CheckTasks(listToDoTaskDM, planDayParameters);

            List<TaskDM> remainingTask = new List<TaskDM>();
            remainingTask.AddRange(listToDoTaskDM);


            DateTime lastTaskFinishTime = planDayParameters.CurrentDateTime;

          //  TaskDM? taskBefore = lastDoneTask;

            for (int i = 0; i < listToDoTaskDM.Count; i++)
            {
                TaskDM taskDM = listToDoTaskDM[i];
               
                PlanDayTaskVM task = new PlanDayTaskVM();
                task.Id = taskDM.Id;
                task.Name = taskDM.Name;
                task.Description = taskDM.Description?.ToHTML();
                task.AnchorBegin = taskDM.AnchorBegin;
                task.AnchorEnd = taskDM.AnchorEnd;

                int timeBegin = timeService.GetTimeBegin(taskDM, lastDoneTask, currentDateTime, currentDateTime.Date);
                lastDoneTask = null;
                task.TimeBegin = timeService.ConvertToTimeHHMM(timeBegin);
                remainingTask.Remove(taskDM);
                int timeEnd = timeService.GetTimeEnd(taskDM, remainingTask, timeBegin);

                int time = timeEnd - timeBegin;
                lastTaskFinishTime = lastTaskFinishTime.AddMinutes(time);
                currentDateTime = lastTaskFinishTime;
                task.TimeEnd = timeService.ConvertToTimeHHMM(timeEnd);
                task.Time = time;

                if (taskDM.Time > time)
                {
                    int diff = taskDM.Time - time;
                    task.Message = "Zadanie należy zrealizować szybciej o " + timeService.ConvertToTimeHourMinutes(diff);
                    task.MessagePositiv = false;
                }
              
                task.Points = new List<PlanDayPointVM>();
                if (i == 0)
                {
                    task.Points.AddRange(GeneratePlanPoints(pointRepository.ReadAllDonePointsForTask(task.Id), RealizationState.ToDo));
                }

                List<TaskPointDM> listOfPoint = pointRepository.ReadAllToDoPointsForTask(taskDM.Id);
                listOfPoint = triggerService.CheckPoints(listOfPoint, planDayParameters);
                task.Points.AddRange(GeneratePlanPoints(listOfPoint, RealizationState.ToDo));
                if (task.Points.Count == 0)
                {
                    PlanDayPointVM point = GetDefaultPoint(taskDM, RealizationState.ToDo);
                    task.Points.Add(point);
                }
                planDay.Add(task);
             //   taskBefore = taskDM;
            }
        }

        private PlanDayPointVM GetDefaultPoint(TaskDM task, RealizationState state)
        {
            PlanDayPointVM planDayPointVM = new PlanDayPointVM();
            planDayPointVM.Id = -1;
            planDayPointVM.Name = task.Name;
            planDayPointVM.State = state;
            return planDayPointVM;
        }

        

        private List<PlanDayPointVM> GeneratePlanPoints(ICollection<TaskPointDM> listDM, RealizationState? state)
        {
            List<PlanDayPointVM> listVM = new List<PlanDayPointVM>();
            foreach (TaskPointDM pointDM in listDM)
            {
                PlanDayPointVM planDayPointVM = new PlanDayPointVM();
                planDayPointVM.Id = pointDM.Id;
                planDayPointVM.Name = pointDM.Name;
                planDayPointVM.Description = pointDM.Description?.ToHTML();
                if (state != null)
                {
                    planDayPointVM.State = state.Value;
                }
                listVM.Add(planDayPointVM);
            }
            return listVM;
        }

      

        ///wspolne

       

       

    }
}
