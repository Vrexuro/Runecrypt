# Desain: Peti Boss, Pintu, Kenaikan Kesulitan, dan Cerita

> Status: **disetujui user 18 Nov 2026**. Hasil brainstorming 17–18 Nov 2026, berangkat dari usulan user di sesi baseline M1 (GDD §13).
> Setelah disetujui: GDD, roadmap, dan daftar MVP di CLAUDE.md diperbarui. Rencana kerja ditulis per milestone (M3, M4, M5) tepat sebelum milestone itu dimulai, sesuai aturan roadmap.

## 1. Maksud

Setelah memainkan M1, user merasa game ini perlu tiga hal: **alasan untuk terus maju** (pintu, lantai, musuh yang makin kuat), **kejutan** (peti boss yang bisa berbalik menjadi jebakan), dan **cerita**, supaya Runecrypt tidak hanya soal mengalahkan musuh. Semuanya harus tetap pas untuk expo: satu run 6–8 menit, pengunjung baru di setiap giliran, dan Unity dikerjakan satu orang.

**Tujuan**
- Pemain langsung tahu jenis pintu di depannya: pintu biasa, pintu boss, atau jalan ke lantai berikutnya.
- Peti boss terasa menegangkan sampai tutupnya terbuka.
- Pengunjung paham siapa Pyromancer dan kenapa ia turun ke makam, dalam waktu sekitar 20 detik.
- Lantai yang lebih dalam terasa lebih berbahaya, tetapi tetap adil.

**Bukan tujuan (di luar MVP)**
- Boss tengkorak (sang guru), boss lain, dan dasar makam.
- Komik untuk Frost Warden dan Stormcaller. Sistemnya siap, kontennya menyusul.
- Cutscene di luar peti boss dan komik pembuka.

**Anti-tujuan (gagal walaupun fiturnya jalan)**
- Cutscene membuat antrean expo tersendat, karena terlalu panjang atau tidak bisa dilewati.
- Zonk terasa curang. Contohnya pengunjung yang baru sekali main langsung kena zonk, atau zonk muncul dua kali berturut-turut.
- Ruangan penuh musuh sampai layar gelap tidak terbaca lagi.
- Menambah boss, kutukan, atau komik baru mengharuskan mengubah kode.

## 2. Batasan
- Unity 6.3 URP 2D. **Tidak ada paket baru.** `com.unity.timeline` 1.8.13 sudah terpasang.
- Semua art dibuat lewat script Python di `ArtSource/scripts/`, termasuk panel komik dan sprite peti, Mimik, serta pintu.
- Konten berupa data (ScriptableObject atau prefab), sesuai prinsip arsitektur di CLAUDE.md.
- Logika (undian peti, rumus kesulitan, urutan komik) ditulis dengan TDD memakai EditMode test.

## 3. Pendekatan cutscene (dipilih user: C, campuran)

| Pendekatan | Kelebihan | Kekurangan |
|---|---|---|
| A. Unity Timeline untuk semua | Visual, cocok untuk animasi di dunia game, materi animasi dan timer di CPMK 2 terlihat jelas | Sulit diuji otomatis. Komik class baru harus disusun manual. |
| B. Pemutar panel berbasis data untuk semua | Class baru cukup ditambah aset data. Bisa diuji. | Kaku untuk animasi peti dan Mimik |
| **C. Campuran (dipilih)** | Komik memakai B, peti boss memakai A. Masing-masing dipakai di tempat yang paling cocok. | Ada dua sistem, tetapi keduanya kecil |

## 4. Kenaikan kesulitan (disetujui user)

**Per ruangan.** HP dan damage musuh **tidak** naik. Yang bertambah hanya jumlah musuh, lewat budget di GDD §6.2.
- **Batas: paling banyak 8 musuh per ruangan.** Supaya layar tetap terbaca di gelap dan pemain tidak dikerubungi dari luar layar.

**Per lantai** (kedalaman *d*, mulai dari 1):
- HP musuh: ×(1 + 0,15 × (d − 1)). Aturan ini sudah ada di GDD §9.3.
- Damage pukulan musuh biasa: 1 + floor((d − 1) / 2) HP. Artinya setengah hati setiap 2 lantai:

| Lantai | Damage per pukulan |
|---|---|
| 1–2 | 1 HP |
| 3–4 | 2 HP |
| 5–6 | 3 HP |

- *Keputusan saya:* damage boss memakai tambahan yang sama: 2 HP di lantai 1–2, 3 HP di lantai 3–4, dan seterusnya.
- Aturan lain di GDD §9.3 tetap berlaku: radius cahaya pemain −10% per lantai (minimal 60%), budget musuh +25% per lantai.
- Rumusnya ada di satu kelas Core yang murni logika, dan angkanya disimpan di aset data. Menyetel ulang angka tidak perlu mengubah kode.

## 5. Pintu

| Pintu | Tampilan | Perilaku |
|---|---|---|
| **Pintu ruangan** | Kayu dan besi gelap, tanpa cahaya | Jeruji turun saat musuh muncul, lalu naik setelah musuh habis (sudah ada di GDD) |
| **Pintu ruang harta** (opsional) | Pintu ruangan dengan bingkai bersisik emas | Sama dengan pintu ruangan |
| **Pintu boss** | **Mengikuti boss di dalamnya.** Prefab pintunya disimpan di data boss. | Memberi petunjuk tentang boss yang menunggu |
| **Gerbang Segel** | Batu besar dengan rune api yang menyala terang. Sekaligus menjadi sumber cahaya. | Muncul setelah boss kalah, membawa pemain ke lantai berikutnya |

**Pintu boss Sang Pemadam (MVP)**
- Bentuknya tirai jubah gelap, dengan asap hitam merembes dari celah dan dua mata merah di baliknya.
- Saat pemain berada dalam jarak sekitar 3 tile, obor di kiri dan kanan pintu padam, dan cahaya pemain sedikit meredup. Angka pastinya disetel saat playtest.
- Contoh untuk boss lain di masa depan: boss tengkorak memakai bingkai tulang dan rahang tengkorak.

**Gerbang Segel.** Gerbang ini menggantikan simpul "Turun lebih dalam?" di flowchart GDD §3.3.
- Saat pemain menyentuh gerbang, muncul pilihan: **Turun lebih dalam** (kedalaman +1) atau **Keluar makam** (ke Layar Akhir dan leaderboard).
- Saat boss lantai 1 kalah, tungku mati di arena boss menyala kembali. Ini menjadi "akhir kecil" untuk pengunjung yang berhenti di lantai 1. Bentuknya hanya efek visual, tidak memakai cutscene.

## 6. Peti Boss

Peti muncul di arena boss bersamaan dengan Gerbang Segel.

**Kapan isinya diundi.** Isi peti diundi **saat peti muncul**, bukan saat dibuka. Dengan begitu, Mimik sudah bisa memperlihatkan tandanya sebelum dibuka.

**Tabel hasil** (aset data, bobotnya bisa disetel):

| Hasil | Bobot | Yang terjadi |
|---|---|---|
| Hadiah | 70 | 1 relic dari kumpulan relic yang belum dimiliki pemain. Relic langka lebih jarang muncul. |
| Mimik | 15 | Peti hidup menyerang. Kalau dikalahkan, Mimik menjatuhkan hadiah seperti di baris di atas. |
| Kutukan | 15 | 1 kutukan kecil yang diambil dari daftar kutukan |

**Pengaman**
1. Peti boss pertama dalam satu run selalu berisi Hadiah.
2. Kalau peti sebelumnya zonk (Mimik atau Kutukan), peti berikutnya selalu berisi Hadiah.
3. Kalau kumpulan relic sudah habis, Hadiah diganti dengan pemulihan HP penuh.

*Keputusan saya:* relic diberi kolom kelangkaan (Biasa atau Langka) dengan bobot 3 : 1. Pembagian 15 relic MVP ke dua kelompok itu dilakukan saat M4.

**Kutukan (MVP: 1 kutukan, disimpan sebagai data)**
- **Kutukan Redup:** radius cahaya pemain −20% sampai lantai berikutnya selesai. Ikon kutukan tampil di HUD.
- *Keputusan saya:* pengurangan −20% dihitung **setelah** batas minimal 60% di §9.3. Dengan begitu, kutukan tetap terasa di lantai yang dalam.
- *Keputusan saya:* peti boleh diabaikan. Pemain bisa langsung masuk ke Gerbang Segel tanpa membuka peti.
- Kutukan berikutnya, misalnya Kutukan Lamban, cukup ditambah sebagai aset data tanpa mengubah kode pemain.

**Mimik (musuh ke-5 MVP)**
- **Wujud:** peti kayu makam dengan gigi, lidah, dan mata merah.
- **Tanda sebelum dibuka:** setiap 4–6 detik peti "bernapas" (squash 2 frame) atau berkilat merah di celah tutupnya.
- **Perilaku:** melompat ke arah pemain 3 kali, lalu diam 1,2 detik. Saat diam, Mimik bisa diserang dengan aman.
- **HP awal:** sekitar 3 kali Tulang Penjaga, lalu disetel saat playtest. HP dan damage-nya mengikuti aturan per lantai.
- **Gaya mati: berdarah.** Mimik punya daging, jadi gaya mati berdarah yang Anda inginkan untuk musuh berdaging akhirnya tampil di expo.

## 7. Cutscene Peti Boss (Unity Timeline)

**Kapan dan berapa lama**
- Diputar setiap kali peti boss dibuka.
- Pembuka sekitar 1 detik dan sama untuk semua hasil: peti bergetar, cahaya merembes dari celah, kamera mendekat.
- Akhir sekitar 1–1,5 detik, dan ada tiga versi:
  - **Hadiah:** cahaya emas meledak, lalu relic melayang naik.
  - **Mimik:** gigi terlihat, mata menyala, peti meraung, lalu pertarungan dimulai.
  - **Kutukan:** asap hitam menyembur dan menghisap cahaya di sekitar pemain.
- Total durasi paling lama 2,5 detik.

**Aturan selama cutscene**
- Selama cutscene, pemain tidak bisa diserang dan tidak bisa bergerak.
- Tombol Lewati (tombol tembak, Enter, atau tombol A gamepad) langsung melompat ke keadaan akhir.
- Mimik baru mulai menyerang setelah cutscene selesai atau dilewati.

## 8. Komik Pembuka (pemutar panel berbasis data)

Komik ini mengisi simpul "Layar Cerita singkat" di flowchart GDD §3.3.

**Kapan diputar**
- Setiap kali run baru dimulai dari layar judul.
- Restart dengan R setelah tumbang tidak memutar komik lagi.

**Isi dan durasi**
- 5 panel, masing-masing sekitar 4 detik. Setiap panel berisi gambar, teks bahasa Indonesia, dan gerak kamera pelan dari satu posisi ke posisi lain.
- Panel adalah pixel art buatan script dengan kanvas lebih besar dari sprite biasa. Ukuran kanvas diputuskan saat M5.

**Kontrol**
- **Satu kali tekan tombol Lewati melewatkan seluruh komik.** Antrean expo diutamakan.

**Data**
- Satu aset data berisi urutan panel untuk setiap class. Menambah class baru berarti menambah satu aset, tanpa kode baru.

**Pengurangan kalau jadwal tertinggal:** 5 panel menjadi 3, misalnya panel 1, 3, dan 5.

### Naskah Pyromancer (disetujui user)
1. **Desa di kaki bukit, malam hari.** Di puncak bukit berdiri Runecrypt yang selalu menyala. *"Selama api makam menyala, malam tidak pernah benar-benar gelap."*
2. **Murid terakhir.** Pyromancer muda berlatih menjaga bara di bawah bimbingan gurunya, penjaga api terakhir yang masih hidup. *"Api bukan untuk membakar. Api untuk menjaga."*
3. **Cahaya padam.** Cahaya di puncak bukit mati, dan obor-obor desa ikut padam satu per satu. Sang guru turun ke makam dan tidak pernah kembali.
4. **Bara yang kembali.** Yang kembali hanya lentera sang guru, berisi satu bara kecil yang masih hidup. Bara itu condong ke arah makam.
5. **Gerbang makam.** *"Kalau bara ini padam, tidak ada lagi cahaya."* Lalu muncul judul **RUNECRYPT**.

## 9. Susunan Cerita per Lantai (disetujui user)

| Lantai | Yang terjadi dalam cerita | Status |
|---|---|---|
| 1 | Musuh Tulang adalah para penjaga api yang jatuh. Pemain mengalahkan wujud pertama Sang Pemadam, tungku makam menyala lagi, dan Sang Pemadam mundur ke bawah. | MVP |
| 2 dan seterusnya | Sang Pemadam makin kuat karena sudah menelan lebih banyak cahaya. Tulisan dinding dan pecahan lentera menunjukkan sang guru pernah lewat. | MVP: 3 tulisan dinding *(keputusan saya)* |
| 3 | Boss tengkorak, yaitu sang guru, di balik pintu berbingkai tulang. Kalau kalah, ia menyerahkan baranya. | Kalau sempat |
| Dasar makam | Akhir cerita | Belum dibuat. Lantai berlanjut tanpa batas. |

GDD §2.2 diperbarui. Kalimat "tanpa cutscene panjang" diganti dengan "komik pendek yang bisa dilewati".

## 10. Penempatan di Roadmap

| Milestone | Tambahan |
|---|---|
| M3 Musuh | Mimik: perilaku, tanda, dan gaya mati berdarah |
| M4 Lantai | 3 jenis pintu, data pintu di boss, rumus kesulitan per ruangan dan per lantai, batas 8 musuh, kolom kelangkaan relic |
| M5 MVP | Peti boss dan tabel undian, Kutukan Redup, cutscene peti (Timeline), komik pembuka, Gerbang Segel, tungku yang menyala lagi, 3 tulisan dinding |

**Urutan pemotongan GDD §11** mendapat tambahan, setelah "attract mode" dan sebelum "minimap": pintu ruang harta → komik 5 panel menjadi 3 → tulisan dinding.

**Bagian yang tidak pernah dipotong** bertambah: peti boss dengan ketiga hasilnya, dan Gerbang Segel.

**Daftar MVP di CLAUDE.md** menjadi: 1 class, generator ruangan, **5 musuh (termasuk Mimik)**, 15 relic, 2 reaksi, 1 boss, **peti boss dengan Mimik dan kutukan, komik pembuka, cutscene peti**, HUD, leaderboard.

## 11. Bukti selesai (dipakai saat menulis rencana per milestone)
- **EditMode test:**
  - rumus kesulitan untuk d = 1 sampai 6;
  - batas 8 musuh;
  - undian peti: peti pertama selalu Hadiah, tidak ada zonk dua kali berturut-turut, kumpulan relic kosong diganti pemulihan HP, dan distribusi bobot dalam toleransi pada 10.000 undian dengan seed tetap;
  - pemutar komik: maju per durasi, satu tekan Lewati mengakhiri komik, tidak diputar ulang saat restart dengan R.
- **Uji di Play mode:** cutscene peti bisa dilewati, pemain kebal selama cutscene, dan Mimik baru menyerang setelah cutscene.
- **Playtest M6:** penguji bisa menebak pintu boss tanpa diberi tahu, dan dari daftar hasil zonk tidak ada yang merasa dicurangi.

## 12. Diserahkan ke pembuat
- Angka akhir HP Mimik, durasi tanda "bernapas", dan besar peredupan di pintu boss. Disetel saat playtest.
- Ukuran kanvas panel komik dan gaya tipografinya.
- Struktur prefab Timeline: satu Timeline dengan 3 jalur akhir, atau 3 Timeline.
- Posisi peti dan Gerbang Segel di arena boss.
