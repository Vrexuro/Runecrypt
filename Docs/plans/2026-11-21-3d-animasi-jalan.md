# Animasi Jalan Tulang dan Pyromancer 3D: Rencana Implementasi

> **Untuk agen pelaksana:** WAJIB memakai sub-skill superpowers:subagent-driven-development atau superpowers:executing-plans untuk mengerjakan rencana ini task demi task. Setiap langkah memakai checkbox (`- [ ]`).

**Tujuan:** Tulang Penjaga berlari kecil dengan kaki yang benar-benar menapak, dan jubah Pyromancer melangkah. Keduanya kembali ke Idle saat diam, dengan laju klip mengikuti kecepatan.

**Arsitektur:**
- Script Blender membuat setiap karakter dalam empat langkah: menggabungkan bagian-bagian model menjadi satu mesh berbobot, membuat armature, menulis action `Idle` dan `Move` sebagai NLA strip, lalu mengekspor FBX yang menimpa file lama.
- Builder editor mengatur impor Generic, membuat AnimatorController (blend tree 1D pada `Speed`, laju state dari `SpeedMul`), dan memasang Animator ke prefab.
- Komponen `Locomotion3D` mengisi kedua parameter dari `Rigidbody.linearVelocity`, dengan bantuan fungsi murni `LocomotionMath.SpeedMul`.

**Teknologi:** Blender 5.3 (Python, dijalankan lewat `mcp__UnityMCP__blender_bridge` `run_python` dengan `exec(open(path).read())`), Unity 6000.3.26f1, URP 17.3, Unity Test Framework (EditMode + PlayMode).

**Spec:** `Docs/specs/2026-11-21-3d-animasi-jalan-design.md`. Angka awal pose dan amplitudo ada di `Docs/research/2026-11-21-gerak-tulang-pyromancer/laporan.md`.

## Batasan Global
- Semua kerja di worktree `D:\Claude\PGIM\Runecrypt3D`, branch `feature/3d-playable`. Unity: `set_active_instance` `Runecrypt3D@96f0bb2f7a5f2cd8`. Checkout `D:\Claude\PGIM\Runecrypt` tidak disentuh.
- Tanpa paket Unity baru, tanpa aset pihak ketiga, dan `execute_code` hanya dengan izin user setiap kali dipakai.
- Script model `spike3d_tulang_blender.py` dan `m3d_pyromancer_blender.py` tidak diubah. Script rig dijalankan setelahnya. FBX tidak diedit tangan.
- Blender 5.3: jangan memakai `Action.fcurves` (sudah dihapus sejak 5.0). Key ditulis dengan `pose_bone.keyframe_insert`.
- Angka dari spec, dipakai persis:

  | | Tulang | Pyromancer |
  |---|---|---|
  | `v0` | 2,0 | 5,0 |
  | Siklus Move | 0,2667 detik | 0,5333 detik |
  | Clamp | 0,5–1,3 | 0,6–1,2 |
  | Threshold Move | 1,0 | 3,0 |
  | Mundur | — | `SpeedMul` negatif kalau komponen maju < −0,3·\|v\| |
  | Idle | sekitar 2,5 detik | sekitar 3 detik |

- Klip dibuat pada 60 fps: Move Tulang 16 frame, Move Pyromancer 32 frame, Idle Tulang 150 frame, Idle Pyromancer 180 frame. Frame terakhir sama dengan frame 0. Tanpa key scale.
- Nama tulang: `root, hips, spine, chest, neck, head, upperarm/forearm/hand.L/.R`, ditambah `thigh/shin/foot.L/.R` (Tulang) atau `robe_front.L_1/_2, robe_front.R_1/_2, robe_back.L_1/_2, robe_back.R_1/_2` (Pyromancer). `.L` = x positif di Blender, yaitu sisi kiri karakter. Karakter menghadap −Y di Blender dan +Z di Unity.
- Test dijalankan dengan scene `Assets/Runecrypt/Scenes/M1_Prototype.unity` terbuka dan console bersih. Error kompilasi membuat play-mode test habis waktu 180 detik. Objek uji PlayMode diletakkan di x ≥ 100, `EnterPlayMode` dipanggil di level teratas, dan langkah fisika dihitung dari `Time.fixedTime`.
- Commit tanpa baris atribusi Claude atau `Co-Authored-By`. `ProjectSettings/VersionControlSettings.asset` dan file ProjectSettings yang hanya berubah akhir baris tidak di-commit.

## Review Focus
1. **Tulang terhalang dinding atau pilar** harus tampil Idle, bukan berlari di tempat (test PlayMode di Task 5).
2. **Pyromancer mundur atau bergeser menyamping sambil membidik:** klip diputar terbalik saat mundur dan tetap maju saat menyamping (test di Task 4).
3. **Tulang di gelap (2,4 tile/detik)** memutar Move 1,2×, sedangkan kecepatan di atas clamp tetap dibatasi 1,3 (test di Task 4).
4. **Kedipan putih saat kena hit** harus tetap bekerja setelah model menjadi `SkinnedMeshRenderer` (test prefab di Task 5).
5. **Mata tetap menghadap +Z setelah rig dan ekspor ulang** (`Prefab_EyesFaceForward`, helper diperluas di Task 2).

---

### Task 1: Helper rig Blender dan ekspor uji

**Files:**
- Create: `ArtSource/scripts/m3d_rig.py`
- Sementara (dihapus di akhir task): `Assets/Runecrypt3D/Models/RigProbe.fbx`

**Interfaces:**
- Produces (di-`exec` oleh script karakter, semua dalam satuan Blender):
  - `bake_mesh(col_name: str, out_name: str, weigh) -> bpy.types.Object`
    - `weigh(obj_name: str, co: Vector) -> dict[str, float]` dipanggil per vertex, dengan `co` dalam koordinat dunia.
    - Fungsi ini menyalin semua objek mesh dan kurva di koleksi yang `hide_render == False` ke mesh baru, dengan modifier sudah diterapkan (`bpy.data.meshes.new_from_object` dari objek ter-evaluasi). Setiap vertex diberi bobot sesuai `weigh`, lalu semua salinan digabung jadi satu objek `out_name`.
    - Koleksi sumber tidak diubah. Objek lama bernama `out_name` (termasuk `*_Export` lama) dihapus lebih dulu.
    - Objek yang tidak dikenali `weigh` (mengembalikan dict kosong) menghentikan script dengan `AssertionError` yang menyebut nama objeknya.
  - `make_armature(name: str, bones: list[tuple]) -> bpy.types.Object`
    - Setiap tuple berbentuk `(nama, induk_atau_None, head_xyz, tail_xyz, connect: bool)`. Semua tulang deform, rotasi Euler `XYZ`.
  - `bind(mesh, arm)`: memasang modifier Armature dan menjadikan arm induk mesh tanpa mengubah posisi dunia.
  - `key_clip(arm, name: str, frames: int, pose_fn)`
    - `pose_fn(phase: float, pb)` mengisi `pb[nama].rotation_euler` dan `.location` untuk `phase = f / frames`.
    - Di setiap frame `0..frames` semua tulang di-key. Action kemudian didorong ke NLA track bernama `name`, dan `animation_data.action` dikosongkan.
  - `two_bone_ik(hip, target, l1, l2) -> (thigh_rad, knee_rad)`: IK analitis di bidang sagital (y-z). Gagal assert kalau jarak > 0,98·(l1+l2).
  - `export_fbx(arm, mesh, path: str)`: menyeleksi hanya `arm` dan `mesh`, lalu memanggil `bpy.ops.export_scene.fbx` dengan:
    - `use_selection=True`, `object_types={'ARMATURE','MESH'}`, `add_leaf_bones=False`, `use_armature_deform_only=True`;
    - `bake_anim=True`, `bake_anim_use_nla_strips=True`, `bake_anim_use_all_actions=False`, `bake_anim_force_startend_keying=True`, `bake_anim_simplify_factor=0.0`;
    - sumbu dan skala sesuai hasil Step 2.
  - Skrip ini juga mengatur `scene.render.fps = 60`.

- [ ] **Step 1: Tulis `m3d_rig.py`**, berisi fungsi-fungsi di atas tanpa kode karakter.

- [ ] **Step 2: Ekspor uji di Blender** (`run_python`). Buat koleksi `RigProbe` berisi:
  - dua kubus: `Body` di (0,0,0,5) dan penanda `Nose` di (0,−0,3,0,5);
  - armature `root → hips → head`, bobot kaku;
  - action `Idle` (diam) dan `Move` (hips naik 0,1 di frame 8);
  - ekspor ke `Assets/Runecrypt3D/Models/RigProbe.fbx`.

  Pakai sumbu default exporter dulu, dan ganti kalau Step 3 gagal.

- [ ] **Step 3: Periksa di Unity** (`refresh_unity`, lalu `manage_asset` atau `find_gameobjects` pada instance model). Lulus kalau:
  - take bernama tepat `Idle` dan `Move` (tanpa `Armature|`);
  - transform `root/hips/head` ada dan tidak ada tulang `_end`;
  - skala model 1;
  - penanda `Nose` berada di +Z model.

  Tulis setelan sumbu yang lulus sebagai default di `export_fbx`, dengan komentar satu baris.

- [ ] **Step 4: Hapus** `RigProbe.fbx`, `.meta`-nya, dan koleksi `RigProbe` di Blender.

- [ ] **Step 5: Commit** dokumen dan helper:
  ```bash
  git add Docs/specs/2026-11-21-3d-animasi-jalan-design.md Docs/plans/2026-11-21-3d-animasi-jalan.md Docs/research/2026-11-21-gerak-tulang-pyromancer ArtSource/scripts/m3d_rig.py
  git commit -m "docs+art(3d): desain, riset, rencana animasi jalan; helper rig Blender"
  ```

### Task 2: Rig dan klip Tulang Penjaga

**Files:**
- Create: `ArtSource/scripts/m3d_tulang_rig.py`
- Create: `Assets/Runecrypt3D/Tests/CharacterRigTests.cs`
- Modify: `Assets/Runecrypt3D/Editor/Arena3DBuilder.Arena.cs` (impor rig)
- Modify: `Assets/Runecrypt3D/Tests/PlaytestFixTests.cs:13-29` (helper `EyeCenter`)
- Regenerate: `Assets/Spike3D/Models/TulangPenjaga3D.fbx`

**Interfaces:**
- Consumes: helper dari Task 1.
- Produces:
  - FBX berisi mesh `TulangPenjaga_Export`, armature `Tulang_Rig`, dan klip `Idle` serta `Move`.
  - Method builder `static void ConfigureRig(string modelPath)`: rig Generic, `avatarSetup = CreateFromThisModel`, `animationCompression = Off`, `clipAnimations` = `defaultClipAnimations` dengan `loopTime = true`, lalu `SaveAndReimport`. Dipanggil di `BuildArena` sebelum `RemapPyromancerMaterials`.

- [ ] **Step 1: Tulis test yang gagal.** Buat `CharacterRigTests` (EditMode) dengan dua test:
  ```csharp
  [TestCase("Assets/Spike3D/Models/TulangPenjaga3D.fbx", 16 / 60f, "thigh.L")]
  public void Model_HasGenericLoopingClips(string path, float moveSeconds, string bone)
  {
      var imp = (ModelImporter)AssetImporter.GetAtPath(path);
      Assert.AreEqual(ModelImporterAnimationType.Generic, imp.animationType);
      var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).ToDictionary(c => c.name);
      Assert.IsTrue(clips["Idle"].isLooping && clips["Move"].isLooping);
      Assert.AreEqual(moveSeconds, clips["Move"].length, 0.02f);
      var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
      Assert.IsNotNull(model.GetComponentInChildren<SkinnedMeshRenderer>());
      Assert.IsTrue(model.GetComponentsInChildren<Transform>().Any(t => t.name == bone));
  }
  ```
  Ubah `PlaytestFixTests.EyeCenter` supaya juga membaca `SkinnedMeshRenderer`: mesh diambil dari `sharedMesh`, dan posisi vertex dari `r.transform.TransformPoint`. Ini pose bind, cukup untuk memeriksa arah hadap.

- [ ] **Step 2: Jalankan dan pastikan gagal.** Jalankan `run_tests` EditMode dengan filter `CharacterRigTests|PlaytestFixTests`. Yang diharapkan: `Model_HasGenericLoopingClips` FAIL (tidak ada klip `Idle`), sedangkan `Prefab_EyesFaceForward` tetap PASS.

- [ ] **Step 3: Tulis `m3d_tulang_rig.py`.** Script dimulai dengan `exec` model dan helper.
  - **Tulang dan posisinya** diambil dari titik ujung bagian di script model. Tidak ada tulang tambahan.
    - `hips` (0,0,0,265)→(0,0,0,32).
    - `thigh.s` = titik ujung Femur. `shin.s` = titik ujung Shin. `foot.s` dari pergelangan (z 0,045) ke depan (y −0,08, z 0,02).
    - `upperarm` = titik ujung UpperArm. `forearm` = titik ujung Forearm. `hand` ke titik Fist.
    - `spine` naik sampai z 0,40, `chest` sampai 0,50, `neck` sampai 0,55, `head` sampai 0,95.
  - **`weigh`** memberi bobot kaku berdasarkan awalan nama objek:

    | Awalan objek | Tulang |
    |---|---|
    | Skull, SocketDark, Eye, EarHole, NoseDark, Mouth, Tooth*, Jaw*, Chin | `head` |
    | Neck | `neck` |
    | Vertebra_0..2 | `spine` |
    | Vertebra_3..5, Sternum, Rib, Scapula | `chest` |
    | Pelvis | `hips` |
    | Shoulder, UpperArm | `upperarm` |
    | Forearm | `forearm` |
    | Fist | `hand` |
    | Femur, Shin, Foot | `thigh`, `shin`, `foot` |
    | Blade, Guard, Grip | `hand.R` |
    | Shield*, Rivet | `forearm.L` |

    Akhiran `_1` berarti `.L`, akhiran `_-1` berarti `.R`, termasuk pada `_knobN`.
  - **Move (16 frame).**
    - Kaki kiri menapak di fase 0–0,5, kaki kanan di 0,5–1.
    - Saat menapak, target pergelangan relatif pinggul bergerak lurus dari y = −0,1333 ke +0,1333. Itu `v0·T/2`, dengan depan = −Y.
    - Saat mengayun, target kembali ke depan dengan angkat sin setinggi 0,04.
    - `hips` turun sekitar 5 cm dari pose rest selama Move, cukup agar jangkauan pinggul ke pergelangan ≤ 0,98·(l1+l2). Kaki rest hampir lurus (pinggul ke pergelangan sekitar 0,22), jadi tanpa jongkok kaki tidak sanggup mencapai jarak langkah. Di atas itu ada bob ±0,5 cm, terendah tepat setelah kontak.
    - Sudut paha dan lutut dihitung dengan `two_bone_ik`, kaki rata ke lantai.
    - Torso roll ±3°, yaw ±4° dengan chest berlawanan.
    - Lengan perisai diam. Lengan pedang bergoyang ±8°, ujungnya tertinggal 2 frame.
    - `head` tertinggal 2 frame di setiap langkah kedua.
  - **Idle (150 frame).** Kaki tetap. `hips` roll 1–2°, `head` bergeser lalu kembali, pedang turun lalu naik. Tanpa naik-turun napas.
  - **Self-check sebelum ekspor:**
    - setiap vertex punya tepat satu group berbobot 1,0;
    - selama fase menapak, pergelangan kaki bergeser 0,0333 ± 0,004 per frame (`v0/60`) dengan z bergeser tidak lebih dari 5 mm;
    - pose frame 16 sama dengan pose frame 0.
  - **Ekspor** ke `Assets/Spike3D/Models/TulangPenjaga3D.fbx`.

- [ ] **Step 4: Tambah `ConfigureRig`** ke builder, lalu panggil untuk `TulangModel` di `BuildArena`.

- [ ] **Step 5: Jalankan script dan build.** Jalankan `run_python` dengan `exec(open(r'D:/Claude/PGIM/Runecrypt3D/ArtSource/scripts/m3d_tulang_rig.py').read())` dan pastikan semua self-check lolos. Lalu `refresh_unity`, menu `Runecrypt/3D/Build Arena`, dan `read_console` (harus bersih).

- [ ] **Step 6: Jalankan test dan pastikan lulus.** Jalankan `run_tests` EditMode dengan filter `CharacterRigTests|PlaytestFixTests`. Semua harus PASS.

- [ ] **Step 7: Commit.**
  ```bash
  git add ArtSource/scripts/m3d_tulang_rig.py Assets/Spike3D/Models/TulangPenjaga3D.fbx* Assets/Runecrypt3D/Editor/Arena3DBuilder.Arena.cs Assets/Runecrypt3D/Tests/CharacterRigTests.cs* Assets/Runecrypt3D/Tests/PlaytestFixTests.cs Assets/Runecrypt3D/Prefabs Assets/Runecrypt3D/Scenes
  git commit -m "feat(3d): rig dan klip Idle/Move Tulang Penjaga, kaki menapak"
  ```

### Task 3: Rig dan klip Pyromancer dengan bobot jubah prosedural

**Files:**
- Create: `ArtSource/scripts/m3d_pyromancer_rig.py`
- Modify: `Assets/Runecrypt3D/Tests/CharacterRigTests.cs` (TestCase kedua)
- Modify: `Assets/Runecrypt3D/Editor/Arena3DBuilder.Arena.cs` (`ConfigureRig(PyroModel)`)
- Regenerate: `Assets/Runecrypt3D/Models/Pyromancer3D.fbx`

**Interfaces:**
- Consumes: helper dari Task 1 dan `ConfigureRig` dari Task 2.
- Produces: FBX berisi mesh `Pyromancer_Export`, armature `Pyro_Rig`, dan klip `Idle` serta `Move`.

- [ ] **Step 1: Tambah TestCase yang gagal:**
  ```csharp
  [TestCase("Assets/Runecrypt3D/Models/Pyromancer3D.fbx", 32 / 60f, "robe_front.L_2")]
  ```

- [ ] **Step 2: Jalankan dan pastikan gagal.** `run_tests` dengan filter `CharacterRigTests`. Yang diharapkan: TestCase Pyromancer FAIL.

- [ ] **Step 3: Tulis `m3d_pyromancer_rig.py`.**
  - **Tulang inti.**
    - `hips` (0,0,0,40)→(0,0,0,45), `spine`→0,52, `chest`→0,60, `neck`→0,64, `head`→0,95.
    - Lengan mengikuti titik `sh` dan `wr` Sleeve: `upperarm.s` dari `sh` ke titik tengah, `forearm.s` dari titik tengah ke `wr`, `hand.s` dari `wr` ke titik Hand.
  - **Rantai jubah** di bawah `hips` berada pada azimut 45° (front.L, x+ y−), 135° (back.L, x+ y+), 225° (back.R), dan 315° (front.R).
    - `_1` dari radius 0,13 pada z 0,50 ke radius 0,20 pada z 0,28.
    - `_2` (`connect=False`) dari radius 0,20 pada z 0,28 ke hem (radius 0,26, z 0,06).
  - **`weigh`:**

    | Bagian | Bobot |
    |---|---|
    | Hood, HoodRim, FaceVoid, Eye | `head` |
    | Shoulders | `chest` |
    | Belt, SkullCharm, CharmEye | `hips` |
    | SleeveInside, Hand | `hand.s` |
    | Ember, Flame | `hand.R` |
    | Sleeve | menurut proyeksi t sepanjang `sh→wr`: `upperarm` di t < 0,4, `forearm` di t > 0,6, dicampur linear di antaranya |
    | Robe | prosedural, lihat di bawah |
    | Burn, FrontFold | menyalin bobot vertex Robe terdekat (KD-tree `mathutils.kdtree`) |

    Bobot Robe prosedural:
    - **Tinggi:** z ≥ 0,45 penuh `hips`. Dari 0,45 ke 0,28 pindah dari `hips` ke `_1`. Dari 0,28 ke 0,15 pindah dari `_1` ke `_2`. Di bawah 0,15 penuh `_2`.
    - **Azimut:** dibagi linear antara dua rantai terdekat.
    - Hasilnya dinormalkan.
  - **Move (32 frame).**
    - Pakai fase "paha bayangan": `a(phase) = sin(2π·phase)`, dengan kaki kiri maju di fase 0,25.
    - `robe_front.s_1` rotasi X maju 18°·a_s. `robe_back.s_1` 8° berlawanan arah.
    - `_2` memakai kurva yang sama tetapi tertinggal 2 frame.
    - `hips` bob 6,5 cm puncak ke lembah, dua kali per siklus, terendah tepat setelah kontak (fase 0,03 dan 0,53).
    - `_2` diberi key lokasi yang membatalkan bob `hips` di ruang dunia (dikonversi ke ruang lokal tulang), supaya hem tetap di lantai.
    - Sway roll `hips` ±3°, ujung hem 1,5×.
    - `upperarm.L` berayun ±20° berlawanan kaki kiri. `forearm.L` tertinggal 2 frame. Lengan kanan dan bara stabil.
    - `head` tertinggal 2–3 frame dan menunduk 3° setelah kontak.
  - **Idle (180 frame).** Dada naik 1–1,5 cm, roll `hips` 1–2°, hem ikut 5 frame kemudian, bara naik-turun sekitar 1 cm.
  - **Self-check sebelum ekspor:**
    - bobot setiap vertex berjumlah 1 ± 1e-4;
    - tidak ada vertex tanpa bobot;
    - pose frame 32 sama dengan pose frame 0;
    - di setiap frame Move, z terendah mesh ter-evaluasi tetap di rentang −0,01 sampai 0,05 (hem tidak melayang dan tidak tenggelam).
  - **Ekspor** ke `Assets/Runecrypt3D/Models/Pyromancer3D.fbx`.

- [ ] **Step 4: Panggil `ConfigureRig(PyroModel)`** di `BuildArena`.

- [ ] **Step 5: Jalankan script dan build.** Jalankan script lewat `run_python`, lalu `refresh_unity`, `Runecrypt/3D/Build Arena`, dan `read_console` (harus bersih).

- [ ] **Step 6: Jalankan test dan pastikan lulus.** `run_tests` dengan filter `CharacterRigTests|PlaytestFixTests`. Semua harus PASS. Remap material Pyromancer harus tetap berlaku, ditandai `GlowMaterial_*` PASS dan material tidak kembali ke default.

- [ ] **Step 7: Commit.**
  ```bash
  git add ArtSource/scripts/m3d_pyromancer_rig.py Assets/Runecrypt3D/Models/Pyromancer3D.fbx* Assets/Runecrypt3D/Editor/Arena3DBuilder.Arena.cs Assets/Runecrypt3D/Tests/CharacterRigTests.cs Assets/Runecrypt3D/Prefabs Assets/Runecrypt3D/Scenes
  git commit -m "feat(3d): rig dan klip Idle/Move Pyromancer, jubah berbobot prosedural"
  ```

### Task 4: `LocomotionMath` dan `Locomotion3D`

**Files:**
- Create: `Assets/Runecrypt3D/Scripts/LocomotionMath.cs`
- Create: `Assets/Runecrypt3D/Scripts/Locomotion3D.cs`
- Create: `Assets/Runecrypt3D/Tests/LocomotionMathTests.cs`
- Modify: `Assets/Runecrypt3D/Scripts/Enemy3D.cs` (hapus `BobHeight`, `BobRate`, dan baris `visual.localPosition` di `Update`; field `visual` ikut dihapus kalau tidak dipakai lagi, beserta `SetRef(e, "visual", …)` di builder)

**Interfaces:**
- Produces:
  - `public static class LocomotionMath { public static float SpeedMul(float speed, float forwardSpeed, float v0, float minMul, float maxMul, bool allowReverse); }`
    - Hasilnya `clamp(speed / v0, minMul, maxMul)`.
    - Kalau `allowReverse` dan `forwardSpeed < -0.3f * speed`, hasilnya dinegatifkan.
  - `public class Locomotion3D : MonoBehaviour`, dengan `[RequireComponent(typeof(Rigidbody))]`:
    - Field serialized: `Animator animator`, `float designSpeed = 2f`, `float minMul = 0.5f`, `float maxMul = 1.3f`, `bool reverseWhenBackward`, `Transform facing`.
    - `Update()` membaca `linearVelocity` di bidang x-z. `forwardSpeed` adalah dot kecepatan dengan arah maju `facing` (atau `transform`) di bidang x-z.
    - Lalu mengisi `animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime)` dan `animator.SetFloat("SpeedMul", SpeedMul(...))`. Hash parameter di-cache.

- [ ] **Step 1: Tulis test yang gagal** (EditMode):
  ```csharp
  [TestCase(0f, 0f, 2f, .5f, 1.3f, false, .5f)]    // diam: batas bawah
  [TestCase(2f, 2f, 2f, .5f, 1.3f, false, 1f)]     // Tulang normal
  [TestCase(2.4f, 2.4f, 2f, .5f, 1.3f, false, 1.2f)] // Tulang di gelap
  [TestCase(5f, 5f, 2f, .5f, 1.3f, false, 1.3f)]   // di atas clamp
  [TestCase(2f, -2f, 2f, .5f, 1.3f, false, 1f)]    // Tulang tidak pernah terbalik
  [TestCase(5f, -5f, 5f, .6f, 1.2f, true, -1f)]    // Pyromancer mundur
  [TestCase(5f, 0f, 5f, .6f, 1.2f, true, 1f)]      // menyamping tetap maju
  [TestCase(5f, -1f, 5f, .6f, 1.2f, true, 1f)]     // serong sedikit ke belakang tetap maju
  public void SpeedMul_Cases(float speed, float fwd, float v0, float min, float max, bool rev, float expected)
      => Assert.AreEqual(expected, LocomotionMath.SpeedMul(speed, fwd, v0, min, max, rev), 1e-4f);
  ```

- [ ] **Step 2: Jalankan dan pastikan gagal.** `run_tests` dengan filter `LocomotionMathTests`. Yang diharapkan: error kompilasi `LocomotionMath` tidak ada. Setelah kerangka kelas kosong (return 0) ditambahkan, test FAIL.

- [ ] **Step 3: Implementasi** `LocomotionMath.SpeedMul` dan `Locomotion3D`, lalu hapus bob di `Enemy3D`.

- [ ] **Step 4: Jalankan test dan pastikan lulus.** `run_tests` dengan filter `LocomotionMathTests|Enemy3DPlayModeTests`. Semua harus PASS dan console bersih.

- [ ] **Step 5: Commit.**
  ```bash
  git add Assets/Runecrypt3D/Scripts/LocomotionMath.cs* Assets/Runecrypt3D/Scripts/Locomotion3D.cs* Assets/Runecrypt3D/Scripts/Enemy3D.cs Assets/Runecrypt3D/Tests/LocomotionMathTests.cs* Assets/Runecrypt3D/Editor/Arena3DBuilder.Arena.cs
  git commit -m "feat(3d): Locomotion3D mengisi Speed dan SpeedMul dari kecepatan; bob kode Tulang dihapus"
  ```

### Task 5: AnimatorController dan pemasangan di prefab

**Files:**
- Modify: `Assets/Runecrypt3D/Editor/Arena3DBuilder.Arena.cs` (`BuildController`, `BuildPlayer`, `BuildTulang`)
- Create: `Assets/Runecrypt3D/Animation/Tulang3D.controller`, `Pyromancer3D.controller` (dibuat builder)
- Create: `Assets/Runecrypt3D/Tests/LocomotionWiringTests.cs`

**Interfaces:**
- Consumes: `Locomotion3D` dari Task 4 dan klip `Idle`/`Move` dari Task 2–3.
- Produces: `static AnimatorController BuildController(string name, string modelPath, float v0, float minMul)`.
  - Menghapus aset lama, lalu membuat `Assets/Runecrypt3D/Animation/{name}.controller` berisi parameter float `Speed` dan `SpeedMul` (default 1).
  - Isinya satu state `Locomotion` dari `CreateBlendTreeInController`:
    - `blendParameter = "Speed"` dan `useAutomaticThresholds = false`;
    - anak `Idle` pada 0 dan `Move` pada `minMul * v0`;
    - `speedParameterActive = true` dan `speedParameter = "SpeedMul"`.
  - Builder memasang controller ke `Animator` milik model (`applyRootMotion = false`), lalu menambahkan `Locomotion3D` ke root prefab:
    - Tulang: (2, 0,5, 1,3, reverse false).
    - Player: (5, 0,6, 1,2, reverse true, `facing` = Visual).

- [ ] **Step 1: Tulis test yang gagal.**

  `LocomotionWiringTests`, EditMode:
  ```csharp
  [TestCase("Tulang3D", 2f, .5f, 1.3f, false)]
  [TestCase("Player3D", 5f, .6f, 1.2f, true)]
  public void Prefab_HasSpeedDrivenAnimator(string prefab, float v0, float min, float max, bool rev)
  ```
  Assert:
  - Animator di anak memakai `AnimatorController`;
  - default state `layers[0]` memakai `BlendTree` dengan `blendParameter == "Speed"`;
  - threshold anak `{0, min*v0}` dengan motion bernama `Idle` lalu `Move`;
  - `speedParameterActive` dan `speedParameter == "SpeedMul"`;
  - field serialized `Locomotion3D` sama dengan (v0, min, max, rev);
  - khusus Tulang3D, `flashRenderers` berisi minimal satu `SkinnedMeshRenderer`.

  `LocomotionWiringTests`, PlayMode. Pakai instance prefab `Tulang3D` di (100,0,0), dengan `Enemy3D` dibuang lewat `DestroyImmediate` sebelum langkah fisika pertama:
  - `Tulang_Moving_SwingsLegs`: setiap langkah fisika `linearVelocity = (2,0,0)`. Setelah 30 langkah, `Speed > 1.5`, dan rotasi lokal `thigh.L` berbeda lebih dari 5° di antara dua sampel yang terpaut 4 langkah fisika.
  - `Tulang_Stopped_GoesIdle`: setelah 30 langkah bergerak, kecepatan dibuat 0. Setelah 1 detik, `Speed < 0.1`.
  - `Tulang_PushingWall_GoesIdle`: kubus statis 1×1×1 di layer `Walls` di (100,8, 0,5, 0), dan setiap langkah `linearVelocity = (2,0,0)`. Setelah 40 langkah, `Speed < 1.0` (di bawah threshold Move).

- [ ] **Step 2: Jalankan dan pastikan gagal.** `run_tests` dengan filter `LocomotionWiringTests`. Yang diharapkan: semua FAIL (belum ada Animator controller atau `Locomotion3D`).

- [ ] **Step 3: Implementasi** `BuildController` dan pemasangannya di `BuildPlayer` serta `BuildTulang`. Jalankan `Runecrypt/3D/Build Arena`, lalu `read_console` (harus bersih).

- [ ] **Step 4: Jalankan test dan pastikan lulus.** `run_tests` dengan filter `LocomotionWiringTests`. Semua harus PASS. Kalau `Tulang_PushingWall_GoesIdle` gagal karena PhysX tidak mengenolkan kecepatan, ganti sumber `Speed` dengan perpindahan posisi per frame, lalu catat alasannya di komentar.

- [ ] **Step 5: Commit.**
  ```bash
  git add Assets/Runecrypt3D/Editor/Arena3DBuilder.Arena.cs Assets/Runecrypt3D/Animation Assets/Runecrypt3D/Tests/LocomotionWiringTests.cs* Assets/Runecrypt3D/Prefabs Assets/Runecrypt3D/Scenes
  git commit -m "feat(3d): Animator Idle/Move berdasar kecepatan untuk Tulang dan Pyromancer"
  ```

### Task 6: Verifikasi visual, suite penuh, dan uji main user

**Files:** script rig (hanya kalau amplitudo perlu disetel) beserta FBX hasilnya.

- [ ] **Step 1: Screenshot Play mode** dari `Arena3D`. Screenshot diambil dari kamera game saat Tulang mendekat, saat Pyromancer berlari maju dan mundur, dan saat keduanya diam. Yang dicek:
  - kaki Tulang mengayun dan tidak meluncur;
  - jubah Pyromancer naik-turun dan hemnya tidak terangkat;
  - Idle aktif saat diam.

  Kalau ada amplitudo yang tidak terbaca, setel di script rig, jalankan ulang Task 2 Step 5 atau Task 3 Step 5, lalu ulangi screenshot.
- [ ] **Step 2: Suite penuh.** Buka scene `M1_Prototype`, lalu `run_tests` EditMode tanpa filter. Yang diharapkan: semua PASS (132 lama + baru) dan console bersih.
- [ ] **Step 3: Uji main user** di `Arena3D`. Catat temuannya di ledger `D:/Claude/PGIM/Runecrypt/.superpowers/sdd/2026-11-20-3d-playable/progress.md`.
- [ ] **Step 4: Commit** hasil setelan terakhir, kalau ada:
  ```bash
  git commit -m "tune(3d): amplitudo animasi jalan setelah uji main"
  ```
