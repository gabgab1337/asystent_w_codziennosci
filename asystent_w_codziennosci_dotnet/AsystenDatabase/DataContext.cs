using AssistantDatabase.Model;
using Microsoft.EntityFrameworkCore;

namespace AssistantDatabase
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskDM>()
                .HasOne(p => p.Caregiver)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskDM>()
                .HasOne(p => p.AsdPerson)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TriggerDM>()
               .HasOne(p => p.Caregiver)
               .WithMany()
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TriggerTaskDM>()            
            .HasKey(tt => new { tt.TriggerId, tt.TaskId });

            modelBuilder.Entity<TriggerTaskDM>()           
                .HasOne(tt => tt.Task)
                .WithMany(t => t.TriggerTasks)             
                .HasForeignKey(tt => tt.TaskId);            

            modelBuilder.Entity<TriggerTaskDM>()
                .HasOne(tt => tt.Trigger)
                .WithMany(t => t.TriggerTasks)
                .HasForeignKey(tt => tt.TriggerId);


            modelBuilder.Entity<TriggerPointDM>()
           .HasKey(tp => new { tp.TriggerId, tp.PointId });

            modelBuilder.Entity<TriggerPointDM>()
                .HasOne(tp => tp.Point)
                .WithMany(t => t.TriggerPoints)
                .HasForeignKey(tp => tp.PointId);

            modelBuilder.Entity<TriggerPointDM>()
                .HasOne(tt => tt.Trigger)
                .WithMany(t => t.TriggerPoints)
                .HasForeignKey(tt => tt.TriggerId);



            modelBuilder.Entity<DoneTaskDM>()
           .HasKey(dt => new { dt.TaskId, dt.FinishDate });
           
            modelBuilder.Entity<DoneTaskDM>()
                .HasOne(dt => dt.Task)
                .WithMany(t => t.Done)
                .HasForeignKey(dt => dt.TaskId);


            modelBuilder.Entity<DonePointDM>()
             .HasKey(dp => new { dp.PointId, dp.FinishDate  });

            modelBuilder.Entity<DonePointDM>()
                .HasOne(dp => dp.Point)
                .WithMany(p => p.Done)
                .HasForeignKey(dp => dp.PointId);
        }

        public DbSet<UserDM> Users { get; set; }

        public DbSet<TaskDM> Tasks { get; set; }

        public DbSet<TaskPointDM> Points { get; set; }

        public DbSet<TriggerTaskDM> TriggerTasks { get; set; } = default!;

        public DbSet<TriggerPointDM> TriggerPoints { get; set; } = default!;

        public DbSet<TriggerDM> Trigers { get; set; }

        public DbSet<DoneTaskDM> DoneTasks { get; set; }

        public DbSet<DonePointDM> DonePoints { get; set; }

        public DbSet<ErrorDM> Errors { get; set; }
        
    }
}
