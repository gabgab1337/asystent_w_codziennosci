using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.Triggers.TriggersTime;
using AssistantLogic.ViewModel;
using AsystentView.Session;
using Microsoft.AspNetCore.Mvc;
using AssistantLogic.Common;

namespace Asystent_w_codzienności.Controllers
{
    public class TaskController : Controller
    {
        private readonly ILogger<TaskController> logger;
        private ITriggerService triggerService;
        private IPointService pointService;
        private ITaskService taskService;
        private SessionManager sessionManager;

        private const string NOT_SELECTED_PROTAGE = "Aby przejść do zakładki z zadaniami należy najpierw wybrać podopiecznego";
        public TaskController(
            SessionManager sessionManager,
            ILogger<TaskController> logger, 
            ITriggerService triggerService, 
            IPointService pointService,
            ITaskService taskService)
        {
            this.sessionManager = sessionManager;
            this.logger = logger;
            this.triggerService = triggerService;
            this.pointService = pointService;
            this.taskService = taskService;
        }

        public IActionResult Index()
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }

            ListTaskBaseInformationVM model = taskService.ListOfTaskBaseInfoVMForAsdPerson(ProtageId, ProtageName, sessionManager.User.WeekDayForTaskListCaragiver);

            model.WeekDayType = sessionManager.User.WeekDayForTaskListCaragiver;
            model.WeekDayTypes = TriggerWeekDayType.Sunday.GetTypesList();
            return View(model);
        }

        [HttpPost]
        public IActionResult Tasks(ListTaskBaseInformationVM model)
        {
            var user = sessionManager.User;
            user.WeekDayForTaskListCaragiver = model.WeekDayType;
            sessionManager.User = user;

            return RedirectToAction("Index");
        }


        public IActionResult Add()
        {

            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {

                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }
            //TaskAddEditVM user = TaskConverter.ToTaskAddEditVM(new TaskDM());

            return View(new TaskAddEditVM());
        }

        


        [HttpPost]
        public IActionResult Add(TaskAddEditVM model)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            taskService.Add(model, CaregiverId, ProtageId);
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(int id)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }


            return View(taskService.GetTaskAddEditVM(id));
        }

        [HttpPost]
        public IActionResult Update(TaskAddEditVM model)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }
            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }
            taskService.Update(model);

            return RedirectToAction("Index");
        }




        [HttpPost]
        public IActionResult Details(int id)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }

            return View(taskService.GetTaskDetailsVM(id, CaregiverId));
        }

        [HttpPost]
        public IActionResult AddTrigger(int taskId, int triggerId)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }

            // TaskDM taskDM = taskRepository.GetByIdWithDetails(id);
            // TaskDetailsVM task = TaskConverter.ToTaskDetailsVM(taskDM);
            taskService.AddTrigger(taskId, triggerId, negation: false);
            int id = taskId;
            return RedirectToActionPreserveMethod("Details","Task",new { id });
        }

        [HttpPost]
        public IActionResult AddNegationTrigger(int taskId, int triggerId)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }

            taskService.AddTrigger(taskId, triggerId, negation: true);
            int id = taskId;
            return RedirectToActionPreserveMethod("Details", "Task", new { id });
        }

        

        [HttpPost]
        public IActionResult AddPoint(int taskId, PointAddEditVM point, IFormFile Photo)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }
            if (Photo != null && Photo.Length > 0)
            {
                // Ścieżka zapisu pliku
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder);

                // Generowanie unikalnej nazwy pliku
                var fileName = Guid.NewGuid() + Path.GetExtension(Photo.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Zapis pliku na serwerze
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Photo.CopyTo(stream);
                }

                // Ustaw URL zdjęcia w modelu punktu
                point.PhotoUrl = $"/uploads/{fileName}";
            }
            point.TaskId = taskId;
            pointService.Add(point);
            return RedirectToActionPreserveMethod("Details", "Task", new { id = taskId });
        }
        [HttpPost]
        public IActionResult EditPoint(int taskId, int pointId, PointAddEditVM point, IFormFile Photo, bool DeletePhoto)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }
            var existingPoint = pointService.GetPointDetailsVM(pointId);
            if (DeletePhoto) // Jeśli zaznaczono "Usuń zdjęcie"
            {
                point.PhotoUrl = null;

                // Usuń plik z serwera, jeśli istnieje
                if (!string.IsNullOrEmpty(existingPoint?.Image))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingPoint.Image.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            else if (Photo != null && Photo.Length > 0) // Sprawdzamy czy przesłano nowe zdjęcie
            {
                // Ścieżka zapisu pliku
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder);

                // Generowanie unikalnej nazwy pliku
                var fileName = Guid.NewGuid() + Path.GetExtension(Photo.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Zapis pliku na serwerze
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Photo.CopyTo(stream);
                }
                // Usuń stare zdjęcie
                if (!string.IsNullOrEmpty(existingPoint?.Image))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingPoint.Image.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                // Ustaw URL zdjęcia w modelu punktu
                point.PhotoUrl = $"/uploads/{fileName}";
            }
            point.TaskId = taskId;
            point.Id = pointId;
            pointService.Update(point);
            return RedirectToActionPreserveMethod("Details", "Task", new { id = taskId });
        }
        [HttpPost]
        public IActionResult DeletePoint(int taskId, int pointId)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }

            
            pointService.Delete(pointId);
            return RedirectToActionPreserveMethod("Details", "Task", new { id = taskId });
        }

        [HttpPost]
        public IActionResult UpPoint(int taskId, int pointId)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }

            
            pointService.Up(taskId, pointId);
            int id = taskId;
            return RedirectToActionPreserveMethod("Details", "Task", new { id });
        }

        [HttpPost]
        public IActionResult DownPoint(int taskId, int pointId)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }

            pointService.Down(taskId, pointId);
            int id = taskId;
            return RedirectToActionPreserveMethod("Details", "Task", new { id });
        }
        [HttpPost]
        public IActionResult UpTask(int id)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }


            taskService.Up(sessionManager.User.SelectedProtageId.Value, id, sessionManager.User.WeekDayForTaskListCaragiver);
           
            return RedirectToActionPreserveMethod("Index", "Task");
        }

        [HttpPost]
        public IActionResult DownTask(int id)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }

            taskService.Down(sessionManager.User.SelectedProtageId.Value, id, sessionManager.User.WeekDayForTaskListCaragiver);
           
            return RedirectToActionPreserveMethod("Index", "Task");
        }

        [HttpPost]
        public IActionResult RemoveTrigger(int taskId, int triggerId)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }

            taskService.RemoveTrigger(taskId, triggerId);
            int id = taskId;
            return RedirectToActionPreserveMethod("Details", "Task", new { id });
        }
        
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!Permisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            if (!ProtageSelected())
            {
                return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
            }
            taskService.Delete(id);
            return RedirectToAction("Index");
        }
        private int CaregiverId
        {
            get
            {
                return sessionManager.User.Id;
            }
        }
        private int ProtageId
        {
            get
            {
                return sessionManager.User.SelectedProtageId.Value;
            }
        }
        private string ProtageName
        {
            get
            {
                return sessionManager.User.SelectedProtageName;
            }
        }
        private bool ProtageSelected()
        {
            return sessionManager.User.SelectedProtageId.HasValue;
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
    }
}
