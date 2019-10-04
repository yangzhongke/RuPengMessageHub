# RuPengMessageHub
“Rupeng Message Hub” is an online chatting engine, which can host multiple applications. It can support 2,500 concurrent users on a single server (Memory:2 GB, Bandwidth: 5M).

RuPengMessageHub.Server is the core server, and all chat applications must connect to it.

RuPengMessageHub.TestWeb is an example of chat application.

Before connection to the hubserver, a token should be retrieved using code bellow:
```
var token = await client.GetTokenAsync("1", "yzk", "rupenggongkaike", DateTime.Now.ToFileTime(), "wpstt@999_6xx!aa");
```

The settings, like UserId, AppKey, and AppSecret, must comply with that of appsettings.json in RuPengMessageHub.Server.
```
  "AppInfos": [
    {
      "Id": "6CEB02AB-925D-47DF-9547-2437B952A204",
      "AppKey": "rupenggongkaike",
      "AppSecret": "wpstt@999_6xx!aa",
      "AppName": "Online chatroom for rupeng"
    },
    {
      "Id": "6CEB02AB-925D-47DF-9547-2437B952A206",
      "AppKey": "rupengIM",
      "AppSecret": "xx_3@66_6xx@aa",
      "AppName": "IM App of Rupeng"
    }
```

[Introduction and video tutorial](https://www.rupeng.com/Courses/Chapter/938)
