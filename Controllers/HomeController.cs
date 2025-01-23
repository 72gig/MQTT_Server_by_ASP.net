using Microsoft.AspNetCore.Mvc;
using IActionResultExample.Models;
using System.Text.Json;
using Newtonsoft.Json;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace IActionResultExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        [Route("home")]
        public IActionResult Index()
        {
            string type = new string("test");
            return View();
            //return RedirectToAction("ReporrData", "Report", new { type =  "test"});
            //return RedirectToActionPermanent("ReportData", "Report", new { type = "test" });  // 這個寫法可以轉移到控制器叫做ReportController 方法叫做 ReportData 的Route

            //return new LocalRedirectResult($"ReportInput/{type}"); //應可連到其它外部網站

            //return RedirectPermanent($"ReportInput/{type}");
        }



        // 在前端顯示資料
        [Route("ReportSensor/{type?}")]
        public IActionResult SensorData([FromQuery]string? type){
            
            ViewData["thisreport"] = type;
            return View();
        }

        [Route("Home/getMqttData")]
        [HttpPost]
        public string getMqttData(string type){
            //預定連線到sql 取得資料後顯示到前端
            //getSqlData();
            private readonly ApplicationDbcontext _context;

            List<string> data_ids = new List<string>() {"ABC","BCD"};
            //Console.WriteLine(type);
            var returnValue = new {
                item = 1,
                data = from s in data_ids
                        select s
            };
            
            return JsonConvert.SerializeObject(returnValue);
        }


    }

}