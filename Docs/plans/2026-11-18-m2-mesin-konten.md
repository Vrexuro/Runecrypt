# M2 Mesin Konten: Rencana Implementasi

> **Untuk agen pelaksana:** WAJIB memakai sub-skill superpowers:subagent-driven-development atau superpowers:executing-plans (user memilih **Native**) untuk mengerjakan rencana ini task demi task. Setiap langkah memakai checkbox (`- [ ]`).

**Tujuan:** Relic dan upgrade sebagai data yang mengubah stat pemain dan perilaku bola api (split, pierce, bounce, homing), ditambah Kobaran, status Stun, dan garis tepi Tersingkap. Hasilnya bisa dicoba di arena M1.

**Arsitektur:**
- **Logika murni di `Runecrypt.Core`**, diuji dengan EditMode test:
  - `PlayerStats` menumpuk `StatMod` dari relic.
  - `ShotStats` adalah hasil perhitungan stat untuk satu tembakan.
  - Pola sebar, pemilih sasaran, dan belokan homing juga ada di Core.
- **Komponen di `Runecrypt.Game`** hanya membaca hasil perhitungan itu:
  - `Fireball` diluncurkan dengan `ShotStats`.
  - `PlayerController` menghitung ulang stat setiap kali relic bertambah.
- **Menambah relic** cukup dengan membuat aset `RelicDefinition` baru (prinsip arsitektur di CLAUDE.md). Kode pemain tidak berubah.

**Teknologi:** Unity 6000.3.26f1, URP 17.3 2D, Input System, Unity Test Framework (EditMode, ditambah test `EnterPlayMode` di assembly yang sama), Python 3.12 dengan Pillow untuk sprite.

**Spec:**
- `Docs/GDD.md`: §4.1, §4.2, §5.2, §5.3, §5.4, §5.5, §8.1.
- Roadmap: baris M2 di `Docs/plans/2026-11-17-roadmap.md`.

## Batasan Global
- **Tanpa paket baru dan tanpa asset pack.** Sprite baru dibuat lewat script di `ArtSource/scripts/`. PNG hasil generate tidak boleh diedit dengan tangan.
- **"Menambah relic baru tidak boleh mengharuskan mengubah kode pemain."** Efek relic M2 harus bisa ditulis sepenuhnya sebagai daftar `StatMod`.
- **Angka GDD dipakai persis:**
  - bola api: damage 3, 3 tembakan/detik, jangkauan 7 tile;
  - Terbakar: 1 damage/detik selama 2 detik;
  - Kobaran: cooldown 6 detik, radius 2,5 tile, damage 6;
  - cahaya pemain: radius 3 tile;
  - Tersingkap: +25%;
  - split: menyebar 10°.
- **Kontrol Kobaran:** klik kanan, RT, dan Shift kiri (GDD §8.1).
- **Homing hanya memilih musuh terang** (GDD §5.5 aturan 2). Cahaya bola api itu sendiri tidak dihitung saat memilih sasaran.
- **Test `EnterPlayMode` berjalan di dalam scene M1 yang sedang terbuka.** Pemain, kamera, dan lampu scene ikut aktif. Objek test harus ditempatkan jauh dari arena (x ≥ 100) dan memakai `Physics2D.simulationMode = Script` seperti di `CombatPlayModeTests`.
- Komentar dan teks UI ditulis dalam bahasa Indonesia, nama kode dalam bahasa Inggris, mengikuti kode yang sudah ada.
- **Gerbang selesai:** semua EditMode test lulus, console bersih, dan user sudah melihat hasilnya.

## Review Focus
1. **Cahaya bola api sendiri.** Kalau radius cahaya bola api ikut dihitung, homing akan memilih musuh gelap yang kebetulan dekat bola api. Pemilihan sasaran harus mengabaikan sumber cahaya milik bola api itu (Task 3 dan Task 4).
2. **Pierce memukul musuh yang sama dua kali.** Bola api yang masih bersentuhan dengan musuh yang sama selama beberapa frame hanya boleh memukulnya sekali (Task 4).
3. **Bounce dan dinding.** Bola api yang cepat tidak boleh menembus dinding atau tersangkut memantul di sudut. Kalau jatah pantulan habis, bola api hancur saat kena dinding seperti di M1 (Task 4).
4. **Tumpukan persentase negatif.** Contohnya Arang Berat −15% kecepatan. Hasil stat tidak boleh nol atau negatif: laju tembak, kecepatan, ukuran, dan jangkauan diberi batas bawah (Task 1).
5. **Kumpulan relic habis.** Kalau semua relic sudah dimiliki, arena tidak boleh memunculkan pickup kosong atau error (Task 3 dan Task 9).

---

### Task 0: Persiapan
- [ ] Pastikan branch aktif adalah `feature/m2-mesin-konten`, dibuat dari `feature/m1-5-juice` (sudah dibuat 18 Nov 2026).
- [ ] Jangan commit `ProjectSettings/VersionControlSettings.asset`. Perubahan itu milik user dan masih menunggu keputusan soal version control.
- [ ] Jalankan seluruh EditMode test. Hasil yang diharapkan: **71/71 lulus**.

### Task 1: Model stat yang bisa ditumpuk

**File:**
- Buat: `Assets/Runecrypt/Scripts/Core/Stats.cs` (`Stat`, `StatOp`, `StatMod`, `PlayerStats`, `ShotStats`)
- Buat: `Assets/Runecrypt/Scripts/Core/ModifierDefinition.cs` (`ModifierDefinition`, `RelicDefinition`, `UpgradeDefinition`)
- Test: `Assets/Runecrypt/Tests/EditMode/StatTests.cs`

**Antarmuka (dipakai task berikutnya):**
- `enum Stat { Damage, FireRate, Range, ProjectileSpeed, ProjectileSize, ProjectileCount, Pierce, Bounce, HomingTurn, BurnDuration, BurnDamage, LightRadius, ProjectileLight, MaxHp, MoveSpeed, DashCooldown, BlazeCooldown, BlazeDamage }`
- `enum StatOp { Add, Percent }`. Nilai `Percent` berupa pecahan, misalnya 0.2 = +20%.
- `[Serializable] public struct StatMod { public Stat stat; public StatOp op; public float value; }`
- `class PlayerStats`:
  - `void Add(IEnumerable<StatMod> mods)`
  - `float Get(Stat s, float baseValue)`, dengan rumus `(base + ΣAdd) × (1 + ΣPercent)` dan batas bawah `0`
  - `int GetInt(Stat s, int baseValue)`, dibulatkan ke bawah
  - `ShotStats Shot(ShotStats b)`
- `struct ShotStats { float Damage, Speed, Range, Size, LightScale, HomingTurn, BurnDuration, BurnDamagePerTick; int Count, Pierce, Bounce; }`
- `PlayerStats.Shot` menghitung setiap field dari stat yang sesuai.
- **Batas bawah (Review Focus 4):** `Speed ≥ 1`, `Range ≥ 1`, `Size ≥ 0.25`, `Count ≥ 1`.
- `ModifierDefinition : ScriptableObject`, dengan field `displayName`, `[TextArea] description`, `Sprite icon`, `StatMod[] mods`, `bool healFull`, `float heal`.
- `RelicDefinition : ModifierDefinition` memakai `[CreateAssetMenu(menuName = "Runecrypt/Relic")]`.
- `UpgradeDefinition : ModifierDefinition` memakai `[CreateAssetMenu(menuName = "Runecrypt/Upgrade")]`. Aset upgrade baru dibuat di M4 bersama level-up.

- [ ] **Langkah 1: Tulis test yang gagal** di `StatTests.cs`:
  - `NoMods_ReturnsBase`: `Get(Stat.Damage, 3) == 3`.
  - `AddsThenPercent`: mod Add +1 dan Percent +0.2 memberi `Get(Damage, 3) == 4.8`, dengan toleransi 1e-4.
  - `TwoSplitRelics_Stack`: dua kali `Add({ProjectileCount, Add, 1})` memberi `GetInt(ProjectileCount, 1) == 3`.
  - `NegativePercent_ClampedAtFloor`: Percent −1.5 pada Speed memberi `Shot(...).Speed == 1`.
  - `Shot_AppliesArangBerat`: base damage 3, speed 12, size 1, ditambah mod Size +0.4, Damage +0.2, Speed −0.15, memberi Damage 3.6, Speed 10.2, Size 1.4.
- [ ] **Langkah 2:** jalankan `run_tests` EditMode dengan filter `StatTests`. Yang diharapkan: gagal kompilasi karena tipe belum ada.
- [ ] **Langkah 3:** buat kedua file sesuai antarmuka di atas.
- [ ] **Langkah 4:** jalankan `StatTests` lagi. Yang diharapkan: **5/5 lulus**.
- [ ] **Langkah 5:** commit `feat: model stat yang bisa ditumpuk dan data relic/upgrade`.

### Task 2: HP maksimum yang bisa berubah dan Terbakar yang mengikuti stat

**File:**
- Ubah: `Scripts/Core/Health.cs`, `Scripts/Core/BurnStatus.cs`, `Scripts/Core/HitInfo.cs`, `Scripts/Game/Enemy.cs`
- Test: `Tests/EditMode/HealthTests.cs`, `Tests/EditMode/TimingTests.cs` (lokasi test BurnStatus yang sudah ada)

**Antarmuka:**
- `Health`:
  - `Max` sekarang punya `private set`.
  - `void SetMax(float max)` menurunkan `Current` kalau melebihi `max`, dan tidak berbuat apa-apa kalau sudah mati.
  - `void Heal(float amount)` dibatasi sampai `Max`, dan tidak berbuat apa-apa kalau sudah mati.
- `BurnStatus.Apply(float now, float duration, float damagePerTick)`. Apply terakhir yang menentukan damage per tick. `Apply(now)` yang lama tetap ada dan memakai nilai dari konstruktor.
- `HitInfo`:
  - konstruktor baru `(float damage, Element element, bool appliesBurn, Vector2 point, float burnDuration = 2f, float burnDamagePerTick = 1f)`;
  - field `BurnDuration` dan `BurnDamagePerTick`.
  - Semua pemanggil yang sudah ada tetap terkompilasi.
- `Enemy.TakeHit` memanggil `burn.Apply(Time.time, hit.BurnDuration, hit.BurnDamagePerTick)`.

- [ ] **Langkah 1: Tulis test yang gagal:**
  - `SetMax_Raise_KeepsCurrent`: max 6, current 6, `SetMax(8)`, lalu `Heal(8)` memberi `Current == 8`.
  - `SetMax_Lower_ClampsCurrent`: max 6, `SetMax(4)` memberi `Current == 4`.
  - `Heal_WhenDead_DoesNothing`.
  - `Burn_ApplyWithDuration_LastsLonger`: `Apply(0, 4, 1)` memberi total `Tick(4) == 4`.
  - `Burn_ApplyWithDamage_UsesNewDamage`: `Apply(0, 2, 2)` memberi `Tick(1) == 2`.
- [ ] **Langkah 2:** jalankan test tersebut. Yang diharapkan: GAGAL atau tidak terkompilasi.
- [ ] **Langkah 3:** implementasikan.
- [ ] **Langkah 4:** jalankan seluruh EditMode test. Yang diharapkan: semua lulus, termasuk 71 test lama.
- [ ] **Langkah 5:** commit `feat: HP maksimum berubah dan Terbakar mengikuti stat`.

### Task 3: Logika tembakan murni

**File:**
- Buat: `Scripts/Core/ShotMath.cs` (`SpreadPattern`, `TargetSelector`, `Homing`), `Scripts/Core/RelicPicker.cs`
- Ubah: `Scripts/Core/LightField.cs`
- Test: `Tests/EditMode/ShotLogicTests.cs`

**Antarmuka:**
- `SpreadPattern.Directions(Vector2 aim, int count, float spacingDegrees) : Vector2[]`. Sudut ke-i adalah `(i − (count−1)/2) × spacing`, semuanya ternormalisasi.
- `TargetSelector.Nearest(Vector2 from, IReadOnlyList<Vector2> points, float maxRange, Func<Vector2, bool> isLit) : int`. Mengembalikan indeks titik terang terdekat dalam `maxRange`, atau `-1` kalau tidak ada.
- `Homing.Steer(Vector2 velocity, Vector2 toTarget, float maxDegrees) : Vector2`. Memutar `velocity` ke arah sasaran paling banyak `maxDegrees`, dengan besar kecepatan tetap.
- `LightField.IsLit(Vector2 point, ILightSource ignore)`. Overload yang melewati satu sumber cahaya (Review Focus 1). `IsLit(point)` yang lama memanggilnya dengan `null`.
- `RelicPicker.PickUnowned(IReadOnlyList<RelicDefinition> pool, ICollection<RelicDefinition> owned, System.Random rng) : RelicDefinition`. Mengembalikan `null` kalau semua relic sudah dimiliki atau pool kosong.

- [ ] **Langkah 1: Tulis test yang gagal:**
  - `Spread_OneShot_IsAim`.
  - `Spread_TwoShots_Are10DegreesApart`: sudut antara kedua arah 10° ±0.01, simetris terhadap arah bidik.
  - `Nearest_IgnoresDarkPoints`: titik gelap di jarak 1 dan titik terang di jarak 3 memberi indeks titik terang.
  - `Nearest_OutOfRange_ReturnsMinusOne`.
  - `Steer_ClampsTurn`: velocity (10,0), sasaran (0,1), maks 30° memberi sudut 30° dan besar 10.
  - `Steer_SmallAngle_Snaps`: sasaran 5° dengan maks 30° memberi sudut 5°.
  - `IsLit_IgnoresGivenSource`: satu sumber dengan radius 5 di titik asal, dan `IsLit((1,0), thatSource) == false`. Pakai `FakeSource` yang sudah ada.
  - `PickUnowned_SkipsOwned`: pool {A, B}, owned {A}, selalu B untuk 20 seed.
  - `PickUnowned_AllOwned_ReturnsNull`.
- [ ] **Langkah 2:** jalankan. Yang diharapkan: gagal kompilasi.
- [ ] **Langkah 3:** implementasikan.
- [ ] **Langkah 4:** jalankan `ShotLogicTests`. Yang diharapkan: **9/9 lulus**.
- [ ] **Langkah 5:** commit `feat: logika sebar, homing, dan pemilih relic`.

### Task 4: Bola api dengan modifier

**File:**
- Ubah: `Scripts/Game/Fireball.cs`, `Scripts/Game/Enemy.cs`
- Test: `Tests/EditMode/ProjectilePlayModeTests.cs` (baru)

**Antarmuka:**
- `Fireball.Defaults : ShotStats`. Dibaca dari field serial: damage 3, speed 12, range 7, size 1, burn 2 detik × 1, count 1.
- `Fireball.Launch(Vector2 dir)` meluncurkan dengan `Defaults`.
- `Fireball.Launch(Vector2 dir, ShotStats s)` menerapkan `s`:
  - `transform.localScale = s.Size`;
  - radius setiap `LightSourceBehaviour` anak dikali `s.LightScale`.
- `Enemy`:
  - `static IReadOnlyList<Enemy> Active`, didaftarkan di `OnEnable` dan dihapus di `OnDisable`;
  - `bool IsLit(ILightSource ignore)`;
  - `void MarkTargeted(float now)`, yang menampilkan child `homingMarker` selama 0,1 detik setelah panggilan terakhir;
  - `float CurrentHp => health.Current`, getter baca-saja untuk test.

**Perilaku:**
- **Dinding.** Ditangani di `FixedUpdate` dengan `Physics2D.CircleCast` sepanjang langkah frame itu, memakai mask `Walls`.
  - Kalau kena dinding dan `Bounce > 0`: kecepatan dipantulkan dengan `Vector2.Reflect` terhadap normal, posisi dipindah ke `hit.centroid`, dan `Bounce--`.
  - Kalau `Bounce == 0`: bola api hancur.
  - Cabang `Walls` di `OnTriggerEnter2D` dihapus.
- **Jangkauan** dihitung dari jarak tempuh total, bukan jarak dari titik awal, karena bola api bisa memantul.
- **Pierce.** `HashSet<IDamageable>` mencatat sasaran yang sudah dipukul, jadi sasaran yang sama tidak dipukul dua kali (Review Focus 2). Setiap pukulan mengurangi `Pierce`. Bola api hancur setelah memukul sasaran ketika `Pierce` sudah `< 0`. Flag `spent` yang lama tetap dipakai untuk pukulan terakhir.
- **Homing,** kalau `HomingTurn > 0`. Setiap `FixedUpdate`:
  1. Pilih `Enemy.Active` terdekat dengan `TargetSelector.Nearest(..., maxRange 6, e => e.IsLit(ownLight))`.
  2. `Homing.Steer(v, to, HomingTurn × dt)`.
  3. `target.MarkTargeted(now)`.
- **Mematikan lampu bola api** sebelum memukul (aturan M1) tetap berlaku hanya pada pukulan terakhir. Kalau masih bisa menembus, lampunya tetap menyala.

- [ ] **Langkah 1: Tulis test PlayMode yang gagal.** Ikuti pola `CombatPlayModeTests`, di x ≥ 100.
  - `Fireball_HitsOnlyOneTarget` (yang lama) tetap lulus.
  - `Pierce1_HitsTwoEnemiesInLine`: dua musuh HP 3 sejajar. Dengan `Pierce = 1`, hasilnya 2 kematian.
  - `Pierce_DoesNotHitSameEnemyTwice`: satu musuh HP 100, bola api `Pierce = 3` menembusnya. Tanpa sumber cahaya di dekatnya, yang diharapkan `100 − CurrentHp == 3`. Matikan lampu bola api di test, atau buat bola api tanpa lampu.
  - `Bounce0_DestroyedByWall`: bola api ke dinding (BoxCollider2D di layer `Walls`). Setelah 30 langkah, objeknya sudah null.
  - `Bounce1_ComesBack`: bola api ke kanan menuju dinding di x+3. Setelah 30 langkah, `velocity.x < 0` dan objek masih ada.
  - `Homing_TurnsTowardLitEnemy_NotDarkOne`:
    - Musuh A diberi `FakeSource` dengan radius 2 yang terdaftar di `LightFieldHost.Field`. Musuh B gelap dan lebih dekat.
    - Bola api `HomingTurn = 360`.
    - Setelah 20 langkah, jarak ke A berkurang lebih banyak daripada jarak ke B.
    - `FakeSource` di-unregister di blok `finally`, karena `LightFieldHost.Field` statis dan bertahan antar test.
- [ ] **Langkah 2:** jalankan. Yang diharapkan: test baru gagal.
- [ ] **Langkah 3:** implementasikan perilaku di atas. Child `homingMarker` dibuat oleh builder di Task 9. Kalau `null`, `MarkTargeted` hanya mencatat waktu.
- [ ] **Langkah 4:** jalankan seluruh suite. Yang diharapkan: semua lulus.
- [ ] **Langkah 5:** commit `feat: bola api dengan split-ready, pierce, bounce, dan homing ke musuh terang`.

### Task 5: Pemain memakai stat dan relic

**File:**
- Ubah: `Scripts/Game/PlayerController.cs`
- Test: `Tests/EditMode/ProjectilePlayModeTests.cs`

**Antarmuka:**
- `PlayerStats Stats { get; }`
- `IReadOnlyList<RelicDefinition> Relics { get; }`
- `void AddRelic(RelicDefinition r)`:
  1. tambahkan ke `Relics`;
  2. `Stats.Add(r.mods)`;
  3. `Recalculate()`;
  4. kalau `healFull`, `Health.Heal(Health.Max)`, atau kalau `heal > 0`, `Health.Heal(heal)`.
- `void Recalculate()` menetapkan nilai-nilai berikut dari stat:
  - `fire.Seconds = 1 / max(0.5, Get(FireRate, shotsPerSecond))`;
  - `dash.Seconds = Get(DashCooldown, dashCooldown)`;
  - `moveSpeed` efektif;
  - `Health.SetMax(Get(MaxHp, maxHp))`;
  - `lightSource.Radius = Get(LightRadius, 3)`, lewat field serial baru `lightSource`, yaitu lampu pemain.
- **Menembak:**
  1. `var shot = Stats.Shot(fireballPrefab.Defaults)`;
  2. untuk setiap arah di `SpreadPattern.Directions(aim, shot.Count, 10)`, instansiasi lalu `Launch(dir, shot)`;
  3. feedback tembakan tetap diputar sekali per tembakan.

- [ ] **Langkah 1: Tulis test PlayMode yang gagal:**
  - `SplitRelic_SpawnsTwoFireballs`: buat `RelicDefinition` di memori dengan mod ProjectileCount +1. Panggil `AddRelic`, lalu panggil metode tembak. Untuk itu ekstrak `internal void Shoot(Vector2 aim)` dari `Update`. Yang diharapkan: ada 2 objek `Fireball` baru.
  - `HeartRelic_RaisesMaxAndHeals`: HP 6, kena 2 damage, lalu relic MaxHp +2 dengan `healFull`. Yang diharapkan: `Max == 8` dan `Current == 8`.
  - `LanternRelic_GrowsPlayerLight`: Percent +0.25 pada LightRadius. Yang diharapkan: `lightSource.Radius == 3.75`.
- [ ] **Langkah 2:** jalankan. Yang diharapkan: gagal.
- [ ] **Langkah 3:** implementasikan. Tambahkan `[assembly: InternalsVisibleTo("Runecrypt.Tests.EditMode")]` kalau belum ada.
- [ ] **Langkah 4:** jalankan seluruh suite. Yang diharapkan: semua lulus.
- [ ] **Langkah 5:** commit `feat: pemain menumpuk relic lewat stat`.

### Task 6: Kobaran

**File:**
- Ubah: `Assets/Runecrypt/Input/RunecryptControls.inputactions`. Wrapper `RunecryptControls.cs` di-generate ulang oleh Unity saat import (`generateWrapperCode: 1`). Jangan edit wrapper itu dengan tangan.
- Buat: `Scripts/Game/BlazeRing.cs`
- Ubah: `PlayerController.cs`
- Test: `Tests/EditMode/ProjectilePlayModeTests.cs`

**Antarmuka:**
- Action baru `Gameplay/Blaze` bertipe Button dengan binding `<Mouse>/rightButton`, `<Gamepad>/rightTrigger`, dan `<Keyboard>/leftShift`. Binding mouse/keyboard masuk grup KeyboardMouse, binding gamepad masuk grup Gamepad.
- `BlazeRing : MonoBehaviour`:
  - `void Init(float damage, float radius, float burnDuration, float burnDamagePerTick)`;
  - urutannya: aktifkan `LightSourceBehaviour` anak (radius 3,5, menyala 0,5 detik) **sebelum** memberi damage, supaya musuh di dalam ring Tersingkap;
  - lalu `Physics2D.OverlapCircleAll(pos, radius)`, dan setiap `IDamageable` selain pemain menerima `HitInfo(damage, Fire, true, pos, burnDuration, burnDps)`;
  - menghancurkan diri setelah 0,6 detik.
- `PlayerController`:
  - field `BlazeRing blazePrefab`;
  - `Cooldown blaze` dengan nilai `Get(BlazeCooldown, 6)`;
  - Kobaran memakai radius 2,5 dan damage `Get(BlazeDamage, 6)`, dengan burn dari `Stats.Shot`;
  - Kobaran dipicu oleh `g.Blaze.WasPressedThisFrame()`.
- Angka 3,5 dan 0,5 detik untuk cahaya adalah keputusan rencana. Tujuannya supaya musuh di tepi ring tetap Tersingkap untuk tembakan lanjutan.

- [ ] **Langkah 1: Tulis test PlayMode yang gagal:**
  - `Blaze_DamagesEnemiesInRadius_WithLitBonus`: musuh HP 100 di jarak 2 dan musuh di jarak 3, tanpa dinding. Yang diharapkan: musuh dekat menerima tepat 7,5 (6 × 1,25), musuh jauh menerima 0.
  - `Blaze_TriggersFrostPuddle`: genangan di jarak 1. Yang diharapkan: muncul tepat satu `SteamCloud`.
- [ ] **Langkah 2:** jalankan. Yang diharapkan: gagal.
- [ ] **Langkah 3:** implementasikan. Setelah mengedit `.inputactions`, refresh Unity lalu tunggu `is_compiling == false`. `RunecryptControls.cs` harus berisi `Blaze`.
- [ ] **Langkah 4:** jalankan seluruh suite. Yang diharapkan: semua lulus, console bersih.
- [ ] **Langkah 5:** commit `feat: Kobaran (klik kanan / RT / Shift kiri)`.

### Task 7: Status Stun dan ikon status

**File:**
- Ubah: `Scripts/Game/Enemy.cs`
- Test: `Tests/EditMode/CombatPlayModeTests.cs`

**Antarmuka:**
- `Enemy.Stun(float until)` memakai pola yang sama dengan `Blind`.
- Saat stun:
  - musuh tidak mengejar (`chase = 0`), tetapi knockback tetap berlaku;
  - musuh tidak memberi contact damage.
- Field serial `GameObject blindIcon` dan `stunIcon` dinyalakan sesuai status di `Update`.
- Ledakan Cahaya (M3) akan memanggil `Stun(now + 0.5f)`.

- [ ] **Langkah 1: Tulis test yang gagal**, mengikuti `BlindEnemy_DoesNotDamagePlayer`:
  - `StunnedEnemy_DoesNotDamagePlayer`;
  - `StunnedEnemy_DoesNotChase`: musuh dengan moveSpeed 2 dan pemain di x+5. Setelah stun 1 detik dan 20 langkah, posisi musuh bergeser kurang dari 0,05.
- [ ] **Langkah 2:** jalankan. Yang diharapkan: gagal.
- [ ] **Langkah 3:** implementasikan.
- [ ] **Langkah 4:** jalankan seluruh suite. Yang diharapkan: semua lulus.
- [ ] **Langkah 5:** commit `feat: status Stun dan ikon Buta/Stun`.

### Task 8: Garis tepi Tersingkap

**File:**
- Buat: `Assets/Runecrypt/Shaders/SpriteOutline.shader`, `Scripts/Game/Feedback/ExposedOutline.cs`
- Ubah: `Editor/PixelArtImporter.cs`, `Scripts/Game/Enemy.cs`
- Test: `Tests/EditMode/FeedbackComponentTests.cs`

**Antarmuka:**
- **`SpriteOutline.shader`:**
  - unlit, dibuat mengikuti `SpriteFlash.shader` (Core2D.hlsl, `SetUpSpriteInstanceProperties`, `UnityFlipSprite`, pass `Universal2D`);
  - menggambar `_OutlineColor` (#FF8A3D) di piksel transparan yang punya tetangga 4 arah beralpha lebih dari 0, berdasarkan `_MainTex_TexelSize`;
  - piksel lain dibuat transparan.
- **`ExposedOutline : MonoBehaviour`** ([RequireComponent SpriteRenderer]):
  - `SetSource(SpriteRenderer)` dan `SetVisible(bool)`;
  - `LateUpdate` menyalin sprite, flipX, dan sorting layer, dengan order = sumber − 1;
  - pola salinannya sama dengan `HitFlash`.
- **`Enemy.Update`** memanggil `outline.SetVisible(IsLitNow && !dead)`.
- **`PixelArtImporter`** memakai `spriteMeshType = FullRect` (lewat `TextureImporterSettings`), supaya piksel garis tepi di luar mesh rapat tetap tergambar.

- [ ] **Langkah 1: Tulis test yang gagal:** `ExposedOutline_CopiesSpriteAndHidesWhenNotVisible`, mengikuti pola test `HitFlash` di `FeedbackComponentTests`.
- [ ] **Langkah 2:** jalankan. Yang diharapkan: gagal.
- [ ] **Langkah 3:** implementasikan, lalu reimport `Assets/Runecrypt/Art`.
- [ ] **Langkah 4:** jalankan seluruh suite. Yang diharapkan: semua lulus.
- [ ] **Langkah 5: Verifikasi visual** lewat screenshot Play mode. Tulang di dalam cahaya pemain harus bergaris tepi jingga 1 piksel, dan Tulang di gelap hanya terlihat matanya. Kalau sprite menyentuh tepi kanvas 16x16, garis di sisi itu terpotong. Catat di ledger, jangan memperbesar kanvas.
- [ ] **Langkah 6:** commit `feat: garis tepi jingga untuk musuh Tersingkap`.

### Task 9: Konten, sprite, builder, dan pickup relic di arena

**File:**
- Ubah: `ArtSource/scripts/make_juice_sprites.py`. Tambahkan:
  - `relic_orb` 16x16;
  - `blaze_ring` 48x48;
  - `icon_blind` 5x5 (pusaran abu-abu);
  - `icon_stun` 5x5 (bintik kuning);
  - `homing_marker` 3x3 (bara jingga);
  - palet dari GDD §7.1.
- Buat: `Scripts/Game/RelicPickup.cs`, `Editor/M2ContentBuilder.cs`
- Ubah: `Editor/M1ActorBuilder.cs` (memanggil `M2ContentBuilder` dan mewiring prefab), `Scripts/Game/ArenaDirector.cs`
- Test: `Tests/EditMode/ProjectilePlayModeTests.cs`

**Antarmuka:**
- `RelicPickup`:
  - `void Init(RelicDefinition r)`;
  - trigger dengan `PlayerController` memanggil `AddRelic(r)`, memutar feedback (memakai profil `Shot` sementara, SFX baru di M5), lalu menghancurkan diri;
  - punya Light2D kecil lewat `LightSourceBehaviour` dengan radius 1.
- `ArenaDirector`:
  - field `List<RelicDefinition> relicPool` dan `RelicPickup relicPickupPrefab`;
  - setiap kali gelombang bersih, panggil `RelicPicker.PickUnowned(relicPool, player.Relics, rng)`, lalu munculkan pickup di `RoomCenter` kalau hasilnya tidak `null` (Review Focus 5);
  - `OnGUI` menampilkan nama relic yang dimiliki di bawah baris HP.
  - *shortcut: pickup setiap gelombang hanya untuk mencoba relic di M2, diganti ruang harta di M4.*
- `M2ContentBuilder.BuildAssets()` membuat 10 aset di `Assets/Runecrypt/Data/Relics/` lewat `M1ActorBuilder.LoadOrCreate`:

| # | Aset | Mod |
|---|---|---|
| 1 | Bara Kembar | ProjectileCount Add 1 |
| 2 | Taring Obsidian | Pierce Add 1 |
| 3 | Cermin Retak | Bounce Add 2 |
| 4 | Mata Ngengat | HomingTurn Add 180 |
| 5 | Sumbu Panjang | Range Percent 0.3, ProjectileLight Percent 0.3 |
| 6 | Lilin Ganda | FireRate Percent 0.2 |
| 7 | Abu Pekat | BurnDuration Add 2 |
| 8 | Arang Berat | ProjectileSize Percent 0.4, Damage Percent 0.2, ProjectileSpeed Percent −0.15 |
| 9 | Lentera Tulang | LightRadius Percent 0.25 |
| 10 | Jantung Tungku | MaxHp Add 2, healFull |

- Relic 11–15 (Mahkota Asap, Gema Guntur, Sayap Jelaga, Rantai Nyala, Tengkorak Pijar) butuh perilaku baru di luar stat. Ditunda ke M3 dan M4, dan dicatat di roadmap.
- Builder juga membuat dan mewiring:
  - prefab `BlazeRing` (sprite `blaze_ring` dengan `ExpandFade` yang sudah ada, ditambah lampu);
  - prefab `RelicPickup`;
  - material `SpriteOutline.mat`;
  - di prefab musuh: child `ExposedOutline`, `blindIcon`, `stunIcon`, dan `homingMarker`;
  - di pemain: `lightSource` dan `blazePrefab`;
  - di arena: `relicPool` dan `relicPickupPrefab`.

- [ ] **Langkah 1: Tulis test PlayMode yang gagal:** `RelicPickup_GivesRelicOnce`. Pemain menyentuh pickup. Yang diharapkan: `Relics.Count == 1` dan pickup sudah null.
- [ ] **Langkah 2:** jalankan. Yang diharapkan: gagal.
- [ ] **Langkah 3:** jalankan script sprite dengan `python -I ArtSource/scripts/make_juice_sprites.py`, implementasikan `RelicPickup`, builder, dan `ArenaDirector`, lalu jalankan menu `Runecrypt/M1/Build All`.
- [ ] **Langkah 4:** jalankan seluruh suite. Yang diharapkan: semua lulus, console bersih tanpa error dan tanpa warning baru.
- [ ] **Langkah 5: Verifikasi visual di Play mode:**
  1. Bersihkan gelombang 1 sampai pickup muncul, lalu ambil.
  2. Kumpulkan Bara Kembar. Screenshot dua bola api yang menyebar.
  3. Coba Cermin Retak. Pantulan harus terlihat.
  4. Coba Mata Ngengat. Bola api berbelok ke musuh terang, dan penanda bara terlihat.
  5. Tekan klik kanan. Ring Kobaran muncul, ruangan sekitar terang sesaat, dan angka damage berwarna kuning.
- [ ] **Langkah 6:** commit `feat: 10 relic data, pickup relic di arena, builder M2`.

### Task 10: Dokumen dan uji oleh user
- [ ] Perbarui roadmap: M2 selesai, relic 11–15 dipindah ke M3 (Mahkota Asap, Gema Guntur) dan M4 (Sayap Jelaga, Rantai Nyala, Tengkorak Pijar), dan aset upgrade dibuat di M4.
- [ ] Tambahkan baris riwayat di GDD §14: versi 0.4, Kobaran klik kanan, 10 relic data.
- [ ] Commit `docs: roadmap dan GDD setelah M2`.
- [ ] User mencoba relic dan Kobaran di arena. Catat temuannya di GDD §13 sebagai build "M2".
- [ ] Review akhir seluruh branch, lalu `superpowers:finishing-a-development-branch`. **Merge ke main hanya dengan izin user.**
