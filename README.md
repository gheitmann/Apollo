# Apollo for MediaPortal 1.39

An original-design compatibility update of Apollo 1.1.0.0 by Wizard123 and YSMP.
The 1080p tile interface, artwork and existing themes are preserved. No new images
or redesign are included. The source package was released on 15 March 2016.

## ALPHA1 candidate

Use `dist/Apollo-Setup-ALPHA1.exe` with an installed **MediaPortal** client.
Close MediaPortal first. Setup selects the installed client or asks for its folder.
The optional `--mediaportal "C:\path\to\client"` argument selects a custom path.
`--verify` performs preparation and binding checks without installing anything.

The native package `dist/Apollo-MP139-x86-ALPHA1.mpe1` is also available for
MediaPortal Extension Installer users. Source is maintained at https://github.com/gheitmann/Apollo.
Builds remain experimental until the documented local smoke test is complete.

The setup embeds all skin files and its launcher in one EXE. Its presentation
adapter works on a private copy of the installed MPE assembly; no MediaPortal
core binary is distributed or replaced. Other client versions receive a
compatibility warning rather than being rejected by their version number alone.
The support plugin is AnyCPU; thin installer launchers select the installed client
architecture. x86 remains the compatibility target; local UI testing uses the
existing x64 installation. This is not full x64 certification.

## Changes from the original 1.1.0.0

- Rebuilt the small Apollo support plugin for current MediaPortal interfaces.
  It uses the configured directories, initializes tile lists and backs up settings
  only when Apollo is selected. Filesystem failures are logged without preventing
  application shutdown.
- Corrected two malformed, currently commented-out tile-editor XML variants and
  removed a missing optional MusicInfoHandler include.
- Added native renderer/audio, slideshow OSD, actor-fetch and key-timeout settings
  using Apollo controls. Existing layouts and control bindings are retained.
- Suppressed raw user-caption placeholders when the support plugin is disabled.
- Reconciled the incorrect font archive reference. Fonts remain inside the skin.
- Removed replacement of MediaPortal's global English language file and removed
  the old third-party plugin/restore executable payloads. Existing plugin versions
  are not downgraded by this skin package.
- Preserved existing `SkinSettings.xml` on file replacement; the EXE launcher
  also backs it up and restores it across the native upgrade operation.
- Reused the reviewed installer filename-repaint and progress presentation fixes.

## Optional plugins

Use current x86-compatible versions of the plugins you need. The original skin
supports Moving Pictures, MP-TVSeries, Latest Media Handler, Fanart Handler and
other integrations; these third-party plugins are not bundled or repaired by
this package. Some tile content requires those plugins and populated databases.
Keep third-party or MediaPortal core troubleshooting separate from skin changes.

## Build and validation

See [validation](docs/VALIDATION.md) for actual checks and remaining limits.
`tools/build.ps1` compiles against a supplied MediaPortal x86 client.
`tools/migrate.py` applies the recorded XML/package transformations to the extracted
original, `tools/audit.py` checks native control bindings, and `tools/package.py`
builds and byte-checks the MPE1 and private setup bundle. Installer sources reuse
the Backdrop implementation with Apollo names and architecture selection.

The local runtime and synthetic fixtures are outside this source folder. No
personal MediaPortal configuration, databases, logs or test media are packaged.

## Attribution

Original skin and plugin: **Wizard123 and YSMP**. Apollo is listed under the
GNU General Public License in the official MediaPortal catalogue:
https://www.team-mediaportal.com/extensions/widescreen/apollo

Retained artwork and third-party names belong to their respective authors.
The independent Mono.Cecil installer dependency includes its MIT license in
the private setup bundle. Compatibility changes and generated assembly source
are supplied here for review.
