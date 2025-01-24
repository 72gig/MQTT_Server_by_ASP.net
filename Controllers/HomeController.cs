using Microsoft.AspNetCore.Mvc;
using IActionResultExample.Models;
using System.Text;
using Newtonsoft.Json;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using MqttWatch.Data;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.IO;

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
            
            var readValue = "[]";
            List<MqttTable> Jsonitems = new List<MqttTable>();
            if(System.IO.File.Exists("output.json")){
                readValue = System.IO.File.ReadAllText("output.json");
            }
            try{
                Jsonitems = JsonConvert.DeserializeObject<List<MqttTable>>(readValue);
                Jsonitems.RemoveAll(Jsonitems => Jsonitems.topic != type);
            }
            catch(Exception ex){
                Console.WriteLine(ex);
            }
            
            
            var returnValue = JsonConvert.SerializeObject(Jsonitems);

            return returnValue;
        }


    }
}