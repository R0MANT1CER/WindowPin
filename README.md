# WindowPin  🔩

> Click a floating pin, then click any window — it stays on top.  
> 点击浮动钉子 → 瞄准任意窗口 → 该窗口永远置顶。

---

## 🤔 Why? / 为什么要做这个？

Laptop screens are too small. When you split-screen to copy code, both windows shrink to half size and become unusable.  

**WindowPin** lets you pin the reference window on top, then maximize your editor — the pinned window floats above, always visible, no split-screen needed.  

笔记本屏幕太小了——分屏抄代码时两个窗口各占一半，等比例缩小后根本没法看。这个工具让你把参考页面钉在最上层，然后把编辑器全屏，参考窗口始终浮在上面，不再需要分屏。

---

## 🎮 Usage / 用法

| Action / 操作 | Effect / 效果 |
|:---|:---|
| **Click** 🟢 green pin / 单击绿钉子 | Enter aim mode (cursor → ✚) |
| Click target window / 点击目标窗口 | Window pinned to top / 窗口永浮最上层 |
| **Click** 🔴 red pin / 单击红钉子 | Enter aim mode |
| Click a pinned window / 点击已置顶窗口 | Window unpinned / 取消置顶 |
| `Ctrl` + `Shift` + `T` | Pin / unpin foreground window / 置顶/取消当前窗口 |
| **Drag** any pin / 拖动钉子 | Move it around / 自由移动 |

---

## 🎨 Visual feedback / 颜色反馈

| Color / 颜色 | Meaning / 含义 |
|:---|:---|
| 🟡 Gold / 金色 | Top-most applied / 置顶成功 |
| 🟠 Orange / 橙色 | Top-most removed / 取消置顶 |
| ⚪ Gray / 灰色 | Already in that state / 已是该状态 |

---

## 🐛 Known Issues / 已知问题

- **Duplicate browser windows** — Two identical browser windows (same class name, same size) can confuse window targeting. The pin may hit window B when you aimed at window A.  
  **相同浏览器窗口** — 两个同款浏览器窗口同时存在时，钉子可能瞄 A 却命中了 B。

- **Some terminals & apps don''t respond** — Certain terminal emulators (especially those with built-in always-on-top) and UWP/ApplicationFrameWindow apps may ignore `WS_EX_TOPMOST`. This is a limitation of the Windows window manager, not WindowPin itself.  
  **部分终端和 UWP 应用无效** — 自带置顶功能的终端和部分 UWP 应用可能无视置顶指令，这是 Windows 窗口管理器的限制。

- **Top-most windows cover the pins** — After pinning, the target window enters the top-most layer alongside the pins. WindowPin forces itself above, but rapid z-order changes can occasionally bury the pins.  
  **钉子偶尔被压** — 置顶后目标和钉子在同一个顶层，虽然做了碾压逻辑，极端情况钉子仍可能被短暂遮挡。

---

## 🔧 Build / 编译

```powershell
dotnet build -c Release
```

Output → `bin\Release\net8.0-windows\WindowPin.exe`

Requires: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## 🛠 Tech / 技术

- .NET 8 WinForms · `WH_MOUSE_LL` global hook · `SetWindowPos` / `SetWindowLongPtr` / `GetTopWindow`

---

## 📜 License

MIT
