# TODOs

- check the breach jump / breach burn gizmo icons in game, with and without VGE (purple variants shadow the orange ones from `Mods/VanillaGravshipExpanded/Textures/`)
- profile the postfix enabling zoomed-out thruster-glow recoloring on our old heavily modded save
- review pass on tightness of xml comments since they ship in the release bundle and bloat player downloads

## Scope

- One warcasket set for VFE Pirates: armor + shoulder pads + helmet, as three
  `VFEPirates.WarcasketDef`s parented on `VFEP_WarcasketArmorBase`,
  `VFEP_WarcasketShoulderPadBase`, `VFEP_WarcasketHelmetBase` (see VFEP
  `1.6/Defs/ThingDefs_Misc/Apparel_Various.xml` and `Apparel_Headgear.xml`). `WarcasketDef` adds
  only `shortDescription`, `isArmor`, `isShoulderPads`, `isHelmet` over `ThingDef`.
- Tuned for Odyssey's end-game threats, but Odyssey must stay optional. Decide what, if
  anything, is Odyssey-only (e.g. a pawnkind/apparel-tag hook into VFEP's `VFEP_Salvager_*`
  Odyssey pawnkinds, which VFEP adds via `1.6/Patches/Odyssey.xml`), and ship that from the
  `Mods/Odyssey` + `1.6/Mods/Odyssey` compat roots drafted in `LoadFolders.xml`.
- Odyssey's `PatchOperationFindMod` in VFEP matches by display name; our gate uses the package
  id `ludeon.rimworld.odyssey` via `IfModActive`.

## Open questions

- Acquisition: foundry recipe/research only, or also raid/trader presence? VFEP tags its own
  parts `WarcasketVeteran` for pawnkind generation; check `PawnKinds_Junkers.xml` before reusing
  the tag, since reusing it puts our set on every Junker veteran. Presence is already a
  question of frequency, not of yes/no: VFEP's `PawnGenerator.GeneratePawn` postfix fills any
  warcasket slot the apparel budget left empty from every loaded `WarcasketDef` at random, ours
  included (`StaticStartup.FillWarcasketDefLists` has no tag or research filter), and the
  `WarcasketAll` tag already makes all three pieces candidates for `VFEP_General`. AI pawns
  never cast apparel abilities (VEF's `Pawn.TryGetAttackVerb` postfix only draws on
  `LearnedAbilities`), so a raider in the set is a shielded 1.5-blunt spacer suit with no jump.
- Research gating: which VFEP research project(s) to parent on (`ResearchProjects_Various.xml`).

## Infrastructure follow-ups

- **Cut a 0.1.0 pre-release to exercise CI before the real release.** The release workflow
  fetches VEF and VFEP from the Workshop with SteamCMD (anonymous login) and injects them via
  `VEF_PATH` / `VFEP_PATH`, but it has never run at all. Push a pre-release tag such as
  `v0.1.0-rc.1` (BTG's first tag was `v0.1.0-alpha.1`): the workflow marks a release
  pre-release only when the tag contains `alpha`, `beta` or `-rc`, so a bare `v0.1.0` would
  publish as a normal release. The CHANGELOG section heading must match the tag without its
  `v` (`## [0.1.0-rc.1]`) or the notes step fails. The existing `## [0.1.0] - TBD`
  placeholder needs replacing either way. Check the Workshop fetch step, the translation gate
  and the zip's contents, then delete the pre-release and its tag.
- **Unit tests.** Five siblings (BTG, BionicThumbGuild, PWU, UWU, XenogermTraderStock) carry a
  headless xUnit net472 suite at `Tests/1.6/<Mod>.Tests.csproj` (Krafs ref, no live game;
  XenogermTraderStock's CLAUDE.md has the mono/copy-target notes). The pure logic here that is
  worth covering: the space preview's `PreviewState` bit packing, space-flight duration
  (`spaceFlightSpeedFactor` against the `spaceFlightMaxSeconds` cap), the thruster glow curve
  lookup, and whatever of the planet/space landing rules can be pulled out from `Map`.
- **First Workshop publish.** Upload writes `About/PublishedFileId.txt`; commit it (every
  sibling tracks it), add the Workshop link to the README's Installation section and the
  siblings' Steam subscriber/download/favorite/view badges beside the RimWorld one, and paste
  `.steamworkshop/Description/English.txt` into the page (add an art credit line to its Links
  section if any of the art is commissioned).
