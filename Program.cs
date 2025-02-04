using System.Text;
using MQTTnet;
using MQTTnet.Server;
using static System.Console;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using MqttWatch.Data;
using System.IO;
using System.Text.Json;

// 把資料庫相關的參數寫在程式碼的方式不安全
// // 開啟 postgresql server, 保存資料
// // 設定變數
// var myServer = "";
// var userName = "";
// var password = "";
// var dataBase = "";
// 
// // 建立字串
// string[] loginArray = {"Host=", myServer, ";Username=", userName,
//                   ";Password=", password, ";Database=", dataBase};
// 
// // 用join把字串連接起來
// var connString = String.Join("",loginArray);

// 存在環境變數
var connString = Environment.GetEnvironmentVariable("connectionStringSetting");

// 連線需要使用的程式碼
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
var dataSource = dataSourceBuilder.Build();

// 如果登入的資料錯誤 會在這裡出現錯誤
var conn = await dataSource.OpenConnectionAsync();

WriteLine("連線 Postgresql 資料庫");


// 如果沒有, 建立保存資料的資料表
await using (var cmd = new NpgsqlCommand(
    @"create table if not exists mqtt_table (
        Key_id serial primary key,
        Topic text not null,
        Payload text not null,
        Record_time time not null);", conn)){
    await cmd.ExecuteNonQueryAsync();
}


// 預設有三個連線, Port 預設1883
var mqttOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().WithConnectionBacklog(3);

// 建立Mqtt
var server = new MqttFactory().CreateMqttServer(mqttOptions.Build());
//Add Interceptor for logging incoming messages
server.InterceptingPublishAsync += Server_InterceptingPublishAsync;
server.ClientConnectedAsync += Server_GetConnectedClient;

WriteLine("開啟MQTT");
// 開啟MQTT
await server.StartAsync();
// WriteLine("Press any key to stop Server ...");
// ReadKey();


Task Server_GetConnectedClient(ClientConnectedEventArgs arg)
{
    Console.WriteLine("get connected client");
    return Task.CompletedTask;
}

Task Server_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
{
    WriteLine("run task message");
    // Convert Payload to string
    var payload = arg.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(arg.ApplicationMessage?.Payload);


    WriteLine(
        " Time: {0} -- Message: ClientId = {1}, Topic = {2}, Payload = {3}",

        DateTime.Now,
        arg.ClientId,
        arg.ApplicationMessage?.Topic,
        payload);


    // 寫資料到 Postgresql
    using (var cmd = new NpgsqlCommand("insert into mqtt_table (Topic, Payload, record_time) values (@t,@p,@r)", conn)){

        cmd.Parameters.AddWithValue("t", arg.ApplicationMessage?.Topic);
        cmd.Parameters.AddWithValue("p", HexStringToString(arg.ApplicationMessage?.Payload));
        cmd.Parameters.AddWithValue("r", DateTime.Now);
        cmd.ExecuteNonQuery();
    }

    static string HexStringToString(byte[] hex){
        // 將byte改為 string
        return Encoding.ASCII.GetString(hex);
    }

    // 保存到 Controller 可以讀取的地方
    var JsonResult = new List<Dictionary<string, object>>();
    using(var readCmd = new NpgsqlCommand("select * from mqtt_table where record_time >= @d::date", conn)){
        readCmd.Parameters.AddWithValue("d", DateTime.Now.Date);
        using(var read = readCmd.ExecuteReader()){
            while (read.Read()){
                // 遍歷每行的每個欄位
                var row = new Dictionary<string, object>();
                for (int i = 0; i < read.FieldCount; i++)
                {
                    row[read.GetName(i)] = read.GetValue(i);
                }
                JsonResult.Add(row);
            }
        }
    }
    string JsonData = JsonSerializer.Serialize(JsonResult, new JsonSerializerOptions { WriteIndented = true});
    File.WriteAllText("output.json", JsonData);

    return Task.CompletedTask;
}


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// 有版本問題 不使用
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseNpgsql(connString)
//);

var app = builder.Build();

app.UseStaticFiles();
app.MapControllers();

// 結束時關閉資料庫
IHostApplicationLifetime lifetime = app.Lifetime;
lifetime.ApplicationStopped.Register(() => {
    conn.Close();
    WriteLine("關閉資料庫連線");
    try {
        if(File.Exists("output.json")) {
            WriteLine("即將刪除暫存檔案");
            File.Delete("output.json");
        }
    }
    catch (Exception ex) {
        WriteLine("在刪除檔案時出現問題 {ex.Message}");
    }
});

app.Run();

