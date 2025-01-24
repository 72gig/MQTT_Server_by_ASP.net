using Microsoft.AspNetCore.Mvc;
using IActionResultExample.Models;
using System.Text.Json;
using Newtonsoft.Json;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using MqttWatch.Data;
using System.Threading.Tasks;

namespace IActionResultExample.Controllers
{
    public class HomeController : Controller
    {
        // 有版本問題 不使用
        //private readonly ApplicationDbContext _context;

        //public HomeController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

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
            // 有版本問題
            //var mqttData =  _context.mqtt_table.FromSqlRaw("select * from mqtt_table").ToList();
            

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