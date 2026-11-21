# Walk-cycle and idle fundamentals for a rigid-part low-poly character (scripted keys in Blender, FBX to Unity Generic)

Scope note: the target is a ~1 m tall biped built from rigid parts (each mesh piece driven 100% by one bone). Values from human gait research are for adults (~1.7-1.8 m) and must be scaled. Anything not backed by a fetched source is in Inferences or Gaps, not in Cited Findings.

## 1. Key poses of a walk cycle, their frame positions, and timing for slow vs brisk walks

### Takeaway
The standard 24-frame (24 fps) cycle is two steps of 12 frames: Contact 1 / 13 / 25 (25 = 1), Down 4 / 16, Passing 7 / 19, Up (push-off) 10 / 22. Williams-derived timing tables put a brisk "natural" walk at 12 frames per step, a stroll at 16, a tired walk at 20, a slow step at 24.

### Cited Findings
- 24-frame chart: contact on frames 1, 13, 25; down on 4 and 16; push-off ("up") on 10 and 22; frames 1 and 25 are identical; the body stays at the same origin for all three contact poses. Passing is "exactly in-between" contacts. — [Monmouth Animation, Movement: Walk Cycle](https://animation.monmouth.edu/instruct/?p=351)
- When previewing the loop, play frames 1-24 only; including frame 25 (duplicate of 1) causes a hitch at the loop point. — [Monmouth Animation](https://animation.monmouth.edu/instruct/?p=351)
- The same 1/4/7/10/13/16/19/22 layout (contact, down, passing, up) is used by Pluralsight's Blender walk-cycle tutorial and Adobe's walk-cycle guide (via search summary; the Pluralsight page itself could not be loaded). Some tutorials put the low point on frames 3/15 instead of 4/16. — [Pluralsight, Walk Cycle Animation in Blender](https://www.pluralsight.com/blog/tutorials/walk-cycle-animation-in-blender); [Adobe, animation walk cycle](https://www.adobe.com/creativecloud/animation/discover/animation-walk-cycle)
- Conflicting placement: Studio Liddell describes the passing pose at frame 12 of a cycle, which does not match the 7/19 convention (search-summary level only). — [Studio Liddell](https://studioliddell.com/news/animation-walk-cycles-masterclass-build-studio-quality)
- Timing table attributed to Williams' *The Animator's Survival Kit* (24 fps): 8-frame cycle (4/step) very fast run; 12 (4/step as printed) run or very fast walk; 16 (6/step as printed) slow run or "cartoon" walk; 24 (12/step) brisk, business-like "natural walk"; 32 (16/step) strolling; 40 (20/step) elderly or tired; 48 (24/step) slow step; 64 (32/step) slow dragging walk. Note: the page's "steps per second" column is internally inconsistent (e.g. it says 24 frames = 3 steps/s, but 12 frames/step at 24 fps is 2 steps/s), and its 12- and 16-frame rows look like they list frames per step wrong. Trust the frames-per-step figures for 24/32/40/48/64. — [Monmouth Animation](https://animation.monmouth.edu/instruct/?p=351)
- Williams and Goldberg use "frame" to mean 1/24 s, which matters when converting to 30 fps. — search summary of [AWN comment](https://www.awn.com/comment/1008092)
- Forum guidance: adult walk about 12-14 frames per step; a normal gait 8-16 frames per step at 24 fps. — search summary of [CG Cookie, 24 frames for a walk cycle](https://cgcookie.com/community/21098-24-frames-for-a-walk-cycle)
- Biomechanics: the centre of gravity makes two vertical oscillations per gait cycle (one per step). Troughs come during double-limb support (loading response and pre-swing; the animator's Contact/Down) and peaks at midstance/midswing (the animator's Passing/Up). — [OUHSC, Kinematic analyses of gait](https://ouhsc.edu/bserdac/dthompso/web/gait/knmatics/gait.htm)

### Inferences
- Note on "down" placement: biomechanics puts the trough at double support, right after heel strike, which matches Down at frames 3-4. The high point is at mid-stance, which falls between Passing (7) and Up (10). Most animators key the high on Up (10/22) for a springier look; for a heavy, stiff skeleton, a high on Passing (7/19) with Up only slightly lower reads as less bouncy.
- Script-ready frame tables (0-based Blender frames, cycle length N, last key at frame N = copy of frame 0):

| Pose | 24 f @24fps (brisk, 2 steps/s) | 32 f @24fps (stroll, 1.5 steps/s) | 40 f @24fps (tired/slow, 1.2 steps/s) | 30 f @30fps (same speed as 24@24) |
|---|---|---|---|---|
| Contact (L heel fwd) | 0 | 0 | 0 | 0 |
| Down | 3 | 4 | 5 | 4 |
| Passing | 6 | 8 | 10 | 8 |
| Up | 9 | 12 | 15 | 11 |
| Contact (R heel fwd) | 12 | 16 | 20 | 15 |
| Down | 15 | 20 | 25 | 19 |
| Passing | 18 | 24 | 30 | 23 |
| Up | 21 | 28 | 35 | 26 |
| Contact (= frame 0) | 24 | 32 | 40 | 30 |

  (Derived by splitting each step into quarters: Contact 0, Down 1/4, Passing 1/2, Up 3/4. The 30 fps column is the 24 fps chart times 1.25, rounded.)
- For a shambling undead skeleton, a 32- or 40-frame cycle at 24 fps (16-20 frames per step) fits the "tired/dragging" end of Williams' table. In-game the clip speed can be scaled in Unity, so one authored cycle can cover a range.
- Steps per second = fps / frames-per-step. 24-frame cycle at 24 fps = 2 steps/s = 120 steps/min.

### Gaps
- Could not load Williams' own text; the timing table comes from a secondary course page that credits him and has arithmetic errors in its steps-per-second column.
- No source fetched for normal human cadence in steps/min.

## 2. Typical amplitudes (hip bob, side shift, pelvis yaw/roll, shoulder counter-rotation, thigh, knee, arm, head)

### Takeaway
Adult COM moves about 5 cm vertically (two bobs per cycle) and about 5-6 cm side to side (one sway per cycle). Hip flexes about 30° and extends about 10°. Knee is nearly straight (about 5°) at contact, 10-15° after loading, peaks about 60-65° in swing. Pelvic obliquity range is about 7-9°. Arm swing is about 25° of shoulder amplitude. For a 1 m character, scale the linear values by about 0.55-0.6.

### Cited Findings
- Vertical COG displacement about 5 cm (2 in) peak-to-peak in a normal adult male; lateral about 6 cm (2.5 in), with one lateral oscillation per cycle and the largest lateral excursion at the end of mid-stance (body over the stance foot). — [OUHSC, Kinematic analyses of gait](https://ouhsc.edu/bserdac/dthompso/web/gait/knmatics/gait.htm)
- At midstance/midswing the stance hip, knee and ankle are each flexed about 5°. — [OUHSC](https://ouhsc.edu/bserdac/dthompso/web/gait/knmatics/gait.htm)
- Pelvis rotates forward on the loading-response limb and backward on the pre-swing limb (yaw toward the leading leg). During midstance it tilts (drops) toward the swing limb. The page gives no degree values. — [OUHSC, Pelvic Rotation](https://ouhsc.edu/bserdac/dthompso/web/gait/knmatics/detrot.htm); [OUHSC gait](https://ouhsc.edu/bserdac/dthompso/web/gait/knmatics/gait.htm)
- A 2014 review reports whole-body COM peak-to-peak amplitude of about 5 cm both vertically and mediolaterally at natural speed. — search summary of [SFU Locomotion Lab, Physical Therapy Review 2010](https://www.sfu.ca/locomotionlab/assets/physicaltherapyreview2010.pdf)
- Pelvic obliquity (roll) range: 9.4 ± 3.5° in women, 7.4 ± 3.4° in men. Vertical COM displacement 3.7% (women) vs 3.3% (men) of leg length (Smith et al. 2002). — search summary of [Gender differences in pelvic motions and COM displacement](https://katalog.lib.cas.cz/KNAV/EdsRecord/edsair,edsair.doi.dedup.....5711e2752a1f2d3be6fabfdd42692807)
- Hip: about 30° flexion and 10° extension from neutral. Hip is about 30° flexed at initial contact and again in terminal swing. — search summary of [Missouri, Normal Gait](https://medicine.missouri.edu/sites/default/files/pm-r/Normal_Gait.pdf) and [Lower-limb walking pattern](https://www.wikipedia.com/wiki/Lower-limb_walking_pattern)
- Knee: flexes 10-15° during loading response (max about 15% of cycle). Peak swing flexion about 60°, or up to 65° in late stance/early swing per the Missouri slides. — same sources as above
- Ankle needs at least 10-20° plantar flexion at about 1.25 m/s. — search summary of [Lower-limb walking pattern](https://www.wikipedia.com/wiki/Lower-limb_walking_pattern)
- Arm swing is pendulum-like with a shoulder movement amplitude of 24.6 ± 2.4° (2018 normal-gait study; the snippet does not say whether this is flexion+extension total or at which speed). Arm swing amplitude grows with walking speed. — search summary of [Springer, s11055-018-0647-8](https://link.springer.com/article/10.1007/s11055-018-0647-8); [MDPI Appl. Sci. 15(12) 6593](https://mdpi.com/2076-3417/15/12/6593)
- Elbow flexion-extension ROM in healthy gait: 29.7 ± 10.2° (dominant arm). — search summary of [KoreaScience 2023](https://koreascience.kr/article/JAKO202326857029300.pdf)
- Pelvic rotation and pelvic list contribute little to smoothing the vertical COM path; hip flexion, stance knee flexion and ankle-foot interaction matter more (Lin, Gfoehler & Pandy 2014). — [J Biomech 2014, Quantitative evaluation of the major determinants of human gait](https://www.doi.org/10.1016/j.jbiomech.2014.02.002)
- Thorax counter-rotation against the pelvis is commonly assumed to cancel angular momentum, but a 2008 study (2.0-5.2 km/h) found pelvis and thorax contribute under 10% of total body angular momentum. — [Coordination of leg swing, thorax rotations and pelvis rotations during gait](https://read.qxmd.com/read/17669652/coordination-of-leg-swing-thorax-rotations-and-pelvis-rotations-during-gait-the-organisation-of-total-body-angular-momentum)

### Inferences
- Scaling to a 1 m character: adult values (5 cm bob, 5-6 cm sway) at about 1.75 m scale to about 2.8 cm bob and 3-3.5 cm sway at 1 m. For a stiff, rigid-part look, 2-3 cm bob and 1.5-3 cm sway are reasonable. The sway can be pushed up for a lurching undead gait.
- Suggested starting keys for a 24-frame cycle (0-based, left foot forward at 0). Signs assume Blender bone local axes are aligned so +X rotation flexes the joint forward; check each bone's roll before trusting the signs.

| Channel | 0 Contact | 3 Down | 6 Passing | 9 Up | 12 Contact (mirror) |
|---|---|---|---|---|---|
| hips loc Z (bob, 1 m char) | 0 | -1.5 cm (low) | +1.0 cm | +1.5 cm (high) | 0 |
| hips loc X (sway) | 0 | toward L foot | max toward L (≈1.5-3 cm) | toward L | 0 (then mirror to R) |
| hips yaw (Z) | +4° (L hip forward) | +2° | 0 | -2° | -4° |
| hips roll | 0 | drop toward swing (R) side 3-4° | 2° | 0 | mirror |
| chest yaw (counter) | -3...-5° | | 0 | | +3...+5° |
| thigh.L (lead) | +30° flex | +25° | 0 to -5° | -10° ext | -10° ext (trail) |
| shin.L | -5° (near straight) | -15° (loading) | -5° | -10° | -40...-60° (swing peak a few frames later) |
| thigh.R (trail) | -10° ext | -5° | +15...+20° (swinging) | +25° | +30° |
| shin.R | -30...-40° (toe-off) | -60° (swing peak) | -45° | -15° | -5° |
| upperarm (opposite leg) | ±12-15° each way (≈25° total) | | 0 | | mirror |
| forearm | 5-15° bend, more on forward swing | | | | |
| head | counter-rotate to cancel chest yaw and pitch; keep near world-level | | | | |

  The second step (frames 12-24) is the mirror of the first with L/R swapped. Hip yaw peaks are taken from the commonly cited (unverified) Saunders figure of about 4° each side. Hip roll is set at half the 7-9° obliquity range cited above.
- Head stabilization: in a rigid FK chain, counter-key the head (and neck) by about -50% to -100% of chest yaw and of the hips/chest pitch bob, so the skull stays near level. This is an animator convention; no number was found in the sources.

### Gaps
- Could not confirm the classic Saunders figures (about 4° pelvic rotation per side, about 5° pelvic list) from a primary source.
- No fetched source for shoulder-girdle (thorax) counter-rotation degrees, head-stabilization amplitude, or a split of shoulder flexion vs extension in arm swing.
- Knee flexion over time (exact frames for the 60° peak) comes from biomechanics phase percentages, not from an animation source. Swing-knee peak is around 70-75% of the gait cycle by general knowledge, not verified here.

## 3. Making the cycle loop seamlessly (first = last, Cycles modifier, interpolation)

### Takeaway
Key frame N identical to frame 0, add a Cycles F-modifier (first in the stack, Repeat Motion both sides) to every F-curve so Blender's auto handles become cycle-aware, and export/play frames 0..N-1 (or 0..N with Unity's Loop Time, which drops the duplicate). Use linear or constant interpolation only for deliberately mechanical moves; for a rigid skeleton, Bezier with auto-clamped handles plus a few snappy holds usually looks better.

### Cited Findings
- The Cycles modifier "makes the curve repeat itself" and must be the first modifier in the stack (incompatible with Smooth (Gaussian)). Modes: No Cycles, Repeat Motion, Repeat with Offset (each copy offset so its first key matches the previous last key), Repeat Mirrored. — Blender Manual, Graph Editor > F-Curve Modifiers > Cycles (local copy `data/manual/editors/graph_editor/fcurves/modifiers.rst`; online: [docs.blender.org F-Curve modifiers](https://docs.blender.org/manual/en/latest/editors/graph_editor/fcurves/modifiers.html))
- "Trivially cyclic" curves (both ends Repeat Motion or Repeat with Offset, other options default) get extra support: automatic Bezier handle placement becomes aware of the cycle for a smooth transition, and Cycle-Aware Keying remaps out-of-range key insertions and keeps both end keys in sync. — Blender Manual, F-Curve modifiers and Animation editors > Cycle-Aware Keying ([docs.blender.org](https://docs.blender.org/manual/en/latest/editors/graph_editor/fcurves/modifiers.html))
- An action with a Manual Frame Range and "Cyclic Animation" enabled automatically makes newly added curves cyclic with a period matching the frame range (when Cycle-Aware Keying is on). — Blender Manual, Animation editors / Actions (local `animation/animation_editors.rst`, `animation/actions.rst`)
- Including the duplicate last frame in playback causes a visible hitch at the loop. — [Monmouth Animation](https://animation.monmouth.edu/instruct/?p=351)
- Matching the first and last keyframes is the basic seam fix for idles as well. — search summary of [AnimSchool, Breathing Life into Idle Animations](https://blog.animschool.edu/2024/06/14/breathing-life-into-idle-animations)

### Inferences
- Script recipe (bpy): set `action.use_frame_range = True`, `action.frame_start = 0`, `action.frame_end = N`, `action.use_cyclic = True`. Insert keys with `pose_bone.keyframe_insert("rotation_euler" | "rotation_quaternion" | "location", frame=f)`. Then for each `fcurve` (in Blender 5.x, actions use layered slots, so iterate the channelbag's fcurves for the armature slot) add `fcurve.modifiers.new('CYCLES')` and call `fcurve.update()`. Keep frame N a copy of frame 0. For Unity, export the range 0..N and tick Loop Time (and Loop Pose if needed) on the clip.
- Use Euler (XYZ) rotation mode on bones for scripted keys: readable degree values, independent per-axis curves, and no quaternion sign flips at the loop. Unity resamples on import anyway.
- Interpolation for a stiff/mechanical feel: Bezier with `AUTO_CLAMPED` handles prevents overshoot past key values (overshoot reads as rubbery on rigid parts). `LINEAR` on rotations gives a robotic, puppet-like gait with sharp direction changes at each key. A middle ground is Bezier on hips/arms and linear on a few "clunk" channels (e.g. the foot slap at contact). `CONSTANT` is for stepped previews only. These are style choices; no source was fetched comparing them.
- Avoid the Smooth modifier on cyclic curves (it conflicts with Cycles, per the manual).

### Gaps
- The exact Blender 5.x Python path for an action's F-curves under the layered-action system (slots/channelbags) should be checked with the local API docs before scripting. It was not verified in this pass.
- No source found comparing how FBX export samples Bezier vs linear curves. FBX export bakes animation at the Sampling Rate, so interpolation is baked into per-frame keys either way.

## 4. Idle loop basics (breathing/sway amplitude, loop length, offsets)

### Takeaway
Breathing is a small chest rise and expand with a slower inhale than exhale. Shoulders, head and arms trail the chest by a few frames. Loop length advice conflicts (short, at most about 45 frames, vs as long as possible). A base idle plus 2-4 random variants hides repetition, which players notice after about 90 s.

### Cited Findings
- Exaggerate chest motion while blocking, then scale it down. The inhale is often slower than the exhale. Each part reacts with a slight delay; shoulders react after the chest, not at the same time. — search summary of [AnimSchool, Breathing Life into Idle Animations](https://blog.animschool.edu/2024/06/14/breathing-life-into-idle-animations)
- Use subtle chest scale and offset the chest from the core so a wave moves through the body. — search summary of [Blender Artists, character breathing](https://blenderartists.org/t/character-breathing/1208159)
- Loop length conflict: one Blender forum tip says keep the idle at most 45 frames; a student's Unreal-idle note says make it as long as possible, with 2-3 small actions inside. — search summary of [Blender Artists](https://blenderartists.org/t/character-breathing/1208159); [lucylueng arts.ac.uk](https://lucylueng.myblog.arts.ac.uk/?p=3020)
- A single idle loop becomes noticeable after about 90 seconds. The standard fix is a continuous base idle plus 2-4 secondary idles played at random intervals. — search summary of [MoCap Online, Idle Animation: What Game Developers Need to Know](https://mocaponline.com/blogs/mocap-news/idle-animation-loop)
- Second Life's wiki covers subtle breathing motion for looping avatar animations (not fetched in detail). — [Second Life Wiki, Animating Breathing and Other Subtle Motion](https://wiki.secondlife.com/wiki/Animating_Breathing_and_Other_Subtle_Motion)

### Inferences
- Rigid-part constraint: with each part bound 100% to one bone, you cannot squash the chest mesh unless you key the chest bone's scale. Scale exports through FBX to Unity as transform scale, which also scales child bones. To avoid that, use rotation and location only: chest pitch back 1.5-3° and chest/spine loc Z +0.3-0.6 cm on inhale. A skeleton has no real breath, so a slow hunch and settle reads better than lungs.
- Starting numbers for a 1 m character: loop 48-72 frames at 24 fps (2-3 s, one breath). Inhale over about 55% of the loop, exhale over about 45%. Hips loc Z ±0.3-0.5 cm. Weight-shift sway hips loc X ±0.5-1 cm, one cycle per loop or per two loops. Head trails the chest by 2-4 frames. Upperarms trail the chest by 3-6 frames with ±1-3° rotation. Jaw or head jitter for an undead can sit on a separate F-curve with a Noise modifier. Note: Noise is not cyclic, so it will not loop cleanly unless its phase or period is tied to the loop length.
- Offsetting for overlap with scripted keys: generate each channel from the same pose list, then shift its keys by k frames modulo N (wrap around), so the loop stays seamless with the Cycles modifier.

### Gaps
- No authoritative numeric breathing amplitude (cm or degrees) or breaths-per-minute standard for game idles was found. Adobe Character Animator exposes a breaths-per-minute setting, but no default value was retrieved.

## 5. Minimal rig for a ~1 m biped: bone list, naming, mirroring, IK vs FK, root/hips relationship

### Takeaway
Use Blender's .L/.R suffix convention so mirroring, Symmetrize and Flip Names work. Keep a world-level `root` bone at the floor with `hips` as its only child, and give the rest of the body a clean FK hierarchy. For in-place game cycles the root stays still and only `hips` carries bob and sway. Pure FK is enough for scripted keys when the feet are allowed to slide slightly; IK only pays off if you need planted feet.

### Cited Findings
- Blender recognizes left/right counterparts by "L"/"R"/"left"/"right" (any case) with a separator: underscore, dot, dash or space (e.g. `hand_L`, `hand.l`, `hand-l`, `hand LEFT`), or with no separator only for full words (`handLeft`). The marker can be a prefix or suffix. Short "L"/"R" without a separator (`handL`) does not work. — Blender Manual, Armatures > Bones > Editing > Naming (local `animation/armatures/bones/editing/naming.rst`; online [docs.blender.org naming](https://docs.blender.org/manual/en/latest/animation/armatures/bones/editing/naming.html))
- Before mirroring or flipping, Blender strips number extensions like ".001". Duplicating `blah.L` gives `blah.L.001`, and Flip Names turns it into `blah.R`. — same
- AutoName Left/Right adds ".L" to bones whose root has positive X and ".R" for negative X. A bone exactly at X = 0 gets only a period suffix. So the character should face -Y (Blender's front view) with its left side on +X for auto-naming to match anatomical left. — same
- The body stays at the same origin in all contact poses of an in-place cycle; only the up/down bob and the pose change. — [Monmouth Animation](https://animation.monmouth.edu/instruct/?p=351)
- Using IK to author motion is fine for export because the result is baked into bone animation, which does export; constraints themselves do not survive FBX. — [Unity Discussions, How to import rigged assets from Blender to Unity properly](https://discussions.unity.com/t/how-to-import-rigged-assets-from-blender-to-unity-properly/682151)

### Inferences
- Recommended minimal hierarchy (17 deform bones plus root; names match the brief and Blender's mirror rules):
  ```
  root                (at floor, origin; never animated in in-place clips; future root motion goes here)
  └─ hips             (pelvis; carries bob/sway/yaw/roll; parent of legs and spine)
     ├─ spine
     │  └─ chest
     │     ├─ neck
     │     │  └─ head        (optional child: jaw, for a skeleton's chattering)
     │     ├─ upperarm.L ─ forearm.L ─ hand.L
     │     └─ upperarm.R ─ forearm.R ─ hand.R
     ├─ thigh.L ─ shin.L ─ foot.L   (optional toe.L)
     └─ thigh.R ─ shin.R ─ foot.R
  ```
  Optional `shoulder.L/R` (clavicle) under chest only if the shoulder shrug is needed for attacks. Optional prop bones (e.g. `weapon` or `prop.R` under `hand.R`) for attack clips. A death clip can drive `root`/`hips` falling and individual parts, so no extra bones are needed. For a pile-of-bones collapse, rigid parts can later be detached in Unity.
- Uncheck "Deform" on any helper/IK/control bones and export with Only Deform Bones, so Unity gets only the clean list above. Turn off Add Leaf Bones so no `*_end` bones are added (these are an FBX export option; the manual has no description text for them).
- FK vs IK for scripted keys: FK is simpler to script (one Euler per bone per key, exact degree values from the table in section 2, trivial mirroring by negating Y/Z rotation and swapping .L/.R). Its weakness is foot sliding during stance, which is minor on an in-place cycle at a top-down or 3/4 camera. IK (foot target bones plus a pole) gives planted feet but means scripting world-space foot paths and then baking (Pose > Animation > Bake Action, Visual Keying) before FBX export. For a 6-8 week single-developer schedule, FK first is the pragmatic choice.
- Root/hips for in-place cycles: keep `root` at (0,0,0) with no keys (or a constant key) in walk/idle. Put all bob/sway/twist on `hips`. In Unity Generic, set the Root node to `root`. With no motion on root, Unity's root motion stays zero and the character controller moves the GameObject. If root motion is wanted later (e.g. a lunge attack), animate `root` translation in that clip only.
- Bone orientation: build bones along +Y in Blender (default) with consistent roll, so "+X rotation = forward flex" holds for all limb bones. This makes the angle tables directly scriptable.

### Gaps
- No primary source was fetched specifying Unity Generic import settings (Root node, Avatar Definition "Create From This Model") in Unity 6. These come from general Unity knowledge and should be checked in the Unity 6 manual.
- No source compares IK vs FK effort specifically for scripted (non-interactive) keyframing.

## 6. Rigid binding in Blender: weight-1.0 vertex groups + Armature modifier vs bone-parenting objects, and FBX-to-Unity behavior

### Takeaway
Both work, and they produce different Unity components. (a) Armature modifier with each part's vertices at weight 1.0 in exactly one vertex group gives a SkinnedMeshRenderer. This is the most widely reported "safe" path through FBX, and hard 1.0 weights avoid normalization issues. (b) Object bone-parenting gives plain MeshRenderers as child transforms of the bone nodes. Unity's Blender/FBX import has handled this since about 2017.1, but there are reported offset/round-trip bugs and some engines turn these into skinned meshes. For rigid parts, (a) is the more predictable choice; (b) is lighter at runtime but should be test-exported first.

### Cited Findings
- Hard-weight workflow: parent the mesh to the armature with "Empty Groups", then in Edit Mode select one vertex of a part, Ctrl+L to select linked, and assign to the matching group at weight 1.0. — search summary of [Polycount discussion](https://polycount.com/discussion/comment/2739654/)
- Some software cannot handle non-normalized weights; Unity auto-normalizes weights on import. Single-bone 1.0 weights avoid the issue. — search summary of [Blender Artists thread](https://blenderartists.org/t/manual-vertex-group-assignment-from-blender-not-isnt-recognized-as-a-rig-in-csp-please-help-t-t/1649975); [Blender bug T46020](https://developer.blender.org/T46020)
- In other engines (Babylon.js), a mesh with no Armature modifier and no weights in the output file does not deform with the rig. — search summary of [Babylon.js forum](https://forum.babylonjs.com/t/parenting-mesh-to-bones-in-blender-throws-off-the-mesh-s-orientation-after-exporting-to-babylon/6759)
- Unity: older versions failed on objects parented to bones; a user reports Unity 2017.1's Blender importer handles them correctly. Imported bones get a -90° X rotation and some carry helper scale. Constraints and drivers do not export and must be baked or rebuilt. — [Unity Discussions 682151](https://discussions.unity.com/t/how-to-import-rigged-assets-from-blender-to-unity-properly/682151)
- A Unity user's rigid gear meshes parented to bones in Blender came in as SkinnedMeshRenderers; replies suggest pinning parts to a rig or using a plain hierarchy for mechanical models. — search summary of [Unity Discussions, Model without a Skinned Mesh](https://discussions.unity.com/t/model-without-a-skinned-mesh/495246)
- FBX expects bone-parented children to attach at the bone head, while Blender attaches at the tail. This caused offsets when importing such FBX files into Blender 3.6.9/4.0.2 (importer side, with Force Connect Children). — [Blender add-ons issue #105217](https://projects.blender.org/blender/blender-addons/issues/105217)
- On re-import into Blender, parenting to bone can be lost. — search summary of [Blender bug T57536](https://developer.blender.org/T57536)
- The FBX exporter can write bones that do not take part in skinning. — search summary of [Blender bug T101237](https://developer.blender.org/T101237)
- An armature exported without a weighted mesh can have its rest pose changed by the FBX exporter (reported issue). — search summary of [Blender issue #113073](https://projects.blender.org/blender/blender/issues/113073)
- Blender FBX export options: "All Actions" exports every action compatible with the armature, each with its own keyframe range (otherwise only the active action). "Apply Transform" is experimental and "known to be broken with armatures/animations". The manual lists Only Deform Bones, Add Leaf Bones, Armature FBXNode Type, Key All Bones, NLA Strips, Force Start/End Keying, Sampling Rate and Simplify without descriptions. — Blender Manual, Import-Export > FBX (local `addons/import_export/scene_fbx.rst`; online [docs.blender.org FBX](https://docs.blender.org/manual/en/latest/addons/import_export/scene_fbx.html))
- Roblox's rigging docs describe the rigid-limb approach (each limb fully articulating with its bone) as ideal for non-organic characters like robots. — search summary of [Roblox, Rig a simple mesh](https://create.roblox.com/docs/en-us/art/modeling/rig-a-simple-mesh)

### Inferences
- Recommended path for Tulang/Pyromancer: join all rigid parts into one mesh object (or keep a few), add an Armature modifier, and assign each part's vertices to exactly one bone's vertex group at 1.0 (scriptable: for each part, `vg = obj.vertex_groups.new(name=bone_name); vg.add(indices, 1.0, 'REPLACE')`). The result in Unity is one SkinnedMeshRenderer with one draw call and predictable import. With rigid weights there is no deformation, so the stiff look is kept exactly.
- Alternative (bone parenting, `obj.parent = arm; obj.parent_type = 'BONE'; obj.parent_bone = 'shin.L'`): gives MeshRenderers under bone transforms in Unity. Handy for later detaching parts on death (scatter bones physically) and for swapping weapons. Test once by exporting a 2-part sample and checking in Unity that the parts sit correctly under the bone GameObjects at rest and in animation.
- A hybrid is common: skin the body via rigid weights, and bone-parent swappable props (weapon, torch) or attach them in Unity to the `hand.R` transform.
- Export settings to try first (to verify): Selected Objects (armature + meshes), Apply Scalings "FBX Units Scale", Forward -Z / Up Y defaults, Apply Transform off, Armature: Only Deform Bones on, Add Leaf Bones off, Bake Animation on, All Actions on (one action per clip: Walk, Idle, Attack, Death), NLA Strips off, Force Start/End Keying on, Simplify 0 (or low) so small idle motions are not stripped, Sampling Rate 1.0. In Unity: Rig > Animation Type Generic, Avatar "Create From This Model", Root node `root`. Then per clip: Loop Time on for Walk/Idle.

### Gaps
- No official Unity 6 or Blender 5.x document was found that states definitively how bone-parented objects map into Unity (MeshRenderer child vs SkinnedMeshRenderer); the evidence is forum reports from 2013-2024. Verify with a test export.
- The Blender manual has no descriptions for Simplify, Sampling Rate, Only Deform Bones or Add Leaf Bones, so their recommended values above are inferences.
