# ALPHA3 — installer dependency detection fix

Fixes ALPHA2 installations being rejected with **MediaPortal 0.0.0.0** in the dependency dialog.

- Initialize the native compatibility context from the selected MediaPortal.exe assembly version instead of the private helper EXE version.
- Generate ApolloPlugin dependency metadata from the rebuilt DLL; remove stale metadata inherited from the original package.
- Include native MediaPortal, Skin and Apollo plugin dependency checks in setup verification, with the selected client version in diagnostics.
- Preserve the existing Apollo layouts, artwork and settings protection. No installed MediaPortal core binaries or registry values are modified by this correction.

**Validation:** native dependency regression checks pass with MediaPortal 1.39 x86 and x64 assemblies. Both reject a deliberately broken zero-version context and pass after correction. The complete single-EXE verification path passes against the regular local x64 installation. All 2,069 payload files and 14 private bundle entries are byte-verified; all 1,510 original artwork/font assets remain unchanged.

A completed interactive wizard installation and a new client playback/UI pass are not claimed for this installer-only correction. The reporting user's installation has not been retested. x86 remains the compatibility target; this is not full x64 plugin certification.

Download **Apollo-Setup-ALPHA3.exe** and close MediaPortal before installing. The MPE1 is an alternative for native Extension Installer users. Checksums and detailed validation notes are included.
