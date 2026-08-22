using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;

namespace AssistantDatabase.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly DataContext context;

        public TaskRepository(DataContext context)
        {
            this.context = context;
        }

        public void Add(TaskDM task, int caregiverId, int asdPersonID)
        {
            task.AsdPerson = context.Users.Find(asdPersonID);
            task.Caregiver = context.Users.Find(caregiverId);

            int? maxNumber = context.Tasks
                        .Select(x => x.Number)
                        .DefaultIfEmpty()
                        .Max();
            if (maxNumber.HasValue)
            {
                task.Number = maxNumber.Value + 1;
            }
            else
            {
                task.Number = 1;
            }


            context.Tasks.Add(task);
            context.SaveChanges();
        }

        public TaskDM GetById(int id)
        {
            return context.Tasks.Find(id);
        }

        public TaskDM? GetByIdWithDetails(int id)
        {
            return context.Tasks.Find(id);
        }

        public void Update(TaskDM task)
        {
            context.Tasks.Update(task);
            context.SaveChanges();
        }

        public List<TaskDM> ReadAllForCaregiver(int caregiverId)
        {
            return context.Tasks.Where(t => t.Caregiver.Id == caregiverId).OrderBy(x => x.Number).ToList();
        }

        public List<TaskDM> ReadAllForAsdPerson(int asdPersonId)
        {
            return context.Tasks.Where(t => t.AsdPerson.Id == asdPersonId).OrderBy(x => x.Number).ToList();
        }
        public List<TaskDM> ReadAllDoneTasksForAsdPerson(int asdPersonId, DateTime currentDate)
        {
            return context.Tasks
                .Where(t => t.AsdPerson.Id == asdPersonId && t.Done.Any(d => d.FinishDate == currentDate))
                .OrderBy(x => x.Number)
                .ToList();
        }

        public TaskDM? ReadLastDoneTaskForAsdPerson(int asdPersonId, DateTime currentDate)
        {
            return context.Tasks
                .Where(t => t.AsdPerson.Id == asdPersonId && t.Done.Any(d => d.FinishDate == currentDate))
                .OrderBy(x => x.Number).LastOrDefault();
        }

        public DateTime ReadLastDoneTaskTime(int taskId, DateTime currentDate)
        {
            return context.DoneTasks.Where(dt => dt.TaskId == taskId && dt.FinishDate.Date == currentDate).First().FinishTime;
        }

        public List<TaskDM> ReadAllToDoTasksForAsdPerson(int asdPersonId, DateTime currentDate)
        {
            return context.Tasks
                .Where(t => t.AsdPerson.Id == asdPersonId && !t.Done.Any(d => d.FinishDate == currentDate))
                .OrderBy(x => x.Number)
                .ToList();
        }

        public void AddTrigger(int taskId, int triggerId, bool negation)
        {
            TaskDM task = context.Tasks.Find(taskId);
            TriggerDM trigger = context.Trigers.Find(triggerId);
            var triggerTask = new TriggerTaskDM
            {
                Trigger = trigger,
                Task = task,
                Negation = negation
            };
            context.TriggerTasks.Add(triggerTask);
            context.SaveChanges();
        }
        public void RemoveTrigger(int taskId, int triggerId)
        {
            var triggerTask = context.TriggerTasks
           .SingleOrDefault(tt => tt.TriggerId == triggerId && tt.TaskId == taskId);

            if (triggerTask != null)
            {
                context.TriggerTasks.Remove(triggerTask);
                context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var task = context.Tasks.Find(id);

            if (task != null)
            {
                context.Tasks.Remove(task);
                context.SaveChanges();
            }
        }

        public void Up(int asdPersonId, int taskId)
        {
            TaskDM? task = context.Tasks.Find(taskId);

            if (task != null)
            {
                int? lessNumber = context.Tasks
                       .Where(p => p.AsdPerson.Id == asdPersonId && p.Number < task.Number)
                       .Select(x => x.Number)
                       .DefaultIfEmpty()
                       .Max();

                if (lessNumber.HasValue)
                {
                    TaskDM? lessTask = context.Tasks
                        .FirstOrDefault(p => p.AsdPerson.Id == asdPersonId && p.Number == lessNumber);

                    if (lessTask != null)
                    {
                        int pom = task.Number;
                        task.Number = lessTask.Number;
                        lessTask.Number = pom;

                        context.SaveChanges();
                    }
                }
            }
        }

        public bool UpOverNumber(int asdPersonId, int taskId, int number)
        {
            TaskDM? task = context.Tasks.Find(taskId);

            if (task != null)
            {
                int? lessNumber = context.Tasks
                       .Where(p => p.AsdPerson.Id == asdPersonId && p.Number < task.Number && p.Number>=number)
                       .Select(x => x.Number)
                       .DefaultIfEmpty()
                       .Max();

                if (lessNumber.HasValue)
                {
                    TaskDM? lessTask = context.Tasks
                        .FirstOrDefault(p => p.AsdPerson.Id == asdPersonId && p.Number == lessNumber);

                    if (lessTask != null)
                    {
                        int pom = task.Number;
                        task.Number = lessTask.Number;
                        lessTask.Number = pom;

                        context.SaveChanges();
                        return true;
                    }
                }
            }
            return false;
        }

        public void Down(int asdPersonId, int taskId)
        {
            TaskDM? task = context.Tasks.Find(taskId);

            if (task != null)
            {
                int? biggerNumber = context.Tasks
                       .Where(p => p.AsdPerson.Id == asdPersonId && p.Number > task.Number)
                       .Select(x => x.Number)
                       .DefaultIfEmpty()
                       .Min();

                if (biggerNumber.HasValue)
                {
                    TaskDM? biggerTask = context.Tasks
                        .FirstOrDefault(p => p.AsdPerson.Id == asdPersonId && p.Number == biggerNumber);
                    
                   if (biggerTask != null)
                    {
                        int pom = task.Number;
                        task.Number = biggerTask.Number;
                        biggerTask.Number = pom;

                        context.SaveChanges();
                    }
                }
            }
        }

        public bool DownUnderNumber(int asdPersonId, int taskId, int number)
        {
            TaskDM? task = context.Tasks.Find(taskId);

            if (task != null)
            {
                int? biggerNumber = context.Tasks
                       .Where(p => p.AsdPerson.Id == asdPersonId && p.Number > task.Number && p.Number <= number)
                       .Select(x => x.Number)
                       .DefaultIfEmpty()
                       .Min();

                if (biggerNumber.HasValue)
                {
                    TaskDM? biggerTask = context.Tasks
                        .FirstOrDefault(p => p.AsdPerson.Id == asdPersonId && p.Number == biggerNumber);

                    if (biggerTask != null)
                    {
                        int pom = task.Number;
                        task.Number = biggerTask.Number;
                        biggerTask.Number = pom;

                        context.SaveChanges();
                        return true;
                    }
                }
            }
            return false;
        }

        public void DoneTask(int taskId)
        {
            TaskDM? task = context.Tasks.Find(taskId);
            if (task != null)
            {
                var doneTask = new DoneTaskDM
                {
                    TaskId = taskId,
                    Task = task,
                    FinishDate = DateTime.Today,
                    FinishTime = DateTime.Now
                };
                context.DoneTasks.Add(doneTask);
                context.SaveChanges();
            }
        }
    }
}
