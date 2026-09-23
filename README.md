# BatchRenamer 批量重命名工具

一个 Windows 小工具：把文件夹里的文件按当前顺序批量重命名为数字（例如 5 个文件依次重命名为5，4，3，2，1），保留原扩展名。

## 功能

- 批量重命名为数字，保留原扩展名
- 排序方式：名称 / 修改时间 / 创建时间 / 大小 / 类型
- 编号方向：从大到小 / 从小到大
- 用 0 补齐位数（如 05，04…）
- 拖拽文件夹或文件到窗口
- 浅色 / 深色主题（自动记住上次选择）
- 高 DPI 适配（在 125% / 150% 缩放屏幕下清晰显示）
- 两阶段重命名，避免重名冲突；出错自动回滚

## 构建

需要 .NET Framework 4.x（Windows 10 / 11 自带）。在项目目录下双击 `build.bat`，或手动执行：

```bat
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe ^
  /nologo /target:winexe /optimize+ /codepage:65001 ^
  /out:BatchRenamer.exe ^
  /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll ^
  RenameTool.cs
```

> 编译产物为 `BatchRenamer.exe`，可直接改名为 `批量重命名.exe`。

## 使用

把编译出的 exe 放到要重命名的文件夹里，双击运行即可；也可以用「浏览」按钮或直接把文件夹拖进窗口。

## 技术说明

- C# / WinForms，单文件源码（`RenameTool.cs`）
- 无第三方依赖
- 界面为自绘 Fluent 风格（圆角、悬停反馈、深色模式）
