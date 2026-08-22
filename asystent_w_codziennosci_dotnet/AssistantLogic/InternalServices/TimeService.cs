using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;

namespace AssistantLogic.InternalServices
{
    public class TimeService : ITimeService
    {
        private ITaskRepository taskRepository;

        public TimeService(ITaskRepository taskRepository)
        {
            this.taskRepository = taskRepository;
        }

        public int GetTimeBegin(TaskDM task, TaskDM? taskBefore)
        {
            return GetTimeBegin(task, taskBefore,  DateTime.Now, DateTime.Today);
        }


        public int GetTimeBegin(TaskDM task, TaskDM? taskBefore,  DateTime currentDateTime, DateTime currentDate)
        {
            DateTime lastTaskFinishTime = currentDateTime;
            if (taskBefore != null)
            {
                lastTaskFinishTime = taskRepository.ReadLastDoneTaskTime(taskBefore.Id, currentDate);
            }
            if(task == null)
            {
                return lastTaskFinishTime.Hour * 60 + lastTaskFinishTime.Minute;
            }
            if (task.AnchorBegin)
            {
                DateTime anchorTime = currentDate.AddMinutes(task.TimeBegin.Value);
                if (anchorTime.CompareTo(lastTaskFinishTime) <= 0) //jestespy spoznieni i musimy natychmiast startować
                {
                    if(taskBefore == null)
                    {
                        return ConvertToMinutes(anchorTime);
                    }
                    else
                    {
                        return ConvertToMinutes(lastTaskFinishTime);
                    }
                }
                else
                {
                    return task.TimeBegin.Value;
                }
            }
            else
            {
                return ConvertToMinutes(lastTaskFinishTime);
            }
        }

        public int GetTimeEnd(TaskDM task, List<TaskDM> listToDoTaskDM, int timeBegin)
        {
            if (task.AnchorEnd)
            {
                return task.TimeEnd.Value;
            }

            double pecentSpeed = CalculatePecentSpeed(task, listToDoTaskDM, timeBegin);
            return timeBegin + (int)(((task.Time * pecentSpeed) / 100) + 0.5);
        }

        

        public double CalculatePecentSpeed(TaskDM task, List<TaskDM> listToDoTaskDM, int timeBegin)
        {
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
                    return 100;
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

                    return realPercent;
                }
            }
            else
            {
                return 100;
            }
        }

        public string ConvertToTimeHHMM(int allMinutes)
        {
            int hours = allMinutes / 60;
            int minutes = allMinutes % 60;
            return $"{hours:D2}:{minutes:D2}";
        }
        public int ConvertToMinutes(DateTime time)
        {
            return time.Hour * 60 + time.Minute;
        }

        public string ConvertToTimeHourMinutes(int allMinutes)
        {
            int hours = allMinutes / 60;
            int minutes = allMinutes % 60;
            string result = "";
            if (hours > 0)
            {
                result = hours + " godz ";
            }
            if (minutes > 0)
            {
                result += minutes + " min ";
            }

            return result;
        }
    }
}
