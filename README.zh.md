# STS2-RitsuLib

English README: [README.md](README.md)

面向 Slay the Spire 2 Mod 的个人共享框架库。

本库主要是为了自己使用而开发，因此开发进度比较随缘，功能以个人需求为驱动。

因为不喜欢 [BaseMod](https://github.com/Alchyr/BaseLib-StS2) 的设计以及编码习惯，因此自己实现了该库。
目前与 BaseMod 没有冲突。

文档入口: [Docs/README.md](Docs/README.md)

## Debug 兼容模式

RitsuLib 提供了一个用于本地化缺失场景的 debug 兼容模式。

- 配置项: debug_compatibility_mode
- 默认值: 关闭（false）
- 开启后行为: LocTable 缺失键不再直接抛异常，而是回退为 Key 占位符并输出警告日志。

Windows 下 settings 文件路径:

%appdata%\SlayTheSpire2\steam\<user_id>\mod_data\com.ritsukage.sts2-RitsuLib\settings.json

## 许可证

MIT
