# 资源配置与回退规则

本文是资源配置结构、占位角色回退与资源路径诊断的参考文档。

RitsuLib 使用资源配置对象来描述可覆写的美术、场景、材质以及相关资源。

本文专门解释这些配置对象的结构，以及它们背后的回退规则。

---

## 为什么要有资源配置对象

资源覆写当然可以做成一长串平铺的虚属性。

但 RitsuLib 选择把它们组织成记录类型形式的资源配置，因为这样更适合长期扩展：

- 相关资源会自然聚合在一起
- 局部覆写时可读性更高
- 回退合并规则更明确
- 从默认依赖占位角色的旧框架迁移时，也不用放弃结构化设计

对角色尤其如此，因为角色资源横跨场景、UI、VFX、音频、Spine 和多人模式贴图。

---

## 角色资源配置结构

`CharacterAssetProfile` 被拆成多个嵌套记录类型：

- `CharacterSceneAssetSet`
- `CharacterUiAssetSet`
- `CharacterVfxAssetSet`
- `CharacterSpineAssetSet`
- `CharacterAudioAssetSet`
- `CharacterMultiplayerAssetSet`

这样你只改一个类别时，不会把其他类别也拖成噪音。

例如：

```csharp
public override CharacterAssetProfile AssetProfile => new(
    Scenes: new(
        VisualsPath: "res://MyMod/scenes/character/my_character.tscn",
        EnergyCounterPath: "res://MyMod/ui/energy/my_energy_counter.tscn"),
    Ui: new(
        IconTexturePath: "res://MyMod/ui/top_panel/icon.png",
        MapMarkerPath: "res://MyMod/map/map_marker.png"),
    Audio: new(
        AttackSfx: "event:/sfx/characters/my_character/attack"));
```

---

## 占位角色回退

`ModCharacterTemplate` 现在提供：

```csharp
public virtual string? PlaceholderCharacterId => "ironclad";
```

它的行为是：

- 先读取你显式写下的 `AssetProfile`
- 缺失项再从 `CharacterAssetProfiles.FromCharacterId(PlaceholderCharacterId)` 补齐
- 如果 `PlaceholderCharacterId` 为 `null`，则彻底关闭回退

这让你既拥有类似 BaseLib 的迁移便利，又保留了 Ritsu 式的结构化角色 API。

---

## 角色资源配置如何合并

RitsuLib 对角色资源配置的合并是“按类别、按字段”进行的。

这意味着：

- 你提供一个自定义 `Scenes` 记录，不会影响 `Ui`
- 你只写 `RestSiteAnimPath`，不会把 `MerchantAnimPath` 清空
- 你只写 `AttackSfx`，不会把其余默认音效抹掉

这点非常重要，因为角色资源在实际开发里几乎从来不是一次性全量替换的。

---

## CharacterAssetProfiles 辅助 API

`CharacterAssetProfiles` 提供了这些工具方法：

- `FromCharacterId(string)`
- `Ironclad()` / `Silent()` / `Defect()` / `Regent()` / `Necrobinder()`
- `Resolve(profile, placeholderCharacterId)`
- `Merge(fallback, profile)`
- `FillMissingFrom(...)`
- `WithPlaceholder(...)`
- `WithScenes(...)`、`WithUi(...)`、`WithVfx(...)`、`WithSpine(...)`、`WithAudio(...)`、`WithMultiplayer(...)`

它们主要服务两类场景：

- 新角色的局部资源编写
- 从默认假定占位角色的旧框架迁移到 RitsuLib

---

## 其他内容的资源配置

RitsuLib 也为其他内容提供了类似的资源配置记录类型：

- `CardAssetProfile`
- `RelicAssetProfile`
- `PowerAssetProfile`
- `OrbAssetProfile`
- `PotionAssetProfile`
- `AfflictionAssetProfile`
- `EnchantmentAssetProfile`
- `ActAssetProfile`

这些配置对象更小，是因为它们各自的资源表面本来就更小。

---

## 路径辅助工厂

对于符合原版命名习惯的资源布局，RitsuLib 提供了几个常用辅助方法：

- `CharacterAssetProfiles.FromCharacterId(...)`
- `ContentAssetProfiles.Card(...)`
- `ContentAssetProfiles.Relic(...)`
- `ContentAssetProfiles.Power(...)`
- `ContentAssetProfiles.Orb(...)`
- `ContentAssetProfiles.Potion(...)`
- `ContentAssetProfiles.Affliction(...)`
- `ContentAssetProfiles.Enchantment(...)`
- `ContentAssetProfiles.Act(...)`

另外还有 `CharacterAssetPathHelper`，可用于推导角色相关默认路径，例如角色视觉场景、能量球、角色选择背景、小地图标记等。

当你的资源布局本来就遵循某种命名约定时，这些辅助方法会很省事。

如果这些资源背后是自定义 Godot 场景，请记得场景根节点和带脚本的子节点往往需要使用 Mod 本地包装类，编辑器绑定才会更稳定。详见 [Godot 场景编写说明](GodotSceneAuthoring.md)。

---

## 能量球场景、大能量图标、文本图标是三层能力

RitsuLib 明确把它们拆开：

- `CustomEnergyCounterPath`：完整战斗能量球场景
- `BigEnergyIconPath`：通过 `EnergyIconHelper` 解析的大图标
- `TextEnergyIconPath`：富文本描述里的小图标

这样拆的原因是：

- 场景替换才是自定义能量球的正确抽象
- 纹理路径才是池图标的正确抽象
- 把三件事混进一个 API 只会让职责变糊

---

## 缺失路径诊断

RitsuLib 现在通过 `AssetPathDiagnostics` 统一校验资源路径覆写。

当前行为：

- 路径为空 -> 忽略 override
- 路径存在 -> 使用 override
- 路径不存在 -> 输出一次警告，并回退到原始资源

警告里会尽量带上：

- 宿主类型
- 若可用则带上模型条目标识
- 对应的配置成员名
- 缺失路径本身

这让资源接错线时比以前更容易定位。

---

## 哪些内容会做路径校验

路径校验主要覆盖这类“真正的资源路径”：

- 卡牌贴图、材质、覆盖层、横幅
- 遗物 / 能力 / 球体 / 药水图标
- Act 背景
- 角色视觉场景、能量球、小地图资源、轨迹场景、Spine 数据
- 卡池级能量图标路径

而像音效事件 id 这种“不是 Godot 资源路径”的字符串不会走 `ResourceLoader` 校验。

所以角色 SFX 覆写字段仍然被当作普通字符串值处理，而不是资源路径。

---

## 推荐的角色资源写法

对大多数自定义角色，比较推荐的模式是：

1. 保留 `PlaceholderCharacterId = "ironclad"`，或者改成你想继承风格的基础角色
2. 只覆写真正自定义的资源
3. 能量图标相关优先放在 pool 级 `BigEnergyIconPath` / `TextEnergyIconPath`
4. 只有真的要换完整能量球 UI 时，再使用 `CustomEnergyCounterPath`

这样内容编写面会比较小，同时还能保留安全回退。

---

## 推荐的普通内容资源写法

对卡牌及其他内容：

- 当多个资源字段属于同一个决策时，优先使用 `AssetProfile`
- 只有单点特例时，再直接覆写某个 `Custom...Path`
- 当你的资源布局与辅助方法约定一致时，优先考虑 `ContentAssetProfiles.Card(...)` 这类工厂

尤其是卡牌，把立绘、边框、覆盖层、横幅放在一个配置对象里通常会更清晰。

---

## 相关文档

- [角色与解锁脚手架](CharacterAndUnlockScaffolding.md)
- [内容注册规则](ContentAuthoringToolkit.md)
- [Godot 场景编写说明](GodotSceneAuthoring.md)
- [诊断与兼容层](DiagnosticsAndCompatibility.md)
- [框架设计](FrameworkDesign.md)
