# bili-live-dm-console
A cross-platform bilibili live danmaku console receiver.  

Inspired and getting some core code from [copyliu/bililive_dm](https://github.com/copyliu/bililive_dm)

## Install
### Windows
Go to [Microsoft dotnet website](http://www.microsoft.com/net/download) to get a binary file to install dotnet **SDK** environment.  

### Linux and macOS
Follow the step from this [website](http://www.microsoft.com/net/download) to install dotnet **SDK** environment.  

>This application need dotnet 2.0 or above SDK.  

## Usage
```
git clone https://github.com/yyc12345/bili-live-dm-console.git
dotnet restore

dotnet run -- ROOM_ID [-s] [-nr] [-debug]
```

ROOM\_ID is your bilibili live room's id. It must be a number. Some room id is equal with url id but some not.  
-s is a optional parameter. If you sign it, it mean that application need to search actual room id and you must make sure your ROOM\_ID parameter is url id.  
-nr also is a optional parameter. By sign it, you can disable the function of recording danmaku into file.  
-debug is a optional parameter for developer. Signing it will make application show more information during running.  
Press tab can block output danmaku and then press enter to continue outputing danmaku and blocked danmaku will be outputed immediately.  

## Screenshot
Debian 9.1/KDE  
![](example.png)
