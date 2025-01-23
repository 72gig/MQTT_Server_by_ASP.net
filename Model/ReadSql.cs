using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace IActionResultExample.Models
{
    public class Readsql{
        [FromQuery]
        public string? Type { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }

    }

    public class sqlResult{
        // 找單號 日期 客戶 金額
        [FromQuery]
        public string? code { get; set; }
        public DateTime? date { get; set; }
        public string? partnercode { get; set; }
        public string? partner { get; set; }
        public string? productcode { get; set; }
        public string? product { get; set; }
        public double? cost { get; set; }
        public double? qty { get; set; }

    }

    public class SensorItem{
        [FromQuery]
        public string? title { get; set; }
        public string? context { get; set; }

    }

    public class SetMqttSql{
        [FromQuery]
        public int? id { get; set; }
        public string? topic { get; set; }
        public string? payload { get; set; }

    }


    public class ReadMqttSql : SetMqttSql{
        public List<SetMqttSql> Mqttinsql(){

            List<SetMqttSql> listSql = new List<SetMqttSql>();
            // 設定變數
            var myServer = "";
            var userName = "";
            var password = "";
            var dataBase = "";
            // 建立字串
            string[] array = {"Host=", myServer, ";Username=", userName,
                            ";Password=", password, ";Database=", dataBase};
            // 用join把字串連接起來
            var connString = String.Join("",array);

            var conn = new NpgsqlConnection(connectionString: connString);
            conn.Open();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = conn;

            cmd.CommandText = $"select * from mqtt_table order by id desc limit 100";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while ( reader.Read()){
                SetMqttSql mqttDate = new SetMqttSql();

                mqttDate.id = (int?)reader["id"];
                mqttDate.topic = (string?)reader["topic"];
                mqttDate.payload = (string?)reader["payload"];

                listSql.Add(mqttDate);
            }
            conn.Close();

            return listSql;
        }
    
    }
}