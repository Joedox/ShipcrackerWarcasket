# TODOs

## Open questions

- Salvagers: watch what Vanilla Expanded does with Odyssey's salvager faction, then revisit
  whether they should field the set.

## Infrastructure follow-ups

- **Run tests and lint as an early step in the release flow**
- **Fix the l10n smoke engine upstream (rimworld-l10n v1.3.1).** v1.3.0's multi-world change
  gave `refresh.pinned_modsconfig` a required `mods` argument but missed the smoke engine's
  caller (`smoke/startup_smoke.py`, `launch()`), so `integration-smoke-test.py` crashes with a
  `TypeError` before booting, in every consumer. The crash also comes after `launch()` has
  deleted `Player.log`, and `player_log_path()` then refuses to run until one exists again.
  v1.0.0-rc.1's smoke test ran through a one-off wrapper supplying `SMOKE_ACTIVE_MODS`. Fix in
  the canonical checkout (pass `SMOKE_ACTIVE_MODS`, add a test that drives `launch()`, and make
  the log path not depend on the file existing), tag v1.3.1, then `bump-consumer.sh` here.
- **First Workshop publish.** Upload writes `About/PublishedFileId.txt`; commit it (every
  sibling tracks it), add the Workshop link to the README's Installation section, fill the
  id into the README's commented-out Steam badges and uncomment them, and paste
  `.steamworkshop/Description/English.txt` into the page.
