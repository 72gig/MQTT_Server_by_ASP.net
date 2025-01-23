using System.Text;
using MQTTnet;
using MQTTnet.Server;
using static System.Console;
using Npgsql;
using Microsoft.EntityFrameworkCore;


// 開啟 postgresql server, 保存資料
// 設定變數
var myServer = "127.0.0.1";
var userName = "serveruser";
var password = "p915B3Y4d";
var dataBase = "mqttuserecord";

// 建立字串
string[] loginArray = {"Host=", myServer, ";Username=", userName,
                  ";Password=", password, ";Database=", dataBase};

// 用join把字串連接起來
var connString = String.Join("",loginArray);

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
        Payload text not null);", conn)){
    await cmd.ExecuteNonQueryAsync();
}


// 開啟 sql server, 保存資料
// 目前不設定



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
        " TimeStamp: {0} -- Message: ClientId = {1}, Topic = {2}, Payload = {3}, QoS = {4}, Retain-Flag = {5}",

        DateTime.Now,
        arg.ClientId,
        arg.ApplicationMessage?.Topic,
        payload,
        arg.ApplicationMessage?.QualityOfServiceLevel,
        arg.ApplicationMessage?.Retain);


    // 寫資料到 Postgresql
    using (var cmd = new NpgsqlCommand("insert into mqtt_table (Topic, Payload) values (@t,@p)", conn)){

        cmd.Parameters.AddWithValue("t", arg.ApplicationMessage?.Topic);
        cmd.Parameters.AddWithValue("p", arg.ApplicationMessage?.Payload);
        cmd.ExecuteNonQuery();
    }

    return Task.CompletedTask;
}


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connString)
);

var app = builder.Build();

app.UseStaticFiles();
app.MapControllers();

// 結束時關閉資料庫
IHostApplicationLifetime lifetime = app.Lifetime;
lifetime.ApplicationStopped.Register(() => {
    conn.Close();
    WriteLine("關閉資料庫連線");
});

app.Run();

