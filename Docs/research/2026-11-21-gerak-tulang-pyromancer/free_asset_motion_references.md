# Free (and low-cost) skeleton warrior and robed mage packs: how their animations move (motion reference only)

Scope note: reference only. No files were downloaded. Everything below comes from store pages, listings and search snippets. Many store pages do not state frame counts, fps, bone counts or in-place vs root motion; those are listed as gaps rather than guessed. Several sites (OpenGameArt, Polycount) returned HTTP 403 to the fetcher, so their content is only known through search-result snippets, which is flagged where used.

## Which free packs exist with skeleton/undead warriors, what animations they ship, and what the gait looks like?

### Takeaway
The strongest free, CC0 references are KayKit Skeletons (4 free skeletons incl. Warrior and Mage, chunky low-poly) plus the free KayKit Character Animations pack, which ships dedicated "skeleton versions" of idle, walk, hit, death and spawn. A paid Fab skeleton warrior adds a useful clip-list idea (Collapse + Reassemble). No source found describes the gait in words; the motion itself must be observed from the preview GIFs/videos.

### Cited Findings
**Catalog (skeleton/undead):**
1. **KayKit – Character Pack: Skeletons (Kay Lousberg)** — Free tier: Warrior, Rogue, Mage, Minion skeletons; Extra tier ($7.95+): Skeleton Golem (boss-sized) and Necromancer; Source tier ($11.95+) has .blend files for all 6 — [itch.io](https://kaylousberg.itch.io/kaykit-skeletons)
   - "Fully rigged and animated" with "basic movement animations included"; Source tier lists 75 animations; compatible with the free KayKit Character Animations pack. No clip names, bone count or polycount given on the page — [itch.io](https://kaylousberg.itch.io/kaykit-skeletons)
   - 10+ weapons/accessories in free tier (swords, shields, axes, crossbows, staffs); single 1024x1024 gradient atlas, downsample-able to 128x128; FBX + glTF; CC0 — [itch.io](https://kaylousberg.itch.io/kaykit-skeletons)
2. **KayKit – Character Animations (free tier, v1.1, Dec 10 2025)** — 161 humanoid animations (free tier "150+"); split per rig: Rig_Medium (regular characters, 100+ clips) and Rig_Large (large characters, ~23–25 clips, more planned) — [itch.io](https://kaylousberg.itch.io/kaykit-character-animations)
   - Categories: General (idle, hit, death, spawn, interact), Movement (walk, run, jump, crawl, sneak, dodge, crouch), Melee (1H, 2H, unarmed, dual wield, blocking), Ranged incl. magic/spellcasting, Simulation, Tool, and **Special: "skeleton versions" of idling, walking, getting hit, dying and spawning** for the Skeleton pack and mechanical characters — [itch.io](https://kaylousberg.itch.io/kaykit-character-animations)
   - Creator warns retargeted use on other humanoids "might not look good on other characters" — [itch.io](https://kaylousberg.itch.io/kaykit-character-animations)
   - Example naming style from the 1.1 devlog (tool set): paired verb/noun clips such as `Chop` + `Chopping`, `Dig` + `Digging`, `Hammer` + `Hammering`, and lettered variants `Holding_A/B/C`, `Work_A/B/C` — i.e. a one-shot clip plus a looping clip, and A/B/C variants — [itch.io devlog](https://kaylousberg.itch.io/kaykit-character-animations/devlog/1139588/character-animations-update-11)
3. **Kevin Iglesias – Skeleton Animations FREE (Unity Asset Store)** — free, v1.0, released Jan 10 2025, Unity 2022.3; keywords "Animated", "Undead"; standard Asset Store EULA (not CC0). Clip list not visible in the fetched page (Package Content tab missing). A paid full version exists (asset 217503) — [Unity Asset Store](https://assetstore.unity.com/packages/3d/animations/skeleton-animations-free-217504); [paid version](https://assetstore.unity.com/packages/3d/animations/skeleton-animations-217503)
4. **Quaternius – Ultimate Monsters** — 50 "fully animated monsters", CC0, Oct 2022, FBX/OBJ/Blend(/glTF). Page does not name the monsters, so presence of a skeleton is unconfirmed — [quaternius.com](https://quaternius.com/packs/ultimatemonsters.html)
5. **Quaternius – Universal Animation Library 2** — 130+ clips on a "universal humanoid rig", includes **zombie locomotion**, melee/armed combos split into separate hits with recoveries; ~60–70% free (Standard), Source tier has the .blend rig; CC0 — [quaternius.com](https://quaternius.com/packs/universalanimationlibrary2.html)
6. **Low Poly – Skeleton Warrior (Fab, paid, ~IDR 365k)** — 15 clips: Attack01–04, Block, **Collapse**, Death, GotHit, Idle, **IdleBreak**, **Reassemble01/02**, Taunt, Walk, WalkBackward; sword/shield detachable; 23 blendshapes (e.g. shield shape); **123 bones** (page notes mobile max 75) — [Fab](https://www.fab.com/listings/392e8e41-48a5-4d38-b6f2-13ca477a7ef7)
7. **PolyReady – Undead Skeleton (free sample)** — Idle only (full paid: Walk, Run, Attack, Skill 1–3, Death); 24,130 polys / 16,686 verts (not low-poly by this project's standard); hand-made rig and animation — [Unreal forums](https://forums.unrealengine.com/t/polyready-undead-skeleton-animated-low-poly-game-ready-free-sample-idle-animation/2741448)
8. **TurboSquid – Low Poly Animated Warrior Skeleton** — $45, 24 animations incl. walking, props sword and shield (search snippet only) — [TurboSquid](https://www.turbosquid.com/3d-models/3d-low-poly-animated-warrior-skeleton-character-2148656)
9. **OpenGameArt – Animated low poly hero with sword and shield (CC0, human, not skeleton)** — clips walk, attack, defend, death, hurt, idle (search snippet; page 403'd) — [OpenGameArt](https://opengameart.org/content/animated-low-poly-hero-with-sword-and-shield)

### Inferences
- The KayKit route (Skeleton models + skeleton-specific clips in the free animation pack) is the best low-poly, ~1 m, chunky-proportion match, and its "skeleton version" walk exists precisely because a normal human walk looked wrong on the skeleton. Watching the regular vs skeleton walk side by side in the preview is the most direct lesson available.
- Clip-list ideas worth borrowing for an original skeleton: `Idle` + `IdleBreak` (occasional fidget), `Collapse` → `Reassemble` (undead-specific spawn/revive, doubles as a spawn animation), `WalkBackward`, `Block`, separate `GotHit` and `Death`. KayKit's "one-shot + looping" pairing (`Chop`/`Chopping`) is a clean naming convention for attack-start vs attack-loop.
- 123 bones (Fab skeleton) is far above what a chunky low-poly enemy needs; KayKit/Quaternius-style rigs are much leaner (see bone question below).

### Gaps
- No page describes in words how any skeleton's walk looks (posture, arm carry, bob). The previews are video/GIF only and were not viewed.
- How sword+shield are carried during walk (shield held forward vs swinging at side) is not described on any page fetched; KayKit only says "no skeleton will be left empty-handed".
- Frame counts, fps and in-place vs root motion are unstated for every skeleton pack above.
- Exact KayKit skeleton clip names (e.g. whether `Walking_D_Skeletons` or similar) could not be confirmed online.

## Which free packs have robed mages/wizards, and how do their robes move in walk/run?

### Takeaway
Free robed casters exist (KayKit Skeletons Mage + Necromancer, KayKit Adventurers Mage with cape, Quaternius LowPoly RPG wizard), but no free page documents whether robes get dedicated bones. Forum practice converges on: a few skirt bones whose top segment follows the nearest thigh's rotation (or is hand-animated to the leg rhythm), keeping bone count low; common failure is legs clipping through the robe when they cross.

### Cited Findings
**Catalog (robed/caster):**
10. **KayKit Adventurers – Mage (free tier)** — purple hat and **cape**, book and staff; page does not say robe. Free tier also has Knight, Barbarian, Rogue (hooded/unhooded), Ranger; Extra adds Druid, Engineer, Barbarian_Large. "Basic movement animations included"; updated to be compatible with KayKit Character Animations; CC0 — [itch.io](https://kaylousberg.itch.io/kaykit-adventurers)
11. **KayKit Skeletons – Mage (free) and Necromancer (Extra)** — robed undead casters in the same rig as the warrior — [itch.io](https://kaylousberg.itch.io/kaykit-skeletons)
12. **Quaternius – LowPoly RPG Characters** — CC0, 6 rigged, animated, textured fantasy characters incl. a **wizard**, warrior, rogue, monk, ranger; FBX/OBJ/Blend; Nov 2020 (from search snippet of the OpenGameArt listing; page itself returned 403) — [OpenGameArt](https://opengameart.org/content/lowpoly-rpg-characters)
13. **Sketchfab – Cloaked Assassin (paid store item, not free)** — 1.4k tris / 718 verts; clips idle, walk, run, jump, kick, hurt, die, push/pull/climb set, take/draw sword, sword attack 1–2, idle sword, T-pose for retargeting; **"cloak and sword are separate items from the model"**; hood removable by blendshape — [Sketchfab](https://sketchfab.com/3d-models/cloaked-assassin-1c3a03b47c624628852602a27cb650f2)

**Marketplace evidence about robes (paid examples, cgtrader listings, via search snippets):**
- A skeleton mage reviewer noted arm bones clip a little through the robe; another wizard listing warns the calf hidden under the robe is not modeled, so lower legs can show through in some animations; a dark-mage pack ships the skirt as a separate removable part — [cgtrader search results: skeleton mage](https://www.cgtrader.com/3d-models/character/fantasy/skeleton-mage-game-ready), [Skeleton Dark Mage](https://www.cgtrader.com/3d-models/character/fantasy-character/skeleton-dark-mage)
- A tiny low-poly wizard: "fully rigged", simple walk cycle, 619 vertices — [cgtrader](https://www.cgtrader.com/3d-models/character/other/little-low-poly-wizard-model)

**How animators handle robes/long skirts (forum practice, from search snippets):**
- Polycount: one animator kept legs on their own bones and the skirt on its own bones animated to the leg rhythm to avoid clipping; another used a hybrid where the skirt is weighted to both leg bones and skirt bones to avoid adding many bones — [Polycount thread](https://polycount.com/discussion/134601/animation-for-games-how-to-rig-a-skirt) (403 on fetch; snippet only)
- Blender Artists: top skirt bones constrained to locally follow the rotation of the nearest thigh, with wiggle bones on lower segments for bounce; warned it only works well with cylindrical thighs and within limits — [Blender Artists](https://blenderartists.org/t/how-do-you-rig-weight-paint-a-short-skirt-properly/1376331)
- Pins parented to thigh/calf got jumbled because the legs cross in a walk cycle — [Rainbox forum](https://rainboxprod.coop/forum/viewtopic.php?p=2822)
- Roblox devs: with mesh deformation the skirt must be hand-animated to match walk speed; a common workaround splits the skirt into left/right upper-leg parts — [Roblox DevForum](https://devforum.roblox.com/t/help-with-a-deforming-mesh-skirt/1650239)
- Legs passing through clothes when walking is a recurring problem thread — [Blender Artists](https://blenderartists.org/t/how-do-i-avoid-having-my-characters-legs-passing-through-his-cloths-when-he-walks/1442833/1)

### Inferences
- For a ~1 m chunky robed caster, three options in rising cost: (a) rigid robe weighted to hips/spine only, steps read through vertical bob and a small hip sway, feet peeking out at the hem; (b) robe front/back halves weighted partly to the thighs (the "hybrid" weighting) so the hem swings with each step; (c) 2–4 robe bones (front, back, maybe sides) whose root follows the thigh rotation at a reduced amount, with a delayed lower segment for follow-through. (b) or (c) with front/back panels avoids the "legs crossing" problem that left/right panels have.
- Hiding unmodeled calves under the robe is a known trap: keep stride short and knees low, or model the legs fully.
- Keeping the cape/cloak as a separate mesh piece (Cloaked Assassin, KayKit Mage cape) is a common low-poly pattern.

### Gaps
- No free pack page states whether its mage has skirt bones. The KayKit Mage wears a cape (not confirmed robe); the Quaternius wizard's robe rigging is undocumented.
- Robe behavior in run (vs walk) is not described anywhere fetched.

## Mixamo animations relevant to undead walks, sword-and-shield, mutant, feminine/robed walks, magic idle

### Takeaway
Could not verify any Mixamo clip names or descriptions from a primary source: Mixamo's catalog is a login/JS app and searches returned only third-party packs.

### Cited Findings
- Search for "Sword And Shield Walk", "Zombie Walk", "Mutant Walking" returned no Mixamo catalog page; only third-party zombie packs (e.g. a paid UE pack with "Zombie with sword and shield" animations) — [Unreal Marketplace listing](https://docs.unrealengine.com/marketplace/ko/product/zombie-attack-animation-pack-94-animations-root-motion-sword-shield-hand?lang=ko)
- A zombie mocap vendor argues Mixamo's generic humanoid walks often fall short for horror games (vendor opinion, not fact) — [mocaponline blog](https://mocaponline.com/blogs/mocap-news/new-zombie-animations)
- Free CC0 alternatives for zombie locomotion: Quaternius Universal Animation Library 2 — [quaternius.com](https://quaternius.com/packs/universalanimationlibrary2.html)

### Inferences
- The clip names in the question are widely cited in tutorials, but this research did not confirm them; an animator should browse mixamo.com directly (free Adobe login) and treat the in-browser preview as the reference.

### Gaps
- Exact Mixamo names, cycle lengths and motion traits (posture, arm carry, rhythm) are unverified. Not reported to avoid fabrication.

## Typical bone counts and hierarchies in KayKit/Quaternius low-poly rigs

### Takeaway
Neither KayKit nor Quaternius publishes bone counts or bone names on their pages; what is published is that KayKit uses two shared humanoid rigs (Rig_Medium, Rig_Large) across all characters, and Quaternius uses one "universal humanoid rig" for retargeting.

### Cited Findings
- KayKit rigs are "made as humanoid rigs"; in Unity set Generic → Humanoid on import, or use the `Rig_Medium_Avatar` from the KayKit Free Sample Pack and "Copy avatar from Existing" for other KayKit characters — [itch.io devlog](https://kaylousberg.itch.io/kaykit-character-animations/devlog/1139588/character-animations-update-11)
- Rig_Medium for regular characters, Rig_Large for large characters (golem/barbarian_large) — [itch.io](https://kaylousberg.itch.io/kaykit-character-animations)
- An older KayKit Godot proof-of-concept describes characters as a body mesh plus "five non-body bones" parts (named examples `head`, `armLeft`) attached via BoneAttachment, i.e. rigid separate parts parented to bones rather than one skinned mesh (older pack generation) — [GitHub doctor-g/KayKitAnimationInGodot](https://github.com/doctor-g/KayKitAnimationInGodot)
- Quaternius UAL: one universal humanoid rig; rig and naming are inside the Source .blend only — [quaternius.com UAL2](https://quaternius.com/packs/universalanimationlibrary2.html); [80.lv / digitalproduction coverage via search](https://digitalproduction.com/2026/02/10/130-animations-one-rig-zero-drama/)
- For contrast, a detailed paid skeleton warrior uses 123 bones — [Fab](https://www.fab.com/listings/392e8e41-48a5-4d38-b6f2-13ca477a7ef7)

### Inferences
- The reusable lesson is architectural: one shared humanoid rig for every character of a size class, so walk/attack/death clips are authored once and shared (skeleton warrior and robed caster on the same rig, with a few extra robe bones on the caster that the shared clips simply leave at rest).
- A minimal clean rig for a chunky ~1 m enemy is plausibly ~15–25 deform bones (hips, spine, chest, neck/head, 2×3 arm, 2×3 leg, plus hand weapon/shield slots), but this number is an inference, not sourced from KayKit/Quaternius.

### Gaps
- Exact bone counts and names for Rig_Medium and the Quaternius universal rig: only obtainable by opening the files (out of scope).
- Whether KayKit has dedicated hand-slot/weapon bones could not be confirmed online.

## Licenses

### Takeaway
KayKit (all three packs), Quaternius (Ultimate Monsters, LowPoly RPG Characters, UAL 1/2) and Kenney are CC0; Unity Asset Store free items use the Asset Store EULA; Fab/TurboSquid/Sketchfab store items are paid commercial licenses. Irrelevant for this project beyond completeness, since none will be used.

### Cited Findings
- KayKit Skeletons, Adventurers, Character Animations: CC0, "Name your own price", creator asks not to resell unmodified copies — [Skeletons](https://kaylousberg.itch.io/kaykit-skeletons), [Adventurers](https://kaylousberg.itch.io/kaykit-adventurers), [Animations](https://kaylousberg.itch.io/kaykit-character-animations)
- Quaternius Ultimate Monsters CC0 — [quaternius.com](https://quaternius.com/packs/ultimatemonsters.html); UAL2 CC0 — [quaternius.com](https://quaternius.com/packs/universalanimationlibrary2.html)
- Kenney Animated Characters 1: CC0 (humans; no clip/rig details on page, low relevance) — [kenney.nl](https://kenney.nl/assets/animated-characters-1)
- Kevin Iglesias Skeleton Animations FREE: Unity Asset Store EULA — [Unity Asset Store](https://assetstore.unity.com/packages/3d/animations/skeleton-animations-free-217504)
- KayKit Character Animations creator states no generative AI was used — [itch.io](https://kaylousberg.itch.io/kaykit-character-animations)

### Inferences
- None beyond the above.

### Gaps
- Fab skeleton warrior license terms showed "-" on the fetched page.
