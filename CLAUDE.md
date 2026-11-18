# Runecrypt

Proyek akhir mata kuliah Pengembangan Gim (PGIM), dipamerkan di expo kampus. Game baru, bukan mod.
**Expo: perkiraan pertengahan Januari 2027**, tanggal pasti belum ada. Dari 16 Nov 2026 tersisa sekitar 8 minggu, dipotong libur akhir tahun.
**Tim: 5 orang**, tapi untuk sekarang hanya user sendiri (dibantu Claude) yang mengerjakan Unity. Scope Unity dihitung untuk 1 orang: MVP dulu, fitur tambahan hanya kalau ada sisa waktu.
**Penilaian (RPS 2026/2027, `D:\Claude\PGIM\RPS Pengembangan Gim 2026_2027.docx.pdf`).** Tidak ada rubrik khusus untuk game-nya. Bobot yang menyangkut tugas besar:
- CPMK 3 (30%): GDD harus lengkap sesuai kaidah. Dinilai dari tugas besar yang selesai ditambah penilaian antarteman (60%) dan presentasi progres per kelompok (40%).
- CPMK 4 (25%): evaluasi game dan presentasi perbaikan. Tugas besar dinilai dari hasil evaluasi (40%), sisanya UAS. Jadi hasil playtest dan perbaikan yang dilakukan harus terdokumentasi.
- CPMK 2 (25%): pengembangan di Unity. Materi kuliahnya meliputi scene, kondisional, input, timer, animasi, dan suara, jadi game sebaiknya memperlihatkan semuanya.
- Urutan sesi: presentasi ide dan GDD (17), mini project 1–5 (18–22), prototipe (23), presentasi progres (24), playtesting dan perbaikan (25–26), presentasi akhir dan persiapan Informatics Expo (27–28), lalu UAS.
- Posisi kelas: pertemuan 15 pada 16 Nov 2026. **Presentasi ide dan progres GDD (sesi 17) sudah dekat**, jadi GDD diprioritaskan sebelum kode.
Balas user dalam **Bahasa Indonesia**.

## Konsep (sudah diputuskan)
- Judul sementara **Runecrypt** (belum dicek bentrok nama). Roguelite action RPG berbasis ruangan.
- Struktur genre seperti The Binding of Isaac: ruangan acak dari prefab buatan tangan, pintu terkunci sampai musuh habis, relic bertumpuk. **Tema, art, item, dan karakter harus orisinal, jangan meniru Isaac** (nama, karakter, art, musik, layout).
- Tone gelap dan menegangkan, tetap seru. Hook: makam terkutuk yang menelan cahaya, dan elemen pemain adalah satu-satunya sumber cahaya.
  - Api: cahaya hangat, jangkauan kecil. Es: biru redup tapi luas. Petir: kilatan yang menerangi seluruh ruangan sesaat.
  - Musuh di gelap hanya terlihat dari mata merahnya.
  - **Cahaya juga aturan main** (diputuskan 17 Nov 2026): musuh yang terang menerima damage +25%, homing dan efek pencari sasaran hanya bisa memilih musuh terang, dan musuh di gelap bergerak 20% lebih cepat. Dihitung di kode (radius dan raycast dinding), bukan dari Light2D. Detail: GDD bagian 5.5.
  - Reaksi: Api+Es = Uap (damage area, membutakan), Es+Petir = Shatter, Api+Petir = Ledakan cahaya.
- Class: Pyromancer (api), Frost Warden (es), Stormcaller (petir).
- **Elemen kedua untuk reaksi datang dari ruangan dan musuh** (diputuskan 17 Nov 2026), contohnya genangan es, tiang petir, dan musuh berelemen. Pemain tidak berganti elemen. Dengan begitu MVP 1 class tetap bisa memicu reaksi.
- Progres dalam run: relic mengubah cara menyerang (split, pierce, bounce, homing), level-up memperdalam elemen (pilih 1 dari 3), Shrine menukar max HP dengan relic terkutuk.
- Satu run 6–8 menit untuk expo: tutorial singkat, 8–12 ruangan, boss 3 fase dengan telegraph AoE, lalu leaderboard lokal.
- **Booth:** sekitar 5 laptop ditambah 1 monitor milik user. Spesifikasi laptop **tidak** dijadikan batasan: grafis memakai kualitas terbaik, termasuk ShadowCaster2D, tanpa mode kualitas rendah (keputusan user 17 Nov 2026). Leaderboard lokal per laptop, tanpa jaringan.
- **Kontrol:** setiap laptop memakai mouse dan keyboard laptop, ditambah 1 gamepad untuk stasiun monitor. Kontrol utama: WASD dan bidik mouse 360°. Gamepad: twin-stick. Panah keyboard sebagai cadangan. Semua lewat Input System.
- Durasi (usulan, belum final): run inti pendek sampai boss, lalu opsi turun lebih dalam tanpa batas (prefab sama, lebih gelap dan lebih sulit). Leaderboard mencatat kedalaman dan waktu.
- Harus tetap terbaca di ruang expo yang terang: rim light, mata musuh menyala, telegraph terang. Sediakan headphone di booth.
- **MVP:** 1 class, generator ruangan, 5 musuh (termasuk Mimik), 15 relic, 2 reaksi, 1 boss, peti boss (hadiah, Mimik, atau kutukan), 3 jenis pintu dan Gerbang Segel, komik pembuka, cutscene peti, HUD, leaderboard. Detail: `Docs/specs/2026-11-18-peti-pintu-cerita-design.md` (disetujui 18 Nov 2026).
- **Kalau sempat:** 3 class, 30 relic, Shop/Shrine, co-op lokal 2 pemain, lantai kedua.
- Detail desain: `Docs/GDD.md`. **Struktur GDD wajib mengikuti panduan dosen** di `D:\Claude\PGIM\Game-Design-Document-Template.pdf` (Game Name, Genre, Game Elements, Player, Technical Specs, Game Play + Outline, Key Features, Design Guidelines, Game Design Definitions, Game Flowchart, Player Definition/Properties/Rewards, UI) dan slide `Copy of Pengembangan Gim 10 - Dokumen Desain Gim.pptx (1).pdf` (GDD harus mencakup mechanics, UI, visuals, audio, story, level specs; GDD itu living document). Tulis game play secara deskriptif dengan nama spesifik, jangan pakai istilah generik seperti "enemy_1". Rencana kerja: `Docs/plans/`. Halaman konsep lama (masih versi 3D, perlu diperbarui): https://claude.ai/artifact/MyWY4YWfySHoiNCpeFdDn8 (salinan di `ArtSource/runecrypt.html`).

## Art
- **2D pixel art 16x16.** Semua aset dibuat Claude sendiri, **tanpa asset pack pihak ketiga**, demi gaya yang konsisten.
- Sprite dibuat lewat script Python + Pillow (Python 3.12) di `ArtSource/scripts/` (`make_sprites.py`, `make_anim.py`). Output di `ArtSource/prototype/`. Jangan edit PNG hasil generate dengan tangan; ubah script-nya lalu jalankan ulang.
- Utamakan makhluk melayang atau berjubah supaya animasinya realistis untuk dibuat. Tambahkan juice di engine: flash putih saat kena damage, squash and stretch, dissolve.
- Catatan terbuka: wisp masih terlihat imut dan perlu dibuat lebih seram. Animasi serang, animasi mati, dan boss berukuran besar belum diuji.

## Audio
- SFX dan musik dibuat Claude lebih dulu, lewat script Python di `ArtSource/scripts/`. Kalau hasilnya kurang, baru boleh memakai sumber SFX gratis. Utamakan lisensi CC0, dan catat sumber serta lisensinya di GDD (bagian kredit).

## Engine
- Unity **6000.3.26f1** (6.3 LTS), template Universal 2D (URP 17.3, 2D Renderer). Paket terpasang antara lain Input System, Tilemap (+Extras), 2D Animation, Test Framework.
- Cahaya memakai URP 2D: `Light2D` dan `ShadowCaster2D`.
- Repo git lokal, branch utama `main`. Fitur dikerjakan di branch `feature/<nama>` di checkout yang sama, karena Unity Editor dan MCP terbuka di folder ini (bukan worktree). Tanpa remote.

## Unity lewat MCP
- MCP for Unity (CoplayDev) memakai transport HTTP Local di `http://127.0.0.1:8080/mcp`, dengan server `UnityMCP`.
- Kalau tool `mcp__UnityMCP__*` tidak muncul: Unity harus terbuka, server di **Window → MCP for Unity** harus berjalan, lalu mulai ulang sesi Claude.
- Pakai skill `unity-mcp-skill` (unity-mcp-orchestrator) untuk semua pekerjaan Unity. Mulai dari resource (`mcpforunity://editor/state`), lalu kerjakan dengan tool. Setelah menulis script, tunggu `is_compiling == false`, lalu `read_console`. Verifikasi visual dengan screenshot.
- Hasil skill-inspector 2026-11-16: **CAUTION**. Isinya hanya dokumentasi (tanpa skrip). `manage_editor deploy_package/restore_package`, `manage_packages` (add/remove), dan `execute_code` **boleh dipakai setelah user mengizinkan** di chat. Minta izin untuk setiap pemakaian, sebutkan apa yang akan dilakukan.

## Aturan pemakaian skill (wajib)
User ingin SETIAP tahap memakai skill Claude yang terpasang sesuai kebutuhan. Sebelum bertindak: **sebutkan skill yang dipakai, lalu panggil lewat tool Skill.**
| Tahap | Skill |
|---|---|
| Ide atau desain baru, perubahan GDD | `superpowers:brainstorming` |
| Rencana kerja sebelum implementasi | `superpowers:writing-plans` |
| Menjalankan rencana | `superpowers:executing-plans` / `incremental-implementation` |
| Logika gameplay (damage, reaksi, generator, relic) | `superpowers:test-driven-development` (EditMode test, Unity Test Framework) |
| Error, bug, kompilasi gagal | `superpowers:systematic-debugging` |
| Semua operasi Unity Editor | `unity-mcp-skill` |
| Sebelum menyatakan selesai | `superpowers:verification-before-completion` |
| Review kode | `code-review-and-quality` / `superpowers:requesting-code-review` |
| Git (setelah repo dibuat) | `git-workflow-and-versioning` |
| Skill baru, misalnya "sprite Runecrypt" (palet, ukuran, aturan gaya) | `anthropic-skills:skill-creator` |
| Memasang skill dari luar | `skill-inspector` dulu |
| File aturan atau konteks seperti ini | `context-engineering` |

## Prinsip arsitektur
- **Game harus tetap mudah ditambah fitur atau konten setelah jadi** (permintaan user 17 Nov 2026). Konten seperti relic, musuh, reaksi, ruangan, dan class sebaiknya berupa data atau prefab, bukan kode yang ditulis ulang. Menambah relic baru tidak boleh mengharuskan mengubah kode pemain.

## Batasan
- Jangan menulis kode gameplay sebelum rencana kerja disetujui user.
- Jangan menambah asset pack, paket, atau dependensi tanpa izin user.
- Scope expo diutamakan: MVP selesai dan bisa dimainkan dulu, baru tambahan.
