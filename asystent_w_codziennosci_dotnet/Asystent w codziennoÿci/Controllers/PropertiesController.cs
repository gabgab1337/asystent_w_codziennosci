using AssistantLogic.ViewModel;
using AsystentView.Session;
using Microsoft.AspNetCore.Mvc;

namespace Asystent_w_codzienności.Controllers
{
    public class PropertiesController : Controller
    {
        private SessionManager sessionManager;

        public PropertiesController(
          SessionManager sessionManager)
        {
            this.sessionManager = sessionManager;
        }

        public IActionResult Index()
        {
            PropertiesVM model = new PropertiesVM();
            model.Colors.Add(new ColorVM("White", "White", "Paleta biała"));
            model.Colors.Add(new ColorVM("Blue", "#0d6efd", "Paleta niebieska"));
            model.Colors.Add(new ColorVM("Green", "#198754", "Paleta zielona"));
            
            return View(model);
        }
    }
}
