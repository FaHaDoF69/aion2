# 🚀 Aion2 项目快速入门指南

## 📁 项目结构（重构后）

```
aion2/
├── 📦 Aion2Helper.Core/      # 共享类库 - 所有通用代码
├── 🖥️  Aion2Helper.Client/    # 用户端 - 游戏辅助程序
├── ⚙️  Aion2Helper.Admin/     # 管理端 - 数据管理程序
├── 📄 aion2.sln              # 解决方案文件
├── 📂 SQL/                   # 数据库脚本
└── 📂 docs/                  # 文档
```

## ✨ 各项目说明

### 🖥️ Aion2Helper.Client (用户端)
**用途**: 在游戏客户端旁运行的辅助工具

**主要功能**:
- ✅ 游戏窗口自动检测
- ✅ 拍卖行物品识别（OCR + 图像识别）
- ✅ 价格监控和提醒
- ✅ 自动购买功能
- ✅ 价格趋势分析

**运行方式**:
```bash
dotnet run --project Aion2Helper.Client/Aion2Helper.Client.csproj
```

### ⚙️ Aion2Helper.Admin (管理端)
**用途**: 管理所有机器的数据

**主要功能**:
- ✅ 查看所有机器的监控物品
- ✅ 查看所有机器的购买记录
- ✅ 统计分析（活跃机器数、总购买次数、总利润）
- ✅ 批量管理监控物品

**运行方式**:
```bash
dotnet run --project Aion2Helper.Admin/Aion2Helper.Admin.csproj
```

### 📦 Aion2Helper.Core (共享库)
**用途**: 被Client和Admin引用的共享代码库

**包含内容**:
- 📊 Models: 所有数据模型
- 🗄️ Data: 数据库上下文
- 🔧 Services: 业务逻辑服务
- 🛠️ Utils: 工具类
- ⚙️ Config: 配置类

## 🔨 开发指南

### 编译所有项目
```bash
dotnet build aion2.sln
```

### 清理并重新编译
```bash
dotnet clean aion2.sln
dotnet build aion2.sln
```

### 单独编译某个项目
```bash
dotnet build Aion2Helper.Client/Aion2Helper.Client.csproj
dotnet build Aion2Helper.Admin/Aion2Helper.Admin.csproj
dotnet build Aion2Helper.Core/Aion2Helper.Core.csproj
```

## 📝 代码修改指南

### 添加新功能时：

1. **如果是共享功能** (如新的数据模型、新的服务类):
   - ✅ 添加到 `Aion2Helper.Core` 项目
   - ✅ Client和Admin都可以使用

2. **如果是用户端专用** (如游戏窗口相关功能):
   - ✅ 添加到 `Aion2Helper.Client` 项目的 `Forms/` 文件夹

3. **如果是管理端专用** (如数据统计报表):
   - ✅ 添加到 `Aion2Helper.Admin` 项目

### 修改数据库相关代码：

1. 修改 Models → 在 `Aion2Helper.Core/Models/`
2. 修改 DbContext → 在 `Aion2Helper.Core/Data/Aion2DbContext.cs`
3. 修改服务类 → 在 `Aion2Helper.Core/Services/`

## 🗄️ 数据库说明

两个项目共享同一个数据库，数据通过`MachineCode`字段区分不同机器。

**用户端**: 只查看和操作当前机器的数据  
**管理端**: 可以查看所有机器的数据

## ⚠️ 注意事项

1. ✅ 修改Core项目代码后，需要重新编译Client和Admin项目
2. ✅ 添加新的NuGet包到Core项目时，记得还原包：`dotnet restore`
3. ✅ images文件夹只在Client项目中，Admin项目不需要游戏图像
4. ✅ 两个前端项目都使用相同的命名空间 `Aion2Helper`

## 🎯 接下来做什么？

### 用户端开发
- 在 `Aion2Helper.Client` 中继续完善游戏辅助功能
- 优化图像识别准确率
- 添加更多监控策略

### 管理端开发
- 完善监控物品的添加/编辑功能
- 添加数据导出功能（Excel/CSV）
- 添加更丰富的数据可视化图表
- 实现远程管理功能

### 共享库优化
- 优化数据库查询性能
- 添加更多通用工具类
- 完善日志记录功能

---

**重构完成时间**: 2025-10-07  
**当前版本**: v2.0.0  
**编译状态**: ✅ 全部成功 (0 Warning, 0 Error)

