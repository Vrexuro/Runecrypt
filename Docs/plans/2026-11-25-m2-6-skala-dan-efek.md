# M2.6 Skala dan Efek — Rencana Implementasi

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Arena 2D menjadi 30×17 tile dengan kamera yang mengikuti pemain, musuh datang 3 gelombang per ruangan dengan batas 12 hidup bersamaan, cahaya skill lebih luas, serta efek tembakan dan Kobaran yang lebih heboh tanpa menutupi mata musuh.

**Architecture:**
- Logika murni ada di `Runecrypt.Core` dan diuji dengan EditMode test: jadwal gelombang dan batas kamera.
- `ArenaDirector` memakai jadwal itu. Kamera ditaruh di bawah `CameraRig` yang mengikuti pemain, sehingga `CameraShake` cukup menggeser `localPosition` kamera anak.
- Efek baru berupa prefab yang dibangun builder. Kilatan cahaya memakai Light2D tanpa `LightSourceBehaviour`.

**Tech Stack:** Unity 6000.3.26f1, URP 17.3 2D, Pixel Perfect Camera, Unity Test Framework, Python 3.12 + Pillow.

**Spec:** `Docs/specs/2026-11-25-m2-6-skala-dan-efek-design.md` (disetujui user 25 Nov 2026).

## Global Constraints
- Tidak ada paket baru. Sprite efek baru dibuat lewat script di `ArtSource/scripts/`; PNG hasil generate tidak diedit tangan.
- Branch `feature/m2-6-skala-dan-efek` dari `feature/m2-5-isi-makam`. Commit tanpa atribusi Claude. Jangan commit `VersionControlSettings.asset` atau perubahan yang hanya akhir baris.
- Nilai dari spec:
  - arena **30×17**;
  - **3 gelombang per ruangan**, gelombang berikutnya saat **sisa ≤ 4**, batas **12** musuh hidup bersamaan, **8** per gelombang;
  - cahaya bola api **1,75**, Kobaran **4,5 tile selama 1,0 dtk**, cahaya pemain tetap **3**;
  - efek visual ≤ **0,4 dtk**, kilatan cahaya ≤ **0,2 dtk** tanpa bayangan dan tanpa aturan cahaya;
  - **≥ 60 FPS** dengan charger tercolok;
  - kit makam **3 Anglo, 5 Guci, 3 Nisan, 2 Sarkofagus**, pilar **4**.
- Test play mode berjalan di scene kosong; pakai `WaitUntilOrTimeout`, bukan `WaitForSeconds` (lihat `CryptKitPlayModeTests`).
- Menu builder tidak boleh membuat test menulis ulang scene yang di-commit.

## Review Focus
1. **Kamera di pojok ruangan** tidak boleh memperlihatkan area di luar dinding. (Task 2, `CameraBounds_ClampsAtCorners`)
2. **Kamera berguncang saat pemain bergerak** tidak boleh bergeser permanen dari pemain. (Task 3, `Shake_ReturnsToFollowPosition`)
3. **Gelombang yang tertahan batas 12** tetap muncul setelah ada musuh mati, dan ruangan tidak berakhir sebelum semua gelombangnya keluar. (Task 1 dan 3, `Waves_RoomNotDoneWhilePending`)
4. **Kilatan efek tidak membuat musuh Tersingkap**, walau Light2D-nya terlihat terang. (Task 6, `HitFlare_DoesNotRevealEnemy`)
5. **Benda kit, pilar, dan spawn point di arena 30×17** tidak saling menutupi, dan spawn point tetap ≥ 1,5 tile dari benda dan posisi awal pemain. (Task 4, `CryptKit_KeepsSpawnsAndStartClear`, tes yang sudah ada)

---

### Task 0: Branch dan dokumen
- [ ] **Step 1:** `git checkout -b feature/m2-6-skala-dan-efek` dari `feature/m2-5-isi-makam`. Pastikan Unity sudah menyelesaikan reimport dan console bersih.
- [ ] **Step 2:** Commit spec dan rencana ini: `docs: desain dan rencana M2.6 Skala dan Efek`.

### Task 1: Jadwal gelombang (Core)

**Files:** Create `Assets/Runecrypt/Scripts/Core/WaveSchedule.cs`; Test `Assets/Runecrypt/Tests/EditMode/ScaleTests.cs`

**Interfaces:**
- Produces: `public class WaveSchedule` dengan konstruktor `WaveSchedule(int wavesPerRoom = 3, int nextWaveAtAlive = 4, int maxAlive = 12, int maxPerWave = 8)`, dan anggota:
  - `int Room`, `int WaveInRoom` (1-based);
  - `bool IsRoomStart` (benar untuk gelombang pertama ruangan);
  - `int NextWaveSize()` (bobot ruangan dan gelombang, ≤ `maxPerWave`, ruangan 1 ringan);
  - `bool ShouldStartNextWave(int alive, int pending)` (dalam ruangan: `alive ≤ 4 && pending == 0`; antar-ruangan: `alive == 0 && pending == 0`);
  - `int CanSpawnNow(int alive)` (= `maxAlive − alive`, minimal 0);
  - `void AdvanceWave()`;
  - `bool RoomDone(int alive, int pending)` (gelombang terakhir ruangan sudah keluar, `alive == 0`, dan `pending == 0`).

- [ ] **Step 1: Test gagal:**
  - `Waves_RoomStartsEveryThirdWave` (gelombang 1, 4, 7 → `IsRoomStart`; 2, 3, 5 tidak);
  - `Waves_NextInRoomWhenFourLeft` (`ShouldStartNextWave(5,0)` false, `(4,0)` true, `(4,2)` false);
  - `Waves_NextRoomOnlyWhenEmpty` (setelah gelombang 3: `(1,0)` false, `(0,0)` true);
  - `Waves_SizeCappedAndFirstRoomLight` (semua ukuran ≤ 8; total ruangan 1 ≤ 12);
  - `Waves_CapTwelve` (`CanSpawnNow(10)` = 2, `CanSpawnNow(13)` = 0);
  - `Waves_RoomNotDoneWhilePending` (`RoomDone(0, 3)` false).
- [ ] **Step 2: Jalankan.** Expected: FAIL (tipe belum ada).
- [ ] **Step 3: Implementasikan** tanpa Unity API.
- [ ] **Step 4: Jalankan.** Expected: PASS.
- [ ] **Step 5: Commit:** `feat(core): jadwal gelombang per ruangan (3 gelombang, batas 12)`.

### Task 2: Batas kamera (Core)

**Files:** Create `Assets/Runecrypt/Scripts/Core/CameraBounds.cs`; Test `ScaleTests.cs`

**Interfaces:**
- Produces: `public static class CameraBounds`:
  - `Vector2 Clamp(Vector2 target, Rect room, Vector2 halfView)`: kalau ruangan lebih kecil dari tampilan di satu sumbu, pakai pusat ruangan di sumbu itu;
  - `Vector2 SnapToPixel(Vector2 p, int ppu)`.

- [ ] **Step 1: Test gagal:**
  - `CameraBounds_FollowsInMiddle`;
  - `CameraBounds_ClampsAtCorners` (target di pojok ruangan 30×17 dengan `halfView` (10, 5,625) → kamera berhenti sehingga tepi tampilan = tepi ruangan);
  - `CameraBounds_CentersWhenRoomSmallerThanView`;
  - `SnapToPixel_RoundsToSixteenth`.
- [ ] **Step 2: Jalankan.** Expected: FAIL.
- [ ] **Step 3: Implementasikan.**
- [ ] **Step 4: Jalankan.** Expected: PASS.
- [ ] **Step 5: Commit:** `feat(core): batas dan snap piksel kamera`.

### Task 3: Kamera mengikuti dan gelombang di ArenaDirector

**Files:**
- Create `Assets/Runecrypt/Scripts/Game/CameraFollow.cs`.
- Modify `Feedback/CameraShake.cs` (pakai `localPosition`; hapus catatan shortcut), `ArenaDirector.cs`, `Editor/M1Builder.cs` (`BuildCamera` membuat `CameraRig` → `Main Camera` anak), `Editor/M1JuiceBuilder.cs` (CameraShake tetap di kamera).
- Test `CryptKitPlayModeTests.cs` (ubah `ArenaDirector_RaisesRoomStartedEachWave`) dan `ScaleTests.cs`.

**Interfaces:**
- Consumes: `WaveSchedule`, `CameraBounds`.
- Produces: `CameraFollow : MonoBehaviour` dengan `[SerializeField] Transform target`, `[SerializeField] Rect room`, `[SerializeField] float smoothTime = 0.08f`. Di `LateUpdate`, posisi rig = `SnapToPixel(Clamp(SmoothDamp(...)))`, dan target dicari dari `PlayerController.Instance` kalau kosong. `ArenaDirector` memicu `RoomEvents.Started` hanya pada `IsRoomStart`, menahan spawn sesuai `CanSpawnNow`, dan memunculkan pickup relic sekali per ruangan saat `RoomDone`.

- [ ] **Step 1: Test gagal:**
  - ubah `ArenaDirector_RaisesRoomStartedEachWave` menjadi `ArenaDirector_RaisesRoomStartedEachRoom` (bunuh semua musuh berulang kali; `Started` = 1 selama gelombang 1–3, lalu 2 saat gelombang 4);
  - `ArenaDirector_NeverExceedsTwelveAlive` (`maxPerWave` dinaikkan lewat SerializedObject; `Enemy.Active` tak pernah > 12 + templat);
  - `CameraFollow_TracksPlayerWithinRoom`;
  - `Shake_ReturnsToFollowPosition` (Review Focus 2: setelah trauma habis, kamera anak kembali ke `localPosition` dasar dan rig tetap di posisi ikut).
- [ ] **Step 2: Jalankan.** Expected: FAIL.
- [ ] **Step 3: Implementasikan.** Kecepatan ikut boleh disetel selama tertinggal ≤ 1 tile saat pemain berlari.
- [ ] **Step 4: Jalankan test ini dan seluruh suite.** Expected: PASS.
- [ ] **Step 5: Commit:** `feat: kamera mengikuti pemain dan 3 gelombang per ruangan dengan batas 12`.

### Task 4: Arena 30×17

**Files:**
- Modify `Editor/M1Builder.cs` (`W = 30`, `H = 17`, `MinX = -15`, `MinY = -9`; 4 pilar; batas `room` untuk `CameraFollow`), `Editor/M1ActorBuilder.cs` (spawn point di tepi, Genangan Beku), `Editor/M25CryptBuilder.cs` (posisi dan jumlah kit; overlay dinding atas di sel `y = MinY + H − 1`).
- Test `CryptKitTests.cs` (jumlah benda baru), `WallTileTests.cs` (kalau memakai konstanta lama).

- [ ] **Step 1: Ubah test dulu:** `CryptKit_HasExpectedProps` menjadi 3/5/3/2. Tambah `Arena_IsThirtyBySeventeenWithClosedWalls` (tilemap Walls berisi sel tepi lengkap 30×17 dan `CameraFollow.room` = rect itu).
- [ ] **Step 2: Jalankan.** Expected: FAIL.
- [ ] **Step 3: Implementasikan.**
  - Koordinat bebas, selama `CryptKit_KeepsSpawnsAndStartClear` lulus.
  - Spawn point ada di dekat keempat tepi; sebagian berada di luar tampilan awal kamera.
  - Kit makam tetap di pinggir dan sudut.
  - Jalankan `Runecrypt/M1/Build All`.
- [ ] **Step 4: Jalankan seluruh suite.** Expected: PASS, dan console bersih.
- [ ] **Step 5: Commit:** `feat: arena 30x17 dengan 4 pilar dan kit makam yang diskalakan`.

### Task 5: Cahaya skill lebih luas

**Files:** Modify `Editor/M1ActorBuilder.cs` (cahaya bola api 1 → 1,75), `Scripts/Game/BlazeRing.cs` dan `Editor/M2ContentBuilder.cs` (`lightRadius` 4,5, `lightTime` 1,0); Test `ScaleTests.cs`.

- [ ] **Step 1: Test gagal:**
  - `FireballPrefab_LightRadius175` (radius `LightSourceBehaviour` di prefab Fireball = 1,75);
  - `BlazeRingPrefab_LightFourAndHalfForOneSecond`;
  - `PlayerPrefab_BaseLightStaysThree`.
- [ ] **Step 2: Jalankan.** Expected: FAIL (kecuali yang terakhir, yang memang penjaga nilai lama).
- [ ] **Step 3: Implementasikan** dan jalankan `Build All`.
- [ ] **Step 4: Jalankan.** Expected: PASS.
- [ ] **Step 5: Commit:** `feat: cahaya bola api 1,75 dan Kobaran 4,5 tile selama 1 detik`.

### Task 6: Efek heboh yang tetap terbaca

**Files:**
- Create `Assets/Runecrypt/Scripts/Game/Feedback/FlareFade.cs`.
- Modify `ArtSource/scripts/make_juice_sprites.py` (cincin Kobaran lebih tebal, kilatan moncong, percikan), `Editor/M1JuiceBuilder.cs` (prefab `MuzzleFlash` dan `HitBurst`; profil `Shot`, `HitLit`, `HitDark` memakainya; mata musuh di sorting order 10), `Editor/M1ActorBuilder.cs` (ekor bara pada prefab Fireball), `Editor/M2ContentBuilder.cs` (Kobaran: cincin tebal, kilatan besar, percikan, guncangan sedang).
- Test `ScaleTests.cs`, `CryptKitPlayModeTests.cs`.

**Interfaces:**
- Produces: `FlareFade : MonoBehaviour` (butuh `Light2D`) dengan `[SerializeField] float duration = 0.15f`. Intensitas turun ke 0 lalu objek dihancurkan. Komponen ini tidak pernah punya `LightSourceBehaviour`.

- [ ] **Step 1: Test gagal:**
  - `HitFlare_DoesNotRevealEnemy` (Review Focus 4: titik di dekat `HitBurst` yang baru muncul → `LightFieldHost.Field.IsLit` false);
  - `EffectPrefabs_FlaresShortAndShadowless` (semua `FlareFade` di prefab efek: `duration ≤ 0.2`, `shadowsEnabled == false`, tanpa `LightSourceBehaviour`);
  - `EffectPrefabs_VisualsShort` (umur partikel dan `ExpandFade.duration` ≤ 0,4);
  - `EnemyEyes_DrawAboveAllEffects` (sorting order mata Tulang > sorting order tertinggi di semua prefab efek pemain).
- [ ] **Step 2: Jalankan.** Expected: FAIL.
- [ ] **Step 3: Implementasikan.**
  - Regenerasi sprite lewat script.
  - Efek pemain berada di sorting order ≤ 5, dan mata musuh di 10.
  - Ukuran sprite dan partikel efek tidak melebihi radius serangannya.
- [ ] **Step 4: Jalankan seluruh suite.** Expected: PASS.
- [ ] **Step 5: Ambil screenshot Play mode** saat Kobaran dipakai di tengah banyak musuh. Mata merah harus tetap terlihat.
- [ ] **Step 6: Commit:** `feat: efek tembakan, kena, dan Kobaran lebih heboh dengan aturan keterbacaan`.

### Task 7: GDD, roadmap, dan serah ke user

**Files:** Modify `Docs/GDD.md` (§4.1, §5.1, §5.5, §6, §7.1, §9.1, §9.4, §10, §13, §14), `Docs/plans/2026-11-17-roadmap.md`.

- [ ] **Step 1: Tulis perubahan GDD dari spec §6.** Ini termasuk catatan pengecualian kilatan di §5.5 dan catatan kapasitas booth di §9.4.
- [ ] **Step 2: Tambahkan baris M2.6 di roadmap** dan geser M3.
- [ ] **Step 3: Ukur FPS di Play mode** (charger tercolok, Kobaran di tengah 12 musuh). Expected: ≥ 60. Kalau kurang, kurangi partikel lebih dulu, bukan cahaya aturan.
- [ ] **Step 4: Jalankan seluruh suite.** Expected: semua lulus, dan console bersih. Pastikan scene di disk tidak berubah oleh test.
- [ ] **Step 5: Commit:** `docs: GDD dan roadmap untuk M2.6, pelajaran Soul Knight`.
- [ ] **Step 6: Serahkan ke user untuk playtest**, bersama playtest M2.5, dengan screenshot sebelum dan sesudah.
