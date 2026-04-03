# YouTube视频下载器 换新版

一款简洁高效的 YouTube 视频下载工具，支持多种格式和画质选择。

## 功能特点

- 支持 YouTube 视频下载
- 支持标准链接和短链接（youtu.be）
- 多种画质可选：360p、480p、720p、1080p、best
- 自动选择最佳画质
- 视频音频自动合并
- 内置 ffmpeg 视频处理
- 实时下载进度显示
- 下载历史记录管理
- 简洁直观的 WinUI 3 用户界面
- 侧边栏导航

## 技术栈

| 分类 | 技术 |
|------|------|
| **编程语言** | C# |
| **UI 框架** | WinUI 3 (Windows App SDK) |
| **.NET 版本** | .NET 8.0 |
| **下载引擎** | yt-dlp |
| **视频处理** | ffmpeg |
| **架构** | MVVM |
| **打包格式** | MSIX |
| **平台** | Windows 10 (1809+) / Windows 11 |

## 系统要求

- 操作系统：Windows 10（版本 1809）或更高版本
- Windows 11
- 网络连接（用于下载视频）
- 硬盘空间：约 200MB（包含 yt-dlp 和 ffmpeg）

## 开始使用

### 安装方式

1. 从 [Microsoft Store](https://apps.microsoft.com/detail/9p7gwpc2pnjd?hl=zh-CN&gl=CN) 下载并安装

### 使用方法

1. 打开应用
2. 在首页输入 YouTube 视频链接
3. 选择 desired 画质（360p、480p、720p、1080p 或 best）
4. 点击下载按钮
5. 在下载列表中实时查看下载进度
6. 下载完成后可在下载历史中找到记录

## 项目结构

```
YouTubeDownloader/
├── Assets/                 # 应用图标和图片资源
├── Models/                 # 数据模型
├── ViewModels/             # MVVM ViewModel
├── Views/                  # 页面视图
├── Services/               # 服务层（YouTubeService 等）
├── Helpers/                # 辅助工具类
├── Package.appxmanifest    # MSIX 清单文件
├── MainWindow.xaml        # 主窗口
└── YouTubeDownloader.csproj # 项目文件
```

## 核心模块

### HomePage / HomeViewModel
- 视频链接输入和解析
- 画质选择
- 视频信息展示

### DownloadListPage / DownloadListViewModel
- 下载任务列表
- 实时进度显示
- 暂停/取消下载

### HistoryPage / HistoryViewModel
- 下载历史记录
- 查看已完成下载

### SettingsPage / SettingsViewModel
- 下载路径设置
- 引擎更新

### AboutPage / AboutViewModel
- 应用信息
- 版本信息

## 隐私说明

- 不收集任何个人数据
- 所有处理在本地设备完成
- 不上传用户数据到服务器
- 下载文件保存在用户选择的位置

## 许可证

MIT License

## 第三方组件

| 组件 | 许可证 | 用途 |
|------|--------|------|
| yt-dlp | Unlicense | YouTube 视频解析和下载 |
| ffmpeg | LGPL v2.1 | 视频和音频合并处理 |

## 关于

- **版本：** 2.0.0
- **开发者：** Alootcold
- **发布平台：** Microsoft Store

## 致谢

- [yt-dlp](https://github.com/yt-dlp/yt-dlp) - 优秀的开源视频下载工具
- [FFmpeg](https://ffmpeg.org/) - 强大的多媒体处理框架
- [Microsoft WinUI 3](https://learn.microsoft.com/windows/apps/winui/winui3/) - 现代 Windows UI 框架
