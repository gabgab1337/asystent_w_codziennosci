using System.Data;
using AssistantDatabase;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace AssistantLogic.InternalServices
{
    public class CurrentActivityService : ICurrentActivityService
    {

        private readonly DataContext context;
        private readonly ITaskRepository taskRepository;
        private readonly IPointRepository pointRepository;
        private readonly IUserRepository userRepository;
        private readonly ITriggerService triggerService;
        private readonly ITimeService timeService;

        public CurrentActivityService(
            DataContext context,
            ITaskRepository taskRepository,
            ITriggerService triggerService,
            IPointRepository pointRepository,
            IUserRepository userRepository,
            ITimeService timeService)
        {
            this.context = context;
            this.taskRepository = taskRepository;
            this.triggerService = triggerService;
            this.pointRepository = pointRepository;
            this.userRepository = userRepository;
            this.timeService = timeService;
        }

        public async Task<ActivityCompletionResult> CompletePointAsync(
            int asdPersonId,
            int pointId,
            CancellationToken cancellationToken = default)
        {
            // Serializable prevents two concurrent retries from both observing the point/task as
            // unfinished. Repository SaveChanges calls enlist in this transaction automatically.
            await using var transaction = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            TaskPointDM? point = pointRepository.ReadOwnedByAsdPerson(pointId, asdPersonId);
            UserDM? user = userRepository.GetById(asdPersonId);
            if (point == null || user == null)
            {
                return new ActivityCompletionResult { Found = false };
            }

            DateTime today = DateTime.Today;
            bool pointWasDone = pointRepository
                .ReadAllDonePointsForTask(point.TaskId)
                .Any(donePoint => donePoint.Id == pointId);

            if (!pointWasDone)
            {
                pointRepository.DonePoint(pointId);
            }

            bool taskHasRemainingPoints = pointRepository
                .ReadAllToDoPointsForTask(point.TaskId)
                .Count > 0;
            bool taskWasDone = taskRepository
                .ReadAllDoneTasksForAsdPerson(asdPersonId, today)
                .Any(doneTask => doneTask.Id == point.TaskId);

            bool taskCompleted = taskWasDone || !taskHasRemainingPoints;
            if (!taskWasDone && !taskHasRemainingPoints)
            {
                taskRepository.DoneTask(point.TaskId);
            }

            // Weather-dependent trigger selection belongs to the next GET. Clearing the pointer
            // is the safe state transition here: the next read chooses the next eligible task.
            if (taskCompleted && user.CurrentTaskId == point.TaskId)
            {
                userRepository.SetCurrentTaskId(user.Id, null);
            }

            await transaction.CommitAsync(cancellationToken);

            return new ActivityCompletionResult
            {
                Found = true,
                TaskId = point.TaskId,
                PointId = pointId,
                AlreadyCompleted = pointWasDone,
                TaskCompleted = taskCompleted,
                CurrentTaskId = user.CurrentTaskId
            };
        }

        public async Task<ActivityCompletionResult> CompleteTaskAsync(
            int asdPersonId,
            int taskId,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            TaskDM? task = taskRepository.ReadOwnedByAsdPerson(taskId, asdPersonId);
            UserDM? user = userRepository.GetById(asdPersonId);
            if (task == null || user == null)
            {
                return new ActivityCompletionResult { Found = false };
            }

            bool taskWasDone = taskRepository
                .ReadAllDoneTasksForAsdPerson(asdPersonId, DateTime.Today)
                .Any(doneTask => doneTask.Id == taskId);

            if (!taskWasDone)
            {
                taskRepository.DoneTask(taskId);
            }

            // Do not clear a pointer that has already moved on. This makes a retry of the same
            // explicit task id harmless even when a GET selected the next task in the meantime.
            if (user.CurrentTaskId == taskId)
            {
                userRepository.SetCurrentTaskId(user.Id, null);
            }

            await transaction.CommitAsync(cancellationToken);

            return new ActivityCompletionResult
            {
                Found = true,
                TaskId = taskId,
                PointId = null,
                AlreadyCompleted = taskWasDone,
                TaskCompleted = true,
                CurrentTaskId = user.CurrentTaskId
            };
        }

        public async Task<ActualActivityVM> CreateAndPersistActualActivityAsync(
            int asdPersonId,
            PlanDayParameters parameters,
            DateTime currentDate,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            UserDM user = userRepository.GetById(asdPersonId)
                ?? throw new InvalidOperationException($"User {asdPersonId} no longer exists.");

            ActualActivityVM activity = CreateActualActivity(
                asdPersonId,
                user.CurrentTaskId,
                parameters,
                currentDate);

            int? nextTaskId = activity.TaskVM != null && activity.TaskVM.Id > 0
                ? activity.TaskVM.Id
                : null;
            userRepository.SetCurrentTaskId(asdPersonId, nextTaskId);

            await transaction.CommitAsync(cancellationToken);
            return activity;
        }

        public void DonePoint(int taskId, int? pointId)
        {
            if (pointId == null)
            {
                taskRepository.DoneTask(taskId);
            }
            else
            {
                pointRepository.DonePoint(pointId.Value);
            }
        }

        public ActualActivityVM CreateActualActivity(int asdPersonId, int? currentTaskId, PlanDayParameters parameters, DateTime currentDate)
        {
            List<TaskDM> listToDoTaskDM = taskRepository.ReadAllToDoTasksForAsdPerson(asdPersonId, currentDate);
            listToDoTaskDM = triggerService.CheckTasks(listToDoTaskDM, parameters);

            TaskDM? task = GetCurrentTask(listToDoTaskDM, currentTaskId, parameters);
            List<ActualPointVM> toDoPoints = new List<ActualPointVM>();
            List<ActualPointVM> donePoints = new List<ActualPointVM>();
            if (task != null)
            {
                toDoPoints = GetToDoPoints(task, parameters);
                donePoints = GetDanePoints(task);
                while (task != null && toDoPoints.Count == 0 && donePoints.Count > 0)
                {
                    taskRepository.DoneTask(task.Id);
                    if (listToDoTaskDM.Count > 0)
                    {
                        task = listToDoTaskDM[0];
                        // GetCurrentTask already took the previous task out of this list. Drop the one
                        // just promoted too, so the next iteration cannot complete it again and the
                        // following-task name is not the task now in progress.
                        listToDoTaskDM.RemoveAt(0);
                        toDoPoints = GetToDoPoints(task, parameters);
                        donePoints = GetDanePoints(task);
                    }
                    else
                    {
                        task = null;
                    }
                }
            }


            ActualActivityVM actualActivity = new ActualActivityVM();



            TaskDM? taskBefore = taskRepository.ReadLastDoneTaskForAsdPerson(asdPersonId, DateTime.Today);
            actualActivity.TaskNameBefore = taskBefore?.Name;

            TaskDM? taskAfter = null;
            if (listToDoTaskDM.Count > 0)
            {
                taskAfter = listToDoTaskDM[0];
            }


            actualActivity.TaskNameAfter = taskAfter?.Name;

            if (task != null)
            {
                actualActivity.TaskVM = new ActualTaskVM();
                actualActivity.TaskVM.Id = task.Id;
                actualActivity.TaskVM.Name = task.Name;
                actualActivity.TaskVM.Description = task.Description;
                actualActivity.TaskVM.Number = task.Number;
                actualActivity.TaskVM.AnchorBegin = task.AnchorBegin;
                actualActivity.TaskVM.AnchorEnd = task.AnchorEnd;

                int timeBegin = GetTimeBegin(task, taskBefore);
                int timeEnd = GetTimeEnd(task, listToDoTaskDM, timeBegin);

                actualActivity.TaskVM.TimeBegin = timeService.ConvertToTimeHHMM(timeBegin);
                actualActivity.TaskVM.TimeBeginValue = timeBegin;
                actualActivity.TaskVM.TimeEnd = timeService.ConvertToTimeHHMM(timeEnd);
                actualActivity.TaskVM.TimeEndValue = timeEnd;
                //tutaj dodać przypadek jak sie okaze ze nie ma juz zadnych punktów do zrealizowania



                actualActivity.TaskVM.ActualPoints = donePoints;
                if (donePoints.Count + toDoPoints.Count == 0)
                {
                    actualActivity.TaskVM.ActualPoints.Add(GetDefaultPoint(task));
                }
                actualActivity.TaskVM.ActualPoints.AddRange(toDoPoints);
                if (toDoPoints.Count > 0)
                {
                    actualActivity.TaskVM.CurrentPointId = toDoPoints[0].Id;
                }
            }
            else
            {
                actualActivity.TaskVM = new ActualTaskVM();
                actualActivity.TaskVM.Id = -1;
                actualActivity.TaskVM.Name = "Czas wolny";
                actualActivity.TaskVM.Description = "Na dzisiaj nie ma już więcej zadań";
                actualActivity.TaskVM.Number = 0;
                actualActivity.TaskVM.AnchorBegin = false;
                actualActivity.TaskVM.AnchorEnd = false; ;

                int timeBegin = GetTimeBegin(task, taskBefore);
                int timeEnd = GetTimeEnd(task, listToDoTaskDM, timeBegin);

                actualActivity.TaskVM.TimeBeginValue = timeBegin;
                actualActivity.TaskVM.TimeBegin = timeService.ConvertToTimeHHMM(timeBegin);
                actualActivity.TaskVM.TimeEndValue = timeEnd;
                actualActivity.TaskVM.TimeEnd = timeService.ConvertToTimeHHMM(timeEnd);

            }

            return actualActivity;
        }

        private List<ActualPointVM> GetToDoPoints(TaskDM task, PlanDayParameters parameters)
        {
            List<ActualPointVM> actualPoints = new List<ActualPointVM>();
            //jezeli zadanie nie ma punktow to dodajemy standadrowy punkt o tresci takiej jak zadanie
            /* if (pointRepository.ReadAllForTask(task.Id).Count == 0)
             {
                 ActualPointVM actualPointVM = new ActualPointVM();
                 actualPointVM.Id = null;
                 actualPointVM.Name = task.Name;
                 actualPointVM.Done = false;
                 actualPointVM.Active = true;
                 actualPoints.Add(actualPointVM);
                 return actualPoints;
             }*/

            List<TaskPointDM> toDoPoints = pointRepository.ReadAllToDoPointsForTask(task.Id);
            toDoPoints = triggerService.CheckPoints(toDoPoints, parameters);
            
            ///tu ustawiam aktualne zadanie
            if (toDoPoints.Count > 0)
            {
                TaskPointDM actualPointDM = toDoPoints[0];
                ActualPointVM actualPointVM = new ActualPointVM();
                actualPointVM.Id = actualPointDM.Id;
                actualPointVM.Name = actualPointDM.Name;
                actualPointVM.Description = actualPointDM.Description;
                actualPointVM.PhotoUrl = actualPointDM.PhotoUrl;
                actualPointVM.Done = false;
                actualPointVM.Active = true;
                actualPoints.Add(actualPointVM);
            }
            for (int i = 1; i < toDoPoints.Count; i++)
            {
                TaskPointDM pointDM = toDoPoints[i];
                ActualPointVM actualPointVM = new ActualPointVM();
                actualPointVM.Id = pointDM.Id;
                actualPointVM.Name = pointDM.Name;
                actualPointVM.Description = null;
                actualPointVM.Done = false;
                actualPointVM.Active = false;
                actualPoints.Add(actualPointVM);
            }

            return actualPoints;
        }

        private List<ActualPointVM> GetDanePoints(TaskDM task)
        {

            List<ActualPointVM> actualPoints = new List<ActualPointVM>();

            List<TaskPointDM> donePoints = pointRepository.ReadAllDonePointsForTask(task.Id);

            foreach (TaskPointDM pointDM in donePoints)
            {
                ActualPointVM actualPointVM = new ActualPointVM();
                actualPointVM.Id = pointDM.Id;
                actualPointVM.Name = pointDM.Name;
                actualPointVM.Description = null;
                actualPointVM.Done = true;
                actualPointVM.Active = false;
                actualPoints.Add(actualPointVM);
            }

            return actualPoints;
        }

        //todo
        //zalezne od klikniecia rozpoczecia zadania
        private int GetTimeBegin(TaskDM task, TaskDM? taskBefore)
        {
            DateTime lastTaskFinishTime = DateTime.Now;
            if (taskBefore != null)
            {
                lastTaskFinishTime = taskRepository.ReadLastDoneTaskTime(taskBefore.Id, DateTime.Today);
            }
            if (task == null)
            {
                return lastTaskFinishTime.Hour * 60 + lastTaskFinishTime.Minute;
            }

            if (task.AnchorBegin)
            {
                DateTime anchorTime = DateTime.Today.AddMinutes(task.TimeBegin.Value);
                if (anchorTime.CompareTo(lastTaskFinishTime) <= 0) //jestespy spoznieni i musimy natychmiast startować
                {
                    if(taskBefore==null)
                    {
                        return timeService.ConvertToMinutes(anchorTime);
                    }
                    else
                    {
                        return timeService.ConvertToMinutes(lastTaskFinishTime);
                    }
                    
                }
                else
                {
                    return task.TimeBegin.Value;
                }
            }
            else
            {
                return timeService.ConvertToMinutes(lastTaskFinishTime);
            }
        }

        private int GetTimeEnd(TaskDM task, List<TaskDM> listToDoTaskDM, int timeBegin)
        {
            if (task == null)
            {
                int endTime = 22 * 60;
                return endTime;
            }

            if (task.AnchorEnd)
            {
                return task.TimeEnd.Value;
            }

            int sumMinutes = task.Time;
            bool isAnhor = false;
            int anhorTime = 0;
            for (int i = 0; i < listToDoTaskDM.Count; i++)
            {
                if (listToDoTaskDM[i].AnchorBegin)
                {
                    isAnhor = true;
                    anhorTime = listToDoTaskDM[i].TimeBegin.Value;
                    break;
                }
                sumMinutes += listToDoTaskDM[i].Time;
                if (listToDoTaskDM[i].AnchorEnd)
                {
                    isAnhor = true;
                    anhorTime = listToDoTaskDM[i].TimeEnd.Value;
                    break;
                }
            }

            if (isAnhor)
            {
                int realMinutesToAnchor = anhorTime - timeBegin;
                if (sumMinutes <= realMinutesToAnchor) //mamy zapas czasu wiec mozemy robic nasze zadania dluzej?
                {
                    return timeBegin + task.Time;
                }
                else //zeby sie wyrobic musimy przyspieszyc prace nad naszymi zadaniami
                {
                    //sumMinutes = 100%
                    //realMinutesToAnchor = x%
                    double realPercent = (realMinutesToAnchor * 100.0f) / sumMinutes;

                    int minRealPercent = 30;//to dodac do jakichs wlaściwości
                    if (realPercent < minRealPercent)
                    {
                        realPercent = minRealPercent;
                    }

                    return timeBegin + (int)(((task.Time * realPercent) / 100) + 0.5);
                }
            }
            else
            {
                return timeBegin + task.Time;
            }
        }
        /**
         * szuka aktualnego zadania i usuwa wszystkie zadania które są przed wyznaczonym zadaniem 
         */
        private TaskDM? GetCurrentTask(List<TaskDM> listToDoTaskDM, int? currentTaskId, PlanDayParameters parameters)
        {
            TaskDM? task = null;


            if (listToDoTaskDM.Count == 0)
            {
                return null;
            }

            if (currentTaskId.HasValue)
            {
                task = listToDoTaskDM.Where(task => task.Id == currentTaskId).FirstOrDefault();
                if (task == null && listToDoTaskDM.Count > 0)
                {
                    task = listToDoTaskDM[0];
                }
                if (task != null)
                {
                    //usuniecie wszystkich zadan przed aktualnie wykonywanym zadaniem
                    listToDoTaskDM.Remove(task);
                }

                return task;
            }
            else
            {
                task = listToDoTaskDM[0];
                listToDoTaskDM.RemoveAt(0);
                return task;
            }
        }

        private ActualPointVM GetDefaultPoint(TaskDM task)
        {
            ActualPointVM actualPointVM = new ActualPointVM();
            actualPointVM.Id = null;
            actualPointVM.Name = task.Name;
            actualPointVM.Done = false;
            actualPointVM.Active = true;
            return actualPointVM;
        }
    }
}
