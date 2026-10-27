# ALPHA2 validation â€” 27 October 2026

Target: MediaPortal 1.39 x86, Windows 11. Original Apollo 1.1.0.0 artwork/layouts
and themes retained. Experimental candidate; source is published for review.

## Completed

- Original package extracted and reconciled by destination. One missing GUID/font
  archive reference was resolved to the unique existing Lato-Regular.ttf entry.
- All 537/537 payload XML definitions parse. A focused native contract audit of
  91 matching window definitions checked 449 existing native field/control
  bindings with no mismatched known types. It does not cover every inherited
  field or third-party window. Seventeen absent fields remain for review, many
  related to optional overlays/details; absence alone is not a proved failure.
- All 1,510/1,510 original image and font payloads are byte-identical to the
  supplied package. No replacement imagery was generated.
- All 2,069/2,069 package payload entries and 14/14 private bundle entries were
  reopened and byte-compared. No global language resources, legacy restore EXE,
  third-party plugin binaries or MediaPortal core binaries are included.
- Current native x86 MPE serializer accepts the package and recognizes its skin
  and platform metadata.
- Native plugin initialization/lifecycle probe passes against official x86
  assemblies; the user property and tile lists initialize, and the settings
  backup matches the input file. Config/Skin paths remain in the isolated root.
- Native x86 file-copy provider installs all 2,069 mapped files into a separate
  disposable target. All 2,068 non-settings payloads match their native extracted
  inputs; an existing sentinel SkinSettings.xml is preserved by NeverOverwrite.
  This exercises the real native copy provider without a wizard or installed
  extension registration. It is not an end-to-end interactive upgrade test.
- Single-EXE VerifyOnly succeeds in normal Windows context: x86 selection,
  private MPE binding, configured Config/Skin/Plugins target paths, readable
  presentation buttons and filename repaint. The reused presentation probe
  records 2,393/2,393 synthetic events; this is not real install-progress proof.
- Full x86 client starts and displays the original Apollo BasicHome tile layout
  in a window. The existing default/Darkness/Fanart artwork is retained.

## Test environment and limits

The x86 runtime is extracted from the official MediaPortal 1.39 installer into
`work/Apollo-runtime` outside this source folder. It uses separate config, skin,
language, database, log, thumbnail and cache directories. Existing managed plugin
versions and synthetic fixture databases were copied only into this test runtime;
they are not part of the skin package. Installed Backdrop/core files were not
changed.

The initial sparse configuration triggers MediaPortal's first-run size check.
A copy of the existing configured test profile was used instead. Its disabled
mouse/plugin settings required normalization. Missing tables in the initially
empty third-party databases were fixture setup failures, not skin fixes.

The visible client ignores automated input; Windows also denies termination of
the identified test process. A manual close was requested before completing the
corrected interaction pass. Thus menu activation, GUI settings changes, theme
switching, playback/seek/pause, populated guide/recording navigation and graceful
exit have NOT been certified in the full client for this build. Actual TV tuning
also requires a tuner mapping. Full historical plugin coverage is outside scope.

The raw user-caption placeholder seen with the plugin disabled was guarded with
`string.isset(#media.user)` in the final payload; its final live appearance remains
to be confirmed after closing/restarting the inaccessible test instance.

The first VerifyOnly run in the restricted sandbox failed to load the private
MPE assembly due to the .NET remote-source restriction. The identical preparation
passed in normal Windows context. No remote-source trust setting was changed.

## Installation and upgrade limits

Prefer the single EXE. It backs up/restores existing Apollo SkinSettings across
the native wizard's replacement flow. The MPE1 alone only guarantees preservation
when its copy provider sees an existing file; native uninstall-first upgrades
or the explicitly accepted original-installer fallback need a separate backup.
The full interactive upgrade route remains untested.

Full x64 coverage is outside scope. Other client versions can display an informed
compatibility warning and offer the original native installer; no untested
cross-version compatibility claim is made.

## Regular local installation and smoke test — 27 October 2026

The regular installed MediaPortal 1.39 x64 client is now the practical test
target. Apollo support is AnyCPU; x86 remains the package compatibility target.
No separate client installation is used for further skin tests.

The backed-up deployment completed successfully: all 2,581 planned destination
files were copied and checked by SHA-256 during deployment. The prior normal
profile is retained in the transaction backup. The existing Backdrop synthetic
media, movie/series/music databases, guide fixtures and thumbnails were reused.

Eight additional official plugin payloads were installed without replacing
pre-existing plugin/dependency files: Extensions 1.5.0.43, Global Search 2.1.0.12,
IMDb+ 2.1.0.581, InfoService 1.9.0.81, LogoManager 1.0.5.63, Trakt 6.0.7.474,
SubCentral 1.2.7.0 and WorldWeather Lite SharpDX 1.3.4.0 (local x64 package).
This was a backed-up file deployment, not eight native MPE wizard runs; extension
registration and individual service configuration are not certified.
LogoManager's optional Dark Glass design has four missing entries upstream and
was omitted as a unit. No account was linked.

The single EXE passes VerifyOnly against the regular client: correct configured
paths, private assembly binding, 3/3 readable buttons and 2,393/2,393 synthetic
presentation events. This does not establish interactive upgrade correctness.

Observed live checks passed:
- Apollo BasicHome opens and shows original tiles and synthetic Latest Movies.
- Mouse activation opens Moving Pictures; keyboard confirmation opens All Movies.
- The catalogue lists three seeded movies with artwork and descriptions.
- Movie details open and the local 20-second synthetic MKV plays in fullscreen.
- The client returns to details after playback. The subsequent stop key was sent
  after the short clip had already ended; manual stop is not certified.
- Alt+F4 closes the normal client; no MediaPortal window remains.

Remaining observations: the fullscreen definition references missing
`video-overlay.png`; LogoManager reports a design download URL returning 404 and
no matches for fictional fixture stations. These were not repaired in this pass.
The first-run absent Apollo texture-cache cleanup warning is also recorded.

Pause/seek, music playback, guide/recording navigation, theme switching and the
remaining historical plugins were not tested in this scoped pass. This is a
usable initial demo path, not complete plugin or x64 certification. See
PLUGIN-COVERAGE.md for the remaining integration inventory.

## ALPHA2 follow-up

The offscreen image control that referenced absent `video-overlay.png` now has
an empty texture, consistent with adjacent unused backgrounds. Native IDs,
visibility conditions and active OSD backgrounds are retained. The missing-file
error is absent in the subsequent normal startup log.

Rebuilt package checks pass again: 537/537 XML files, 449 checked native control
bindings without known type mismatches, 1,510/1,510 original artwork/font bytes
unchanged, 2,069/2,069 payload entries and 14/14 bundle entries byte-verified.
ALPHA2 EXE VerifyOnly succeeds against the installed client.

The local follow-up deployed and checksum-checked 13/13 files with a backup:
updated Apollo XML/support assembly plus missing MyLyrics 2.2.1.288 and mvCentral
1.0.8.453 files and enable flags. Existing shared dependencies were preserved.
Static member-reference resolution checked 223 MyLyrics and 555 mvCentral
non-framework references without unresolved results against the available local
assemblies; this is not complete service or runtime coverage. Both plugin file
versions are discovered by the normal client. Their complete screens/providers
remain unverified. The mvCentral archive omits its optional DLL configuration
file; no replacement config was invented.

Additional live checks: the populated TV guide opens, Right selects the next
programme and changes its detail text, Escape returns to BasicHome, and the
recorded-TV icon view opens with synthetic recordings. Actual tuning/recording
is not supplied by the fixtures.

Music fixture correction is local only. Three fictional titles had metadata but
empty artist/albumartist/genre tables. After a SQLite backup, one lookup entry
was added to each; the previously empty Album Artists screen now visibly lists
Backdrop Test Ensemble. The subsequent empty album result exposed missing
MediaPortal `| value |` delimiters in track metadata. These were normalized after
a second backup. The native-style album-artist query now matches 3/3 titles.
Final album traversal and audio playback after this normalization remain untested.

The reviewed AirPlayer DLL and TVWishList client DLL require 32-bit execution;
BrowseTheWeb includes a Required32Bit Gecko dependency. They were not installed
in the regular x64 client. This does not certify or reject their x86 runtime
compatibility. Further hardware/account-dependent integrations remain listed
in PLUGIN-COVERAGE.md. LogoManager's upstream directory-listing/download issue
is documented and not patched as part of this skin.
