# 钉子 🔩

Windows 窗口置顶工具 — 两个浮动钉子，一键把任意窗口钉在最上层。

## 用法

| 操作 | 效果 |
|------|------|
| 单击 🟢 绿钉子 | 进入瞄准模式 |
| 瞄准后点击任意窗口 | 该窗口置顶 |
| 单击 🔴 红钉子 | 进入瞄准模式 |
| 瞄准后点击已置顶窗口 | 取消置顶 |
| `Ctrl+Shift+T` | 一键置顶/取消当前窗口 |
| 拖动钉子 | 移动位置 |

## 编译

```powershell
dotnet build -c Release
```

输出在 `bin\Release\net8.0-windows\钉子.exe`

## 技术栈

- .NET 8 WinForms
- 全局鼠标钩子 (`WH_MOUSE_LL`)
- Win32 P/Invoke (`SetWindowPos`, `SetWindowLongPtr`)
