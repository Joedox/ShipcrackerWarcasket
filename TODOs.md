# TODOs

## Open questions

- Salvagers: watch what Vanilla Expanded does with Odyssey's salvager faction, then revisit
  whether they should field the set.

## Infrastructure follow-ups

- **Run tests and lint as an early step in the release flow**
- **Cut a release candidate to exercise CI before the real release.** The release
  workflow fetches VEF and VFEP from the Workshop with SteamCMD (anonymous login) and injects
  them via `VEF_PATH` / `VFEP_PATH`, but it has never run at all. `/release major rc` tags
  `v1.0.0-rc.1`: a GitHub prerelease that needs no CHANGELOG section. Check the Workshop
  fetch step, the translation gate and the zip's contents.
- **First Workshop publish.** Upload writes `About/PublishedFileId.txt`; commit it (every
  sibling tracks it), add the Workshop link to the README's Installation section, fill the
  id into the README's commented-out Steam badges and uncomment them, and paste
  `.steamworkshop/Description/English.txt` into the page.
