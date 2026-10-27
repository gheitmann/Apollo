# Changelog

## 1.1.0.2 — ALPHA2

- Remove the dangling TV fullscreen overlay texture reference from an unused offscreen decoration. No replacement artwork or OSD layout changes.
- Extend the regular local smoke test to populated EPG navigation and recorded-TV icon view.
- Install MyLyrics 2.2.1.288 and mvCentral 1.0.8.453 locally after static API review; third-party binaries remain outside the skin package.
- Repair the separate synthetic music fixture lookup tables and MediaPortal multi-value tag encoding. No personal database is packaged.
- Document confirmed x86-only dependencies in the reviewed AirPlayer, BrowseTheWeb and TVWishList packages instead of loading them into the local x64 client.


## 1.1.0.1 â€” local ALPHA1 (from original 1.1.0.0)

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

Regular local installation and the scoped movie-navigation/playback/exit smoke
test passed. Full interactive upgrade and remaining integrations are unverified.
See VALIDATION.md; this experimental release is not full runtime certification.

- Rebuilt support plugin as AnyCPU and added architecture-selected installer launchers for the regular local test installation.
- Published source and maintenance guidance; local plugin/service coverage remains tracked separately.
