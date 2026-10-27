# ALPHA1 validation — 27 October 2026

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

MediaPortal x64 is outside scope. Other client versions can display an informed
compatibility warning and offer the original native installer; no untested
cross-version compatibility claim is made.

## Local installation follow-up

The support plugin was rebuilt as AnyCPU. Matching x86/x64 installer launchers
are included so the existing local x64 client can be used for practical skin
tests. The previous isolated runtime is no longer the test target.

The normal Backdrop synthetic databases and media will be reused in place.
Six official plugin packages were staged: Extensions 1.5.0.43, Global Search
2.1.0.12, IMDb+ 2.1.0.581, InfoService 1.9.0.81, LogoManager 1.0.5.63 and Trakt
6.0.7.474. Their existing files are preserved. LogoManager's optional Dark Glass
design has four missing archive entries upstream and is omitted as a unit.
No account is linked and no scrobbling or external catalogue import is performed.

Local elevated deployment and the final interaction pass are pending. These
prepared plugin payloads are not evidence of full service compatibility.
