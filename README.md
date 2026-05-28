# WindowPin  🔩

> Click a floating pin, then click any window — it stays on top.  
> 点击浮动钉子 → 瞄准任意窗口 → 该窗口永远置顶。

---

## ✨ What is this? / 这是什么？

**WindowPin** is a lightweight Windows desktop tool that lets you pin ANY window to the top layer — browsers, terminals, IDEs, video players, virtual machines, UWP apps… everything works.  

Two floating pin icons live on your screen: 🟢 green to **pin**, 🔴 red to **unpin**.  

No install, no dependencies — just a single `.exe`.  

**WindowPin** 是一个轻量级 Windows 桌面工具。两个浮动钉子悬浮在屏幕上——🟢 绿色置顶，🔴 红色取消——点击钉子进入瞄准模式，再点任意窗口即可。浏览器、终端、IDE、播放器、虚拟机、UWP 应用统统支持。（单文件 exe，即开即用）

---

## 🎮 Usage / 用法

| Action / 操作 | Effect / 效果 |
|:---|:---|
| **Click** 🟢 green pin / 单击绿钉子 | Enter aim mode (cursor → ✚) / 进入瞄准模式 |
| Click target window / 点击目标窗口 | Window pinned to top / 窗口永浮最上层 |
| **Click** 🔴 red pin / 单击红钉子 | Enter aim mode / 进入瞄准模式 |
| Click a pinned window / 点击已置顶窗口 | Window unpinned / 取消置顶 |
| `Ctrl` + `Shift` + `T` | Pin / unpin current foreground window / 置顶/取消当前窗口 |
| **Drag** any pin / 拖动钉子 | Move it around / 自由移动位置 |

---

## 🎨 Visual feedback / 颜色反馈

| Color / 颜色 | Meaning / 含义 |
|:---|:---|
| 🟡 Gold / 金色 | Top-most applied / 置顶成功 |
| 🟠 Orange / 橙色 | Top-most removed / 取消置顶 |
| ⚪ Gray / 灰色 | Already in that state / 已经是该状态 |

---

## 🔧 Build / 编译

```powershell
dotnet build -c Release
```

Output → `bin\Release\net8.0-windows\WindowPin.exe`

Requirements: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## 🛠 Tech Stack / 技术栈

- **.NET 8** WinForms
- Global low-level mouse hook (`WH_MOUSE_LL`)
- Win32 P/Invoke: `SetWindowPos`, `SetWindowLongPtr`, `GetTopWindow`, `GetWindow`
- Z-order window enumeration for precise window targeting

---

## 📜 License

MIT — do whatever you want.
