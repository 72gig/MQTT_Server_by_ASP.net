:: 可以複製這個檔案 將 myHost、myUsername、Password、Database 改為自已使用的數值
:: 在執行這個檔案 在環境變數會留下這筆資料
:: 然後Program.cs 會使用這筆資料進行連線
setx connectionStringSetting "Host=myHost;Username=myUsername;Password=myPassword;Database=myDatabase;"
