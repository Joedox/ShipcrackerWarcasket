# TODOs

## Open questions

- Salvagers: watch what Vanilla Expanded does with Odyssey's salvager faction, then revisit
  whether they should field the set.

## Infrastructure follow-ups

- **Run tests and lint as an early step in the release flow**
- **Tag rimworld-l10n v1.4.0 and let the release pin bump pick it up.** The canonical checkout's
  commit dd91bfe makes the probe dump the errors behind the game's `Translation data for
  language English has N errors` warning with their source files, and the smoke engine report
  each by origin (`tools/tag.sh minor`, not yet pushed). Verified against this mod's list: the
  four are SOS2's `VacskinGland` entries, reported under `other`, not gated.
- **First Workshop publish.** Upload writes `About/PublishedFileId.txt`; commit it (every
  sibling tracks it), add the Workshop link to the README's Installation section, fill the
  id into the README's commented-out Steam badges and uncomment them, and paste
  `.steamworkshop/Description/English.txt` into the page.
