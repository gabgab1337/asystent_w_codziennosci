using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.ViewModel;
using AsystentView.Session;
using Microsoft.AspNetCore.Mvc;

namespace Asystent_w_codzienności.Controllers
{
    public class PointController : Controller
    {
        private readonly ILogger<TaskController> logger;
        //private ITaskRepository taskRepository;
        private ITriggerRepository triggerRepository;
        private IPointRepository pointRepository;
        private ITriggerService triggerService;
        private IPointService pointService;
        
        private SessionManager sessionManager;

        public PointController(SessionManager sessionManager, 
            ILogger<TaskController> logger,
            ITriggerRepository triggerRepository,
            IPointRepository pointRepository,
            ITriggerService triggerService,
            IPointService pointService)
        {
            this.sessionManager = sessionManager;
            this.logger = logger;
            this.triggerRepository = triggerRepository;
            this.pointRepository = pointRepository;
            this.triggerService = triggerService;
            this.pointService = pointService;
        }

        [HttpPost]
        public IActionResult Details(int pointId)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }

            PointDetailsVM point = pointService.GetPointDetailsVM(pointId);
            point.TriggersToAdd = triggerService.ToListOfTriggerBaseInformationVM(triggerRepository.ReadAllForPointToAdd(pointId, CaregiverId));
            point.Triggers = triggerService.ToListOfTriggerBaseInformationVM(triggerRepository.ReadAllForPoint(pointId));
            return View(point);
        }

        [HttpPost]
        public IActionResult RemoveTrigger(int pointId, int triggerId)
        {
            // TaskDM taskDM = taskRepository.GetByIdWithDetails(id);
            // TaskDetailsVM task = TaskConverter.ToTaskDetailsVM(taskDM);
            pointRepository.RemoveTrigger(pointId, triggerId);
            return RedirectToActionPreserveMethod("Details", "Point", new { pointId });
        }


        [HttpPost]
        public IActionResult AddTrigger(int pointId, int triggerId)
        {
            // TaskDM taskDM = taskRepository.GetByIdWithDetails(id);
            // TaskDetailsVM task = TaskConverter.ToTaskDetailsVM(taskDM);
            pointService.AddTrigger(pointId, triggerId, negation: false);
            return RedirectToActionPreserveMethod("Details", "Point", new { pointId });
        }

        [HttpPost]
        public IActionResult AddNegationTrigger(int pointId, int triggerId)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }

            pointService.AddTrigger(pointId, triggerId, negation: true);
            int id = pointId;
            return RedirectToActionPreserveMethod("Details", "Point", new { id });
        }
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
    }
}
