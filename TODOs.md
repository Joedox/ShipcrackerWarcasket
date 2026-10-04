# TODOs

- VGE-root translations: the helmet oxygen comp's `chargeNoun` ("oxygen u³",
  `1.6/Mods/VanillaGravshipExpanded/Patches/HelmetOxygen.xml`) is untranslated in every
  language and absent from `Scripts/expected-injections.json`, apparently because the probe
  refresh boots without VGE, so the checker cannot flag it. Work out how the refresh and checker
  should cover gated roots, then add the entry under that root's `Languages/`.
- profile the postfix enabling zoomed-out thruster-glow recoloring on our old heavily modded save
- `BreachJumpExtension.SpaceIcon` uses `??=` on a `Texture2D`, which bypasses Unity's overloaded
  null check (the family rule in CLAUDE.md; UNT0008). Harmless today because the field lives on a
  def extension that is replaced along with its def on a play-data reload, so it never outlives
  the texture it caches, but swap it for an explicit `== null` re-resolve (see
  `~/dev/PersonaWeaponsUnbound/Source/1.6/Defs/PWU_Textures.cs`) and check whether the Unity
  analyzers are wired into this csproj at all, since they did not flag it. Noted 2026-10-04.

## Open questions

- Acquisition: foundry only, by construction. Warcasket parts are destroyed on drop and
  untradeable, so raid presence (all three pieces carry `WarcasketVeteran` and `WarcasketAll`)
  is threat and flavor, never loot. AI pawns never cast apparel abilities (VEF's
  `Pawn.TryGetAttackVerb` postfix only draws on `LearnedAbilities`), so a raider in the set is a
  shielded 1.5-blunt spacer suit with no jump. Still open: whether the Odyssey compat root
  should add a dedicated tag hook so salvagers field the set more often than the uniform
  veteran pick does.
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
