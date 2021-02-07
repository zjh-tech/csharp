<!--
 * @Descripttion: 
 * @Author: zhengjinhong
 * @Date: 2021-02-07 11:47:29
 * @LastEditors: zhengjinhong
 * @LastEditTime: 2021-02-07 12:00:35
 -->
C# 库和框架

开发环境:
    window: Visual Studio 2019
    linux:  dotnet(yum install dotnet-sdk-3.1)

库:
log:
    同步,异步API
timer: 
    时间路定时器
    调时间: 时间回滚,前进
    Delay的API
    执行时间太长导致的误差越来越大
tcp:
    粘包，断包
    主动被动断开,主动断开时支持发送数据
    本地字节序和网络字节序转换
    监听地址端口重用
    加解密,压缩,coder通用接口

框架:    
    服务发现: 支持tcp/http二种方式
    唯一ID生成
    策划配置加载和热更        
    信号    
    protobuf3.0以上版本使用
  
    tcp:        
        心跳,验证
        白黑名单,验证超时待开发        
        ssclientsession: 用于SdkServer与Server之间
        ssserversession: 用于Server与Server之间
        Package: 包头 + 包体,包体: attach + other
        Coder: 自定义Package
        TimerMeter: 监控逻辑模块时间
        LogicServer: Server与Server接口
工具:
        打表工具: excel转protobuf 自动化代码生成,支持热更

引用的Dll问题:  
    需求: Debug,Release引用对应版本的Dll
    解决方案: *.csproj将Debug/Release改为$(Configuration)        