using AssistantDatabase.Model;
using AssistantLogic.Common;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.Triggers.TriggersTime;
using AssistantLogic.Triggers.Weather;
using AssistantLogic.ViewModel;
using AsystentView.Session;
using Microsoft.AspNetCore.Mvc;

namespace Asystent_w_codzienności.Controllers
{
    public class TriggerController : Controller
    {
        private readonly ILogger<TaskController> logger;
        private readonly ITriggerService triggerService;
        private readonly SessionManager sessionManager;
        
        public TriggerController(
           SessionManager sessionManager,
            ILogger<TaskController> logger, 
            ITriggerService triggerService)
        {
            this.sessionManager = sessionManager;
            this.logger = logger;
            this.triggerService = triggerService;
        }

        public IActionResult Index()
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            return View(triggerService.ToListOfTriggerBaseInformationVMWithProtageName(sessionManager.User.Id, ProtageName));
        }
        public IActionResult Add()
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }

            TriggerAddEditVM triggerAddEditVM = new TriggerAddEditVM();
            triggerAddEditVM.MaxWindPower = ScaleWind.Hurricane;
            PrepareSelectionOpcionsToModel(triggerAddEditVM);
            return View(triggerAddEditVM);
        }

        [HttpPost]
        public IActionResult Add(TriggerAddEditVM model)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            PrepareSelectionOpcionsToModel(model);
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TriggerDM? triggerDM = triggerService.ToTriggerDM(model);
            if(triggerDM != null)
            {
                triggerService.Add(triggerDM, CaregiverId);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(TriggerAddEditVM model)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }

            if (!model.EditValidationMode)
            {
                model = triggerService.GetTriggerAddEditVM(model.Id);
                model.EditValidationMode = true;
                PrepareSelectionOpcionsToModel(model);
                ModelState.Clear();
                return View(model);
            }

            PrepareSelectionOpcionsToModel(model);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TriggerDM? triggerDM = triggerService.ToTriggerDM(model);
            if (triggerDM != null)
            {
                
                triggerService.Update(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            triggerService.Delete(id);
            return RedirectToAction("Index");
        }
        //TriggerAddVM

        private bool Permisions()
        {
            UserSessionModel user = sessionManager.User;
            if (user != null && (user.Type == UserType.caregiver))
            {
                return true;
            }
            return false;
        }
        
        private int CaregiverId
        {
            get
            {
                return sessionManager.User.Id;
            }
        }
        private string ProtageName
        {
            get
            {
                return sessionManager.User.SelectedProtageName;
            }
        }
        private void PrepareSelectionOpcionsToModel(TriggerAddEditVM model)
        {
            
            model.Types = TriggerType.RandomEvent.GetTypesList();
            model.TimeTypes = TriggerTimeType.WeekDay.GetTypesList();
            model.WeekDayTypes = TriggerWeekDayType.Sunday.GetTypesList();
            model.DateIntervalTypes = TriggerDateIntervalType.EveryDay.GetTypesList();
            model.WeatherTypes = WeatherType.Rain.GetTypesList();
            model.RainLevels = RainLevel.LightRain.GetTypesList();
            model.SnowLevels = SnowLevel.LightSnow.GetTypesList();
            model.WindLevels = ScaleWind.Calm.GetTypesList();

            ModelState.Remove("Types");
            ModelState.Remove("TimeTypes");
            ModelState.Remove("WeatherTypes");
            ModelState.Remove("RainLevels");
            ModelState.Remove("SnowLevels");
            ModelState.Remove("WindLevels");
            ModelState.Remove("WeekDayTypes");
            ModelState.Remove("DateIntervalTypes");
        }
        
    }
}
