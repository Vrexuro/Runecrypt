# Animating a robed, legless-looking character so it reads as walking, not floating

Context: an original low-poly hooded fire mage, about 1 m tall, with a bell robe, jagged hem, wide sleeves and a hood. Game speed is about 5 m/s. The camera is orthographic, pitched 42°. Everything must be keyed bones exported via FBX to Unity 6, with no cloth simulation and no third-party packages.

Source quality note: most web material on this topic is forum threads (Blender Artists, Adobe Character Animator, Reallusion) and vendor pages. I found no GDC talk or postmortem with numbers for robe locomotion. thatgamecompany's Journey cloth talk did not surface, and the Polycount "how to rig a skirt" thread returned HTTP 403. The numbers below therefore come from three places: gait biomechanics, a game-locomotion article by an industry technical artist (theodox), and my own derivations. Every derivation is marked as an inference.

## 1. Walking under a robe vs. floating ghost: what separates them?

### Takeaway
The robe reads as walking when the motion is periodic and tied to footfalls: vertical bob at step frequency, alternating left/right asymmetry at the hem, and a short accent at each contact. Hem overlap and lag complete the effect. A floating ghost is the opposite. Its legs and hem are still or move with a smooth sine that has no relation to travel speed, and that stillness is what reads as "eerie/mystic" flight.

### Cited Findings
- A forum animator says completely still legs in an upright pose give a sense of "eerie" or "mystic" flight, so a stepping robe needs the opposite cue: visible leg-driven motion. — [Klei forums: make a character appear to hover](https://forums.kleientertainment.com/forums/topic/52864-animation-is-it-possible-to-make-a-charecter-appear-to-hover/)
- Gliding without stepping is a recognised trope for supernatural characters, which means audiences read it as a deliberate "not walking" signal. — [TV Tropes: Ghostly Glide](https://TVTropes.org/pmwiki/pmwiki.php/Main/GhostlyGlide)
- Long-dress walks in Adobe Character Animator: users keep the legs rigged so they still drive the walk, hide them under the dress, and tune dangle stiffness and gravity so the fabric follows. One user says a stiff thobe "looks strange by the feet" during the walk. In other words, a hem that does not react to the legs looks wrong. — [Adobe community: walking with a long dress](https://community.adobe.com/t5/character-animator-discussions/walking-with-a-long-dress/td-p/9978523); [Adobe community: rigging a thobe dress](https://community.adobe.com/t5/character-animator-discussions/rigging-a-thobe-dress/m-p/13486737)
- In a Character Animator thread, a "floating body" turned out to mean the walk behaviour was not really driving the puppet. With no leg drive underneath, the result reads as floating. — [Adobe community: floating body in walking template](https://community.adobe.com/t5/character-animator-discussions/floating-body-in-walking-template/m-p/11363811)
- Journey's traveler: most of the body is covered by cloth, and only the face and lower legs are visible. The cloth was simulated live in-game rather than baked in Maya, so it reacts to wind and motion. — [CGW: Mysterious Journey](https://www.cgw.com/Publications/CGW/2012/Volume-35-Issue-4-June-July-2012/Mysterious-Journey.aspx); [copcomm digital issue](https://digital.copcomm.com/i/77744/43)
- A slower, more onerous walk animation helps convey weight in Journey. This is a critique, not a dev statement. — [Mechanics of Magic: Journey](https://mechanicsofmagic.com/?p=39591)
- A walk is a "controlled fall" over the planted foot, and a run has no moment with both feet down. The depth of the bounce signals weight and energy. — [theodox: Just Put One Foot In Front of the Other](https://theodox.github.io/pages/pub/one_foot_in_front)

### Inferences
- Journey shows the lower legs under its cloak, and that is a grounding cue. For this mage, show the toe tips or boot tips at the hem at front contact (one frame or a few frames per step). From a 42° top-down view, the hem bottom and floor contact are visible, so this cue is cheap and strong.
- Checklist that separates "walking monk" from "ghost":
  - Bob is locked to step frequency (2 bobs per cycle), with the low point at contact.
  - The hem is asymmetric left/right and alternates each step.
  - Each contact gets a 1–2 frame hit accent: a slight squash of the robe and the hem flaring outward.
  - The root keeps a constant height above the floor, and the shadow stays tight.
  - A ghost gets a slow sine bob of 1–2 s that ignores speed, a symmetric hem, a body raised off the floor, and a hem trailing continuously behind.
- Treat the mage's feet as planted under the robe, and treat contact as the beat everything else syncs to. Sound helps here too: a soft step SFX on each contact frame reinforces grounding far more than any amount of cloth motion.

### Gaps
- I found no primary dev source (GDC, postmortem) describing how Journey, or any action RPG with robed mages, keys legs under a robe. I also found no animator source with numbers for Disney gown walks or Jawa-style shuffles.

## 2. Which secondary-motion cues matter, with amplitudes and phase offsets

### Takeaway
The most important cues, in priority order:
1. Vertical bob at step frequency.
2. Alternating knee bulge at the front of the robe, driven by hidden thighs.
3. Hem lag (overlap) of a few frames behind the body.
4. Forward lean that scales with speed.
5. Side-to-side hem sway once per cycle.
6. Sleeves and hood lagging further down the chain.

The only sourced timing value is that loose garments come to rest about 3–4 frames after the body. The amplitudes below are derived from gait data and scaled to a 1 m character.

### Cited Findings
- Overlapping action: not everything in a character action moves or stops at the same time, and the primary action dictates the secondary. Here the legs are primary and the robe and hem are secondary. — [Tony White, Animator's Workbook: Overlapping Action (O'Reilly)](https://www.oreilly.com/library/view/tony-whites-animators/9780240813073/xhtml/18_Chapter10.xhtml)
- In a 2D animation course example, a coat comes to a full stop about 3–4 frames after the hand and foot. That reads better than everything stopping together. — [LinkedIn Learning/Lynda: 2D Animation Tips and Tricks, moving hold](https://origin-www.lynda.com/3D-Animation-tutorials/19-Moving-Hold-Advanced/480958/699149-4.html) (as summarised by search; course video not watched)
- Classic secondary-motion guidance for skirts: the fabric trails behind a leg swinging forward and drapes in front when the leg swings back. Heavier fabric holds its shape more than light fabric. — summarised from search results pointing to [Peachpit](https://www.peachpit.com/articles/article.aspx?p=23582). Caveat: the fetched page was only an index, so I could not verify the original text.
- A 2D sprite-rig tip: split the dress into hip, upper dress and lower skirt pieces so the knee lift does not poke through. — [Reallusion forum: Character Sprite Morphing](https://forum.reallusion.com/447302/Character-Sprite-Morphing-Any-advice)
- Normal adult vertical excursion of the centre of gravity in walking is about 5 cm (2 in). — [OUHSC: Kinematic analyses of gait](https://ouhsc.edu/bserdac/dthompso/web/gait/KNMATICS/gait.htm)
- Recreational runners at about 168–170 steps/min show 8.3–8.9 cm vertical CoM displacement. A faster cadence reduces the displacement. The paper's abstract and table disagree on the injured/uninjured direction, but the magnitude is consistent. — [uni-konstanz CPA paper](https://ojs.ub.uni-konstanz.de/cpa/article/view/5948/5430)
- Body bob: keep the body high at contact, avoid excessive bobbing, and use a hip drop to show weight. — [Pluralsight: 12 Steps to a Great Walk Cycle](https://www.pluralsight.com/blog/tutorials/12-steps-walk-cycle-animation)
- The body bobs up and down, leans, and the head bobs slightly. — [Alice biped walk-cycle quick reference (PDF)](https://www.alice.org/wp-content/uploads/2019/03/BipedWalkCycleQuickReference.pdf)
- Bounce height, bounce depth, point of balance and side-to-side hip sway together define a character's personality and weight. Keep the bounce separate from ground travel so each can be controlled independently. — [theodox](https://theodox.github.io/pages/pub/one_foot_in_front)
- In a forum reply on leg-driven skirts, the skirt rotation should *not* match the leg exactly. The look is more cloth-like if the skirt "sits on the leg more when the leg comes up". — [Blender Artists: make skirt rig react to leg movement](https://blenderartists.org/t/make-skirt-rig-react-to-leg-movement/1563573) (as summarised by search)

### Inferences (derived values for a 1 m mage, 30 fps, run cycle at 5 m/s; tune by eye)
- **Bob amplitude.** Human walk bob is about 5 cm on 1.75 m (≈2.8% of height), and run bob is about 9 cm (≈5%). Scaled to 1 m, that is about 3 cm walk and about 5 cm run. An ortho camera pitched 42° shows world-vertical motion foreshortened to cos 42° ≈ 0.74 on screen. Stylise up about 1.3× to keep the same read: **run bob ≈ 6–7 cm** at the root/pelvis, peak-to-trough.
- **Bob phase.** Two bobs per cycle (one per step). The low point is at contact and just after it (the "down" pose), and the high point is at passing/flight. Ease into the low point, then snap up.
- **Knee bulge.**
  - The front robe bone of the leading side rotates forward about 10–20° at passing/front-reach and returns at contact. The two sides are 180° out of phase.
  - Apply about 50% of the hidden thigh rotation (the "don't match exactly" advice above). The back robe bones get a smaller, opposite push of about 5–10°.
- **Hem lag.**
  - Each hem bone lags its parent robe bone by about **2 frames at 30 fps** (about 70 ms) in the run cycle. Total lag from pelvis to hem tip is **3–4 frames**, matching the coat example.
  - In a looping keyed cycle, implement this by offsetting the hem curves later in time (shift keys +2 frames, cycle-wrapped) instead of re-keying.
- **Hem trail with speed.** At 5 m/s, keep the whole hem rotated back about 5–10° from the travel direction as a constant offset. The run then reads as pushing through air even in the static parts of the loop.
- **Side sway.**
  - Pelvis yaw/roll of about 3–5° once per cycle, so the robe swings toward the stance side.
  - The hem follows with the same 2–4 frame lag and slightly larger amplitude: about 1.5× the pelvis angle at the tip bone.
- **Lean.** No source gave degrees (see Gaps). As a starting point, about 5° forward for a walk and **about 12–18° for the 5 m/s run**, pivoting at the pelvis. Add an extra 1–2 frames of lean overshoot when starting and stopping.
- **Sleeves.**
  - Wide sleeves swing opposite to the matching leg, like arms, at about 15–25°.
  - Each sleeve tip bone lags its upper arm by about 2 frames.
  - A caster may keep the casting hand forward. In that case the sleeve carries the secondary motion and the hand stays steady for aiming.
- **Hood.**
  - The hood tip lags the head by about 2–3 frames and dips slightly after each contact, roughly 3–5° pitch.
  - Keep the face/eye glow stable so the 360° aim stays readable.
- **Contact accent.** On the contact frame, scale the robe bell about 2–3% wider and shorter for 1–2 frames (squash), then recover. This matches the "flash/squash" juice already planned in CLAUDE.md.

### Gaps
- I found no source giving numeric lag frames specifically for robes or skirts, other than the 3–4 frame coat example. I also found no numeric lean angles for walk vs. run in animation references.

## 3. Rigging: robe bone chains, automatic weights pitfalls, hidden leg bones, spring vs. baked

### Takeaway
Use a small radial set of robe chains on their own bones, parented to the pelvis:
- Front, back, left and right as a minimum. Front-left and front-right separately if you want distinct knee bulges.
- 2–3 bones per chain.
- Hidden, non-rendered thigh bones drive the front/back chains at partial influence through constraints. Bake those to keys before FBX export.

Treat automatic weights only as a start. The robe should blend into pelvis/spine weights near the waist. Never weight the robe to the leg bones directly, because it stretches badly. In Unity, a tiny custom LateUpdate spring script on the hem/sleeve tip bones is possible without any package. The first-party Animation Rigging "Damped Transform" also works, but it requires installing a package, which CLAUDE.md says needs user approval.

### Cited Findings
- **Bone counts.**
  - SwayRig, a VRChat-oriented skirt-bone generator, defaults to 8 chains around the skirt and 3 bones per chain from waist to hem.
  - Easy Ring Bone generates 3–256 chains with 1–32 bones each. It produces only the armature, and you still bind with Ctrl+P → With Automatic Weights or paint manually.
  - These are vendor pages, so read them as typical configurations, not endorsements. — [SwayRig](https://superhivemarket.com/products/swayrig); [Easy Ring Bone docs](https://superhivemarket.com/products/easy-ring-bone-onion365/docs)
- A user planning a skirt rig used 5 chains × 3 segments.
- In the same thread, Bandages' leg-driven skirt example uses one "skirt-thigh" bone per leg that **damped-tracks the knee**, so it does not pick up thigh twist. Non-deforming marker bones sit as children of it. Four deforming skirt bones (front top/bottom, back top/bottom) use Stretch To constraints between the markers. Bandages notes this suits only limited motion ranges. — [Blender Artists: how to rig/weight paint a short skirt](https://blenderartists.org/t/how-do-you-rig-weight-paint-a-short-skirt-properly/1376331)
- **Automatic weights issues, from the same thread.**
  - Pleats clipping, the back stretching too far, and the front caving at the belt.
  - The cause was insufficient pelvis weights on the skirt.
  - Advice: "The skirt rig should be part of the body rig, and the skirt should include weights from the body." The upper skirt uses body bones, and the lower part uses its own leg-driving bones. — [Blender Artists](https://blenderartists.org/t/how-do-you-rig-weight-paint-a-short-skirt-properly/1376331)
- A forum user says automatic weighting cannot blend skirt/dress weights properly, so manual cleanup is always needed. — via search summary of [Blender Artists skirt thread](https://blenderartists.org/t/how-do-you-rig-weight-paint-a-short-skirt-properly/1376331) and [Superhive SwayRig docs](https://superhivemarket.com/products/swayrig/docs)
- A vendor test found that weights copied from the legs stretched a pleated skirt up to 62× its size. This is the reason to keep robe bones separate from leg bones. Vendor claim. — [Superhive: BoneBake FAQ](https://superhivemarket.com/products/bonebake/faq) / [SwayRig](https://superhivemarket.com/products/swayrig)
- A Polycount discussion describes hybrid game skirts weighted to both leg bones and skirt bones, used to avoid adding many bones. That thread was not fetchable, so this is a search summary. — [polycount.com discussion comment](https://polycount.com/discussion/comment/1021490/)
- Stacking two Copy Rotation constraints from two legs on the same skirt bone does not combine them as expected, because constraints evaluate top to bottom independently. Use one driver per leg on separate bones instead. — [Blender Artists: Skirt Rigging Problem](https://blenderartists.org/t/skirt-rigging-problem/670900) (search summary)
- A long ghost robe in a fixed game walk pose had legs clipping through the front. Suggested fixes:
  - Enlarge the lower robe, but check both legs, because fixing one side can shift the clip to the other.
  - Sculpt per problem frame.
  - Delete hidden geometry under the robe.
  - Add robe bones or shape keys.
  - Triangulate (Beauty), then subdivide once to give the bend more vertices. — [Blender Artists: need help with rigging robe for game](https://blenderartists.org/t/need-help-with-rigging-robe-for-game/1536987)
- **"Bone Heat Weighting: failed to find solution" (automatic weights failing).**
  - Common causes: duplicate vertices (fix with Merge by Distance), intersecting separate mesh islands, bones outside the mesh, and **small-scale meshes with tiny faces**.
  - Workaround: scale the whole rig and mesh up before Automatic Weights, then scale back. — [Blender issue #84609: automatic weights fails with small-scale faces/armatures](https://projects.blender.org/blender/blender/issues/84609); [Blender issue #103409: depends on character size or polygon count](https://projects.blender.org/blender/blender/issues/103409); [Blender issue #45493: islands not visible to any bone](https://projects.blender.org/blender/blender/issues/45493); [Blender Artists thread](https://blenderartists.org/t/bone-heat-weighting-failed-to-find-solution-for-one-or-more-bones-error/1340827)
- **Unity in-engine secondary motion options.**
  - Unity Animation Rigging's **Damped Transform** constraint makes an object follow a source's position/rotation with Damp Position and Damp Rotation (0–1) plus a Maintain Aim option. The docs do not describe chains. — [Unity Animation Rigging docs: Damped Transform](https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/constraints/DampedTransform.html)
  - Third-party options exist: UnityChan SpringBone, naelstrof JigglePhysics, and a single-file jiggle gist. All are excluded by the project constraint, but they show the common pattern of a spring per bone with stiffness and drag. — [JigglePhysics](https://github.com/naelstrof/JigglePhysics); [gist](https://gist.github.com/edwios/3dc6cd5b62419ec747b033975a7e1dc2); [com.unity.springbone on OpenUPM](https://openupm.com/packages/com.unity.springbone/)

### Inferences (recommended setup for this mage)
- **Bone budget for a low-poly 1 m bell robe:**
  - Pelvis → 6 robe chains: front-L, front-R, back-L, back-R, side-L, side-R.
  - 2 bones per chain, waist→knee and knee→hem. Use 3 only if the jagged hem needs its own tip bone.
  - About 12–18 robe bones total, plus 2 bones per sleeve (upper, cuff) and 2 hood bones (base, tip).
  - The 8×3 vendor default is more than a low-poly robe needs.
- **Hidden legs:**
  - Keep thigh + shin + foot bones with Deform turned off and no mesh.
  - Animate them like a real run so footfall timing is honest.
  - Robe front-L/back-L bones get a Copy Rotation from the hidden left thigh at about 0.4–0.6 influence, one constraint per bone. The right side does the same with the right thigh.
  - Optional: a damped-track "skirt-thigh" bone that aims at the knee, which avoids picking up twist.
  - Then run **Bake Action with Visual Keying + Clear Constraints** before FBX export, because FBX/Unity does not carry Blender constraints. That is general Blender/FBX knowledge, not cited above, so verify in a test export.
- **Weights:**
  - Bind with Automatic Weights.
  - Then make the top ring of the robe 100% pelvis/spine, the middle a mix of pelvis + upper robe bones, and the hem rings mostly lower robe bones.
  - Remove any leg-bone weights entirely (legs are non-deforming, so they get none).
  - Run Merge by Distance first. Because the model is about 1 m, try scaling ×10 before binding if bone heat fails.
  - Give the knee ring enough loops (at least 2–3 edge loops between waist and hem) so the bulge bends rather than kinks.
- **Jagged hem:** weight each hem spike mostly to the nearest tip/lower bone. Spikes that sit between chains get split 50/50, otherwise they shear.
- **Baked vs. spring:**
  - Bake all core locomotion overlap (bob, knee bulge, hem lag) into the clips, so it is deterministic and reviewable.
  - Optionally, add a ~30-line custom LateUpdate spring on hem/sleeve/hood tip bones only. It blends a little velocity-driven lag on top of the animation: angular spring, stiffness plus damping, clamp at about 15°. That makes turning and stopping (frequent with 360° mouse aim) react without extra clips.
  - This needs no package. Unity's Damped Transform is an alternative if the user approves installing Animation Rigging.
- **Clipping check:** pose the hidden legs at max knee-forward in the run, check both sides, and widen the bell at knee height if needed. With a bell robe this is usually easy, because the robe already flares.

### Gaps
- I could not read the Polycount "animation for games: how to rig a skirt" thread (HTTP 403). It likely has shipped-game bone layouts.
- I found no source on how many robe bones shipped action RPGs (Diablo-like robed casters) use.

## 4. Run vs. walk at faster speeds: lean, step frequency, bob height

### Takeaway
At 5 m/s a 1 m character cannot plausibly "walk". At a realistic stride it would need about 360 steps/min, which is far past the roughly 260 steps/min where human runs start looking silly. The answer is a **run** with an exaggerated stride, about 1.2–1.4 m (120–140% of height). The cadence is then about 210–250 steps/min, and the robe hides the remaining foot slide.

### Cited Findings
- Ground speed = cadence × stride. A typical stride is about 83% of height. Changing stride accounts for more speed change than changing cadence. Blend cycles with different stride and cadence instead of only speeding up playback. — [theodox](https://theodox.github.io/pages/pub/one_foot_in_front)
- Reference table at 30 Hz from the same source:

  | Speed | Stride | Cadence | Frames per cycle | Type |
  |---|---|---|---|---|
  | 1.5 m/s | 1.5 m | 120 spm | 30 | average walk |
  | 2.5 m/s | 2.0 m | 150 spm | 24 | brisk walk |
  | 2.7 m/s | 1.8 m | 180 spm | 20 | easy jog |
  | 4.5 m/s | 2.7 m | 200 spm | 18 | distance runner |
  | 11.2 m/s | 4.8 m | 280 spm | 13 | sprinter |

  Faster than about 14 frames (about 260 spm), human run cycles look silly. Extra flight time lets a run reach speeds past the natural limit. A front leg planted at ≤45° acts as a brake, so lengthen stride via the back kick rather than over-reaching forward. — [theodox](https://theodox.github.io/pages/pub/one_foot_in_front)
- In games, run speeds of 5.5 m/s (Max Payne) to 15 m/s (Quake 4) are common. 5 m/s is therefore a moderate game run. — [theodox](https://theodox.github.io/pages/pub/one_foot_in_front)
- In a run the body leans forward more than in a walk (Preston Blair, via a run-cycle tutorial). — [UW CSE459 run cycle tutorial](https://courses.cs.washington.edu/courses/cse459/03wi/Projects/Adam/run/run.html)
- Runner vertical CoM displacement is about 8–9 cm at about 168–170 spm, and a higher cadence reduces it. — [uni-konstanz CPA paper](https://ojs.ub.uni-konstanz.de/cpa/article/view/5948/5430)
- Slower cadences read as fatigue or solemnity, and faster ones read as hurry or anxiety. — [theodox](https://theodox.github.io/pages/pub/one_foot_in_front)

### Inferences (concrete targets for this mage at 5 m/s, 30 fps)
- **Cycle:** 2 steps in about 16 frames (8 frames per step) gives 225 spm and a stride of 5 / 3.75 ≈ 1.33 m. That is about 1.33× height and is plausible with flight. 18 frames (200 spm, 1.5 m stride) also works and feels heavier and less frantic, which may suit the "dark, tense" tone. Do not go below 14 frames.
- **Bob:** about 5 cm world (run, scaled from about 9 cm on a human), stylised to about 6–7 cm for the 42° ortho view. Lowest at contact plus 1 frame, highest mid-flight.
- **Lean:** about 12–18° forward at the pelvis. No sourced number exists; this is a stylistic starting point. More lean reads as faster.
- **Hem:** larger knee bulges than in a walk (front bones about 15–20°). A constant backward trail of about 5–10°. Side sway reduced to about 3° because runs are narrower. The hem lag stays at 2 frames per bone, which is a larger share of the shorter cycle and so reads more flowy.
- **Walk clip:** only needed if the game has a slow mode, such as aiming or casting slowdown. If so, use about 24–30 frames per cycle, about 3 cm bob, about 5° lean, smaller bulges, and larger side sway (about 5°). Blend walk→run on speed. Otherwise idle + run + start/stop is enough for MVP.
- Because the robe hides the feet, small mismatches between cadence × stride and actual 5 m/s will not show as foot slide. The bob frequency must still match visible ground travel, or the character reads as "running on ice".

### Gaps
- I found no measured lean angles for walk vs. run, in either biomechanics or animation sources.

## 5. Idle loop for a robed caster

### Takeaway
I found no source with specific idle values for robed characters. The general principles from the sources above still apply: secondary motion lags primary, nothing stops at once, and the hem settles after the body. The idle should be a slow breathing cycle that drives a small chest/shoulder rise. The hem and sleeves follow with lag. The fire element (ember or flame in the hand) supplies most of the life. That also matters because the player's light is a gameplay rule.

### Cited Findings
- Loose garments stop 3–4 frames after the body in a moving hold, which applies to the transition from run to idle (hem settle). — [Lynda 2D Animation Tips and Tricks: moving hold](https://origin-www.lynda.com/3D-Animation-tutorials/19-Moving-Hold-Advanced/480958/699149-4.html)
- Overlapping action: secondary parts keep moving after the primary stops. — [Tony White, Animator's Workbook (O'Reilly)](https://www.oreilly.com/library/view/tony-whites-animators/9780240813073/xhtml/18_Chapter10.xhtml)

### Inferences (starting values; unsourced, tune by eye)
- **Loop length:** about 2.5–3.5 s (75–105 frames at 30 fps), roughly one relaxed breath. Make the loop length not a simple multiple of the ember flicker, so the combined motion does not look mechanical.
- **Breath:**
  - Spine/chest rise of about 1–1.5 cm, or a slight robe-top scale of about 1–2%.
  - Shoulders lift about 2°.
  - The hood follows 3–4 frames later.
- **Sway:** a slow pelvis weight shift of about 1–2° roll per loop. The hem follows 4–6 frames later at about 1.5× the angle, because idle lag can be longer than run lag.
- **Hem settle (run→idle transition clip, about 10–15 frames):**
  - The body stops with a small backward rock of about 3–5°.
  - The hem keeps swinging forward past rest by about 8–12°, then makes 1–2 decreasing oscillations.
  - The tip bones lag their parents by 2–3 frames.
- **Hands/ember:**
  - The casting hand bobs about 1 cm, offset half a breath from the chest.
  - The ember/flame flickers through code: light intensity noise, particle and scale noise. Do not key it into the bone clip, because the gameplay light radius matters and shouldn't jitter.
  - The off-hand sleeve hangs with a 2–3° drift.
- **Optional "breaker" idle** (fidget, every 3–4 loops): the mage glances around under the hood, or the flame flares briefly. This reinforces the dark, tense tone and gives the cutscene/HUD a moment of life.

### Gaps
- I found no animator source with numbers for idle breathing amplitude or loop length for game characters, and none specific to robed casters.
