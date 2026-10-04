#!/usr/bin/env python3
# Shipcracker Warcasket's config shim over the shared sidecar-refresh engine
# (l10n/refresh/refresh_expectations.py, the rimworld-l10n submodule),
# which drives the L10nProbe dev mod (source at l10n/probe/; build/deploy it
# only from the canonical ~/dev/rimworld-l10n checkout). The engine holds all
# logic; this file holds only this repo's config and the rationale behind it.
# Usage is unchanged (game must be closed):
#   python3 Scripts/refresh-translation-expectations.py [--no-launch]
# If l10n/ is empty, run: git submodule update --init

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "l10n" / "refresh"))
import refresh_expectations as engine  # noqa: E402  (import after sys.path edit)

engine.REPO_ROOT = Path(__file__).resolve().parent.parent

engine.PACKAGE_ID = "shunter.shipcrackerwarcasket"

# RATIONALE: VFE Pirates is the hard dependency (our defs parent on its
# abstract warcasket bases and would not resolve without it); Vanilla
# Expanded Framework and Harmony are VFEP's own hard deps and load before
# it. Ideology is NOT pinned even though a stat leaf carries MayRequire for
# it: a gated leaf changes a number, never a def or a key. No family
# sibling rides along; this repo's list is its own. See the engine's header
# for the membership rule, the lowercase-id warning, and the pinning
# rationale; order is load order, the probe last.
#
# Plain ids on purpose: when the Workshop copy of a mod and a local Mods/
# copy coexist, RimWorld suffixes the Workshop one "_steam" and the plain id
# names the local copy. The plain id therefore resolves to whichever single
# copy is installed and never breaks when one of them is removed.
_BASE = [
    "brrainz.harmony",
    "ludeon.rimworld",
    "oskarpotocki.vanillafactionsexpanded.core",
    "oskarpotocki.vfe.pirates",
]
_MOD = ["shunter.shipcrackerwarcasket", "shunter.l10nprobe"]

# WORLDS, not one pinned list: the key set depends on which optional
# packages are active, and LoadFolders.xml gates a load root on each.
# Every world is a legitimate player configuration and the checker
# validates each language against each one, so a key that exists only
# behind a gate is demanded only from that gate's Languages/ root.
#   base     - no DLC: the text players without Odyssey read; nothing
#              translatable may depend on Odyssey here, because the main
#              tree loads for them too.
#   odyssey  - the planned Odyssey compat root (LoadFolders.xml's commented
#              Mods/Odyssey pair) opens here the day it goes live; the two
#              MayRequire stat leaves drop a number, not a key, so today
#              this world matches base.
#   vge      - Vanilla Gravship Expanded (requires Odyssey) opens
#              1.6/Mods/VanillaGravshipExpanded/, whose HelmetOxygen patch
#              adds a [MustTranslate] chargeNoun to the main-tree helmet;
#              that key exists only here and is translated only from that
#              root.
engine.WORLDS = {
    "base": _BASE + _MOD,
    "odyssey": _BASE[:2] + ["ludeon.rimworld.odyssey"] + _BASE[2:] + _MOD,
    "vge": _BASE[:2] + ["ludeon.rimworld.odyssey"] + _BASE[2:]
           + ["vanillaexpanded.gravship"] + _MOD,
}

raise SystemExit(engine.main())
