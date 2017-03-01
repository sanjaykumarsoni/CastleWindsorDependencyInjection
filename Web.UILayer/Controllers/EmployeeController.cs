using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EmployeeService;
using System.Web.UI;
using CommonLayer;
using System.ServiceModel;
using DALayer;

namespace Web.UILayer.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None)]
    public class EmployeeController : Controller
    {
        private IEmployeeService service;
        public EmployeeController(IEmployeeService service)
        {
            this.service = service;
        }

        // GET: /Employee/
        public ActionResult Index()
        {
            return View();
        }
        public virtual ActionResult GetAllDetails()
        {
            var empDetails = this.service.GetEmployeeDetail(1);
            return View(empDetails);
        }

        public virtual ActionResult GetAllmployeeDetails()
        {
            IEnumerable<EmployeeDetails> empDetails = this.service.GetAllEmployeeDetail();
            return View(empDetails);
        }

    }
}
