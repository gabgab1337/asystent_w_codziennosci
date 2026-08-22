using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;

namespace AssistantDatabase.Repositories
{
    public class PointRepository : IPointRepository
    {
        private readonly DataContext context;

        public PointRepository(DataContext context)
        {
            this.context = context;
        }

        public TaskPointDM GetById(int id)
        {
            return context.Points.Find(id);
        }

        public List<TaskPointDM> ReadAllForTask(int taskId)
        {
            return context.Points.Where(p => p.TaskId == taskId).OrderBy(x=> x.Number).ToList();
        }
        public List<TaskPointDM> ReadAllDonePointsForTask(int taskId)
        {
            return context.Points
                .Where(p => p.TaskId == taskId &&  p.Done.Any(d => d.FinishDate == DateTime.Today))
                .OrderBy(x => x.Number).ToList();
        }
        public List<TaskPointDM> ReadAllToDoPointsForTask(int taskId)
        {
            return context.Points
               .Where(p => p.TaskId == taskId && !p.Done.Any(d => d.FinishDate == DateTime.Today))
               .OrderBy(x => x.Number).ToList();
        }

        public void Add(TaskPointDM point)
        {
            int? maxNumber = context.Points
                        .Where(p => p.TaskId == point.TaskId)
                        .Select(x => x.Number)
                        .DefaultIfEmpty()
                        .Max();
            if (maxNumber.HasValue)
            {
                point.Number = maxNumber.Value + 1;
            }
            else
            {
                point.Number = 1;
            }

            context.Points.Add(point);
            context.SaveChanges();
        }
        public void Update(TaskPointDM point)
        {
            context.Points.Update(point);
            context.SaveChanges();
        }
        public void Up(int taskId, int pointId)
        {
            TaskPointDM? point = context.Points.Find(pointId);

            if(point!=null)
            {
               int? lessNumber = context.Points
                      .Where(p => p.TaskId == taskId && p.Number < point.Number)
                      .Select(x => x.Number)
                      .DefaultIfEmpty()
                      .Max();

                if(lessNumber.HasValue)
                {
                    TaskPointDM? lessPoint = context.Points
                        .FirstOrDefault(p => p.TaskId == taskId && p.Number == lessNumber);

                    if(lessPoint != null)
                    {
                        int pom = point.Number;
                        point.Number = lessPoint.Number;
                        lessPoint.Number = pom;

                        context.SaveChanges();
                    }
                }
            }
        }

        public void Down(int taskId, int pointId)
        {
            TaskPointDM? point = context.Points.Find(pointId);

            if (point != null)
            {
                int? biggerNumber = context.Points
                       .Where(p => p.TaskId == taskId && p.Number > point.Number)
                       .Select(x => x.Number)
                       .DefaultIfEmpty()
                       .Min();

                if (biggerNumber.HasValue)
                {
                    TaskPointDM? biggerPoint = context.Points
                        .FirstOrDefault(p => p.TaskId == taskId && p.Number == biggerNumber);

                    if (biggerPoint != null)
                    {
                        int pom = point.Number;
                        point.Number = biggerPoint.Number;
                        biggerPoint.Number = pom;

                        context.SaveChanges();
                    }
                }
            }
        }

        



        public void Delete(int id)
        {
            var task = context.Points.Find(id);

            if (task != null)
            {
                context.Points.Remove(task);
                context.SaveChanges();
            }
        }

        public void AddTrigger(int pointId, int triggerId, bool negation)
        {
            TaskPointDM point = context.Points.Find(pointId);
            TriggerDM trigger = context.Trigers.Find(triggerId);
            var triggerPoint = new TriggerPointDM
            {
                Trigger = trigger,
                Point = point,
                Negation = negation
            };
            context.TriggerPoints.Add(triggerPoint);
            context.SaveChanges();
        }
        public void RemoveTrigger(int pointId, int triggerId)
        {
            var triggerTask = context.TriggerPoints
           .SingleOrDefault(tp => tp.TriggerId == triggerId && tp.PointId == pointId);

            if (triggerTask != null)
            {
                context.TriggerPoints.Remove(triggerTask);
                context.SaveChanges();
            }
        }

        public void DonePoint(int pointId)
        {
            TaskPointDM? point = context.Points.Find(pointId);
            if (point != null)
            {
                var donePoint = new DonePointDM
                {
                    PointId = pointId,
                    Point = point,
                    FinishDate = DateTime.Today
                };
                context.DonePoints.Add(donePoint);
                context.SaveChanges();
            }
          
        }
        

    }
}
