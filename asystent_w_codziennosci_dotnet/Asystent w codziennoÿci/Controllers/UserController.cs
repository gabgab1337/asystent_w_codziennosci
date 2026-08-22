using Microsoft.AspNetCore.Mvc;
using AssistantDatabase.Model;
using AsystentView.Session;
using AssistantLogic.ViewModel;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;

namespace Asystent_w_codzienności.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService userService;
        private IErrorService errorService;
        private readonly SessionManager sessionManager;

        private const string NOT_DELETE_YOURSELF = "Nie można usunąć konta, na którym jest się aktualnie zalogowanym.";
        public UserController(
            SessionManager sessionManager,
            IUserService userService,
             IErrorService errorService)
        {
            this.sessionManager = sessionManager;
            this.userService = userService;
            this.errorService = errorService;
        }

        public IActionResult Index(string message)
        {
            try
            {
                if (sessionManager.AdminPermisions())
                {
                    ActualUserVM users = new ActualUserVM();
                    users.Message = message;
                    users.ActualType = ActualUserType;
                    users.Users = userService.ReadBaseInforOfUsersForAdmin(ActualUserId);
                    return View(users);
                }
                else if (sessionManager.CaregiverPermisions())
                {
                    ActualUserVM protages = new ActualUserVM();
                    protages.Message = message;
                    protages.ActualType = ActualUserType;
                    protages.Protages = userService.ReadBaseInfoOfAsdPersonForCaregiver(sessionManager.User.Id, sessionManager.User.SelectedProtageId);
                    return View(protages);
                }
                else
                {
                    return RedirectToAction("PermitionDenied", "User");
                }
            } 
            catch (Exception ex)
            {
                errorService.LogError(ex.ToString());
                return RedirectToAction("PermitionDenied", "User");
            }
            
        }

        public IActionResult Properties()
        {
            PropertiesVM model = new PropertiesVM();
            model.Colors.Add(new ColorVM("White", "White", "Paleta biała"));
            model.Colors.Add(new ColorVM("Blue", "#0d6efd", "Paleta niebieska"));
            model.Colors.Add(new ColorVM("Green", "#198754", "Paleta zielona"));
            model.Colors.Add(new ColorVM("Yellow", "#ffc107", "Paleta żółta"));
            model.Colors.Add(new ColorVM("Red", "#dc3545", "Paleta czerwona"));
            model.Colors.Add(new ColorVM("Cyan", "#0dcaf0", "Paleta cyjanowa"));
            model.Colors.Add(new ColorVM("Gray", "#adb5bd", "Paleta szara"));
            model.Colors.Add(new ColorVM("Indigo", "#6610f2", "Paleta indygo"));
            model.Colors.Add(new ColorVM("Orange", "#fd7e14", "Paleta pomarańczowa"));
            model.Colors.Add(new ColorVM("Pink", "#d63384", "Paleta różowa"));
            model.Colors.Add(new ColorVM("Purple", "#6f42c1", "Paleta fioletowa"));
            model.Colors.Add(new ColorVM("Teal", "#20c997", "Paleta turkusowa"));
            model.SelectedColor = sessionManager.User.ColorPalete;
            return View(model);
        }

        public IActionResult Proteges(string message)
        {
            if (!sessionManager.CaregiverPermisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }

            

            ProtagesVM protages = new ProtagesVM();
            protages.Message = message;
            protages.Protages = userService.ReadBaseInfoOfAsdPersonForCaregiver(sessionManager.User.Id, sessionManager.User.SelectedProtageId);
            return View(protages);
        }

        public IActionResult SelectASDPerson(int id)
        {
            if (!sessionManager.CaregiverPermisions())
            {
                return RedirectToAction("PermitionDenied", "User");
            }
            sessionManager.User = userService.SelectASDPerson(sessionManager.User, id);

            return RedirectToAction("Index", "User");
        }

        public IActionResult LogIn()
        {
            return View(new UserLoginVM());
        }

        [HttpPost]
        public IActionResult LogIn(UserLoginVM model)
        {
            try
            {
                UserSessionModel? user = userService.LogIn(model);
                if (user != null)
                {
                    sessionManager.User = user;
                    ModelState.Remove("Login");
                    ModelState.Remove("Password");
                    if (user.Type == UserType.administrator)
                    {
                        return RedirectToAction("Index");
                    }
                    else if (user.Type == UserType.caregiver)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("CurrentActivity", "ASD");
                    }


                }
                else
                {
                    model.Message = "Błędne dane logowania";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                errorService.LogError(ex.ToString());
                return RedirectToAction("PermitionDenied", "User");
            }
        }

        public IActionResult LogOff()
        {
            sessionManager.User = null;
            return RedirectToAction("LogIn");
        }

        public IActionResult PermitionDenied()
        {
            return View();
        }

        public IActionResult Add()
        {
            if (sessionManager.AdminPermisions())
            {
                return View(userService.GetUserAddVM(ActualUserType));
            }
            else if (sessionManager.CaregiverPermisions())
            {
                return View(userService.GetUserAddVM(ActualUserType));
            }
            else
            {
                return RedirectToAction("PermitionDenied", "User");
            }


        }

        [HttpPost]
        public IActionResult Add(UserAddVM model)
        {
            if (sessionManager.AdminPermisions())
            {
                if(userService.FillUser(model, UserType.administrator))
                {
                    return View(model);
                }
                
                ModelState.Remove("Types");
                
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                userService.CreateUserForAdmin(model);
                return RedirectToAction("Index");
            }
            else if (sessionManager.CaregiverPermisions())
            {
                if (userService.FillUser(model, UserType.caregiver))
                {
                    return View(model);
                }
                model.Type = UserType.asdPerson;

                ModelState.Remove("Types");

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                userService.CreateUserForCaregiver(model, sessionManager.User.Id);
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("PermitionDenied", "User");
            }
        }

        [HttpPost]
        public IActionResult Edit(int id)
        {
            if (sessionManager.AdminPermisions())
            {
                return View(userService.GetUserEditForAdminVM(id, ActualUserType, ActualUserId));
            }
            else if (sessionManager.CaregiverPermisions())
            {
                return View(userService.GetUserEditForCaregiverVM(id, ActualUserType));
            }
            else
            {
                return RedirectToAction("PermitionDenied", "User");
            }

        }

        [HttpPost]
        public IActionResult Update(UserEditVM model)
        {
            if (sessionManager.AdminPermisions())
            {
                UserType userType = userService.GetUserType(model.Id);
                if ((userService.FillExistUser(model, UserType.administrator)) && (model.Username != model.UsernameBefore))
                {
                    return View("Edit", model);
                }
                ModelState.Remove("Types");
                if (userType == UserType.administrator || userType == UserType.caregiver)
                {
                    if (!ModelState.IsValid)
                    {
                        return View("Edit", model);
                    }
                    userService.Update(model);
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("PermitionDenied", "User");
                }
            }
            else if (sessionManager.CaregiverPermisions())
            {
                UserType userType = userService.GetUserType(model.Id);
                if ((userService.FillExistUser(model, UserType.caregiver)) && (model.Username != model.UsernameBefore))
                {
                    return View("Edit", model);
                }
                ModelState.Remove("Types");
                if (userType == UserType.asdPerson)
                {
                    if (!ModelState.IsValid)
                    {
                        return View("Edit", model);
                    }
                    model.Type = UserType.asdPerson;
                    userService.Update(model);
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    return RedirectToAction("PermitionDenied", "User");
                }
            }
            else
            {
                return RedirectToAction("PermitionDenied", "User");
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (sessionManager.User.Id == id)
            {
                return RedirectToAction("Index", "User", new { message = NOT_DELETE_YOURSELF });
            }

            if (sessionManager.AdminPermisions())
            {
                UserType userType = userService.GetUserType(id);
                if(userType==UserType.administrator || userType == UserType.caregiver)
                {
                   
                    userService.Delete(id);
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("PermitionDenied", "User");
                }
            }
            else if (sessionManager.CaregiverPermisions())
            {
                UserType userType = userService.GetUserType(id);
                if (userType == UserType.asdPerson)
                {
                    if (sessionManager.User.SelectedProtageId.HasValue && sessionManager.User.SelectedProtageId == id)
                    {
                        UserSessionModel user = sessionManager.User;
                        user.SelectedProtageId = null;
                        user.SelectedProtageName = "";
                        sessionManager.User = user;
                    }
                    userService.Delete(id);
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("PermitionDenied", "User");
                }
            }
            else
            {
                return RedirectToAction("PermitionDenied", "User");
            }
        }
        [HttpPost]
        public IActionResult SelectColor(string color)
        {
            UserSessionModel user = sessionManager.User;
            user.ColorPalete = color;
            sessionManager.User = user;

            userService.SetColorsPalete(user.Id,color);
            return RedirectToAction("Properties", "User");
        }
        private UserType ActualUserType
        {
            get
            {
                return sessionManager.User.Type;
            }
        }
        private int ActualUserId
        {
            get
            {
                return sessionManager.User.Id;
            }
        }
    }
}
