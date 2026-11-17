# Desain M1.5: Juice dan Suara

Status: **draf, menunggu persetujuan user.** Tanggal 17 Nov 2026. Riset pendukung ada di `Docs/research/2026-11-17-game-feel.md`.

## 1. Maksud dan alasan

Prototipe M1 sudah bisa dimainkan, tapi setiap aksi masih terasa datar:
- Musuh langsung hilang saat mati.
- Tidak ada kedipan, hit-stop, maupun suara.

User ingin game tetap 2D, tetapi terasa berbobot seperti Binding of Isaac, terutama di tiga momen: saat serangan kena, angka damage, serta animasi dan suara saat musuh mati. Yang ditiru dari Isaac hanya **tekniknya**. Art, suara, dan bentuk efek tetap orisinal, dan temanya cahaya.

Alasan kedua adalah penilaian. CPMK 4 menilai evaluasi dan perbaikan yang terdokumentasi. Karena itu milestone ini dirancang supaya menghasilkan bukti sebelum dan sesudah perbaikan.

## 2. Tujuan, bukan tujuan, dan anti-tujuan

**Tujuan.** Semua konten M1 yang sudah ada mendapat feedback lengkap, baik visual maupun suara:
- tembakan bola api,
- bola api mengenai Tulang Penjaga,
- Tulang Penjaga mati,
- Uap,
- pemain terkena serangan.

**Bukan tujuan milestone ini:**
- Garis tepi Tersingkap. Tetap di M2.
- Musik.
- Feedback untuk musuh M3.
- Implementasi efek berdarah. Yang disiapkan hanya slot datanya.
- Animator dan state machine. Tetap di M2.
- Menu volume.

**Anti-tujuan.** Hasil seperti ini dianggap gagal walaupun secara teknis berjalan:
- Kedipan putih satu layar penuh, atau kedipan cepat berulang (risiko fotosensitif).
- Guncangan layar pada setiap tembakan biasa, yang memicu mabuk gerak.
- Efek yang menutupi musuh atau proyektil sehingga game sulit dibaca di ruang expo yang terang.
- FPS turun di bawah 60 saat gelombang 4.
- Memakai aset, nama, atau bentuk efek yang meniru Isaac.

## 3. Batasan

- **Tanpa paket baru.**
  - Tidak ada Cinemachine, jadi guncangan kamera ditulis sendiri.
  - TextMeshPro tidak dipakai, karena angka damage dibuat dari sprite.
- **SFX hanya dari sumber CC0** yang sudah disetujui. Kreditnya dicatat di GDD §14.
- **Sprite baru dibuat lewat script Python** di `ArtSource/scripts/`. Hasil generate tidak diedit dengan tangan.
- **Berbasis data** (prinsip arsitektur CLAUDE.md). Musuh atau efek baru cukup menunjuk aset profil, tanpa mengubah kode pemain atau musuh.
- **Logika murni** (hit-stop, decay guncangan, format angka, batas suara) masuk `Runecrypt.Core` dan diuji dengan EditMode test.

## 4. Pendekatan

| | Pendekatan | Untung | Rugi |
|---|---|---|---|
| A | Juice dulu, uji seru sesudahnya | Uji seru menilai rasa yang sebenarnya | Tidak ada data "sebelum" |
| **B (dipilih user)** | Baseline dulu, lalu juice, lalu uji seru | Bukti sebelum dan sesudah untuk CPMK 4, biaya tambahannya hanya 5 menit | Butuh disiplin mencatat dua kali |
| C | Juice dicicil di M2 sampai M4 | Jadwal tidak berubah | Game datar sampai mendekati sesi 23 dan 24 |

**Urutan B:**
1. User memainkan M1 polos selama 5 menit dan mencatat jawaban pertanyaan tetap di GDD §13 (build "M1-baseline").
2. Milestone M1.5 dikerjakan.
3. Dua penguji di luar tim mencoba build "M1.5", dan user mengulang uji dengan pertanyaan yang sama.

Kedua hasil dicatat berdampingan di GDD §13.

## 5. Isi feedback per kejadian

Semua angka adalah *keputusan Claude* dan boleh disetel ulang setelah uji seru. Sumber angka acuannya ada di catatan riset.

| Kejadian | Visual | Hit-stop | Guncangan (trauma 0–1) | SFX (CC0) |
|---|---|---|---|---|
| **Tembak** | Cahaya pemain berdenyut +30% intensitas selama 0,05 dtk. Kamera tersentak 1 piksel berlawanan arah bidikan. | — | — | Fire Staff 6, 7, atau 9 (diacak), pitch ±5% |
| **Bola api kena, Tersingkap** | Kedipan putih penuh 0,08 dtk, knockback, percikan bara (6 partikel), angka damage kuning terang ukuran besar | 0,03 dtk | — | Kena C (klik tulang dan ekor api) |
| **Bola api kena, gelap** | Kedipan putih redup (alpha 0,35) 0,08 dtk, knockback, angka damage abu-abu ukuran normal | 0,03 dtk | — | Kena C, volume −4 dB |
| **Tick Terbakar** | Angka kecil jingga | — | — | — |
| **Tulang Penjaga mati** | Lihat bagian 5.1 | 0,06 dtk | 0,25 | Mati A |
| **Uap muncul** | Partikel yang sudah ada, ditambah cincin uap yang mengembang 0,2 dtk | 0,04 dtk | 0,3 | Uap (dua desis disambung) |
| **Tick Uap** | Kedipan redup, angka damage putih kecil, tanpa knockback | — | — | — |
| **Pemain kena** | Kedipan merah 0,1 dtk, lalu berkedip-kedip (alpha 0,4 ↔ 1, setiap 0,08 dtk) selama kebal 0,8 dtk | 0,08 dtk | 0,45 | **Belum ada sumber** (lihat bagian 8) |

**Cara Enemy mengenali jenis kena** (`HitInfo` tidak diubah):
- `Element.Fire` berarti bola api, dengan varian Tersingkap atau gelap dari `IsLitNow`.
- `Element.None` dari luar berarti tick Uap.
- Tick Terbakar terjadi di dalam `Enemy.Update`.
- Knockback hanya diberikan untuk bola api.

**Alasan kedipan di gelap tetap muncul walau redup:** kena serangan memperlihatkan siluet musuh sesaat. Ini memberi tahu pemain bahwa tembakannya kena, tanpa menghapus aturan "di gelap hanya mata yang terlihat" untuk waktu lama.

**Denyut cahaya tembakan** hanya mengubah intensitas Light2D, bukan radius. Area yang dihitung terang oleh aturan cahaya tidak berubah.

### 5.1 Kematian Tulang Penjaga, gaya "abu dan tulang"

1. Kedipan putih 0,06 dtk dan hit-stop 0,06 dtk. Tubuh musuh bertahan 0,06 dtk (tanpa collider) supaya kedipan ini benar-benar terlihat *(revisi review akhir)*.
2. Sprite tubuh hilang. Sebagai gantinya muncul:
   - 6–8 serpihan tulang (sprite 4x4 hasil script) yang meluncur menjauhi arah tembakan dengan kecepatan 2–4 tile/dtk sambil berputar. Gesekan menghentikannya dalam 0,3 dtk. Pantulan kecil disimulasikan dengan offset vertikal, karena kamera melihat dari atas. Setelah berhenti, serpihan diam 0,6 dtk lalu memudar;
   - kepulan abu (8 partikel abu-abu gelap).
3. Kilatan cahaya kecil: Light2D radius 1,5, intensitas turun dari 1 ke 0 dalam 0,15 dtk, tanpa bayangan.
4. Mata merah tetap melayang di posisi semula selama 0,3 dtk, lalu padam. Prefab `deathEffect` membawa sprite matanya sendiri, karena objek musuh sudah dihancurkan.
5. **Tumpukan abu tertinggal di lantai** (sprite 16x16 hasil script, urutan render di bawah aktor). Beberapa piksel bara di dalamnya berkedip pelan memakai material unlit. Tumpukan ini tidak memakai Light2D, demi performa. Ia tetap ada sampai scene dimuat ulang. Untuk M1.5 jumlahnya dibatasi 30, dan yang tertua dihapus lebih dulu.

### 5.2 Gaya mati per jenis musuh

`EnemyDefinition` mendapat empat field baru:
- `hitLit` dan `hitDark` (FeedbackProfile),
- `deathEffect` (prefab),
- `remains` (prefab sisa di lantai, boleh kosong).

Profil lainnya berada di data pemiliknya masing-masing:
- `PlayerController`: `shotProfile` dan `hurtProfile`.
- `ReactionDefinition`: `spawnProfile` (Uap).
- **Tulang Penjaga:** "abu dan tulang" seperti di atas.
- **Musuh berdaging di masa depan:** cukup prefab berbeda (cipratan dan noda darah) tanpa mengubah kode. Prefab berdarah **tidak** dibuat di milestone ini.

### 5.3 Angka damage

- **Font.** Digit 0–9 dibuat sebagai sprite pixel 3x5 lewat script Python, sehingga tidak ada lisensi font.
- **Pembulatan.** Damage dibulatkan ke bilangan bulat terdekat, minimal 1. Contohnya 3,75 tampil "4" dan 3 tampil "3", jadi bedanya terlihat. Tick Terbakar 1,25 tampil "1".
- **Ukuran.** Ukuran normal adalah skala 1, yaitu 5 piksel layar pada resolusi internal 320x180. Ukuran besar adalah skala 2.
- **Gerak.** Angka melambung naik dengan kecepatan horizontal acak ±1 tile/dtk dan kecepatan naik 3 tile/dtk, ditarik gravitasi 9 tile/dtk², lalu memudar setelah 0,6 dtk.
- **Material.** Unlit, supaya terbaca di gelap.
- **Pool.** Kapasitasnya 40 angka. Kalau habis, yang tertua dipakai ulang.

## 6. Arsitektur

| Bagian | Assembly | Tanggung jawab |
|---|---|---|
| `FeedbackProfile` (ScriptableObject) | Game | Data satu kejadian: kekuatan dan durasi kedipan, hit-stop, trauma guncangan, prefab partikel, daftar klip SFX, volume, rentang pitch. |
| `HitStopClock` | Core | Logika murni. Permintaan hit-stop yang tumpang tindih diambil yang **terpanjang** (tidak dijumlahkan). Menjawab "apakah sedang berhenti" berdasarkan waktu nyata. |
| `ShakeTrauma` | Core | Logika murni. Trauma 0–1 ditambahkan per kejadian lalu turun linear 1,5/dtk. Offset = trauma × maksimum 4 piksel, dibulatkan ke piksel. *(Direvisi setelah review akhir: trauma² × 3 px membuat guncangan mati dan Uap dibulatkan ke 0 px.)* |
| `DamageNumberFormat` | Core | Logika murni: pembulatan, warna, dan ukuran berdasarkan jenis kena (Tersingkap, gelap, Terbakar, Uap). |
| `VoiceLimiter` | Core | Logika murni. Batas 12 suara bersamaan secara global dan 4 per klip. Kalau penuh, suara tertua diganti. Klip yang sama yang mulai lagi dalam 30 ms dilewati, dan semua SFX dinormalkan ke puncak −6 dBFS supaya bunyi serempak tidak clipping *(revisi review akhir)*. |
| `FeedbackPlayer` | Game | Satu objek di scene. Memutar profil di sebuah posisi: memanggil HitStop, Shake, partikel (pool), dan SFX. |
| `HitStop` | Game | Mengatur `Time.timeScale` memakai `HitStopClock`. Ada flag statis `GamePause.IsPaused` yang dipasang ArenaDirector saat layar Tumbang. Aturannya: hit-stop tidak dimulai kalau flag aktif, dan saat hit-stop selesai, timeScale hanya dikembalikan ke 1 kalau flag tidak aktif. Kasus "pemain mati di tengah hit-stop" dengan begitu tetap berhenti total. |
| `CameraShake` | Game | Menggeser kamera per frame berdasarkan `ShakeTrauma`. Posisi dasar kamera disimpan terpisah supaya kamera tidak bergeser permanen. |
| `HitFlash` | Game | Child SpriteRenderer yang menyalin sprite induk di `LateUpdate`, setelah `SpriteFlipbook` mengganti frame. Memakai shader unlit buatan sendiri `Runecrypt/SpriteFlash` dengan pass yang dirender 2D Renderer (`SRPDefaultUnlit`), keluarannya warna × alpha sprite. Dipakai musuh dan pemain. |
| `DamageNumbers` | Game | Pool angka sprite. |
| `RemainsRegistry` | Game | Mencatat tumpukan abu. Batasnya 30, dan yang tertua dihapus lebih dulu. |
| `SfxPlayer` | Game | Pool AudioSource dan `VoiceLimiter`, dengan satu volume master statis. **Tanpa AudioMixer** di milestone ini, karena aset `.mixer` tidak bisa dibuat lewat script. Mixer dibuat manual di M5 bersama menu volume. |
| `Knockback` | Game, di dalam `Enemy` | Kecepatan dorong ditambahkan ke kecepatan kejar, lalu diredam 12/dtk. Kekuatan 4 tile/dtk dari bola api. Selama dorongan masih kuat, kecepatan kejar ditekan sebanding (`Knockback.Combine`), supaya musuh terlihat mundur, bukan sekadar tertahan *(revisi review akhir)*. Musuh yang terdorong tetap dihitung oleh aturan cahaya seperti biasa. |

**Perubahan pada kode M1:**
- `Enemy.TakeHit` memanggil profil kena yang sesuai (Tersingkap atau gelap) dan meminta angka damage. `Die` memunculkan `deathEffect` dan `remains`.
- `PlayerController` memutar profil tembak dan profil kena.
- `SteamCloud` memutar profil Uap.
- `HitInfo` **tidak** diubah. Arah knockback diambil dari `Point` ke posisi musuh.

**Alur aset SFX:**
- `ArtSource/scripts/make_sfx_cc0.py` adalah turunan bersih dari script percobaan. Script ini memotong dan menormalkan file CC0 mentah menjadi WAV 44,1 kHz mono di `Assets/Runecrypt/Audio/`.
- File mentah (32 MB) disimpan di `ArtSource/cc0_raw/` dan **di-ignore git**. URL asalnya ditulis di kepala script supaya hasilnya bisa dibuat ulang.
- Hanya WAV hasil potongan yang di-commit.
- Script percobaan sintesis `make_sfx_spike.py` dan folder `ArtSource/prototype/sfx/` dihapus.

## 7. Verifikasi

- **EditMode test:**
  - `HitStopClock`: tumpang tindih, berakhir tepat waktu.
  - `ShakeTrauma`: maksimum dibatasi 1, decay, dan offset nol saat trauma nol.
  - `DamageNumberFormat`: 3,75 jadi 4, 3 jadi 3, 1,25 jadi 1, 0,4 jadi 1, ditambah warna per jenis.
  - `VoiceLimiter`: batas global, batas per klip, dan pengganti tertua.
- **Play mode** (lewat MCP dan computer-use):
  - screenshot kedipan, angka, serpihan, dan tumpukan abu;
  - FPS pada gelombang 4 tetap ≥ 60;
  - layar Tumbang tetap berhenti total saat hit-stop terjadi bersamaan.
  - Kasus terakhir juga dikunci dengan PlayMode test bergaya `CombatPlayModeTests`: pemain mati tepat saat hit-stop, lalu `timeScale` tetap 0.
- **Suara.** Dicek user dengan headphone, karena Claude tidak bisa mendengar.
- **Uji seru.** Mengikuti urutan B di bagian 4.

## 8. Yang masih terbuka

- **SFX pemain kena dan dash.** Belum ada di empat paket CC0. Usulan: user mengunduh [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds) (CC0, tombol Download di halaman itu), lalu Claude memilih kandidatnya. Selama belum ada, kejadian ini berjalan tanpa suara.
- **Roadmap.**
  - M1.5 disisipkan dengan target ±29 Nov.
  - Juice dasar dikeluarkan dari M2, sehingga M2 menyusut dan targetnya tetap ±2 Des.
  - "SFX dan musik buatan script" di M5 diganti menjadi "SFX CC0 untuk musuh M3, musik".
- **GDD.** Diperbarui setelah dokumen ini disetujui:
  - §Audio (SFX dari CC0, bukan script),
  - §6 (gaya mati per musuh),
  - §Visual (angka juice di atas),
  - §14 (kredit).
- **Hal yang diserahkan ke builder:** warna persis partikel, bentuk serpihan tulang, dan kurva pudar, selama mengikuti palet yang sudah ada.
