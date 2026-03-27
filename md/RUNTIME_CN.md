# runFullTrust 功能说明

## 为何需要 runFullTrust 功能
本应用需要执行内置的第三方工具来完成 YouTube 视频下载功能。

## 如何在产品中使用

### 核心功能需求
1. yt-dlp：解析和下载 YouTube 视频
2. ffmpeg：视频和音频合并处理

### 无替代方案
- Windows App SDK 不提供原生的 YouTube 视频解析 API
- yt-dlp 和 ffmpeg 是开源社区标准的视频处理工具

## 功能使用范围
- 仅用于执行内置的 yt-dlp.exe 和 ffmpeg.exe
- 不访问用户其他文件或系统数据
- 完全用于用户主动触发的下载任务

## 隐私与安全
- 所有操作在用户本地设备执行
- 不上传任何用户数据
- 不收集任何个人信息
