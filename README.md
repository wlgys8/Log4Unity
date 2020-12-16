# Log4Unity

类似于log4js的日志系统实现。

# Dependencies

- You must have `LitJson.dll` in your Project. [Here is a LitJson.dll wrapper for Unity Package](https://github.com/wlgys8/LitJsonUPM)

- [FileRoller](https://github.com/wlgys8/FileRoller)

# Install

add follow to Package/manifest.json
```json
"com.ms.log4unity":"https://github.com/wlgys8/Log4Unity.git"

```

or install by openupm

```sh
openupm install com.ms.log4unity
```

# Usage


## Quick Start

```csharp

private static MS.Log4Unity.ULogger logger = LogFactory.GetLogger();

void Start(){
    logger.Debug("hello debug");
    logger.Info("hello info");
    logger.Warn("hello warn");
    logger.Error("hello error");
    logger.Fatal("hello fatal");
}

```


## 配置文件

### 1. 查找方式

- 默认情况下，使用`Resources/log4unity.json`作为配置文件
- 可以通过`LogFactory.Configurate`来指定自己的配置文件
- 如果查找不到配置文件，那么不会输出任何日志信息

### 2. 配置格式

```json
{
    "appenders":{
        "console":{
            "type":"UnityLogAppender",
        }
    },

    "catagories":[
        {
            "name":"default",
            "appenders":["console"],
            "level":"all"
        }
    ]
}

```

以上是一个简单的配置文件. 其中`catagories`可以对logger进行分类,`appenders`定义不同类型的输出。

`LogFactory.GetLogger()` 默认获取的`catagory`是 `default`。 
也可使用 `LogFactory.GetLogger(catagory)`指定。

### 3. Catagory配置

目前支持如下字段:

- matchType - string, 类型名字匹配方式，支持 (exact|regex)
    - exact 精确匹配
    - regex 正则匹配
- name - string, 类型名字

- appenders - string[] , 定义日志输出的目标

- level - string, 定义日志输出级别，支持`all,debug,info,warn,error,fatal,off`,默认为`all`

### 4. Appenders

Appender通常由`type`与`configs`组成. 系统通过`type`来索引查找相应的Appender. 并使用`configs`对其进行配置。 内置的Appenders如下:


- UnityLogAppender - 输出到unity的debug系统
    - type : UnityLogAppender
    - configs
        - layout - Layout 日志格式化配置
        - env - Env 执行环境

- FileAppender
    - type : FileAppender
    - configs
        - layout - Layout 日志格式化配置
        - env - Env 执行环境
        - rollType - 支持`Size`,`Session`,`Date`
        - fileName - string 文件输出路径
        - keepExt - 备份时，是否保持后缀
        - maxBackups - number 最多保存文件数量,默认为3
        - maxFileSize - number 单个日志文件大小,单位为byte。默认为1MB
        - flushInterval - number 日志按一定周期从内存持久化到硬盘。默认为1000ms

- CatagoryFilterAppender
    - type: CatagoryFilterAppender
    - configs:
        - env - Env 执行环境
        - catagory - string 过滤的catagory正则匹配
        - appenders - string[] 重定向appender
        - 

- 自定义Appender


其中Env为枚举字符串，定义如下:

- `EditorPlayer` 只在编辑器中有效
- `BuiltPlayer` 只在打包后有效
- `All` 均生效


### 5. 日志格式化 Layout

定义如下:
```json
{
    "type":"layout-type",
}
```

type支持以下类型

- basic
- basic-time
- coloured
- coloured-time
- pattern

type为pattern时，额外支持以下字段:

```json
{
    "type":"pattern",
    "pattern":"<pattern-for-log>"
}
```
`pattern`定义了日志的输出格式。

支持以下字段:

- `%r` time in toLocaleTimeString format
- `%p` log level
- `%c` log category
- `%m` log data
- `%d` date, formatted - default is ISO8601, format options are: ISO8601, ISO8601_WITH_TZ_OFFSET, ABSOLUTE, DATE, or any string - compatible with the date-format library. e.g. %d{DATE}, %d{yyyy/MM/dd-hh.mm.ss}
- `%n` newline
- `%[` start a coloured block (colour will be taken from the log level, similar to colouredLayout)
- `%]` end a coloured block


# Appender Type 类型映射规则

日志系统需要将配置的Appender Type映射为代码中对应的类型。内部会按照如下规则按顺序进行查找，直到获得对应的类型。

- 查找通过`AppenderManager.RegisterAppenderType`注册的Appender类型
- 根据配置表中的`appenderTypesRegister`进行映射
- 直接使用`type`字段


# 编译期优化

### 1. LogLevel和Filter未解决的问题

LogLevel和Filter是在运行期对日志进行过滤的，虽然避免了日志流向Appenders引起的开销，但是却无法避免字符串构造的开销。例如以下例子:

```csharp

void Update(){
    logger.Info("Update");
}

```

我们在Update中输出日志，即便通过LogLevel关掉了日志,但是每帧仍然有构造字符串`"Update"`的开销。还会引起GC卡顿。

### 2. 在编译期进行过滤


对于高频调用的调试日志接口，我们期望更进一步的优化。最好是对它们`Compile Out`。 这时候我们可以使用 `ConditionalLogger`

```csharp
private static MS.Log4Unity.ConditionalLogger logger = LogFactory.GetConditionalLogger();

void Start(){
    logger.EditorDebug("debug in editor only");
    logger.Debug("debug");
    logger.Info("info");
    logger.Warn("warn");
    logger.Error("error");
    logger.Fatal("fatal");
}

```

以上的代码，默认情况下，在编辑器里，只会输出
```
debug in editor only
info
warn
error
fatal

```
打包后，则只会输出

```
info
warn
error
fatal

```

如果我们查看打包后最终编译的代码，会发现结果如下:

```csharp
private static MS.Log4Unity.ConditionalLogger logger = LogFactory.GetLogger().Conditional();

void Start(){
    logger.Info("info");
    logger.Warn("warn");
    logger.Error("error");
    logger.Fatal("fatal");
}

```

即:
- 针对`logger.Editor{XXX}`打头的方法调用，仅会在编辑器环境被编译。

- `logger.Debug`仅在`#DEBUG`为true时，被编译。


这里内部是利用了 `System.Diagnostics.Conditional`标签特性.


关于如何在Unity中加入宏定义，请搜索 `Platform custom #defines`
