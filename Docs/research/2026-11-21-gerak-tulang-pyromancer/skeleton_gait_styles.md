# Skeleton / Undead Locomotion Styles (walk, idle) for an original low-poly skeleton guard with sword and round shield

Scope note: the web has very little primary, animator-authored material on how specific games keyed their skeletons. FromSoftware, Blizzard, Bethesda, Supergiant, Mojang and Red Hook have no findable public breakdowns of skeleton walk or idle cycles (searched; see Gaps). The best sourced material is (a) Harryhausen making-of accounts, (b) general zombie and stylized-walk tutorials, (c) top-down pixel and indie animation notes. Inferences are marked as mine.

## 1. Famous skeleton/undead animation examples and how their motion is described

### Takeaway
Harryhausen's skeletons (*7th Voyage of Sinbad* 1958, *Jason and the Argonauts* 1963) are the canonical reference. Their creepiness came from stop-motion's slightly unnatural, "nightmare" quality and from fencing choreography rehearsed by real fencers, not from a shamble. Game skeletons are documented mostly by behavior (collapse into a bone pile, then reassemble), not by gait.

### Cited Findings
- The *Jason* skeletons were about 8 to 10 inches tall. Each was animated frame by frame on a table in front of rear-projected live action, built on armatures with "cotton wool soaked in latex" shaped into bones. One model was reused from *7th Voyage of Sinbad*. — [Animation Obsessive](https://animationobsessive.substack.com/p/how-ray-harryhausen-conjured-the)
- Seven skeletons fought three actors, with five appendages per skeleton moved every frame (35 movements per frame). It took about 4.5 months, and slow days produced only about 13 frames. — [Animation Obsessive](https://animationobsessive.substack.com/p/how-ray-harryhausen-conjured-the); 13 to 15 frames per day per [Boing Boing](https://boingboing.net/2021/03/01/ray-harryhausen-describes-how-he-shot-the-classic-skeleton-fight-scene-from-jason-and-the-argonauts-1963.html)
- Harryhausen worked at 24 fps. He noted that "the broader the movement of the model, the faster it will appear to move." Per-frame moves could be as small as about 0.5 mm. He cut frames ("kill" footage) or added subtle motion ("fleshing out" time) to sync with actors. — [Animation Obsessive](https://animationobsessive.substack.com/p/how-ray-harryhausen-conjured-the)
- Choreography came first: fencing master Ferdinando Poggi ran rehearsals with stunt performers ("shadowboxing" passes), and they filmed an all-human reference version in numbered jerseys. The skeletons therefore move like trained fencers, not like shamblers. — [Animation Obsessive](https://animationobsessive.substack.com/p/how-ray-harryhausen-conjured-the)
- Harryhausen's stated aim was a "surrealistic experience" rather than realism. He called the fight "odd and creepy," credited stop-motion's "nightmare quality," and drew on Gustave Doré. — [Animation Obsessive](https://animationobsessive.substack.com/p/how-ray-harryhausen-conjured-the)
- In *7th Voyage*, the skeleton matches Sinbad "blow for blow" with its sword, and its blade meets Sinbad's sword and shield. Harryhausen had long wanted to animate a skeleton swordfight and restaged it in *Jason*. — [Wikipedia: The 7th Voyage of Sinbad](https://en.wikipedia.org/wiki/The_7th_Voyage_of_Sinbad); [Framerated review](https://www.framerated.co.uk/7th-voyage-sinbad-1958/)
- Dark Souls III skeletons are described as "fast and aggressive" and may "randomly reassemble instantly." Elden Ring skeletons revive from a glowing bone pile unless that spot is struck. — [Elden Ring Wiki (fandom)](https://eldenring.fandom.com/wiki/Skeleton); [realsport101](https://realsport101.com/article/elden-ring-how-to-kill-skeletons)
- The topple-into-bones-and-reassemble trope goes back to early games, for example a red skeleton that "topples into a pile of bones upon impact, only to reassemble shortly after." — [AV Club](https://www.avclub.com/regenerating-skeletons-games)
- Skyrim: Hulking Draugr "walk hunched over and in a clumsy manner" and are faster and more feral than standard draugr. Draugr and skeletons are a rare case of creatures sharing one animation skeleton. Vanilla has only about 13 draugr walk/run animations. — [Nexus forums / mod pages via search](https://forums.nexusmods.com/topic/5300775-le-draugr-animations-are-so-limited-can-extra-animations-be-added); [Nexus forums](https://forums.nexusmods.com/topic/3174509-undead-animations-for-thralls-very-old-people)
- Diablo III Necromancer team: they "had to go back to Diablo II to recapture the big skull head and skeleton feel," meaning skull proportion was part of the read. — [BlizzPro panel summary](https://blizzpro.com/?p=69359)

### Inferences
- Two distinct traditions exist. **Harryhausen-style skeletons** are competent, upright, fencing soldiers whose uncanniness comes from timing texture: tiny holds and slightly uneven spacing. **Draugr/zombie-style undead** are hunched, clumsy, dead-weight walkers. A "skeleton guard" with sword and round shield sits naturally in the first tradition, with a few undead "glitches" layered on.
- The Diablo note implies that an oversized skull is a proven readability device for skeletons seen from far away or above.
- Reassembly is the genre's signature skeleton behavior. It is a death/revive animation, not locomotion, but it is worth knowing for the death animation.

### Gaps
- No primary animator description of the walk or idle cycles for Dark Souls, Elden Ring, Diablo, Hades, Minecraft, Spelunky or Darkest Dungeon skeletons was found. A Darkest Dungeon Spine search returned only modding questions.
- No source states the frame count or cycle length of any Harryhausen skeleton walk. The skeletons mostly fight rather than walk.

## 2. Shambling undead vs skeleton soldier march: distinguishing traits, and which reads better small and top-down

### Takeaway
A shamble comes from **asymmetry plus dead weight**: the hip leads, the limbs hang and drag, one leg is favored, and the timing lurches between held poses with overshoot-and-settle. A soldier/guard read comes from an **upright, symmetric, metronomic cadence with a fixed guard pose**. For a small figure seen from above, motion of the skull, shoulders, weapon tip and shield reads best. The limp and asymmetry must be exaggerated in the vertical bob and lateral sway, because leg detail is foreshortened.

### Cited Findings
- Zombie walk (2D course): start the move from the hip, let the rest of the body hang loose as dead weight, then move the knee. The result is a jerky, stepped motion that is "not a human walk cycle." Hold each step pose for a few frames, then lurch: a "big move here, then slight move there, and then drag along that ground." On every stop the body "overshoots and settles." — [LinkedIn Learning / Lynda: Animate Monsters & Aliens, Zombie (search excerpt; page fetch failed)](https://origin-www.lynda.com/Animation-tutorials/Zombie/501128/575304-4.html)
- The zombie lurch comes from an uneven stride: one leg drags or carries more weight, and the weight shift lands at the "wrong" moment. — [Mocap Online: zombie animations](https://mocaponline.com/blogs/mocap-news/new-zombie-animations)
- A perfectly symmetrical cycle looks stiff. Limp-like hitches come from deliberately breaking the symmetry. Offsetting the timing of dangling arms adds further asymmetry. — [Ulster BDes animation blog](https://blogs.ulster.ac.uk/b00955450-bdes-animation/?p=349); [UAL stylized walk blog](https://mingenyang.myblog.arts.ac.uk/?p=61)
- When editing a zombie walk, apply changes across the whole cycle so the loop still closes. — [Autodesk Maya 2014 Getting Started: Tweaking the zombie's walk cycle](https://download.autodesk.com/us/maya/2011help/files/WS1a9193826455f5ff219f8b8b1249cf2cb82-4444.htm)
- A stiff, robotic walk usually comes from a rigid torso and no hip rotation, because "the hips move more than anything else." — [Blender Artists / general walk tips via search](https://blenderartists.org/t/walk-cycle-animation/638000); [Creative Bloq](https://www.creativebloq.com/3d/animation-basics-5-pro-tips-getting-your-character-walk-71515773)
- Evenly spaced keys with no easing produce a mechanical feel. The fix is to block contact and passing poses, then add knee lift, foot roll, arm swing, head bob and drag on the head and fingers. — [Pluralsight: 12 steps to a great walk cycle](https://www.pluralsight.com/blog/tutorials/12-steps-walk-cycle-animation)
- Top-down pixel walk/run: 6 frames per cycle, and "the core expression of the motion comes from the bob of the head." The head goes down 1 px, down 1 px, then up 2 px per stride, so the rise is faster than the fall. A constant sine bob looks robotic, so use variable, bouncing motion. — [Slynyrd Pixelblog 55: Top Down Character Animation](https://www.slynyrd.com/blog/2025/3/24/pixelblog-55-top-down-character-animation)
- From a high top-down angle, a skeleton mainly reads through "the top of the skull, the shoulder blades, the ribs and the long sword." — [Summer Engine asset listing: Skeleton Idle Sheet](https://www.summerengine.com/asset-store/skeleton-idle-sheet-4dir-x-4f-407f5ebd)
- A 3/4 top-down view shows a large part of the front plus the tops of objects. Readability there depends on materials and shaders (rim and contrast), not only on the animation. — [Unity blog: Half Past Yellow, Tempest Tower enemies](https://unity.com/blog/half-past-yellow-creating-enemy-visuals-tempest-tower)
- Keep negative space around key shapes so the silhouette survives downscaling. — [itch.io devlog: Pyrohoof readable motion](https://itch.io/devlog/1035305/pyrohoof-a-bat-winged-fire-steed-built-for-readable-motion-and-safe-scaling.amp)

### Inferences
- **Trait table (my synthesis of the sources above):**
  | Trait | Shambling undead | Skeleton soldier / guard |
  |---|---|---|
  | Spine | Hunched forward, head hanging or tilted | Upright, chin level, slight forward lean only when advancing |
  | Rhythm | Uneven: hold, lurch, drag; one long and one short step | Even, metronomic left-right cadence |
  | Symmetry | Broken on purpose (favored leg, one arm dangling) | Symmetric legs; asymmetry lives only in the arms (shield vs sword) |
  | Joints | Loose, overshoot and settle, dragging feet | Crisp: small "snap" into contact pose, short hold, little overshoot |
  | Head | Lolls, lags behind the body (drag) | Stable, locked on target; maybe a tiny delayed jitter |
  | Arms/weapon | Hanging, swinging late | Fixed guard: shield forward, sword low or ready |
  | Feel | Pathetic, eerie, slow | Menacing, disciplined, "still doing its duty after death" |
- For a guard, the strongest read is **soldier cadence plus one undead glitch**: crisp, symmetric marching steps with a small held "settle" after each contact. Add one asymmetric tell, such as a slightly late skull that tilts and re-centers, or one leg landing a couple of frames late. This keeps the guard identity while signaling "dead."
- From a 42° orthographic camera, foot placement and knee bend are foreshortened. The readable channels are (1) the vertical bob of the skull and shoulders, (2) lateral sway/roll of the torso, (3) shield and sword tip motion, and (4) the timing pattern (hold vs lurch). Exaggerate bob and roll well beyond realistic values. A cadence pattern with holds reads better than a smooth sine.
- **Speed check (my calculation; the threshold below is general biomechanics knowledge, not from a source fetched here):** a 1 m-tall biped has roughly a 0.45 to 0.5 m leg. At 2 m/s its Froude number v²/(g·L) is about 0.8, above the roughly 0.5 at which humans switch from walking to running. A realistically proportioned 1 m figure therefore cannot naturally *walk* at 2 m/s. Either the strides are very long for its size, which suits a stiff, wide marching stride, or the cycle is short. With about 0.5 m per step, the full two-step cycle is about 0.5 s (12 frames at 24 fps, 15 at 30 fps). To keep a "slow" feel at that speed, use few, long, deliberate strides with a hold on each contact, so it looks like a heavy march rather than fast patter. Alternatively, lower the game speed or scale the model up. Check foot sliding against the actual root speed in engine.

### Gaps
- No source gives a measured cycle length (seconds or frames) for a commercial game skeleton walk.
- No source specifically compares shamble vs march readability from an isometric camera; the comparison above is inference.
- TV Tropes "Zombie Gait" returned HTTP 403, so its trope definition is not used.

## 3. How skeleton warriors carry sword and shield while walking

### Takeaway
Commercial sword-and-shield locomotion sets use two stances: a **relaxed walk**, with the sword low at the side and the shield carried in front of the off arm, and a **guard walk**, with the shield raised and the sword ready, as separate looping clips. The shield arm barely swings in either. The sword arm carries the counter-swing at reduced amplitude.

### Cited Findings
- A typical forward walk carries "the sword low at the right side and the shield carried in front of the left arm." — [KitsBlox: Shield Knight, Sword and Shield Walk Forward](https://kitsblox.com/product/shield-knight-sword-and-shield-walk-forward-r15-animation)
- Sword and shield packs ship guard locomotion as its own set ("Guard Walk," "Guard Jog," each with start/loop/stop clips, plus guard idle, guard hit and parry). Some ship an "Off Hand Blocking Locomotion Set" and 8-way directional walks. — [Fab sword & shield listing](https://www.fab.com/listings/a4a670ab-3b2d-4028-8e3b-aa1fb24c2b3e); [Unity Asset Store sword & shield animations](https://assetstore.unity.com/packages/3d/animations/sword-and-shield-animations-256766)
- Developers debate whether to author separate shield-up and shield-down walk sets or to raise the shield procedurally (IK or additive layer) over one walk. — [Unreal forums thread](https://forums.unrealengine.com/t/wip-basic-sword-and-shield-melee-third-person-action-game-the-tarks-search/25201/6)
- Harryhausen's skeletons fought with sword and shield in fencing-derived choreography, blade meeting the hero's sword and shield. — [Framerated](https://www.framerated.co.uk/7th-voyage-sinbad-1958/); [Animation Obsessive](https://animationobsessive.substack.com/p/how-ray-harryhausen-conjured-the)
- Flipping the sword and shield hands when mirroring sprite directions is acceptable in gameplay but looks odd during an in-place rotation. — [Slynyrd Pixelblog 55](https://www.slynyrd.com/blog/2025/3/24/pixelblog-55-top-down-character-animation)

### Inferences
- For a guard enemy that approaches the player, a **shield-forward walk** fits best: shield held in front of the chest/belly, angled slightly toward the target, moving only with the torso's roll and bob. A round shield seen from 42° above is a big readable disc, so its bob and tilt are a cheap, highly visible "walk signal."
- Sword arm: small counter-swing (well under a natural arm swing) with the blade low or angled forward/down. The sword tip bobbing out of phase with the skull adds secondary motion that reads from above, matching the asset note that the "sword tip bobs."
- In a 3D model the left/right asymmetry stays correct automatically (no sprite mirroring problem). Keep the shield always on the same side.
- One walk clip plus an additive "shield up" layer (Unity Avatar Mask / additive layer) is the cheaper path for one developer. A separate guard-walk clip is only needed if the enemy has a blocking behavior.

### Gaps
- No source gives arm-swing angles for armed walks.
- No animator-authored description of how a skeleton (rather than a human knight) should carry a shield was found.

## 4. Idle poses and loops for skeleton enemies

### Takeaway
Skeleton idles replace breathing with a **subtle stiff sway**. The skull and shoulders rock, the ribcage shifts, and the weapon tip bobs while the feet stay planted. They often use only 2 to 4 key poses with holds on the extremes. Small idles add a lot of life for little work.

### Cited Findings
- Example top-down skeleton idle: 4 frames per direction, "a subtle undead sway" where "the skull and shoulders rock slightly, ribcage shifts, the rusty sword tip bobs up and down." The stance stays planted, and the motion is "very small," "stiff and menacing." The facing-camera row is the most important. — [Summer Engine: Skeleton Idle Sheet 4dir x 4f](https://www.summerengine.com/asset-store/skeleton-idle-sheet-4dir-x-4f-407f5ebd)
- A comparable undead caster idle uses shoulders that rise and fall, robe sway and a pulsing glow instead of limb movement. — [Summer Engine: Cultist idle (search excerpt)](https://www.summerengine.com/asset-store/cultist-idle-sheet-4dir-x-4f-4086556a)
- Idle timing: hold the extreme poses longer than the in-betweens. Secondary motion should shift by small, consistent amounts, because large jumps look noisy. — [Slynyrd Pixelblog 55](https://www.slynyrd.com/blog/2025/3/24/pixelblog-55-top-down-character-animation)
- "A simple 2 cycle idle" adds great value to an enemy for a few extra minutes of work. — [itch.io post via search](https://itch.io/post/6127579)

### Inferences
- A skeleton has no lungs, so a breathing-style chest rise reads as "alive." A **settle-and-creak** loop reads more undead: a slow lean/roll of the torso, a skull that drifts off-axis and snaps back, a sword tip that dips and re-raises, and the shield re-gripped.
- Jaw chatter is a good audio-synced tell (it pairs well with a bone-clack SFX), but at about 1 m tall under a 42° top-down view the jaw is small and partly hidden by the skull top. A head tilt or nod will read better than jaw motion. Reserve the jaw for an alert/aggro moment.
- Layer two loops of different lengths, for example a 2 s sway with an occasional 0.3 s glitch twitch every few seconds, so a group of guards does not move in unison. Randomize the start offset per instance.

### Gaps
- No sourced frame rate or duration for any game's skeleton idle was found.
- No source covers jaw chatter specifically.

## 5. Animator talks, breakdowns and keying notes (frame counts, cycle duration, exaggeration)

### Takeaway
Concrete numbers are scarce. Sourced figures are Harryhausen's 24 fps, 0.5 mm-per-frame minimum moves and 13 to 15 frames per day; a 6-frame top-down run cycle with a down 1, down 1, up 2 px head bob; and a 4-frame skeleton idle. The general guidance is to block contact and passing poses, break symmetry, hold key poses, and avoid sine-even spacing.

### Cited Findings
- Harryhausen: 24 fps, moves as small as about 0.5 mm per frame, timing held "in his head" with few notes, retimed by killing or fleshing out frames. — [Animation Obsessive](https://animationobsessive.substack.com/p/how-ray-harryhausen-conjured-the)
- 6-frame top-down cycle, with a head bob of down 1, down 1, up 2 px (fast rise, slower fall); a constant sine looks robotic. — [Slynyrd Pixelblog 55](https://www.slynyrd.com/blog/2025/3/24/pixelblog-55-top-down-character-animation)
- 4-frame skeleton idle per direction. — [Summer Engine](https://www.summerengine.com/asset-store/skeleton-idle-sheet-4dir-x-4f-407f5ebd)
- Zombie keying: hip first, hold poses a few frames, lurch, overshoot-and-settle at every stop. — [Lynda zombie lesson (search excerpt)](https://origin-www.lynda.com/Animation-tutorials/Zombie/501128/575304-4.html)
- Refinement order: contact and passing poses, then knee lift, foot roll, arm swing, head bob, shoulders and hips, with drag on the head and fingers and checked arcs. — [Pluralsight](https://www.pluralsight.com/blog/tutorials/12-steps-walk-cycle-animation)

### Inferences
- **Suggested starting spec for this project's guard (inference, to be tuned in engine):**
  - Walk: about 0.5 to 0.6 s per two-step cycle at 2 m/s (12 to 15 frames at 24 to 25 fps). Keys at contact, a held settle 2 to 3 frames after contact, passing, and up. Put the skull bob on the fast-rise, slow-fall pattern, scaled to be clearly visible at the game's on-screen size. Add a few degrees of torso roll toward the planted leg. Shield moves only with the torso. The sword arm has a small counter-swing, with the tip lagging a couple of frames behind (drag). Optional glitch: the skull lands one or two frames after the body on alternate steps.
  - Idle: about 2 to 3 s loop with 3 to 4 key poses and holds on the extremes. The skull drifts and re-centers, the sword tip dips, the shield re-grips. Add a random-offset start per instance.
  - Exaggeration: because Harryhausen noted that broader moves read faster and the camera foreshortens legs, put amplitude into skull, shoulders, shield and sword rather than legs.
- Because the camera is orthographic, perspective gives no depth cue, so vertical bob may be confused with forward motion. Torso roll (side to side) and shield tilt are the safer, unambiguous "walking" channels.

### Gaps
- No GDC talk or ArtStation/80.lv breakdown specifically about skeleton enemy locomotion was found within the search budget. An 80.lv breakdown on a tiny character with a large two-handed weapon exists but was not read: [80.lv](https://80.lv/articles/breakdown-animating-tiny-character-with-massive-two-handed-weapon).
- No sourced cycle durations for Dark Souls, Elden Ring, Diablo, Hades, Minecraft, Spelunky or Darkest Dungeon skeletons were found. None of the numbers in "Suggested starting spec" come from those games.
