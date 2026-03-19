# STS2-RitsuLib

A personal shared framework library for Slay the Spire 2 mods.

Chinese README: [README.zh.md](README.zh.md)

This library is primarily developed for personal use, so development pace is need-driven and not strictly scheduled.

It was created as an alternative to [BaseMod](https://github.com/Alchyr/BaseLib-StS2) due to design and coding style differences.
There is currently no conflict between this library and BaseMod.

Documentation index: [Docs/README.md](Docs/README.md)

## Debug Compatibility Mode

RitsuLib provides a debug compatibility mode for localization lookup failures.

- Setting: debug_compatibility_mode
- Default: disabled (false)
- Behavior when enabled: missing LocTable keys no longer throw immediately; they fall back to key placeholder text and emit a warning log.

Settings file path on Windows:

%appdata%\SlayTheSpire2\steam\<user_id>\mod_data\com.ritsukage.sts2-RitsuLib\settings.json

## License

MIT
