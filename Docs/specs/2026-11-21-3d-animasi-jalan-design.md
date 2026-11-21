# Animasi jalan Tulang Penjaga 3D dan Pyromancer 3D — desain

Tanggal: 21 Nov 2026. Branch: `feature/3d-playable` (worktree `D:\Claude\PGIM\Runecrypt3D`).
Riset pendukung: `Docs/research/2026-11-21-gerak-tulang-pyromancer/laporan.md` (angka awal pose dan amplitudo ada di sana).

## Maksud

Hasil playtest user pada versi 3D: kaki Tulang diam waktu bergerak sehingga ia terlihat melayang, dan Pyromancer juga terlihat meluncur walau kakinya tertutup jubah. Kedua karakter harus terlihat **melangkah**, dan kembali diam saat berhenti. Rig yang dibuat dipakai lagi untuk animasi serang dan mati, jadi strukturnya harus rapi sejak awal.

## Sasaran

- Tulang: kaki benar-benar menapak (tidak meluncur) dan lengan ikut bergerak saat berlari.
- Pyromancer: jubah bergoyang dan "melangkah" (naik-turun seirama langkah, tonjolan lutut di depan jubah, ujung jubah tertinggal).
- Keduanya punya klip Idle dan Move. Animator memilih Idle ↔ Move dari kecepatan gerak, dan laju klip mengikuti kecepatan, termasuk 1,2× untuk Tulang di gelap.
- Seluruh rig, bobot, dan klip dibuat ulang dari script Blender. Tidak ada langkah manual di Blender.

## Bukan sasaran

- Klip serang, mati, kena hit (rig disiapkan, klip belum).
- Condong badan dan pegas jubah prosedural di Unity, klip jalan pelan untuk stik gamepad yang ditekan setengah, ujung sepatu di bawah jubah, SFX langkah. Ditambah setelah playtest kalau terasa kurang.
- Mengubah versi 2D atau `EnemyDefinition` (kecepatan Tulang tetap 2 tile/detik, data dipakai bersama 2D).

## Batasan

- Tanpa paket Unity baru dan tanpa aset pihak ketiga. Aset gratis hanya referensi gerak.
- Blender 5.3: `Action.fcurves` sudah dihapus sejak 5.0, jadi key ditulis lewat `pose_bone.keyframe_insert`.
- Unity 6.3, rig Generic, URP. Depan model tetap +Z (test `PlaytestFixTests.Prefab_EyesFaceForward` harus tetap lulus). Helper `EyeCenter` di test itu sekarang hanya membaca `MeshFilter`, jadi diperluas supaya juga membaca `SkinnedMeshRenderer.sharedMesh`. Pemeriksaannya tidak dilemahkan. *(Keputusan pembuat desain.)*
- Unity yang dipakai adalah instance `Runecrypt3D@96f0bb2f7a5f2cd8` (worktree 3D), bukan checkout 2D.
- FBX dan PNG hasil generate tidak diedit tangan. Script model yang sudah ada (`spike3d_tulang_blender.py`, `m3d_pyromancer_blender.py`) tidak diubah perilakunya.
- Suite EditMode harus tetap lulus semua (sekarang 132/132).

## Pendekatan yang dipertimbangkan

**Cara keseluruhan** sudah dipilih user sebelum sesi ini: rig dan klip dibuat script Python Blender, diekspor ke FBX bersama animasinya, lalu diputar Animator Unity yang dikendalikan kecepatan. Gerak prosedural murni di kode tidak dipilih.

**Gaya gerak Tulang** (pilihan user setelah riset): kaki Tulang pendek (pinggul 0,27 dari lantai), jadi pada 2 tile/detik langkah teratur pasti meluncur.
- Derap cepat (siklus 0,4 detik, kaki meluncur sekitar 25%): tidak dipilih.
- **Lari kecil yang bergemeretak dengan kaki menapak (dipilih):** tubuh atas tetap kaku seperti prajurit.
- Memperlambat atau memperbesar Tulang: tidak dipilih, karena mengubah data bersama 2D.

**Bobot jubah Pyromancer** (pilihan user):
- `ARMATURE_AUTO`: tidak dipilih, karena sering gagal pada mesh berukuran 1 unit dan hasilnya perlu dirapikan tangan.
- **Bobot dihitung script dari tinggi dan sudut vertex (dipilih):** hasilnya deterministik.

**Laju klip**: blend tree 1D Idle ↔ Move ditambah Speed Multiplier per state. `Animator.speed` tidak dipakai, karena ia menskala semua layer, termasuk serangan nanti.

## Yang sudah diputuskan

### Rig (nama tulang inti sama untuk keduanya)

```
root (di lantai, tidak di-key di Idle/Move)
└─ hips
   ├─ spine ─ chest ─ neck ─ head
   │          ├─ upperarm.L ─ forearm.L ─ hand.L
   │          └─ upperarm.R ─ forearm.R ─ hand.R
   ├─ thigh.L ─ shin.L ─ foot.L        (Tulang saja)
   └─ thigh.R ─ shin.R ─ foot.R        (Tulang saja)
```

`.L` adalah sisi kiri karakter, yaitu x positif di Blender karena karakter menghadap −Y. Perisai ada di sisi `.L`, pedang dan bara di `.R`.

- **Tulang:** setiap bagian kaku diberi satu vertex group berbobot 1,0 ke satu tulang sebelum mesh digabung, tanpa weight painting.
  - Tengkorak, rahang, gigi, mata → `head`.
  - Leher → `neck`.
  - Rusuk, sternum, vertebra, belikat → `chest`.
  - Panggul → `hips`.
  - Femur, tulang kering, telapak → `thigh`, `shin`, `foot`.
  - Bahu dan lengan atas → `upperarm`; lengan bawah → `forearm`; kepalan → `hand`.
  - Pedang → `hand.R`; perisai → `forearm.L`.
- **Pyromancer:** tidak punya tulang kaki. Ia punya empat rantai jubah di bawah `hips`, masing-masing dua tulang: `robe_front.L/R` dan `robe_back.L/R`, dengan akhiran `_1` (atas) dan `_2` (bawah).
  - Kerudung, wajah, dan mata → `head` (kaku).
  - Bahu → `chest`.
  - Sabuk dan jimat → `hips`.
  - Tangan, bagian dalam lengan baju, bara, dan api → `hand`.
  - Lengan baju dibagi antara `upperarm` dan `forearm` menurut jarak sepanjang lengan.
  - Jubah dibobot prosedural: cincin atas penuh ke `hips`, makin ke bawah pindah ke `_1` lalu `_2`, dan di sekeliling jubah dibagi antara dua rantai yang bersebelahan. Ujung robek, bara di ujung robek (`Burn_*`), dan lipatan depan mengikuti bobot vertex jubah terdekat.

### Klip dan angka

| | Tulang | Pyromancer |
|---|---|---|
| Kecepatan desain `v0` | 2,0 tile/detik | 5,0 tile/detik |
| Move: satu siklus (2 langkah) | 0,267 detik (8 frame pada 30 fps) | 0,533 detik (16 frame pada 30 fps) |
| Jarak per siklus `v0·T` | 0,533 | 2,667 |
| Idle | sekitar 2,5 detik, goyang kaku tanpa napas | sekitar 3 detik, napas dan jubah ikut terlambat |
| Clamp multiplier | 0,5–1,3 | 0,6–1,2 |
| Threshold Move di blend tree | 1,0 (= 0,5 × v0) | 3,0 (= 0,6 × v0) |

- **Move Tulang:** kaki menapak dihitung dengan IK dua tulang analitis di script. Kaki tumpu bergerak mundur lurus pada `v0`, lalu sudutnya di-key sebagai FK. Ayunan paha sekitar ±30°. Tubuh atas kaku: perisai diam di depan, pedang bergetar kecil dan ujungnya tertinggal, torso bergoyang ke samping. Tengkorak mendarat 1–2 frame terlambat di langkah berselang.
- **Move Pyromancer:** tidak ada kaki. Rotasi tulang jubah dihitung dari fase "paha bayangan" yang sama:
  - Rantai depan sisi kaki depan maju sekitar 15–20°, rantai belakang berlawanan 5–10°.
  - Tulang `_2` tertinggal sekitar 2 frame dari `_1`.
  - `hips` naik-turun dua kali per siklus (6–7 cm puncak ke lembah), titik terendah tepat setelah kontak.
  - Lengan kiri berayun berlawanan langkah. Tangan kanan (bara) tetap stabil ke depan.
  - Ujung jubah tetap di ketinggian lantai walau `hips` naik-turun. Tulang `_2` diberi key lokasi yang membatalkan bob `hips` (tulang `_2` tidak tersambung ke `_1`), jadi yang terlihat adalah jubah memendek dan memanjang, bukan ujung jubah yang terangkat dari lantai atau tenggelam ke lantai. Bobot jubah bagian bawah dibuat condong ke `_2` supaya regangannya rata. *(Keputusan pembuat desain setelah review.)*
- Frame terakhir loop sama dengan frame pertama, dan tidak ada key scale.

### Ekspor dan impor

- Script menggabungkan semua mesh satu karakter menjadi satu mesh dengan modifier diterapkan, memasang armature dan bobot, membuat action `Idle` dan `Move` sebagai NLA strip, lalu mengekspor dengan `bpy.ops.export_scene.fbx` langsung menimpa FBX yang ada. GUID dan remap material di `.meta` tetap berlaku.
- Opsi ekspor: tanpa leaf bone, hanya tulang deform, animasi dari NLA strip (bukan semua action), simplify 0. Sumbu disamakan dengan ekspor lama.
- Unity (diatur di builder, bukan manual): rig Generic, avatar dari model ini, Idle dan Move dengan Loop Time menyala, root motion mati, kompresi animasi mati karena rig kecil mudah bergetar.
- Builder `Runecrypt/3D/Build Arena` membuat AnimatorController per karakter dan memasang Animator di model pada prefab.
  - Isi controller: parameter `Speed` dan `SpeedMul`, satu state Locomotion berisi blend tree 1D (threshold diketik manual sesuai tabel), dan Speed Multiplier state diikat ke `SpeedMul`.

### Kode

- Satu komponen kecil dipakai bersama Player3D dan Enemy3D. Setiap frame ia membaca kecepatan horizontal `Rigidbody.linearVelocity`, lalu mengisi `Speed` (diredam sekitar 0,1 detik) dan `SpeedMul = clamp(speed / v0, min, max)`. Dengan begitu, saat karakter terhalang dinding, animasinya kembali Idle.
- Pyromancer mundur: kalau komponen kecepatan searah hadap tubuh lebih kecil dari −0,3 × |v|, `SpeedMul` dibuat negatif sehingga klip diputar terbalik. Gerak menyamping tetap memakai klip maju.
- Hitungan `SpeedMul` berupa fungsi murni yang diuji EditMode. `v0` disimpan di satu tempat di sisi Unity dan dicocokkan dengan panjang klip hasil impor lewat test.
- Ayunan naik-turun `visual.localPosition` di `Enemy3D` dihapus, karena digantikan klip. `Player3D` tetap memutar `visual` ke arah bidik.

## Diserahkan ke pembuat

- Struktur file script Blender: helper bersama atau satu script per karakter, asalkan dijalankan setelah script model.
- Amplitudo persis, sebagai titik awal dari `laporan.md` yang disetel lewat screenshot.
- Nama test, nama komponen, dan letak `v0` di Unity.
- Pengaturan sumbu ekspor yang membuat depan tetap +Z. Ini dibuktikan dengan satu ekspor uji kecil sebelum klip final, bersama pengecekan nama take dan jumlah tulang di Unity.

## Selesai berarti

- Kaki dan lengan Tulang mengayun saat bergerak dan kakinya tidak meluncur, sedangkan jubah Pyromancer bergoyang dan melangkah.
- Keduanya kembali ke Idle saat diam, dan laju klip mengikuti kecepatan.
- Test baru gagal dulu lalu lulus, dan suite EditMode lulus semua (dijalankan dengan scene M1 terbuka).
- Sudah diverifikasi di Play mode lewat screenshot, sudah dimainkan user, dan sudah di-commit di `feature/3d-playable`.
