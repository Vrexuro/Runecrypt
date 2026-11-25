# Desain: Skala dan Efek (M2.6), serta Pelajaran dari Soul Knight

> Status: **draf, menunggu persetujuan user**. Hasil brainstorming 25 Nov 2026.
> Berangkat dari laporan riset `reports/Analisis Soul Knight dan Prequel.md`.

## 1. Maksud dan alasan

User membandingkan Runecrypt dengan Soul Knight dan Soul Knight Prequel, lalu menyimpulkan tiga hal:
1. **Skill dan efeknya terasa "sepi"**, sedangkan efek di kedua game itu "heboh".
2. **Cahaya dari skill pemain perlu diperluas.**
3. **Map terasa kecil**, jadi ruangan perlu diperbesar. Musuh juga perlu ditambah supaya ruangan besar tidak terasa kosong.

User juga memutuskan bahwa **satu lantai sampai boss boleh sekitar 20 menit** (sebelumnya 6–8 menit), dengan lantai yang lebih dalam tetap menjadi lanjutan opsional tanpa batas.

Dari laporan riset, user memilih empat pelajaran untuk masuk GDD (bagian 6). Bantuan bidik (aim assist) untuk gamepad **tidak** dipakai.

## 2. Tujuan, bukan tujuan, anti-tujuan

**Tujuan M2.6** (dikerjakan sekarang, di arena M1/M2 yang ada):
- Arena terasa luas: ukurannya sekitar 1,5 kali layar, dan kamera mengikuti pemain.
- Arena terasa ramai: musuh datang dalam beberapa gelombang per "ruangan", dengan lebih banyak musuh di layar.
- Tembakan dan Kobaran terasa heboh, dengan cahaya sebagai sumber utama hebohnya.
- Cahaya skill lebih luas, sementara cahaya dasar pemain tetap.

**Bukan tujuan M2.6:**
- Objek elemen yang melukai semua pihak. Masuk M3, bersama Kristal Es dan Tiang Petir.
- Penghalang setengah tinggi. Masuk M4, saat prefab ruangan dibuat.
- Generator lantai dan ruangan bertipe-tipe (M4).
- Bantuan bidik gamepad (ditolak user).
- Efek reaksi Ledakan Cahaya dan Shatter (M3 dan "kalau sempat").

**Anti-tujuan (gagal walaupun fiturnya jalan):**
- Efek pemain menutupi mata merah musuh atau telegraph serangan, seperti kritik terhadap efek skill Prequel.
- Kamera membuat pusing: berguncang terus, berputar, atau tertinggal jauh dari pemain.
- Gelap tidak lagi menegangkan karena cahaya skill terlalu luas atau terlalu lama.
- FPS turun di laptop booth karena partikel dan lampu bertambah.

## 3. Batasan
- Unity 6.3 URP 2D, paket yang sudah terpasang saja (Pixel Perfect Camera sudah dipakai). Tidak ada paket baru.
- Sprite efek baru dibuat lewat script Python di `ArtSource/scripts/`. SFX baru, kalau ada, memakai sumber CC0 yang sudah diunduh, atau meminta izin dulu kalau perlu sumber baru.
- Logika baru (pengelompokan gelombang, batas musuh di layar, kamera mengikuti dengan batas ruangan) ditulis dengan TDD.
- Aturan cahaya tetap: radius aturan = radius Light2D (GDD §5.5).
- Dikerjakan di branch baru dari `feature/m2-5-isi-makam`, karena M2.5 belum di-merge dan masih menunggu playtest.

## 4. Pendekatan (dipilih user: B)

| Pendekatan | Kelebihan | Kekurangan |
|---|---|---|
| A. GDD dulu, implementasi ikut M3/M4 | Tidak ada kerja ganda | Game baru terasa besar dan ramai pertengahan Desember |
| **B. GDD dulu, lalu tahap kecil M2.6 sekarang (dipilih)** | Keluhan "sepi" dan "kecil" langsung terjawab dan bisa diuji bersama playtest M2.5 sebelum presentasi progres. Sebagian besar hanya menyetel angka dan efek pada kode yang ada. | M3 mundur beberapa hari lagi |
| C. Semua sekarang dalam satu tahap | Semua keputusan langsung jadi | Kristal Es dan Tiang Petir harus dibuat mendahului M3; penghalang setengah tinggi tanpa generator; tahap jadi besar |

## 5. Isi M2.6

### 5.1 Arena lebih besar dan kamera mengikuti
- **Ukuran arena: 30×17 tile** (dari 20×11), sekitar 1,5 kali layar ke tiap arah *(keputusan saya: batas bawah usulan "1,5–2 kali layar", supaya tempo tetap terkendali)*. Kamera tetap memakai resolusi 320×180 dan 16 PPU, jadi ukuran karakter di layar tidak berubah.
- **Kamera mengikuti pemain** dengan halus, tanpa rotasi, posisinya dibulatkan ke piksel, dan dibatasi oleh tepi ruangan sehingga tidak memperlihatkan area di luar dinding.
- `CameraShake` diubah dari "posisi dasar sekali" menjadi offset relatif terhadap posisi kamera yang mengikuti. Catatan shortcut di kodenya memang menunggu perubahan ini.
- Ukuran pilar, titik spawn, posisi Genangan Beku, dan posisi kit makam diskalakan ke arena baru. Pilar ditambah dari 2 menjadi 4 supaya ruangan besar tetap punya tempat berlindung. Jumlah benda kit naik sebanding: 3 Anglo, 5 Guci, 3 Nisan, 2 Sarkofagus *(keputusan saya)*.
- Titik spawn ditaruh di tepi ruangan, termasuk di luar jangkauan kamera, supaya musuh datang dari gelap.

### 5.2 Musuh lebih banyak, dalam gelombang
- **Satu "ruangan" di arena = 3 gelombang** *(keputusan saya, dari usulan "2–3")*. Sinyal `RoomEvents.Started` dipicu di awal gelombang pertama setiap ruangan, bukan setiap gelombang lagi. Anglo padam dan guci utuh kembali per ruangan.
- Gelombang berikutnya dalam ruangan yang sama datang saat **sisa musuh ≤ 4**, bukan menunggu semua habis, supaya pertarungan tidak berhenti *(keputusan saya: dengan batas 8 per gelombang, angka 4 membuat batas 12 benar-benar tercapai)*. Ruangan berikutnya baru mulai setelah semua musuh habis, dengan jeda singkat seperti sekarang.
- **Batas musuh hidup bersamaan: 12** (sebelumnya 8 per ruangan di GDD). Musuh gelombang yang melewati batas menunggu sampai ada yang mati.
- **Pickup relic sementara (M2) muncul per ruangan**, setelah ketiga gelombang selesai, bukan per gelombang *(keputusan saya: sejalan dengan arah GDD §4.3 bahwa relic makin langka)*.
- Jumlah per gelombang naik pelan sesuai urutan ruangan, dengan batas 8 per gelombang *(keputusan saya: rumus pastinya diserahkan ke rencana kerja, dengan syarat ruangan pertama tetap ringan untuk pengunjung baru)*.

### 5.3 Cahaya skill lebih luas
- **Bola api:** radius cahaya 1 → **1,75 tile** *(keputusan saya)*. Relic yang menambah cahaya proyektil (Sumbu Panjang) tetap bekerja di atasnya.
- **Kobaran:** radius cahaya 3,5 → **4,5 tile**, durasi 0,5 → **1,0 detik** *(keputusan saya)*.
- **Cahaya dasar pemain tetap 3 tile**, supaya gelap tetap menegangkan dan terang tetap terasa sebagai hadiah dari menyerang.

### 5.4 Efek lebih heboh, tetap terbaca
- **Tembakan:** kilatan moncong singkat, dan bola api meninggalkan ekor bara (partikel kecil berumur pendek).
- **Bola api mengenai sasaran:** ledakan bara lebih besar dan kilatan cahaya sesaat di titik kena.
- **Kobaran:** cincin api lebih tebal yang meluas, kilatan cahaya besar sesaat, guncangan kamera sedang, dan percikan bara ke segala arah.
- **Aturan keterbacaan** (ini kandidat 4 dari laporan):
  - Semua **efek visual** pemain (sprite, partikel, kilatan) **singkat**: puncak terang lalu hilang dalam ±0,4 detik. **Cahaya aturan** Kobaran (4,5 tile selama 1,0 detik) adalah mekanik, bukan efek visual, jadi tidak terkena batas ini *(keputusan saya)*.
  - Ukuran efek visual pemain yang padat (sprite dan partikel) tidak lebih besar dari radius serangannya sendiri. Cahaya boleh lebih luas dari radius serangan, seperti Kobaran sekarang (damage 2,5 tile, cahaya lebih luas supaya musuh di tepi tetap Tersingkap).
  - **Kilatan cahaya singkat** (kilatan moncong, kilatan di titik kena, kilatan besar Kobaran) memakai Light2D **tanpa** `LightSourceBehaviour`, jadi tidak ikut aturan cahaya. Ini mengikuti kilatan efek mati yang sudah ada (M1.5). Durasinya dibatasi ≤ 0,2 detik, dan GDD §5.5 diberi catatan pengecualian ini *(keputusan saya: kalau ikut aturan, setiap tembakan menerangi musuh di titik kena dan ketegangan gelap hilang)*.
  - **Mata musuh dan telegraph serangan selalu digambar di atas semua efek pemain**, lewat sorting order.
  - Jumlah partikel dibatasi supaya FPS di laptop booth aman.

## 6. Perubahan GDD dari keputusan ini

Ditulis ke GDD sebagai bagian dari M2.6:
- **§9.4 Target Durasi:** satu lantai sampai boss sekitar **20 menit**. Hitungan kasar: 10–14 ruangan × 60–90 detik, ditambah boss sekitar 2 menit. Lantai lebih dalam tetap opsional tanpa batas. Catatan untuk tim: dengan 20 menit, satu laptop booth melayani sekitar 3 pengunjung per jam.
- **§9.1 Generator:** ruangan **lebih besar dari satu layar** (sekitar 30×17 tile), dan kamera mengikuti pemain.
- **§6 Musuh:** batas 12 musuh hidup bersamaan (menggantikan 8 per ruangan); musuh datang dalam 2–3 gelombang per ruangan.
- **§5.1 Objek elemen (untuk M3):** Kristal Es dan Tiang Petir **juga melukai siapa pun di dekatnya, termasuk musuh**, sehingga pemain punya alasan memancing musuh ke situ.
- **§7.1 / §9 (untuk M4):** **penghalang setengah tinggi**: nisan rendah menahan gerak tetapi bisa ditembus tembakan; pilar dan sarkofagus menahan semuanya.
- **§7.1 Keterbacaan efek:** aturan di §5.4 di atas.
- **§4.1 Class:** nama Pyromancer, Frost Warden, dan Stormcaller **dipertahankan** (keputusan user, 25 Nov 2026). "Pyromancer" adalah istilah fantasi umum, dan Stormcaller berbeda dari Stormwarden/Stormblade di Soul Knight Prequel.
- **§10 Bukan tujuan:** bantuan bidik gamepad tidak dipakai.
- **Roadmap:** sisipkan M2.6 setelah M2.5, dan geser M3.

## 7. Bukti selesai
- **EditMode test:**
  - Pengelompokan gelombang: `RoomEvents.Started` hanya di awal ruangan (gelombang 1, 4, 7, …); gelombang berikutnya mulai saat sisa musuh ≤ 4; pickup relic muncul sekali per ruangan.
  - Kilatan cahaya baru tidak membuat musuh Tersingkap.
  - Batas 12 musuh hidup tidak pernah terlampaui.
  - Kamera mengikuti pemain dan tidak keluar dari batas ruangan.
  - Guncangan kamera tetap bekerja saat kamera bergerak.
  - Radius cahaya bola api 1,75 dan Kobaran 4,5 selama 1 detik, dan aturan cahaya membaca radius yang sama.
  - Arena 30×17: dinding utuh di keempat sisi, dan spawn point serta benda kit tidak saling menutupi.
- **Visual:** screenshot sebelum dan sesudah, menunjukkan arena lebih luas, lebih ramai, dan efek lebih terang, dengan mata merah tetap terbaca.
- **Performa:** FPS di Play mode **≥ 60** di laptop pengembang dengan charger tercolok, saat Kobaran dipakai di tengah 12 musuh *(keputusan saya; spesifikasi laptop booth tidak dijadikan batasan, sesuai keputusan user 17 Nov 2026)*. Lampu kilatan baru tidak melempar bayangan.
- **Playtest user (GDD §13):** apakah game masih terasa sepi atau kecil, apakah efek terasa heboh tanpa membingungkan, dan berapa lama sampai bosan dalam satu sesi.

## 8. Diserahkan ke builder
- Rumus jumlah musuh per gelombang, selama ruangan pertama ringan dan batas 8 per gelombang serta 12 hidup bersamaan dipatuhi.
- Koordinat pilar, spawn point, Genangan Beku, dan benda kit di arena 30×17.
- Detail partikel dan sprite efek, selama mengikuti aturan keterbacaan di §5.4.
- Kehalusan kamera (kecepatan mengejar), selama tidak tertinggal lebih dari sekitar 1 tile.
