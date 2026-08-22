using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.ViewModel;
using Asystent_w_codzienności.Common;
using AsystentView.Session;
using Microsoft.AspNetCore.Mvc;

namespace Asystent_w_codzienności.Controllers
{
    public class ASDController : Controller
    {
        private IPlanDayService planDayService;
        private ICurrentActivityService currentActivityService;
        private ITimeService timeService;
        private SessionManager sessionManager;

      //  private IErrorService errorService;

        private const string NOT_SELECTED_PROTAGE = "Aby przejść do planu dnia należy najpierw wybrać podopiecznego";

        public ASDController(SessionManager sessionManager, IPlanDayService planDayService, ICurrentActivityService currentActivityService, ITimeService timeService)
        {
            this.planDayService = planDayService;
            this.currentActivityService = currentActivityService;
            this.sessionManager = sessionManager;
            this.timeService = timeService;
         //   this.errorService = errorService;
        }

        public IActionResult Index()
        {
            try
            {
                if (!AsdPermisions())
                {
                    return RedirectToAction("PermitionDenied", "User");
                }
                UserSessionModel user = sessionManager.User;
                user.planDayParameters = planDayService.UpdatePlanDayParameters(user.planDayParameters);
                sessionManager.User = user;

                return View(
                    planDayService.GeneratePlan(
                        sessionManager.User.Id,
                        sessionManager.User.planDayParameters));
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.ToString());
                return RedirectToAction("PermitionDenied", "User");
            }
        }

        /*
                public IActionResult PlanDayPreview()
                {
                    if (!CaregiverPermisions())
                    {
                        return RedirectToAction("PermitionDenied", "User");
                    }
                    if (sessionManager.User.SelectedProtageId == null)
                    {
                        return RedirectToAction("Proteges", "User");
                    }

                    //to w przyszlosci do usuniecia
                   // UserSessionModel user = sessionManager.User;
                   // user.planDayParameters = planDayService.UpdatePlanDayParameters(user.planDayParameters);
                   // sessionManager.User = user;

                    return View(planDayService.CreateModelPlanDayPreview(
                        null, sessionManager.User.SelectedProtageId.Value));


                }
        */
        [HttpPost]
        public IActionResult DonePoint(ActualActivityVM actualActivity)
        {
            try
            {
                if (!AsdPermisions())
                {
                    return RedirectToAction("PermitionDenied", "User");
                }

                int taskId = actualActivity.TaskVM.Id;
                int? pointId = actualActivity.TaskVM.CurrentPointId;
                currentActivityService.DonePoint(taskId, pointId);
                return RedirectToAction("CurrentActivity");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.ToString());
                return RedirectToAction("PermitionDenied", "User");
            }
        }

        public IActionResult PlanDay()
        {
            try
            {
                if (!AsdPermisions())
                {
                    return RedirectToAction("PermitionDenied", "User");
                }

                UserSessionModel user = sessionManager.User;
                user.planDayParameters = planDayService.UpdatePlanDayParameters(user.planDayParameters);
                sessionManager.User = user;


                return View(planDayService.CreateModelPlanDay(
                       user.planDayParameters, sessionManager.User.Id));
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.ToString());
                return RedirectToAction("PermitionDenied", "User");
            }
        }

        public ActionResult GetCurrentActivity()
        {
            try
            {
                if (!AsdPermisions())
                {
                    return PartialView("_ActualActivity", new ActualActivityBaseInfoVM());
                }
                ActualActivityVM actualActivity = sessionManager.ActualActivity;
                if (actualActivity == null)
                {
                    UserSessionModel user = sessionManager.User;
                    user.planDayParameters = planDayService.UpdatePlanDayParameters(user.planDayParameters);
                    actualActivity = currentActivityService.CreateActualActivity(sessionManager.User.Id, user.CurrentTaskId, user.planDayParameters, DateTime.Today);
                    user.CurrentTaskId = actualActivity.TaskVM.Id;
                    sessionManager.User = user;
                }

                ActualActivityBaseInfoVM actualActivityBase = new ActualActivityBaseInfoVM();
                actualActivityBase.Name = DateTime.Now.ToString("HH:mm:ss");
                int currentTime = timeService.ConvertToMinutes(DateTime.Now);
                if (actualActivity.TaskVM == null)
                {
                    actualActivityBase.Name = "Brak zadań";
                    actualActivityBase.State = AstualActivityState.NothinkToDo;
                }
                else if (actualActivity.TaskVM.TimeEndValue < currentTime)
                {
                    actualActivityBase.Name = actualActivity.TaskVM.Name;
                    actualActivityBase.State = AstualActivityState.PostTime;
                    int min = currentTime - actualActivity.TaskVM.TimeEndValue;
                    actualActivityBase.Name = $"{actualActivity.TaskVM.Name} {min} min po czasie";
                }
                else if (actualActivity.TaskVM.TimeEndValue - currentTime < 5)
                {
                    int min = actualActivity.TaskVM.TimeEndValue - currentTime;
                    actualActivityBase.Name = $"{actualActivity.TaskVM.Name} {min} min";
                    actualActivityBase.State = AstualActivityState.LowTime;
                }
                else
                {
                    actualActivityBase.Name = actualActivity.TaskVM.Name;
                    actualActivityBase.State = AstualActivityState.NormalWork;
                }
                return PartialView("_ActualActivity", actualActivityBase);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.ToString());
                return RedirectToAction("PermitionDenied", "User");
            }


           
        }

        public ActionResult CurrentActivity()
        {
            try
            {
                UserSessionModel user = sessionManager.User;
                user.planDayParameters = planDayService.UpdatePlanDayParameters(user.planDayParameters);

                ActualActivityVM actualActivity = currentActivityService.CreateActualActivity(sessionManager.User.Id, user.CurrentTaskId, user.planDayParameters, DateTime.Today);

                user.CurrentTaskId = actualActivity.TaskVM.Id;
                sessionManager.User = user;
                return View(actualActivity);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.ToString());
                return RedirectToAction("PermitionDenied", "User");
            }

            
        }


        public IActionResult PlanDayPreview(PlanDayPreviewVM model)
        {
            try
            {
                if (!CaregiverPermisions())
                {
                    return RedirectToAction("PermitionDenied", "User");
                }
                if (sessionManager.User.SelectedProtageId == null)
                {
                    return RedirectToAction("Proteges", "User", new { message = NOT_SELECTED_PROTAGE });
                }


                //to zostawiamy na kolejne spotkanie
                UserSessionModel user = sessionManager.User;
                user.planDayParameters = planDayService.UpdatePlanDayParameters(user.planDayParameters);

                if (model.Parameters == null)
                {

                    if (user == null)
                    {
                        return View(planDayService.CreateModelPlanDayPreview(
                            null, sessionManager.User.SelectedProtageId.Value, sessionManager.User.SelectedProtageName));
                    }
                    else
                    {
                        return View(planDayService.CreateModelPlanDayPreview(
                       user.planDayParameters, sessionManager.User.SelectedProtageId.Value, sessionManager.User.SelectedProtageName));
                    }
                }
                else
                {
                    user.planDayParameters = model.Parameters;
                    sessionManager.User = user;

                    return View(planDayService.CreateModelPlanDayPreview(
                        model.Parameters, sessionManager.User.SelectedProtageId.Value, sessionManager.User.SelectedProtageName));
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.ToString());
                return RedirectToAction("PermitionDenied", "User");
            }


         
        }
        private bool AsdPermisions()
        {
            try
            {
                UserSessionModel user = sessionManager.User;
                if (user != null && (user.Type == UserType.asdPerson))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.ToString());
                return false;
            }

           
        }

        private bool CaregiverPermisions()
        {

            try
            {
                UserSessionModel user = sessionManager.User;
                if (user != null && (user.Type == UserType.caregiver))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.ToString());
                return false;
            }
            
        }

    }
}
