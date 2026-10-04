# TODOs

- VGE-root translations: the helmet oxygen comp's `chargeNoun` ("oxygen u³",
  `1.6/Mods/VanillaGravshipExpanded/Patches/HelmetOxygen.xml`) is untranslated in every
  language and absent from `Scripts/expected-injections.json`, apparently because the probe
  refresh boots without VGE, so the checker cannot flag it. Adding VGE to the boot list alone
  makes it worse: the checker places an entry by the root that declares the def, and the helmet
  is main-tree, so it would demand the key in the main tree (a startup error without VGE). Needs
  an upstream engine change that attributes patch-added keys to the gate that adds them, then
  the entry under that root's `Languages/`.
- profile the postfix enabling zoomed-out thruster-glow recoloring on our old heavily modded save

## Open questions

- Salvagers: watch what Vanilla Expanded does with Odyssey's salvager faction, then revisit
  whether they should field the set.

## Infrastructure follow-ups

- **Cut a release candidate to exercise CI before the real release.** The release
  workflow fetches VEF and VFEP from the Workshop with SteamCMD (anonymous login) and injects
  them via `VEF_PATH` / `VFEP_PATH`, but it has never run at all. `/release major rc` tags
  `v1.0.0-rc.1`: a GitHub prerelease that needs no CHANGELOG section. Check the Workshop
  fetch step, the translation gate and the zip's contents.
- **First Workshop publish.** Upload writes `About/PublishedFileId.txt`; commit it (every
  sibling tracks it), add the Workshop link to the README's Installation section, fill the
  id into the README's commented-out Steam badges and uncomment them, and paste
  `.steamworkshop/Description/English.txt` into the page.
