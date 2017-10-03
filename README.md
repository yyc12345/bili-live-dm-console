# bili-live-dm-console
A cross-platform bilibili live danmaku console receiver.  

Inspired and got some core code from [copyliu/bililive_dm](https://github.com/copyliu/bililive_dm)

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

dotnet run -- ROOM_INDEX
```

ROOM-INDEX is your bilibili live room's index. It must be a number.  
Press tab can block output danmaku and then press enter to continue outputing danmaku and blocked danmaku will be outputed immediately.  
