# Speed-synced in-place locomotion (Unity 6.3) and Blender Python armature animation to FBX

Scope: in-place Walk/Idle clips authored by Blender Python (Blender 5.x), exported as FBX, imported as a Generic rig in Unity 6000.3 (URP), with the Animator driven by movement speed so feet do not slide. No third-party packages.

Labelling convention used below: every item under "Cited Findings" has a source. Items under "Inferences" are derivations or prior engineering knowledge that I did NOT confirm against a source this session; treat them as "verify before relying on".

---

## 1. Foot-slide-free playback: stride math and how professional games handle it

### Takeaway
For an in-place clip, playback must be scaled by `actualSpeed / clipNaturalSpeed`, where `clipNaturalSpeed = cycleDistance / cycleDuration` and `cycleDistance` is the distance a planted foot travels (relative to the root) over one full cycle (2 steps). Pure playback-rate scaling only holds over a narrow band; AAA engines combine a speed-blended set of clips with playback-rate scaling plus IK "stride warping", or use root motion / distance matching. For a small project, a 1D blend tree (Idle at 0, Walk at its natural speed) plus a clamped speed multiplier is the standard Unity approach.

### Cited Findings
- Unity's own NavMesh+animation guidance: in-place clips "prevent foot-sliding when blending"; set the agent speed to the maximum speed in the blend tree and sync position in `OnAnimatorMove()` — [Unity AI Navigation: Coupling Animation and Navigation](https://docs.unity3d.com/Packages/com.unity.ai.navigation@1.1/manual/CouplingAnimationAndNavigation.html)
- Industry practice described on the Unreal forums: a curve maps character movement speed to animation playback speed across a blend space; example calibration "walk ≈ 100 cm/s, run ≈ 400 cm/s" — [UE forum: Sync walk animation to movement speed](https://forums.unrealengine.com/t/sync-walk-animation-to-movement-speed/41064)
- Counterpoint from the same community: changing playback speed *alone* (without matching the clip's real speed) just causes foot sliding — [UE forum: mismatch between animation speed and character speed](https://forums.unrealengine.com/t/the-mismatch-between-animation-speed-and-character-speed/155482)
- Root motion makes the animation advance exactly with the character, so everything matches; it is the "cleanest" fix when usable — [UE forum: Sync run/walk anim with movement speed](https://forums.unrealengine.com/t/sync-run-walk-anim-with-movement-speed/56475)
- Measuring an in-place clip's implied speed: find the frame range where a foot is planted, measure root-to-foot displacement over that range, divide by elapsed time; then tune the capsule speed (or playback) to match — [UE forum: How to determine the movement speed of in-place animations](https://forums.unrealengine.com/t/how-to-determine-the-movement-speed-of-in-place-animations/2703334)
- Low-speed problem: walk clips played slowly create "tiny steps" / look wrong at low speeds, which is why there is a minimum playback rate before switching to idle — [UE forum: walking animation creates tiny steps at low speeds](https://forums.unrealengine.com/t/walking-animation-creates-tiny-steps-at-low-speeds-at-normal-speeds-works-fine/415728.md)
- Paragon (Epic, GDC 2016, Laurent Delayen) introduced distance matching and speed warping; community implementations note Epic "implemented the speed warping node which scaled down the animation via IK bone manipulation" rather than only blending — [Game Developer: most inspiring animation tech talks of 2016](https://gamedeveloper.com/programming/most-inspiring-game-animation-tech-talks-of-2016); [UE forum: Distance matching locomotion](https://forums.unrealengine.com/t/distance-matching-locomotion-giving-it-a-shot/122622)
- Distance matching = use the clip's distance curve as "inverse root motion": binary-search the distance curve to pick the frame that corresponds to the distance remaining/travelled, with linear interpolation between keys — [Live From Epic HQ: Animation techniques in Paragon](https://forums.unrealengine.com/t/animation-techniques-used-in-paragon-and-january-game-jam-theme-announcement-live-from-epic-hq/52339?page=2)
- Unity Asset Store "Strider" exists specifically to change stride length instead of play rate (evidence that stride scaling is the accepted alternative to play-rate scaling) — [Unity Asset Store: Strider](https://marketplace.unity.com/packages/tools/animation/strider-162020) (third-party; not usable under project constraints, cited only as practice evidence)
- Unity 1D blend tree "Compute Thresholds → Speed" sets each motion's threshold from its root-motion speed; Unity's example: walk 1.5 u/s, jog 2.3 u/s, run 4 u/s; a parameter of 3.0 blends jog/run "with a slight bias toward the jog" — [Unity 6.3 Manual: 1D Blending](https://docs.unity.com/en-us/engine/6000.3/manual/animation-section/animation-mecanim/animation-animator-controller/animation-state-machines/animation-blend-trees/blend-tree-1dblending)

### Inferences
- **Formula (derivation).** Let a looping clip have `N` frames at `fps` (Blender scene fps), so `T = N / fps` seconds per full cycle (left step + right step). Let `D` = distance (in Unity units, after import scale) that a planted foot moves backward relative to the root during one full cycle = stride length (2 steps). Natural speed `v0 = D / T`. For character speed `v`: `playback = v / v0 = v · T / D`. Equivalently `playback = v / (D · cycleRate)` with `cycleRate = 1/T` — this matches the formula in the brief. If only one planted interval is measured (foot down at frame a, up at frame b, displacement d): `v0 = d · fps / (b − a)`.
- **Authoring the clip so `v0` is known exactly (best for Blender-scripted clips).** Because the clip is generated by script, pick the target speed first and derive the keys: choose `v0` (e.g. the character's walk speed in m/s), choose `T` (e.g. 1.0 s = 24 frames at 24 fps), then `D = v0 · T`, step length `= D/2`. During the stance phase, key the planted foot so it moves backward at exactly `v0` (linear interpolation on the foot's forward axis across the stance frames). Then a runtime multiplier of `v / v0` is exact at `v = v0` and approximately correct nearby. Store `v0` in one place (a ScriptableObject or serialized field) and use the same constant in Blender and Unity.
- **Model scale matters.** `D` must be in Unity world units after import scale and after the prefab's transform scale. If the character root is scaled by `s` in the scene, `v0_world = s · v0_clip`.
- **Recommended clamp ranges (practitioner rule of thumb, NOT found in a primary source this session):** a walk cycle scaled to roughly 0.7×–1.3× (some go 0.5×–1.5×) still reads as the same gait; below that, legs look like slow motion, above it they look frantic. Hence: below `~0.5·v0` blend toward Idle rather than slowing the walk further; above `~1.3–1.5·v0` blend to a jog/run clip. For a 2-clip setup (Idle, Walk) and a capped top speed equal to `v0`, the multiplier only needs clamp `[~0.5, ~1.2]`.
- **Ground truth trick for verification:** in Play mode, put a small debug marker at the planted foot's world position at foot-down; if it drifts during stance, `v0` or the multiplier is wrong. Drift direction tells which way (drifts backward = animation too fast).

### Gaps
- I did not retrieve the Paragon GDC slides themselves, so exact speed-warping play-rate clamp defaults from Epic are not cited. The UE "Animation Warping" doc page returned empty content to the fetcher. Unreal's Stride Warping node does have play-rate clamp settings, but I could not confirm the numeric defaults.
- No primary source found giving a numeric "max acceptable playback scale" — the 0.7–1.3 / 0.5–1.5 band above is an unverified rule of thumb.

---

## 2. Unity 6.3 Animator setup, import settings, Rigidbody feed, and tests

### Takeaway
Use a float `Speed` parameter set every frame with damping (`SetFloat(id, value, dampTime, Time.deltaTime)`), a 1D blend tree with Idle at threshold 0 and Walk at threshold `v0`, and drive the state's playback via the per-state Speed Multiplier parameter (`AnimatorState.speedParameterActive` + `speedParameter`) — not `Animator.speed`, which scales every layer and state. Import as Generic, root motion off (in-place), Loop Time on for Idle/Walk. Feed the Animator from `Rigidbody.linearVelocity` (Unity 6 name) projected onto the ground plane. Tests can assert importer settings via `ModelImporter.clipAnimations` and controller structure via `AnimatorController.layers[0].stateMachine`.

### Cited Findings
**Animator parameters and states**
- `Animator.SetFloat` overloads: `SetFloat(string name, float value)`, `SetFloat(string name, float value, float dampTime, float deltaTime)`, plus `int id` variants. `dampTime` = "The damper total time", `deltaTime` = "The delta time to give to the damper." The page does not describe the damping curve — [Unity 6.3 Scripting API: Animator.SetFloat](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/animator/setfloat)
- `AnimatorState` properties (UnityEditor.Animations): `speed` = "The default speed of the motion"; `speedParameter` = "The animator controller parameter that drives the speed value"; `speedParameterActive` = "Define if the speed value is driven by an Animator controller parameter or by the value set in the editor"; `cycleOffset` (+ parameter variants) = "Offset at which the animation loop starts. Useful for synchronizing looped animations. Units is normalized time"; also `writeDefaultValues`, `timeParameter`/`timeParameterActive` (use a parameter as normalized time), `motion`, `transitions`, `iKOnFeet` — [Unity 6.3 Scripting API: AnimatorState](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/animations/animatorstate)
- Note on `timeParameter`: driving normalized time from a parameter is effectively a do-it-yourself distance matching hook (set normalized time = accumulated distance / D mod 1) — property documented at [AnimatorState](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/animations/animatorstate); the distance-matching use is my inference.

**Blend trees**
- 1D blend: thresholds are the parameter values at which a motion has full weight; "Automate Thresholds" spaces them evenly; "Compute Thresholds" derives them from root-motion data (Speed, Velocity X/Y/Z, Angular Speed) — [Unity 6.3 Manual: 1D Blending](https://docs.unity.com/en-us/engine/6000.3/manual/animation-section/animation-mecanim/animation-animator-controller/animation-state-machines/animation-blend-trees/blend-tree-1dblending)
- Per-child time scale (clock-icon column): "to double the speed of a walk clip, specify a value of 2.0"; "Adjust Time Scale > Homogeneous Speed" rescales clips relative to min/max "preserv[ing] the initial relative speed of each clip"; only available when all children are clips, not child blend trees — [Unity 6.3 Manual: Blend tree common options](https://docs.unity.com/en-us/engine/6000.3/manual/animation-section/animation-mecanim/animation-animator-controller/animation-state-machines/animation-blend-trees/blend-tree-additional-options)

**Import settings (ModelImporter / ModelImporterClipAnimation)**
- `ModelImporter` animation/rig properties: `animationType` ("Animator generation mode"), `avatarSetup`, `sourceAvatar`, `clipAnimations` ("Animation clips to split animation into"), `defaultClipAnimations` ("Generate a list of all default animation clip based on TakeInfo"), `importAnimation`, `animationCompression`, `animationRotationError`/`PositionError`/`ScaleError`, `resampleCurves` ("If set to false, the importer will not resample curves when possible"), `bakeAxisConversion` ("Computes the axis conversion on geometry and animation for Models defined in an axis system that differs from Unity's (left handed, Z forward, Y-up)"), `useFileScale`, `globalScale`, `useFileUnits` ("Detect file units and import as 1FileUnit=1UnityUnit, otherwise it will import as 1cm=1UnityUnit"), `motionNodeName`, `optimizeGameObjects`, `preserveHierarchy`, `sortHierarchyByName` — [Unity 6.3 Scripting API: ModelImporter](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/modelimporter)
- `ModelImporterClipAnimation`: `name`, `takeName`, `firstFrame`, `lastFrame`, `loopTime` ("Controls whether Unity blends the first and last frames to create smooth time-based looping"), `loopPose` ("Enable to make the motion loop seamlessly"), `lockRootRotation`, `lockRootHeightY`, `lockRootPositionXZ` (bake root motion into bones vs. store as root motion), `keepOriginalPositionY/XZ`, `keepOriginalOrientation`, `cycleOffset`, `events`, `curves`, `mirror`, `wrapMode`, `loop` — [Unity 6.3 Scripting API: ModelImporterClipAnimation](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/modelimporterclipanimation)
- Practitioner guide (Blender→Unity): enable **Bake Axis Conversion** so the character faces the right way; enable **Loop Time** on clips — [bkjam: Exporting FBX from Blender to Unity (2024)](https://bkjam.github.io/posts/2024-10-19-blender-to-unity-settings)

**Compression and jitter**
- `animationRotationError` is in degrees (0–180); a Japanese tutorial reports the default 0.5° is visually lossless while roughly halving size, while very large values (e.g. 100°) visibly break foot contact — [Unity 6.3 ModelImporterAnimationCompression](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/modelimporteranimationcompression.md); [STYLY: reducing animation size in Unity](https://styly.cc/tips/unity-reduction-animation)
- Compression "Off" keeps most precision at a memory/performance cost; Unity recommends keeping keyframe reduction and lowering error values when more precision is needed; "Optimal" is the default — [Unity 5.5 legacy manual: FBX Importer Animations](https://docs.unity3d.com/550/Documentation/Manual/FBXImporter-Animations.html) (legacy page; concept unchanged, verify UI wording in 6.3)
- Position error is applied as a percentage of the key value (Unity issue tracker), which affects small rigs/tiny translations disproportionately — [Unity Issue Tracker: position axis not processed individually with keyframe reduction](https://issuetracker-mig.prd.it.unity3d.com/issues/each-position-axis-not-being-processed-individually-when-compressing-animation-with-keyframe-reduction)
- Jitter is not always Unity compression: a Blender report shows jitter persisting with compression off and reproducible after re-import to Blender (IK-driven bones) — [Blender T54459: Animation jitter, rigged character FBX export](https://developer.blender.org/T54459)
- Analysis of Unity's compression internals (keyframe reduction + curve representation) — [Nicholas Frechette: Animation Compression: Unity 5](https://nfrechette.github.io/2017/01/30/anim_compression_unity5)

**Rigidbody velocity (Unity 6 rename)**
- Unity 6 renamed `Rigidbody.velocity` → `Rigidbody.linearVelocity` (and `Rigidbody2D` likewise; `drag` → `linearDamping`); old names compile with an obsolete warning. Setting velocity on a kinematic body has no effect — [Unity 6.3 Scripting API: Rigidbody.linearVelocity](https://docs.unity3d.com/ScriptReference/Rigidbody-linearVelocity.html); [Unity 6 API migration guide](https://uhiyama-lab.com/en/notes/unity/unity6-api-migration-guide/); [GameDev.tv thread](https://community.gamedev.tv/t/unity-6000-has-changed-velocity-to-linearvelocity-for-rigidbody2d/254175)

### Inferences
**Recommended controller (2-clip, in-place, Generic):**
- Parameters: `Speed` (float, m/s, horizontal), `SpeedMul` (float, playback multiplier). Base layer default state = `Locomotion` (1D blend tree on `Speed`): Idle at threshold 0, Walk at threshold `v0` (the walk clip's natural speed). Turn **Automate Thresholds off** and type the values; "Compute Thresholds" will not work because in-place clips have no root motion.
- Locomotion state: `speedParameterActive = true`, `speedParameter = "SpeedMul"`. In code: `SpeedMul = speed > v0 ? Mathf.Clamp(speed / v0, 1f, maxMul) : 1f`. Below `v0` the blend tree already mixes Idle in, so do not slow the walk below 1× there — or allow mild slow-down, e.g. `Mathf.Clamp(speed / v0, 0.8f, 1.25f)`, and let the blend handle the rest. Keep one of the two mechanisms responsible for each speed range, otherwise slow speed gets "double slowed".
- **Blend tree vs two states:** a 1D blend tree gives continuous, phase-coherent blending and needs no transition tuning; two states (Idle ↔ Walk) need transitions with `Has Exit Time = false`, conditions `Speed > startThreshold` / `Speed < stopThreshold`, and a short transition duration. Hysteresis example (unverified rule of thumb): start walking at `Speed > 0.15·v0`, stop at `Speed < 0.08·v0`; transition duration ~0.1–0.2 s (fixed duration). Exit time must be off, otherwise the character waits for the clip to finish before reacting.
- **Damping:** `animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime)` (dampTime ~0.05–0.15 s) smooths jitter from physics. Damped value never exactly reaches 0, so when input is zero and speed is below a small epsilon, call the non-damped overload with 0 or rely on the blend tree (weight near 0 is invisible). Unity does not document the damper curve on the SetFloat page (it behaves like an exponential/critically damped approach in practice — unverified).
- **`Animator.speed` vs state multiplier:** `Animator.speed` scales the whole Animator (all layers, all states, transitions, attack clips too), so using it for locomotion would also speed up an attack on an upper-body layer. Per-state Speed Multiplier affects only that state. Use `Animator.speed` only for global effects (hit-stop, slow-mo).
- **Feeding velocity from a Rigidbody:** read in `Update` (or `LateUpdate`) from `rb.linearVelocity`, project to the ground plane (`Vector3.ProjectOnPlane(v, Vector3.up).magnitude` or `new Vector2(v.x, v.z).magnitude`), divide by the character's transform scale if the clip speed `v0` was measured in local units. With interpolation enabled on the Rigidbody the visual and physics positions differ slightly per frame but the velocity value is still the physics one; damping covers this. Alternatively compute velocity from the controller's own desired velocity (the value assigned to `linearVelocity`) to avoid wall-collision noise; if the character is pushing into a wall, the actual physics velocity is ~0, which correctly shows Idle instead of moonwalking.
- **Generic import settings checklist:** Rig → Animation Type = Generic, Avatar Definition = Create From This Model, Root node = the armature root bone (or None for in-place). Animation → per clip: Loop Time on (Idle, Walk), Loop Pose on only if first/last frames differ slightly (if the Blender script keys frame N == frame 0 exactly, Loop Pose is not needed and Unity's loop match indicators should be green). Root motion: leave `Animator.applyRootMotion = false` on the component; in-place clips need no root-transform locks. Compression: start with Optimal / Keyframe Reduction and rotation error 0.5°, position error 0.5%; if small rigs jitter, lower to ~0.1 or set compression Off (clips are tiny anyway).
- **Testing (EditMode, no Play mode needed):**
  - Importer: `var imp = (ModelImporter)AssetImporter.GetAtPath(path); var clips = imp.clipAnimations.Length > 0 ? imp.clipAnimations : imp.defaultClipAnimations;` then assert `imp.animationType == ModelImporterAnimationType.Generic`, a clip named "Walk" exists with `loopTime == true`, and none are named with an `Armature|` prefix. Note: `clipAnimations` is empty until the user (or an AssetPostprocessor) customises the clip list; `defaultClipAnimations` reflects raw FBX takes.
  - Clip asset: `AssetDatabase.LoadAllAssetsAtPath(fbxPath).OfType<AnimationClip>()` (filter out `__preview__` clips) and check `clip.isLooping`, `clip.length` ≈ N/fps.
  - Controller: load the `AnimatorController`, assert parameter `Speed` exists with type Float, `layers[0].stateMachine.defaultState.motion is BlendTree`, its children thresholds are `{0, v0}`, and the state has `speedParameterActive && speedParameter == "SpeedMul"`.
  - Pure logic: put `ComputeSpeedMul(speed, v0, min, max)` in a static/pure C# function and unit-test the math (0 → idle, v0 → 1, 2·v0 → clamped). This is the part TDD should cover.
  - PlayMode (optional): instantiate the prefab, set a velocity, step frames, assert `animator.GetFloat("Speed")` approaches the target and `GetCurrentAnimatorStateInfo(0).IsName("Locomotion")`. Note `Animator.Update(dt)` can be called manually to advance an Animator in tests.
  - Prefer `Animator.StringToHash("Speed")` cached ints in runtime code.
- To automate importer settings rather than clicking: an `AssetPostprocessor.OnPreprocessModel/OnPreprocessAnimation` can set `animationType`, and for clips copy `defaultClipAnimations`, rename (strip `Armature|`), set `loopTime`, assign `clipAnimations`. This keeps the pipeline scriptable end to end.

### Gaps
- Unity 6.3 manual page on transition settings (exit time, fixed duration, interruption source) was not fetched; values above are practitioner defaults, not documentation.
- Exact damping formula behind `SetFloat(..., dampTime, deltaTime)` is not documented on the 6.3 API page.
- I did not verify in Unity 6.3 docs that `Animator.speed` scales all layers (prior knowledge; widely understood but not re-cited here).
- Default values of `animationPositionError` / `animationScaleError` in 6.3 were not confirmed (only rotation 0.5° via a secondary source).

---

## 3. Blender Python (4.4 → 5.x): armature, keyframes, multiple Actions, NLA, slotted actions

### Takeaway
Blender 4.4 introduced slotted actions (Action → Slots + Layers → Strips → Channelbags → F-Curves). The legacy `Action.fcurves` / `Action.groups` / `Action.id_root` were deprecated in 4.4 and **removed in 5.0**, so any 5.x script must either use `pose_bone.keyframe_insert(...)` (still works, creates slot/layer/strip/channelbag automatically) or the explicit channelbag API. `AnimData`, `NlaStrip` and `ActionConstraint` each have an `action_slot` property; assigning an action auto-picks a matching slot, but scripts should set it explicitly.

### Cited Findings
- Slotted actions (4.4): one Action can animate multiple data-blocks; each Action has Slots, and every F-Curve belongs to one slot — [Blender 4.4 Manual: Actions](https://docs.blender.org/manual/en/4.4/animation/actions.html); [Blender developer docs: layered animation](https://developer.blender.org/docs/features/animation/animation_system/layered/)
- Creating data explicitly in 4.4+ (official upgrading guide):
  ```python
  slot = action.slots.new(id_type='OBJECT', name="Suzanne")
  layer = action.layers.new("Layer")
  strip = layer.strips.new(type='KEYFRAME')
  channelbag = strip.channelbag(slot, ensure=True)
  fcurve = channelbag.fcurves.new("location", index=1)
  ```
  Only one layer with one keyframe strip is currently supported. Assigning `anim_data.action` auto-assigns a slot; if no slot is chosen the object is not animated; explicit: `anim_data.action_slot = anim_data.action_suitable_slots[0]`. NLA strips and Action Constraints auto-assign slots similarly — [Blender 4.4 upgrading: Slotted Actions](https://developer.blender.org/docs/release_notes/4.4/upgrading/slotted_actions/)
- `keyframe_insert()` still works unchanged, e.g. `object.pose.bones["Arm_L"].keyframe_insert("rotation_euler", 1)`, and creates whatever is missing (action, assignment, slot, layer+keyframe strip, channelbag, F-Curve, group) — [Blender 4.4 upgrading: Slotted Actions](https://developer.blender.org/docs/release_notes/4.4/upgrading/slotted_actions/)
- Legacy proxies: `action.fcurves` = `action.layers[0].strips[0].channelbag(action.slots[0]).fcurves`; `action.groups` likewise; `action.id_root` = `action.slots[0].target_id_type`; "This legacy API is considered deprecated, and will be removed in Blender 5.0" — [Blender 4.4 upgrading: Slotted Actions](https://developer.blender.org/docs/release_notes/4.4/upgrading/slotted_actions/)
- 5.0 change log lists `fcurves`, `groups`, `id_root` as removed from `bpy.types.Action` — [Blender 5.0 Python API change log](https://docs.blender.org/api/5.0/change_log.html) (via search snippet; full page content not retrieved); [Blender 5.0 release notes: Animation & Rigging](https://developer.blender.org/docs/release_notes/5.0/animation_rigging/)
- 5.0: creating a driver via Python no longer creates an F-Curve modifier but two keyframes; legacy pose-library conversion operators removed — [Blender 5.0 release notes: Animation & Rigging](https://developer.blender.org/docs/release_notes/5.0/animation_rigging/)
- `ActionSlots.new(...)` returns an `ActionSlot`; `ActionSlots.remove(action_slot)` removes the slot "including all animation that is associated with that slot"; `ActionSlot.target_id_type` is settable only while 'UNSPECIFIED' — [Blender 4.4 API: ActionSlots](https://docs.blender.org/api/4.4/bpy.types.ActionSlots.html); [ActionSlot](https://docs.blender.org/api/4.4/bpy.types.ActionSlot.html)
- `AnimData.action_slot`, `NlaStrip.action_slot`, `ActionConstraint.action_slot`: "The slot identifies which sub-set of the Action is considered to be for this data-block/strip, and its name is used to find the right slot when assigning an Action" (bundled API docs of the installed Blender MCP, matching current online API) — [Blender API: NlaStrip](https://docs.blender.org/api/current/bpy.types.NlaStrip.html); [AnimData](https://docs.blender.org/api/current/bpy.types.AnimData.html)
- Official quickstart example (current API):
  ```python
  action = bpy.data.actions.new(name="MyAction")
  slot = action.slots.new(obj.id_type, obj.name)
  strip = action.layers.new("MyLayer").strips.new(type='KEYFRAME')
  channelbag = strip.channelbag(slot, ensure=True)
  fcu_z = channelbag.fcurves.new(data_path="location", index=2)
  fcu_z.keyframe_points.add(2)
  fcu_z.keyframe_points[0].co = 10.0, 0.0
  fcu_z.keyframe_points[1].co = 20.0, 1.0
  adt = obj.animation_data_create()
  adt.action = action
  adt.action_slot = slot
  ```
  — [Blender API Quickstart: Animation](https://docs.blender.org/api/current/info_quickstart.html)
- User report: when an object has NLA tracks, `anim_data.action` may become read-only in some states (tweak mode) — [devtalk: Blender 4.4 Slotted Actions feedback](https://devtalk.blender.org/t/blender-4-4-slotted-actions-feedback/38906) (user report, not official)

### Inferences
**Recommended script skeleton for Blender 5.x (verify in 5.3 before relying on it):**
```python
import bpy, math
arm_obj = bpy.data.objects["Rig"]          # armature object
adt = arm_obj.animation_data_create()

def make_action(name, frames, key_fn):
    act = bpy.data.actions.new(name)
    act.use_fake_user = True              # keep it alive when not assigned
    adt.action = act                      # auto-creates/assigns slot on first key
    for f in frames:
        key_fn(f)                         # sets pose_bone.rotation_* then keyframe_insert(..., frame=f)
    act.use_frame_range = True            # manual frame range (exported take length)
    act.frame_range = (frames[0], frames[-1])
    return act

# after all actions exist: one NLA track+strip per action
adt.action = None
for act in (idle, walk):
    tr = adt.nla_tracks.new(); tr.name = act.name
    st = tr.strips.new(act.name, int(act.frame_range[0]), act)
    st.name = act.name
    # st.action_slot is auto-assigned; set explicitly if needed
```
- Key with `pb.keyframe_insert("rotation_quaternion", frame=f)` or `"rotation_euler"` after setting `pb.rotation_mode`; set `pb.rotation_mode = 'XYZ'` before using Euler. Quaternion avoids gimbal issues and is what FBX bakes to anyway; Euler is easier to script for simple swings.
- **Loop seam:** key the last frame identical to the first (frame N = frame 0 copy) and have the clip range be `[0, N]`; in Unity set the clip range to exclude the duplicate (lastFrame = N−1... or keep N and rely on Loop Time) — either way make it consistent so `T` in the speed formula equals the true cycle length. Alternatively set interpolation to LINEAR at the seam for foot-plant keys.
- **Cycles F-modifier vs baking:** F-curve `Cycles` modifiers are evaluated by Blender and the FBX exporter bakes evaluated values (`bake_anim=True`), so modifiers are baked into the take over the action's frame range. Unity never sees the modifier. Simpler and more robust: key the full cycle explicitly and do not rely on modifiers.
- **Bone layout for Unity:** keep one root bone (e.g. `root` at the origin, deform off or on), parent pelvis/hips to it, deform bones only for skinned parts; name bones `.L/.R` consistently. Bone roll is preserved; Unity sees bone rotations in its own axis system.

### Gaps
- I could not read the full Blender 5.0/5.1/5.2 change logs; any 5.1–5.3 changes to `ActionSlot`, `NlaStrip` or the FBX exporter are unverified. Blender 5.3 specifically was not checked.
- No source checked whether `strips.new(...)` on an NLA track in 5.x requires the action to already have a matching slot, or what happens when one Action holds multiple slots.
- Whether `keyframe_insert` on pose bones in 5.x creates the slot with `target_id_type='OBJECT'` and names it after the object is stated for objects in the docs but not specifically re-verified for pose bones (the F-Curves live on the armature object's slot).

---

## 4. FBX exporter options and known pitfalls (Blender → Unity)

### Takeaway
Export with `bake_anim=True`, either `bake_anim_use_nla_strips=True` + `bake_anim_use_all_actions=False` (one take per NLA strip, named after the strip, avoiding the `Armature|Walk` prefix) or all actions (takes named `<Object>|<Action>`), `add_leaf_bones=False`, `use_armature_deform_only=True`, `apply_scale_options='FBX_SCALE_UNITS'` (or `FBX_SCALE_ALL`) with forward/up chosen to match Unity, and enable Bake Axis Conversion on the Unity side. Defaults in Blender are NOT Unity-friendly (`add_leaf_bones=True`, both NLA and all-actions on).

### Cited Findings
- `bpy.ops.export_scene.fbx` signature and defaults (installed Blender MCP bundled docs, matches current API): `global_scale=1.0, apply_unit_scale=True, apply_scale_options='FBX_SCALE_NONE', use_space_transform=True, bake_space_transform=False, object_types={'ARMATURE','CAMERA','EMPTY','LIGHT','MESH','OTHER'}, use_mesh_modifiers=True, add_leaf_bones=True, primary_bone_axis='Y', secondary_bone_axis='X', use_armature_deform_only=False, armature_nodetype='NULL', bake_anim=True, bake_anim_use_all_bones=True, bake_anim_use_nla_strips=True, bake_anim_use_all_actions=True, bake_anim_force_startend_keying=True, bake_anim_step=1.0, bake_anim_simplify_factor=1.0, axis_forward='-Z', axis_up='Y'` — [Blender API: bpy.ops.export_scene](https://docs.blender.org/api/current/bpy.ops.export_scene.html)
- Option semantics from the same doc: `apply_scale_options` — `FBX_SCALE_NONE` "Apply custom scaling and units scaling to each object transformation, FBX scale remains at 1.0"; `FBX_SCALE_UNITS` "Apply custom scaling to each object transformation, and units scaling to FBX scale"; `FBX_SCALE_CUSTOM`; `FBX_SCALE_ALL` "Apply custom scaling and units scaling to FBX scale". `bake_space_transform` = "WARNING! experimental option, use at own risk, known to be broken with armatures/animations". `use_mesh_modifiers` = "Apply modifiers to mesh objects (except Armature ones) - WARNING: prevents exporting shape keys". `add_leaf_bones` = "Append a final bone to the end of each chain ... (use this when you intend to edit the armature from exported data)". `use_armature_deform_only` = "Only write deforming bones (and non-deforming ones when they have deforming children)". `bake_anim_use_nla_strips` = "Export each non-muted NLA strip as a separated FBX's AnimStack". `bake_anim_use_all_actions` = "Export each action as a separated FBX's AnimStack ... animated objects will get all actions compatible with them, others will get no animation at all". `bake_anim_simplify_factor` = "How much to simplify baked values (0.0 to disable...)". `armature_nodetype` — use Null "unless you experience issues" — [Blender API: bpy.ops.export_scene](https://docs.blender.org/api/current/bpy.ops.export_scene.html)
- Practitioner Blender→Unity settings: Selected Objects; Object Types Mesh + Armature; Apply Scalings = "FBX Unit Scale"; Forward = -Y, Up = Z; Only Deform Bones on; Add Leaf Bones off; Bake Animation with **All Actions off** so only NLA strips export; in Unity enable Bake Axis Conversion and Loop Time. Put a `_TPose` NLA strip last so it becomes the default pose — [bkjam (2024)](https://bkjam.github.io/posts/2024-10-19-blender-to-unity-settings)
- Clip naming: exporting actions yields takes named `<ArmatureObject>|<ActionName>`; the pipe cannot simply be replaced in the exporter without producing an invalid FBX; putting animations on NLA strips and exporting NLA strips gives Unity clips without the prefix — [Blender addons #72680: Vertical bar in action names on FBX export](https://projects.blender.org/blender/blender-addons/issues/72680)
- Historic bug: NLA-strip export produced correctly named takes that all contained the same animation (2.77a); later status unconfirmed — [Blender T48631](https://developer.blender.org/T48631)
- Unchecking "All Actions" can lose global scene animation in some setups; Unity's tracker also reports missing clips when "export all actions" was off in a Blender script — [Blender addons #72995](https://projects.blender.org/blender/blender-addons/issues/72995); [Unity Issue Tracker: not all clips imported from .blend](https://issuetracker.unity3d.com/issues/using-multiple-animation-clips-in-blender-not-all-animation-clips-are-imported-using-a-blend-file?page=2)
- The armature object appears in Unity as an extra parent transform above the root bone ("Armature root" questions) — [Unity Discussions: Blender FBX export, Armature root missing](https://discussions.unity.com/t/blender-fbx-export-armature-root-missing-in-unity/924714)
- Unity `ModelImporter.bakeAxisConversion` converts axis on geometry and animation for files authored in another axis system; `useFileUnits` / `useFileScale` / `globalScale` control unit handling — [Unity 6.3 ModelImporter](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/modelimporter)

### Inferences
- **Recommended export call (Blender 5.x, verify):**
  ```python
  bpy.ops.export_scene.fbx(
      filepath=out, use_selection=True, object_types={'ARMATURE', 'MESH'},
      apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',   # or 'FBX_SCALE_ALL'
      axis_forward='-Z', axis_up='Y', bake_space_transform=False,
      use_mesh_modifiers=True, add_leaf_bones=False,
      primary_bone_axis='Y', secondary_bone_axis='X',
      use_armature_deform_only=True, armature_nodetype='NULL',
      bake_anim=True, bake_anim_use_all_bones=True,
      bake_anim_use_nla_strips=True, bake_anim_use_all_actions=False,
      bake_anim_force_startend_keying=True, bake_anim_step=1.0,
      bake_anim_simplify_factor=0.0)   # 0 = no simplification; avoids tiny-rig jitter
  ```
  Then in Unity: Bake Axis Conversion on, Convert Units on, check the root has scale 1 and rotation 0.
- **Axis choice conflict:** sources disagree in practice — bkjam uses `-Y forward, Z up` + Bake Axis Conversion; the exporter default is `-Z forward, Y up`. With Bake Axis Conversion enabled Unity resolves the axis itself; without it, the common symptom is a root rotated -90° on X. Pick one combination, export a test cube-with-bone, and check rotation (0,0,0) and forward = +Z in Unity. Character should face Blender −Y (Blender "front" view) to face Unity +Z.
- **Scale 100 pitfall:** with `FBX_SCALE_NONE` + metric units, the FBX unit scale stays 1 cm-based in some importers, so Unity shows the root at scale 100 (or the mesh 100× smaller). `FBX_SCALE_UNITS` / `FBX_SCALE_ALL` writes the unit into the FBX scale so Unity's "Convert Units" yields scale 1. Also apply object scale in Blender (`Ctrl+A → Scale`) to the armature and mesh before export, otherwise the scale is baked into bone animation or the root transform.
- **Bone axis surprises:** Blender bones point along local +Y; Unity's convention is different, and `primary_bone_axis`/`secondary_bone_axis` only change how bone orientations are written. Keep the defaults (Y, X) for Generic rigs and never hand-rotate bones in Unity scripts assuming Blender axes — animate everything in Blender. Changing bone axes re-orients bone local spaces, which breaks any Unity code that rotates a bone by a fixed local axis.
- **Extra transform:** the armature object becomes a GameObject (e.g. `Rig`) above the root bone. Keep its transform identity (location 0, rotation 0, scale 1 in Blender after applying transforms). If root motion were used, the motion node would need to be set explicitly (`motionNodeName`); with in-place clips this is irrelevant.
- **Names:** with NLA export, Unity clip names = NLA strip names (set `strip.name = "Walk"`). With all-actions export, clips are `Rig|Walk`; rename via AssetPostprocessor or in the clip list. Avoid both sources being active at once (default exporter settings have both on, which can duplicate takes).
- **Jitter on small rigs:** set `bake_anim_simplify_factor=0.0` in Blender and lower Unity's rotation/position error (or compression Off). Small characters (e.g. 1 m tall, bones of a few cm) make percentage-based position error and the simplify factor proportionally more visible.
- **Mesh modifiers before skinning:** with `use_mesh_modifiers=True`, all non-Armature modifiers (Mirror, Subdivision, Bevel, Solidify) are applied in stack order at export; the Armature modifier itself is exported as skinning. Shape keys are lost when this is on. Modifiers placed *after* the Armature modifier in the stack (e.g. Subdivision after Armature) are still applied to the bind pose, not to the deformed result — order them before the Armature modifier for predictable results, or apply them manually.
- **Export only what you need:** `use_selection=True` with only the armature and its mesh selected keeps cameras/lights/empties out of the FBX; empties would otherwise become extra GameObjects.

### Gaps
- No current (Blender 5.x) confirmation that the NLA-strip naming behaviour (no `Armature|` prefix) and the T48631 "same animation in every take" bug are resolved; must be checked by exporting a two-strip test file in Blender 5.3 and inspecting the takes in Unity.
- Did not find an authoritative source for the exact cause/numbers of the "scale 100" issue; explanation above is from prior knowledge.
- Did not verify how the FBX exporter handles an Action with multiple slots, or whether `bake_anim_use_all_actions` filters actions by slot `target_id_type` in 5.x.
