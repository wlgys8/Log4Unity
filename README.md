# Log4Unity

类似于log4js的日志系统实现。

# Install

```json
"com.ms.log4unity":""

```

# Usage


## Quick Start

```csharp

private static MS.Log4Unity.ILogger logger = LogFactory.GetLogger();

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
            "type":"MS.Log4Unity.UnityLogAppender",
        }
    },

    "catagories":{
        "default":{
            "appenders":["console"]
        }
    }
}

```

以上是一个简单的配置文件. 其中`catagories`可以对logger进行分类,`appenders`定义不同类型的输出。

`LogFactory.GetLogger()` 默认获取的`catagory`是 `default`。 
也可使用 `LogFactory.GetLogger(catagory)`指定。

### 3. Catagory配置

目前支持如下字段:

- appenders - string[] , 定义日志输出的目标

- level - string, 定义日志输出级别，支持`all,debug,info,warn,error,fatal,off`,默认为`all`

### 4. Appenders

Appender通常由`type`与`configs`组成. 系统通过`type`来索引查找相应的Appender. 并使用`configs`对其进行配置。 内置的Appenders如下:

- UnityLogAppender - 输出到unity的debug系统
    - type : MS.Log4Unity.UnityLogAppender
    - configs
        - layout - Layout 日志格式化配置

- FileAppender
    - type : MS.Log4Unity.FileAppender
    - configs
        - layout - Layout 日志格式化配置
        - fileName - string 文件输出路径
        - maxFileCount - number 最多保存文件数量,默认为3
        - maxFileSize - number 单个日志文件大小,单位为byte。默认为10kb

- CatagoryFilterAppender
    - type: MS.Log4Unity.CatagoryFilterAppender
    - configs:
        - catagory - string 过滤的catagory正则匹配
        - appender - string 重定向appender
        - 

- 自定义Appender


### 5. 日志格式化 Layout

定义如下:
```json
{
    "type":"layout-type",
}
```

type支持以下类型

- basic
- coloured
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



