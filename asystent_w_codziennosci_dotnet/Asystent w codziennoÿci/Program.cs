using AssistantDatabase;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Repositories;
using AssistantLogic.IExternalServices;
using AssistantLogic.IInternalServices;
using AssistantLogic.InternalServices;
using AssistantLogic.Services;
using AsystentView.Session;
using System.Globalization;


namespace AsystentView
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pl-PL");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pl-PL");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<DataContext>();
            builder.Services.AddTransient<IUserRepository, UserRepository>();
            builder.Services.AddTransient<ITaskRepository, TaskRepository>();
            builder.Services.AddTransient<ITriggerRepository, TriggerRepository>();
            builder.Services.AddTransient<IPointRepository, PointRepository>();
            builder.Services.AddTransient<ITriggerService, TriggerService>();
            builder.Services.AddTransient<IErrorRepository, ErrorRepository>();
            builder.Services.AddTransient<IPointService, PointService>();
            builder.Services.AddTransient<ITaskService, TaskService>();
            builder.Services.AddTransient<IPlanDayService, PlanDayService>();
            builder.Services.AddTransient<ICurrentActivityService, CurrentActivityService>();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<ITimeService, TimeService>();
            builder.Services.AddTransient<IWeatherService, WeatherService>();
            builder.Services.AddTransient<IErrorService, ErrorService>();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
            });
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddTransient<SessionManager, SessionManager>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
           
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=User}/{action=LogIn}/{id?}");

            app.Run();
        }
    }
}
