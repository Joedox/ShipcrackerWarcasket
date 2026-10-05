# TODOs

## Open questions

- Salvagers: watch what Vanilla Expanded does with Odyssey's salvager faction, then revisit
  whether they should field the set.

## Infrastructure follow-ups

- **Run tests and lint as an early step in the release flow**
- **Upstream l10n: name the active language's load errors in the smoke report.** The game's
  `Translation data for language English has N errors` warning is opaque; the probe could dump
  `LanguageDatabase.activeLanguage.loadErrors` and each `DefInjectionPackage.loadErrors` into the
  log dump so the smoke engine lists them with their file source. On this mod's pinned list the
  four are SOS2's Odyssey-root English DefInjected entries for `VacskinGland` (HediffDef and
  ThingDef, label and description), whose Odyssey defs carry a Royalty `MayRequire`; a boot
  without Royalty has no such defs. Third-party, correctly classified `other`, not gated.
- **First Workshop publish.** Upload writes `About/PublishedFileId.txt`; commit it (every
  sibling tracks it), add the Workshop link to the README's Installation section, fill the
  id into the README's commented-out Steam badges and uncomment them, and paste
  `.steamworkshop/Description/English.txt` into the page.
