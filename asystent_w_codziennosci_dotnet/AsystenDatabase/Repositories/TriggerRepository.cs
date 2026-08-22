using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;

namespace AssistantDatabase.Repositories
{
    public class TriggerRepository : ITriggerRepository
    {
        private readonly DataContext context;

        public TriggerRepository(DataContext context)
        {
            this.context = context;
        }

        public void Add(TriggerDM trigger, int caregiverId)
        {
            trigger.Caregiver = context.Users.Find(caregiverId);
            context.Trigers.Add(trigger);
            context.SaveChanges();
        }

        public TriggerDM? GetById(int id)
        {
            return context.Trigers.Find(id);
        }

        public void Update(TriggerDM trigger)
        {
            context.Trigers.Update(trigger);
            context.SaveChanges();
        }

        public List<TriggerDM> ReadAllForCaregiver(int caregiverId)
        {
            return context.Trigers.Where(t => t.Caregiver.Id == caregiverId).ToList();
        }

      /*  public List<TriggerDM> ReadAllForTask(int taskId)
        {
            return context.TriggerTasks
            .Where(tt => tt.TaskId == taskId)
            .Select(tt => tt.Trigger)
            .ToList();
        }*/

        public List<TriggerDM> ReadAllForTask(int taskId)
        {
            return context.TriggerTasks
                .Where(tt => tt.TaskId == taskId)
                .Select(tt => new TriggerDM
                {
                    Id = tt.Trigger.Id,
                    Caregiver = tt.Trigger.Caregiver,
                    Name = tt.Trigger.Name,
                    Type = tt.Trigger.Type,
                    SubType = tt.Trigger.SubType,
                    Data = tt.Trigger.Data,
                    TriggerTasks = tt.Trigger.TriggerTasks,
                    TriggerPoints = tt.Trigger.TriggerPoints,
                    Negation = tt.Negation // Ustawianie pola Negation na podstawie TriggerTaskDM
                })
                .ToList();
        }

        public List<TriggerDM> ReadAllForTaskToAdd(int taskId, int caregiverId)
        {
            return context.Trigers
             .Where(trigger => !context.TriggerTasks.Any(tt => tt.TaskId == taskId && tt.TriggerId == trigger.Id) &&
             trigger.Caregiver.Id == caregiverId)
             .ToList();
        }

        public List<TriggerDM> ReadAllForPoint(int pointId)
        {
           // return context.TriggerPoints
            //.Where(tp => tp.PointId == pointId)
            //.Select(tt => tt.Trigger)
            //.ToList();
                return context.TriggerPoints
                .Where(tp => tp.PointId == pointId)
                .Select(tp => new TriggerDM
                {
                    Id = tp.Trigger.Id,
                    Caregiver = tp.Trigger.Caregiver,
                    Name = tp.Trigger.Name,
                    Type = tp.Trigger.Type,
                    SubType = tp.Trigger.SubType,
                    Data = tp.Trigger.Data,
                    TriggerTasks = tp.Trigger.TriggerTasks,
                    TriggerPoints = tp.Trigger.TriggerPoints,
                    Negation = tp.Negation // Ustawianie pola Negation na podstawie TriggerTaskDM
                })
                .ToList();

        }
        public List<TriggerDM> ReadAllForPointToAdd(int pointId, int caregiverId)
        {
            return context.Trigers
             .Where(trigger => !context.TriggerPoints.Any(tp => tp.PointId == pointId && tp.TriggerId == trigger.Id) &&
             trigger.Caregiver.Id == caregiverId)
             .ToList();
        }

        public bool AllowDeleteTrigger(int id)
        {
            var hasTriggerPoints = context.TriggerPoints.Any(tp => tp.TriggerId == id);
            var hasTriggerTasks = context.TriggerTasks.Any(tt => tt.TriggerId == id);
            return !hasTriggerPoints && !hasTriggerTasks;
        }
        public void Delete(int id)
        {
            var trigger = context.Trigers.Find(id);
            if (trigger != null)
            {
                if (AllowDeleteTrigger(id))
                {
                    context.Trigers.Remove(trigger);
                    context.SaveChanges();
                }
            }
        }
    }
}
