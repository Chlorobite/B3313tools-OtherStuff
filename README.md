# B3313 Dev Tools - Other Stuff
This repo contains mostly old, and some new-ish things that have been created for B3313 development purposes:
* B3313 2.0 tools:
  * BetaOptimizer: It merges adjacent `gsSP1Triangle` calls into `gsSP2Triangles`, and applies some sort of debloat to animations.
  * F3DVertexMerger: basically an attempt at what [BeeieOptimizer](https://github.com/Chlorobite/B3313tools-BeeieOptimizer) does, but working with the C files instead.
* ApplyRMTweak: a manual implementation of `ABCD0: 12 34 56 78 ...` style tweaks, used by me in place of SM64 Rom Manager, as it would simply not work on my end.
* ArmipsTrollGenerator: extracts full 'troll' setups from SM64 Decomp to be used in [Troll Engine](https://github.com/Chlorobite/B3313tools-TrollEngine), complete with individual .c files and an .asm file that links the resulting object files right into place. It can also mass generate `.definelabel` entries from armips error output.
* dmalog: used for the [F3 2022 trailer](https://youtu.be/LAkwRaXHpeo?t=387) (6:58-7:02). Yes, we actually showed timed DMAs captured using Project64, although the Windows CLI didn't really do the whole 'timed' part really well.
* HexUtils: a tool/playground, mostly related to `ABCD0: 12 34 56 78 ...` style SM64 Rom Manager tweaks, but also has B3313 2.0 & Troll Engine adjacent functions.
* RamEnmptyDetector: detects unused RAM regions of substantial length from a Project64 dump.

[Join the B33h1v3 Discord](https://discord.gg/n8PsDgVkBr) for discussion!

## Licensing
All of the software is released under the BSD-3-Clause license. See LICENSE.md for more details.

Super Mario 64 hacks are not affiliated with or endorsed by Nintendo. A legal copy of Super Mario 64 is required in order to use this repository.
