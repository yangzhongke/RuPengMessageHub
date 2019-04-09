String.prototype.endsWith=function(str){     
  var reg=new RegExp(str+"$");     
  return reg.test(this);        
}

function ChatClient(token, serverRoot) {
	if(!serverRoot.endsWith("/"))
	{
		throw "serverRoot必须以/结束";
	}
    var thisClient = this;
    this.token = token;
    this.connection = new signalR.HubConnectionBuilder()
        //由于chatLib.js是放到RuPengMessageHub.Server上的，所以直接写相对路径就行
        //如果把chatLib.js部署到应用服务器上，则需要写包含服务器地址的绝对路径
        .withUrl(serverRoot+"messageHub", { accessTokenFactory: () => token })
        .configureLogging(signalR.LogLevel.Information)
        .build();

	this.setOnCloseListener=function(f){
		this.connection.onclose(f);
	};
    this.start = function () {
        //返回对象，可以多次then
        return this.connection.start();
    };

    this.getGroupMessages = function (groupId) {
        return this.connection.invoke("GetGroupMessages", groupId).catch(function (err) { console.error("GetGroupMessages失败"+err); });
    };
    this.setOnGroupMessageListener = function (f) {
        this.connection.on("OnGroupMessage", f);
    };
    this.setOnGroupMessagesListener = function (f) {
        this.connection.on("OnGroupMessages", f);
    };
}