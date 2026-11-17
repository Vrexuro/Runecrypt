# M1 Prototipe Inti: Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Satu ruangan abu-abu yang bisa dimainkan. Pyromancer menembakkan api ke Tulang Penjaga dan ke Genangan Beku (es), memicu Uap, dan aturan cahaya sudah berlaku. Tujuannya menguji apakah inti game seru, sekaligus menguji Light2D dan ShadowCaster2D dengan sprite prototipe.

**Architecture:** Logika murni (reaksi, health, cooldown, status Terbakar, aturan cahaya, `LightField`, pemilihan arah bidik) ada di assembly `Runecrypt.Core` tanpa MonoBehaviour, dan diuji dengan EditMode test. MonoBehaviour tipis di `Runecrypt.Game` menghubungkan logika itu ke physics, Input System, dan Light2D. Konten berupa ScriptableObject (`ReactionDefinition`, `EnemyDefinition`, `LightRules`) supaya bisa ditambah tanpa mengubah kode (GDD §12, Pendekatan A).

**Tech Stack:** Unity 6000.3.26f1, URP 17.3 2D Renderer (`Light2D`, `ShadowCaster2D`, `PixelPerfectCamera`), Input System 1.20, Tilemap, Unity Test Framework 1.6 (EditMode), Python 3.12 + Pillow untuk sprite. Unity dioperasikan lewat MCP for Unity (skill `unity-mcp-skill`).

**Spec:** `Docs/GDD.md` versi 0.1, terutama §4.1 (Pyromancer), §5.1 (reaksi), §5.4 (status), §5.5 (aturan cahaya), §6.1 (Tulang Penjaga), §7.1 (visual), §8.1 (kontrol), §12.

## Global Constraints

- Unity 6000.3.26f1. Tidak menambah package, asset pack, atau dependensi baru.
- Semua sprite dibuat lewat script Python di `ArtSource/scripts/`. Output ke `ArtSource/prototype/`, lalu disalin ke `Assets/Runecrypt/Art/Prototype/`. PNG hasil generate tidak diedit dengan tangan.
- Sprite: 16 pixels per unit (1 tile = 1 unit), Filter Mode Point, Compression None.
- Kamera: `PixelPerfectCamera`, reference resolution 320x180, PPU 16.
- Input hanya lewat Input System (tanpa `UnityEngine.Input` lama).
- Grafis kualitas penuh, termasuk ShadowCaster2D, tanpa mode kualitas rendah. Target 60 FPS di laptop pengembang.
- Identifier kode dalam bahasa Inggris dengan namespace `Runecrypt.Core` dan `Runecrypt.Game`. Teks yang tampil ke pemain dalam bahasa Indonesia.
- Angka dari GDD: HP pemain 6, gerak 5 tile/detik, Bola Api damage 3, 3 tembakan/detik, jangkauan 7 tile, Terbakar 1 damage/detik selama 2 detik, dash 3 tile, cooldown 0,8 detik, i-frame 0,2 detik, cahaya pemain radius 3 tile. Uap: radius 2,5 tile, 3 detik, 2 damage per 0,5 detik, membutakan. Genangan Beku muncul lagi setelah 8 detik. Tulang Penjaga HP 12. Tersingkap: damage +25%. Gelap: kecepatan musuh +20%. Reaksi tidak melukai pemain.
- Angka yang tidak ada di GDD dan diputuskan di rencana ini: Tulang Penjaga bergerak 2 tile/detik dengan damage sentuh 1 per 1 detik, Bola Api melaju 12 tile/detik, cahaya Bola Api radius 1 tile, dan musuh yang Terbakar bercahaya dengan radius 0,75 tile.
- Setiap operasi Unity Editor memakai skill `unity-mcp-skill`. Setelah menulis script, tunggu `is_compiling == false`, lalu jalankan `read_console` untuk mengecek error.

## Review Focus

1. **Kursor tepat di atas pemain.** Arah bidik tidak boleh menjadi NaN atau nol. Arah terakhir dipakai. Ditest di Task 5 (`Select_MouseOnPlayer_KeepsLastAim`).
2. **Radius visual dan radius aturan berbeda.** Kalau radius sumber cahaya diubah saat runtime, `Light2D` harus ikut berubah. Ditest di Task 6 (`SettingRadius_UpdatesLight2D`).
3. **Tembakan cepat menumpuk Terbakar.** Dengan 3 tembakan/detik, damage Terbakar tetap 1 per detik dan durasinya hanya diperbarui. Ditest di Task 3 (`Reapply_RefreshesDurationWithoutStackingTicks`).
4. **Beberapa bola api mengenai Genangan Beku di frame yang sama.** Hanya satu Uap yang muncul. Ditest di Task 3 (`Start_MakesNotReadyImmediately`) dan dipakai di Task 9.
5. **Damage setelah mati**, misalnya dari tick Uap atau Terbakar. Event `Died` hanya terpanggil sekali. Ditest di Task 2 (`TakeDamage_AfterDeath_IsIgnored`).

---

## Struktur File

```
Assets/Runecrypt/
  Scripts/Core/            Runecrypt.Core.asmdef (logika murni)
    Element.cs  HitInfo.cs  Health.cs  Cooldown.cs  BurnStatus.cs
    ReactionDefinition.cs  ReactionResolver.cs  LightRules.cs
    LightField.cs (berisi ILightSource)  AimSelector.cs
    EnemyDefinition.cs
  Scripts/Game/            Runecrypt.Game.asmdef (MonoBehaviour)
    IDamageable.cs  LightFieldHost.cs  LightSourceBehaviour.cs
    PlayerController.cs  Fireball.cs  Enemy.cs
    FrostPuddle.cs  SteamCloud.cs  ArenaDirector.cs
  Tests/EditMode/          Runecrypt.Tests.EditMode.asmdef
    ReactionTests.cs  HealthTests.cs  TimingTests.cs
    LightTests.cs  AimSelectorTests.cs  LightSourceBehaviourTests.cs
  Input/RunecryptControls.inputactions (+ RunecryptControls.cs hasil generate)
  Data/  LightRules.asset  Reactions/Uap.asset  Enemies/TulangPenjaga.asset
  Prefabs/  Player  Fireball  TulangPenjaga  FrostPuddle  SteamCloud
  Art/Prototype/  (salinan PNG dari ArtSource/prototype)
  Scenes/M1_Prototype.unity
ArtSource/scripts/make_proto_fx.py
```

---

### Task 0: Repo git

**Files:**
- Create: `.gitignore`, `.gitattributes`

- [ ] **Step 1:** Pakai skill `git-workflow-and-versioning`. Jalankan `git init -b main` di `D:\Claude\PGIM\Runecrypt`.
- [ ] **Step 2:** Isi `.gitignore` dengan: `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, `UserSettings/`, `MemoryCaptures/`, `.vs/`, `.idea/`, `*.csproj`, `*.sln`, `*.slnx`, `*.user`, `*.pidb`, `*.suo`, `*.tmp`, `crashlytics-build.properties`. Isi `.gitattributes` dengan `* text=auto` dan `*.png binary`.
- [ ] **Step 3:** Jalankan `git status`. Expected: hanya `Assets/`, `Packages/`, `ProjectSettings/`, `ArtSource/`, `Docs/`, `CLAUDE.md`, `.gitignore`, dan `.gitattributes` yang muncul. Tidak ada `Library/` atau `*.csproj`.
- [ ] **Step 4:** Commit `chore: init repo Unity Runecrypt`. Perbarui baris "Project ini belum menjadi repo git" di `CLAUDE.md` menjadi "Repo git lokal, branch `main`".

### Task 1: Assembly, Element, dan ReactionResolver

**Files:**
- Create: `Assets/Runecrypt/Scripts/Core/Runecrypt.Core.asmdef` (name `Runecrypt.Core`, tanpa reference)
- Create: `Assets/Runecrypt/Scripts/Game/Runecrypt.Game.asmdef` (reference: `Runecrypt.Core`, `Unity.InputSystem`, `Unity.RenderPipelines.Universal.Runtime`, `Unity.RenderPipelines.Core.Runtime`)
- Create: `Assets/Runecrypt/Tests/EditMode/Runecrypt.Tests.EditMode.asmdef` (reference: `Runecrypt.Core`, `Runecrypt.Game`, `Unity.RenderPipelines.Universal.Runtime`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`; `includePlatforms: ["Editor"]`, `overrideReferences: true`, `precompiledReferences: ["nunit.framework.dll"]`, `defineConstraints: ["UNITY_INCLUDE_TESTS"]`)
- Create: `Core/Element.cs`, `Core/HitInfo.cs`, `Core/ReactionDefinition.cs`, `Core/ReactionResolver.cs`
- Test: `Tests/EditMode/ReactionTests.cs`

**Interfaces:**
- Produces: `enum Element { None, Fire, Ice, Lightning }`
- Produces: `struct HitInfo { float Damage; Element Element; bool AppliesBurn; Vector2 Point; }` with a constructor that takes all four.
- Produces: `[CreateAssetMenu(menuName = "Runecrypt/Reaction")] class ReactionDefinition : ScriptableObject` with public fields `string displayName; Element elementA; Element elementB; float radius; float duration; float damagePerTick; float tickInterval; float blindDuration; GameObject effectPrefab;` and method `bool Matches(Element x, Element y)`, which ignores order.
- Produces: `static ReactionDefinition ReactionResolver.Find(Element x, Element y, IReadOnlyList<ReactionDefinition> book)`. Returns `null` when there is no match or when `x == y`.

- [ ] **Step 1: Write the failing tests.** In `ReactionTests`, build the definitions with `ScriptableObject.CreateInstance<ReactionDefinition>()`. Uap = (Fire, Ice) and Ledakan = (Fire, Lightning).
  ```csharp
  Assert.AreSame(uap, ReactionResolver.Find(Element.Fire, Element.Ice, book));      // Find_FireIce_ReturnsUap
  Assert.AreSame(uap, ReactionResolver.Find(Element.Ice, Element.Fire, book));      // Find_IsOrderIndependent
  Assert.IsNull(ReactionResolver.Find(Element.Ice, Element.Lightning, book));       // Find_NoRule_ReturnsNull (Shatter belum ada)
  Assert.IsNull(ReactionResolver.Find(Element.Fire, Element.Fire, book));           // Find_SameElement_ReturnsNull
  Assert.IsNull(ReactionResolver.Find(Element.Fire, Element.None, book));           // Find_None_ReturnsNull
  ```
- [ ] **Step 2:** Run `run_tests(mode="EditMode")` and then `get_test_job`. Expected: compile error or FAIL, because the types do not exist yet.
- [ ] **Step 3:** Implement the four Core files and the three asmdefs.
- [ ] **Step 4:** Wait for `is_compiling == false`, run `read_console(types=["error"])`, then run the tests. Expected: the console has no errors, and all 5 tests PASS.
- [ ] **Step 5:** Commit `feat(core): element dan resolver reaksi berbasis data`.

### Task 2: Health dan LightRules

**Files:**
- Create: `Core/Health.cs`, `Core/LightRules.cs`
- Test: `Tests/EditMode/HealthTests.cs`

**Interfaces:**
- Produces: `class Health` with constructor `Health(float max)`, properties `float Max`, `float Current`, and `bool IsDead`, event `event Action Died`, and method `void TakeDamage(float amount)`. A negative amount is ignored, `Current` is clamped at 0, and `Died` fires once.
- Produces: `[CreateAssetMenu(menuName = "Runecrypt/Light Rules")] class LightRules : ScriptableObject` with fields `float exposedDamageBonus = 0.25f; float darkSpeedBonus = 0.2f;` and methods `float ApplyDamage(float baseDamage, bool lit)` and `float SpeedMultiplier(bool lit)`.

- [ ] **Step 1: Write the failing tests:**
  ```csharp
  var h = new Health(6); h.TakeDamage(1); Assert.AreEqual(5f, h.Current);            // TakeDamage_ReducesCurrent
  h = new Health(12); int died = 0; h.Died += () => died++;
  h.TakeDamage(20); Assert.AreEqual(0f, h.Current); Assert.IsTrue(h.IsDead);          // TakeDamage_Overkill_ClampsAtZero
  h.TakeDamage(3); Assert.AreEqual(1, died);                                          // TakeDamage_AfterDeath_IsIgnored
  h = new Health(6); h.TakeDamage(-2); Assert.AreEqual(6f, h.Current);                // TakeDamage_Negative_IsIgnored
  var r = ScriptableObject.CreateInstance<LightRules>();
  Assert.AreEqual(3.75f, r.ApplyDamage(3, true), 1e-4);                               // ApplyDamage_Lit_Adds25Percent
  Assert.AreEqual(3f, r.ApplyDamage(3, false), 1e-4);                                 // ApplyDamage_Dark_Unchanged
  Assert.AreEqual(1.2f, r.SpeedMultiplier(false), 1e-4);                              // SpeedMultiplier_Dark_Is120Percent
  Assert.AreEqual(1f, r.SpeedMultiplier(true), 1e-4);                                 // SpeedMultiplier_Lit_Is100Percent
  ```
- [ ] **Step 2:** Run the tests. Expected: FAIL.
- [ ] **Step 3:** Implement `Health` and `LightRules`. Damage is a float so the +25% bonus never vanishes to rounding (3 → 3,75). Rounding happens only in the damage number shown to the player.
- [ ] **Step 4:** Run the tests. Expected: all PASS.
- [ ] **Step 5:** Commit `feat(core): health dan aturan cahaya`.

### Task 3: Cooldown dan BurnStatus

**Files:**
- Create: `Core/Cooldown.cs`, `Core/BurnStatus.cs`
- Test: `Tests/EditMode/TimingTests.cs`

**Interfaces:**
- Produces: `class Cooldown` with constructor `Cooldown(float seconds)`, property `float Seconds { get; set; }`, and methods `bool IsReady(float now)`, `void Start(float now)`, and `float Remaining(float now)`. A new cooldown starts ready. Time is always passed in, never read from `Time.time`.
- Produces: `class BurnStatus` with constructor `BurnStatus(float damagePerTick = 1f, float duration = 2f, float tickInterval = 1f)` and methods `void Apply(float now)`, `bool IsActive(float now)`, and `float Tick(float now)`. `Tick` returns the total damage from every tick that is due and has not been paid out yet. A tick at time t counts only if `t <= end`.

- [ ] **Step 1: Write the failing tests:**
  ```csharp
  var c = new Cooldown(0.8f); Assert.IsTrue(c.IsReady(0));                             // New_IsReady
  c.Start(1f); Assert.IsFalse(c.IsReady(1f));                                          // Start_MakesNotReadyImmediately
  Assert.IsFalse(c.IsReady(1.79f)); Assert.IsTrue(c.IsReady(1.8f));                    // IsReady_AfterSeconds
  Assert.AreEqual(0.3f, c.Remaining(1.5f), 1e-4);                                      // Remaining
  var b = new BurnStatus(); b.Apply(0);
  Assert.AreEqual(0f, b.Tick(0.9f)); Assert.AreEqual(1f, b.Tick(1f));                  // Tick_PaysOncePerSecond
  Assert.AreEqual(1f, b.Tick(2.5f)); Assert.AreEqual(0f, b.Tick(5f));                  // Tick_StopsAtDuration
  Assert.IsFalse(b.IsActive(2.01f));                                                   // IsActive_EndsAfterDuration
  b = new BurnStatus(); b.Apply(0); b.Apply(0.33f); b.Apply(0.66f); b.Apply(1.5f);
  Assert.AreEqual(3f, b.Tick(4f));                                                     // Reapply_RefreshesDurationWithoutStackingTicks (ticks at 1, 2, 3; end 3.5)
  ```
- [ ] **Step 2:** Run the tests. Expected: FAIL.
- [ ] **Step 3:** Implement both classes. `Apply` while the burn is already active only moves `end` forward and does not reset the next tick.
- [ ] **Step 4:** Run the tests. Expected: all PASS.
- [ ] **Step 5:** Commit `feat(core): cooldown dan status Terbakar`.

### Task 4: LightField

**Files:**
- Create: `Core/LightField.cs`
- Test: `Tests/EditMode/LightTests.cs`

**Interfaces:**
- Produces: `interface ILightSource { Vector2 Position { get; } float Radius { get; } bool IsOn { get; } }`
- Produces: `class LightField` with constructor `LightField(Func<Vector2, Vector2, bool> isBlocked)`, where `isBlocked(from, to)` returns true when a wall lies between the two points, and methods `void Register(ILightSource s)`, `void Unregister(ILightSource s)`, and `bool IsLit(Vector2 point)`. A point is lit when at least one source has `IsOn`, `distance <= Radius`, and `!isBlocked(source.Position, point)`. Registering the same source twice adds it only once.

- [ ] **Step 1: Write the failing tests.** Use a `FakeSource` class inside the test and an `isBlocked` lambda that is configurable per test.
  ```csharp
  // source at (0,0), radius 3, no walls
  Assert.IsTrue(field.IsLit(new Vector2(2, 0)));                                       // IsLit_InsideRadius
  Assert.IsTrue(field.IsLit(new Vector2(3, 0)));                                       // IsLit_OnEdge
  Assert.IsFalse(field.IsLit(new Vector2(3.01f, 0)));                                  // IsLit_OutsideRadius_False
  // isBlocked = (a, b) => true
  Assert.IsFalse(blockedField.IsLit(new Vector2(1, 0)));                               // IsLit_BehindWall_False
  // source.IsOn = false
  Assert.IsFalse(field.IsLit(new Vector2(1, 0)));                                      // IsLit_SourceOff_False
  // Unregister(source)
  Assert.IsFalse(field.IsLit(new Vector2(1, 0)));                                      // Unregister_RemovesSource
  // two sources: A at (0,0) r1, B at (10,0) r2 → (9,0) lit, (5,0) not lit
  ```
- [ ] **Step 2:** Run the tests. Expected: FAIL.
- [ ] **Step 3:** Implement `LightField`, storing the sources in a `List<ILightSource>`. Add the comment `// shortcut: linear scan, upgrade to a spatial grid if > 64 lights`.
- [ ] **Step 4:** Run the tests. Expected: all PASS.
- [ ] **Step 5:** Commit `feat(core): LightField untuk aturan cahaya`.

### Task 5: AimSelector dan Input

**Files:**
- Create: `Core/AimSelector.cs`
- Create: `Assets/Runecrypt/Input/RunecryptControls.inputactions`, with **Generate C# Class** turned on, class `RunecryptControls`, namespace `Runecrypt.Game`
- Test: `Tests/EditMode/AimSelectorTests.cs`

**Interfaces:**
- Produces: `readonly struct AimResult { Vector2 Direction; bool AutoFire; }`
- Produces: `static AimResult AimSelector.Select(Vector2 stick, Vector2 keys, Vector2 mouseWorld, Vector2 playerPos, Vector2 lastAim, float deadzone = 0.2f)`. Priority order: (1) the right stick, when its magnitude is above `deadzone`, sets the direction to the normalized stick and `AutoFire = true`; (2) the arrow keys, when non-zero, set the direction to the normalized keys and `AutoFire = true`; (3) otherwise the mouse sets the direction to normalized `(mouseWorld - playerPos)` and `AutoFire = false`. If the mouse distance is below 0.01, `lastAim` is kept.
- Produces, in the `Gameplay` action map: `Move` (Vector2: WASD composite in DigitalNormalized mode plus the left stick), `AimPoint` (Vector2: `<Mouse>/position`), `AimStick` (Vector2: right stick), `AimKeys` (Vector2: arrow composite, DigitalNormalized), `Fire` (Button: left mouse button), `Dash` (Button: Space, `<Gamepad>/buttonSouth`), and `Restart` (Button: R, `<Gamepad>/start`). This follows GDD §8.1. Kobaran and Jeda are left for M2 and M5.

- [ ] **Step 1: Write the failing tests:**
  ```csharp
  var r = AimSelector.Select(new(1,0), new(0,1), new(5,5), Vector2.zero, Vector2.up);
  Assert.AreEqual(Vector2.right, r.Direction); Assert.IsTrue(r.AutoFire);              // Select_StickWins
  r = AimSelector.Select(new(0.1f,0), new(0,1), new(5,5), Vector2.zero, Vector2.right);
  Assert.AreEqual(Vector2.up, r.Direction); Assert.IsTrue(r.AutoFire);                 // Select_KeysWhenStickInDeadzone
  r = AimSelector.Select(Vector2.zero, Vector2.zero, new(0,4), Vector2.zero, Vector2.right);
  Assert.AreEqual(Vector2.up, r.Direction); Assert.IsFalse(r.AutoFire);                // Select_MouseFallback
  r = AimSelector.Select(Vector2.zero, Vector2.zero, new(2,2), new(2,2), Vector2.left);
  Assert.AreEqual(Vector2.left, r.Direction);                                          // Select_MouseOnPlayer_KeepsLastAim
  ```
- [ ] **Step 2:** Run the tests. Expected: FAIL.
- [ ] **Step 3:** Implement `AimSelector`, then create the `.inputactions` asset through MCP (`manage_asset`). Turn on C# class generation in the importer.
- [ ] **Step 4:** Wait for compilation, run `read_console`, then run the tests. Expected: no errors, the tests PASS, and `RunecryptControls.cs` exists.
- [ ] **Step 5:** Verify in Player Settings that Active Input Handling is "Input System Package (New)". Change it only if it is not.
- [ ] **Step 6:** Commit `feat: pemilihan arah bidik dan skema input`.

### Task 6: Sprite prototipe dan sumber cahaya

**Files:**
- Create: `ArtSource/scripts/make_proto_fx.py`. It produces `fireball.png` (16x16, a palette `y`/`O`/`o` core about 6 px wide), `frost_puddle.png` (16x16, pale blue `#9FD3E6` and `#5E8FB0`), `steam_puff.png` (16x16, soft white `w` with a transparent edge), and `skeleton_eyes_0..3.png` (only the `r` pixels taken from `skeleton_0..3.png`, at the same positions).
- Copy: `ArtSource/prototype/{mage_0..3,skeleton_0..3,skeleton_eyes_0..3,floor,wall,fireball,frost_puddle,steam_puff}.png` to `Assets/Runecrypt/Art/Prototype/`
- Create: `Game/LightFieldHost.cs`, `Game/LightSourceBehaviour.cs`
- Test: `Tests/EditMode/LightSourceBehaviourTests.cs`

**Interfaces:**
- Consumes: `LightField` and `ILightSource` from Task 4.
- Produces: `class LightFieldHost : MonoBehaviour`, with `static LightField Field { get; }` (created in `Awake`) and field `LayerMask wallMask`. `isBlocked` is `Physics2D.Linecast(a, b, wallMask).collider != null`.
- Produces: `class LightSourceBehaviour : MonoBehaviour, ILightSource`, with `[SerializeField] float radius` and `[SerializeField] Light2D light2D`. The property `float Radius { get; set; }` is the only way to change the radius, and its setter also writes `light2D.pointLightOuterRadius`. It is applied as well in `Awake` and `OnValidate`. `IsOn` is `isActiveAndEnabled`. It registers itself in `OnEnable` and unregisters in `OnDisable` (when `LightFieldHost.Field` is not null). `OnDrawGizmosSelected` draws a circle of the rule radius.

- [ ] **Step 1:** Write `make_proto_fx.py` using the palette from `make_sprites.py`. Run `python ArtSource/scripts/make_proto_fx.py`. Expected: the 7 new PNG files exist in `ArtSource/prototype/`.
- [ ] **Step 2:** Copy the PNG files into Assets, then set the import settings through MCP (`manage_texture` or `manage_asset`): Sprite (Single), PPU 16, Point, Compression None. Expected: the console has no errors.
- [ ] **Step 3: Write the failing test.** Create a GameObject with `Light2D` (type Point) and `LightSourceBehaviour`, then set `Radius = 4.5f`.
  ```csharp
  Assert.AreEqual(4.5f, light.pointLightOuterRadius, 1e-4);                           // SettingRadius_UpdatesLight2D
  Assert.AreEqual(4.5f, ((ILightSource)src).Radius, 1e-4);
  ```
  Before writing the test, verify the API with `unity_reflect` (type `UnityEngine.Rendering.Universal.Light2D`, member `pointLightOuterRadius`).
- [ ] **Step 4:** Run the test. Expected: FAIL.
- [ ] **Step 5:** Implement `LightFieldHost` and `LightSourceBehaviour`.
- [ ] **Step 6:** Run all the tests. Expected: they PASS.
- [ ] **Step 7:** Commit `feat: sprite efek prototipe dan sumber cahaya yang sinkron dengan Light2D`.

### Task 7: Ruangan abu-abu dan uji cahaya

**Files:**
- Create: `Assets/Runecrypt/Scenes/M1_Prototype.unity`
- Create: layers `Walls`, `Enemies`, `Player`, `Projectiles`, `Reactives` (through `manage_editor`)

- [ ] **Step 1:** Create the scene and add it to Build Settings. Main Camera gets a `PixelPerfectCamera` (reference 320x180, PPU 16) and an orthographic size that fits 20x11 tiles. Add a `LightFieldHost` with `wallMask = Walls`.
- [ ] **Step 2:** Build the room with Tilemaps. `Floor` is 20x11 using `floor.png`. `Walls` is a border around the room plus 2 pillars of 2x2 tiles each, using `wall.png`, on layer `Walls`, with `TilemapCollider2D`, `CompositeCollider2D`, and a static `Rigidbody2D`. Add `ShadowCaster2D` to the walls. Before choosing how, check with `unity_reflect` whether `ShadowCaster2D` in URP 17.3 has a casting source from Collider2D or Tilemap. If it does, use it on the wall Tilemap. If it does not, place a `ShadowCaster2D` on one GameObject per pillar and per wall side.
- [ ] **Step 3:** Set up the lights. Global `Light2D` intensity 0.08, color `#342852`. A temporary point light at the room center with radius 3, color `#FF8A3D`, intensity 1, shadows on, shadow intensity 0.9.
- [ ] **Step 4:** Place 1 skeleton sprite in the light and 1 in the dark. The `skeleton_eyes` child uses the `Sprite-Unlit-Default` material and sorting order +1. Take a screenshot with `manage_camera(action="screenshot", include_image=True)`. Expected: the room is almost black; the lit area is a warm circle; the pillars cast shadows; the skeleton in the dark shows only its 2 red eyes. Show the screenshot to the user.
- [ ] **Step 5:** Commit `feat: ruangan prototipe dan uji Light2D + ShadowCaster2D`.

### Task 8: Pyromancer, Bola Api, dan Tulang Penjaga

**Files:**
- Create: `Game/IDamageable.cs`, `Game/PlayerController.cs`, `Game/Fireball.cs`, `Game/Enemy.cs`, `Core/EnemyDefinition.cs`
- Create: `Data/LightRules.asset`, `Data/Enemies/TulangPenjaga.asset`, `Prefabs/Player.prefab`, `Prefabs/Fireball.prefab`, `Prefabs/TulangPenjaga.prefab`

**Interfaces:**
- Consumes: `Health`, `Cooldown`, `BurnStatus`, `LightRules`, `AimSelector`, `HitInfo`, `RunecryptControls`, `LightFieldHost.Field`, and `LightSourceBehaviour`.
- Produces: `interface IDamageable { void TakeHit(HitInfo hit); }`
- Produces: `[CreateAssetMenu(menuName = "Runecrypt/Enemy")] class EnemyDefinition : ScriptableObject` with fields `string displayName; float maxHp; float moveSpeed; float contactDamage; float contactInterval; Element element;`. TulangPenjaga: 12, 2, 1, 1, None.
- Produces: `PlayerController : MonoBehaviour, IDamageable` (Rigidbody2D, layer `Player`), with `Health Health { get; }` and `event Action Died`. During the i-frame, `TakeHit` is ignored. Its light is a child `LightSourceBehaviour` with radius 3.
- Produces: `Fireball : MonoBehaviour` with the method `void Launch(Vector2 direction)`. Speed 12, range 7 tiles, then destroyed. When it hits an `IDamageable` on layer `Enemies` or `Reactives`, it calls `TakeHit(new HitInfo(3, Element.Fire, true, point))` and is destroyed. Hitting `Walls` destroys it. It carries a child `LightSourceBehaviour` with radius 1, color `#FFCC66`.
- Produces: `Enemy : MonoBehaviour, IDamageable`, with `EnemyDefinition definition`, `LightRules lightRules`, and the method `void Blind(float until)`. Each frame: `lit = LightFieldHost.Field.IsLit(position)`. Move speed is `definition.moveSpeed * lightRules.SpeedMultiplier(lit)` toward the player, or in a random direction (changed every 0.5 s) while blind. `TakeHit` reduces health by `lightRules.ApplyDamage(hit.Damage, lit)` and calls `burn.Apply(now)` when `AppliesBurn` is set. While burning, a child `LightSourceBehaviour` with radius 0.75 is active. On contact with the player it deals `contactDamage` once per `contactInterval`. When it dies it is destroyed and raises the event `static event Action<Enemy> AnyDied`.

- [ ] **Step 1:** Write the 5 scripts. Wait for compilation, then run `read_console`. Expected: 0 errors.
- [ ] **Step 2:** Create the data assets and the 3 prefabs. Player uses `mage_0..3` with a simple 4-frame loop animation, and TulangPenjaga uses `skeleton_0..3` + `skeleton_eyes_0..3`. Remove the temporary point light from Task 7.
- [ ] **Step 3:** In Physics2D settings, turn off collisions for Projectiles↔Player, Projectiles↔Projectiles, and Enemies↔Reactives.
- [ ] **Step 4:** Run the EditMode tests again. Expected: all PASS.
- [ ] **Step 5:** Manual check in Play mode (MCP `manage_editor play`, screenshot):
  - WASD moves at 5 tiles/s, and diagonal movement is not faster.
  - Holding the left mouse button fires 3 shots per second toward the cursor; the arrow keys fire automatically.
  - Space dashes 3 tiles.
  - The Tulang Penjaga moves faster in the dark (compare with the gizmo).
  - Damage per Bola Api hit is 3.75 on a lit enemy and 3 on an enemy in the dark. Shot counts cannot show this, because both die in 4 shots, so check it with a temporary `Debug.Log` of the damage per hit in Play mode, then delete the log.
- [ ] **Step 6:** Commit `feat: Pyromancer, Bola Api, dan Tulang Penjaga dengan aturan cahaya`.

### Task 9: Genangan Beku dan Uap

**Files:**
- Create: `Game/FrostPuddle.cs`, `Game/SteamCloud.cs`
- Create: `Data/Reactions/Uap.asset` (Fire+Ice, radius 2.5, duration 3, damagePerTick 2, tickInterval 0.5, blindDuration 0.6, effectPrefab SteamCloud), `Prefabs/FrostPuddle.prefab`, `Prefabs/SteamCloud.prefab`

**Interfaces:**
- Consumes: `ReactionResolver.Find`, `ReactionDefinition`, `Cooldown`, `IDamageable`, `Enemy.Blind`, and `LightSourceBehaviour`.
- Produces: `FrostPuddle : MonoBehaviour, IDamageable` (layer `Reactives`, trigger collider) with fields `Element element = Ice`, `List<ReactionDefinition> reactionBook`, and `float respawnSeconds = 8`. In `TakeHit`: if the cooldown is not ready, or `Find(hit.Element, element, book)` is null, return. Otherwise call `cooldown.Start(now)`, instantiate `effectPrefab` at this position, call `SteamCloud.Init(def)`, and hide the sprite and collider. When the cooldown is ready again, show them again.
- Produces: `SteamCloud : MonoBehaviour` with `void Init(ReactionDefinition def)`. Every `tickInterval` it runs `Physics2D.OverlapCircleAll(pos, radius, Enemies)` and, for each `Enemy`, calls `TakeHit(new HitInfo(damagePerTick, Element.None, false, pos))` and `Blind(now + blindDuration)`. It is destroyed after `duration`. It carries a `LightSourceBehaviour` with `Radius = def.radius`, color `#D9D1BD`, and a particle system made from `steam_puff.png`. The player is never part of the overlap layer, because reactions do not hurt the player.

- [ ] **Step 1:** Write the 2 scripts, compile, and run `read_console`. Expected: 0 errors.
- [ ] **Step 2:** Create the assets and prefabs, then place 2 FrostPuddles in the room.
- [ ] **Step 3:** Manual check in Play mode:
  - Shooting a puddle brings up a steam cloud that is bright for 3 s.
  - Enemies inside it take damage, wander randomly, and are Tersingkap (lit by the steam's light).
  - The puddle disappears and comes back after 8 s.
  - Holding fire at the puddle produces only 1 cloud.
  - The player is not hurt by the steam.
  - Take a screenshot.
- [ ] **Step 4:** Commit `feat: Genangan Beku dan reaksi Uap`.

### Task 10: Loop arena dan HUD prototipe

**Files:**
- Create: `Game/ArenaDirector.cs`

**Interfaces:**
- Consumes: `Enemy.AnyDied`, `PlayerController.Died`, `PlayerController.Health`, and the `Restart` action.
- Produces: `ArenaDirector : MonoBehaviour` with fields `Enemy enemyPrefab` and `Transform[] spawnPoints`. Wave n (starting at 1) spawns `min(2 + n, 6)` Tulang Penjaga at random spawn points that are at least 4 tiles from the player, with a 0.5 s dark-smoke delay (an alpha fade is enough for M1). The next wave starts 3 s after a wave is cleared. When the player dies, time stops and the text "Tumbang. Tekan R untuk ulang" appears; `Restart` reloads the scene. `OnGUI` shows HP, the wave number, and FPS.

- [ ] **Step 1:** Write the script, compile, and run `read_console`. Expected: 0 errors.
- [ ] **Step 2:** Add 6 spawn points in the dark corners of the room.
- [ ] **Step 3:** Manual check in Play mode: play to wave 4, die, press R, and confirm the scene restarts. Read FPS from the HUD and from `manage_graphics` stats at wave 4 with steam active. Expected: ≥ 60 FPS on the developer laptop. Record the number of active lights and the FPS.
- [ ] **Step 4:** Run all the EditMode tests. Expected: all PASS. Then commit `feat: loop arena prototipe dan HUD sementara`.

### Task 11: Uji seru (gerbang M1)

- [ ] **Step 1:** The user plays for 5 minutes. Then ask 2 people from outside the team to play for 5 minutes each without being told the rules. Ask the fixed questions from GDD §13, plus: "Apakah kamu sengaja menembak genangan es?" and "Apakah terasa bedanya musuh di terang dan di gelap?"
- [ ] **Step 2:** Record the results in the table in GDD §13. Use build "M1".
- [ ] **Step 3:** Decide together with the user:
  - (a) The core is fun: continue to M2 as in the roadmap.
  - (b) The numbers are wrong (too dark, steam too weak, the dark speed-up not noticeable): tune the values in the data assets, then test again.
  - (c) The core is not fun: brainstorm a fix before M2.
  - Write the decision in the roadmap.
- [ ] **Step 4:** Use skill `superpowers:verification-before-completion`: all tests PASS, the console has no errors, and the screenshots have been shown. Commit `docs: hasil uji seru M1`.

---

**Deferred to later milestones (not part of M1):** Kobaran, relics and upgrades, outline and Tersingkap damage numbers, white flash and hit-stop, other enemies, Ledakan Cahaya, the floor generator, doors, final UI, audio.
