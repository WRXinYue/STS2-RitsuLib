# Godot 场景编写说明

本文专门说明 STS2 Mod 在 Godot 场景编写时的一个实践性问题：

- 很多面向场景的游戏类型，最好先在 Mod 里继承一层本地子类，再让编辑器绑定这个本地子类
- 同时，Mod 自己程序集里的 Godot C# 脚本需要在初始化时注册

---

## 为什么会这样

在当前 STS2 modding 使用的 Godot Mono 工作流里，很多来自游戏程序集的 C# 类型，如果直接绑定到 `.tscn` 场景上，在编辑器里并不稳定。

实际经验上，只有当 `.tscn` 里绑定的是“你自己 Mod 程序集里的脚本类型”时，编辑器的打开、序列化、重新绑定等行为才更可靠。

因此，一个稳定的经验法则是：

- 只要某个场景节点需要表现为游戏里的 Godot 类型，就先在 Mod 本地继承一个子类，再把场景绑定到这个子类

---

## 包装子类模式

不要让场景直接绑定 `NEnergyCounter` 这种游戏类型，而是先写一个本地脚本：

```csharp
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace MyMod.Scripts
{
    public partial class MyEnergyCounter : NEnergyCounter
    {
    }
}
```

然后在 `.tscn` 里绑定 `MyEnergyCounter`，而不是直接绑定 `NEnergyCounter`。

这个包装子类完全可以是空的。很多时候它存在的意义，就是给编辑器一个“属于你自己 Mod”的本地脚本类型。

---

## 哪些类型通常适合这样做

常见需要本地包装的面向场景类型包括：

- `NEnergyCounter`
- `NRestSiteCharacter`
- `NCreatureVisuals`
- `NSelectionReticle`
- `MegaLabel`

这些通常都是场景根节点，或者场景中的脚本化子控件。

---

## 通用示例模式

一个自定义能量球场景，通常会这样绑定：

- 根脚本 -> `MyEnergyCounter : NEnergyCounter`
- 标签子节点脚本 -> `MyCounterLabel : MegaLabel`

一个角色视觉场景，通常会这样绑定：

- 根脚本 -> `MyCreatureVisuals : NCreatureVisuals`

一个休息点场景，通常会这样绑定：

- 根脚本 -> `MyRestSiteCharacter : NRestSiteCharacter`

重点不在于类名本身，而在于：场景里绑定的脚本应该属于你自己的 Mod 程序集。

---

## 编辑器侧规则

只要 Godot 编辑器需要打开、序列化或重新绑定某个 Mod 场景里的脚本，就优先使用“Mod 本地子类”。

即使：

- 你暂时没有额外逻辑
- 这个类只是简单继承一行
- 运行时目标类型本来就在游戏程序集里已经存在

这个包装也不是多余的，它本质上就是你自己的场景与编辑器之间的兼容层。

---

## 运行时脚本注册

如果你的 Mod 使用了 Godot C# 场景脚本，请在初始化阶段调用 `EnsureGodotScriptsRegistered(...)`：

```csharp
using System.Reflection;

RitsuLibFramework.EnsureGodotScriptsRegistered(
    Assembly.GetExecutingAssembly(),
    Logger);
```

这会让 Godot 的脚本桥接层去发现并注册你自己程序集里的 C# 脚本。

一个典型的 Mod 初始化入口，应该在内容注册之前完成这一步，这样运行时才能稳定发现你自己的场景脚本。

---

## 推荐工作流

只要你的 Mod 要做自定义 Godot 场景，建议按这个流程：

1. 先确定你需要的游戏侧基类
2. 在 Mod 本地创建一个继承它的薄包装 `partial class`
3. 让 `.tscn` 绑定这个本地脚本
4. 在初始化入口中调用 `EnsureGodotScriptsRegistered(Assembly.GetExecutingAssembly(), Logger)`

这样编辑器绑定和运行时脚本查找都会稳定很多。

---

## 哪些情况通常不需要这样做

以下内容通常不需要额外包装子类：

- 普通内容模型类，例如 card / relic / power / character
- 不作为 Godot 脚本使用的纯 C# 辅助类
- 从不绑定到 `.tscn` / Godot 场景资源的逻辑类

这份说明只针对 Godot 场景编写与脚本绑定问题。

---

## 相关文档

- [快速入门](GettingStarted.md)
- [角色与解锁脚手架](CharacterAndUnlockScaffolding.md)
- [资源配置与回退规则](AssetProfilesAndFallbacks.md)
- [诊断与兼容层](DiagnosticsAndCompatibility.md)
