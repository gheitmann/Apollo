# Changelog

## 1.1.0.1 — local ALPHA1 (from original 1.1.0.0)

- Target MediaPortal 1.39 x86 while preserving Apollo artwork, tiles and themes.
- Rebuild Apollo support for configured paths, robust tile initialization and
  settings backup without hard-coded ProgramData paths or shutdown exceptions.
- Repair malformed tile-editor XML and the unresolved MusicInfoHandler import.
- Expose missing native video, slideshow OSD, actor-fetch and key-timeout options
  using the existing Apollo style and current translated MediaPortal labels.
- Hide unset user-name captions instead of showing property tokens.
- Repair the font archive mapping and retain skin-local fonts.
- Stop replacing global English strings and bundled legacy third-party plugins.
- Preserve existing skin settings during native file replacement; add a
  single-file EXE launcher with backup/restore and guarded presentation fixes.

Full-client interaction/upgrade verification is pending a manual test-window
close. See VALIDATION.md; this is not a stable release or x64 build.

- Rebuilt support plugin as AnyCPU and added architecture-selected installer launchers for the regular local test installation.
- Published source and maintenance guidance; local plugin/service coverage remains tracked separately.
