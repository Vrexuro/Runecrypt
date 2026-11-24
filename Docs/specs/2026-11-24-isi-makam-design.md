# Desain: Isi Makam (M2.5)

> Status: **draf, menunggu persetujuan user**. Hasil brainstorming 24 Nov 2026.
> Versi game: **2D** (keputusan user 23 Nov 2026; eksperimen 3D dihentikan).

## 1. Maksud dan alasan

Setelah memainkan M1 dan M2, user merasa makam **terlalu sepi dan kurang berisi**, baik dari tampilan maupun dari hal yang bisa dimainkan ("dua-duanya"). Aset lingkungan saat ini hanya satu tile lantai, satu tile dinding, dan Genangan Beku. Pilar di arena masih memakai tile dinding biasa.

Riset game sejenis (Binding of Isaac, Enter the Gungeon, Hades) menunjukkan bahwa ruangan mereka terasa hidup karena tiga hal:
- **rintangan dan tempat berlindung** (batu, pilar, meja);
- **benda yang bisa dihancurkan atau dipakai** (guci, tong, meja yang dibalik);
- **dekorasi dan suasana** yang bercerita tentang tempatnya.

Kit ruangan Hades, misalnya, terdiri dari dinding penuh atau setengah, jebakan, pilar, meja, dan guci.

Tahap ini menambah satu **kit makam** berbasis prefab dan data. Kit langsung dipasang di arena M1/M2 yang ada. Kit yang sama nanti dipakai oleh prefab ruangan di M4, jadi tidak ada kerja yang terbuang.

## 2. Tujuan, bukan tujuan, anti-tujuan

**Tujuan**
- Satu ruangan tidak lagi terlihat kosong. Lantai dan dinding punya variasi, dan ada dekorasi yang menunjukkan bahwa ini makam para penjaga api.
- Ada tiga benda yang bisa dimainkan: **Anglo** (sumber cahaya yang bisa dinyalakan), **Guci Abu** (bisa dihancurkan), dan **Nisan/Pilar** (tempat berlindung yang memotong cahaya).
- Anglo memperkuat ciri utama Runecrypt, yaitu cahaya sebagai aturan main (GDD §5.5).
- Semua benda berupa prefab dengan nilai yang bisa disetel, sesuai prinsip arsitektur di CLAUDE.md. Menambah dekorasi atau benda baru tidak mengharuskan mengubah kode pemain atau musuh.

**Bukan tujuan (tahap ini)**
- Jebakan (paku lantai, panah dari dinding, genangan minyak). Ini masuk M4, karena jebakan perlu ditata per ruangan supaya adil.
- Pagar tulang setengah-penutup (menahan gerak, tidak menahan tembakan), dinding rahasia, dan ruang rahasia.
- Tulisan dinding dan jasad sang guru. Keduanya tetap di M5 sesuai spec cerita.
- Generator ruangan dan prefab ruangan (M4).
- Musik.

**Anti-tujuan (gagal walaupun fiturnya jalan)**
- Mata merah musuh di gelap jadi sulit terbaca karena dekorasi terlalu ramai atau terlalu terang. Mata merah adalah cara utama melihat musuh (GDD §5.5).
- Dekorasi terlihat seperti benda yang bisa dipakai, atau sebaliknya. Pemain harus bisa membedakan keduanya tanpa diberi tahu.
- Anglo membuat ruangan terang seluruhnya sehingga ketegangan gelap hilang.
- Guci terlalu banyak sehingga pemain sibuk memecah guci, bukan bertarung.
- Performa turun di laptop booth.

## 3. Batasan
- Unity 6.3 URP 2D, paket yang sudah terpasang saja. Tilemap Extras sudah ada, termasuk `RandomTile` dan `WeightedRandomTile`. **Tidak ada paket baru.**
- Semua sprite dibuat lewat script Python + Pillow di `ArtSource/scripts/`, 16x16, dengan palet GDD §7.1. Tidak ada asset pack.
- SFX mengikuti keputusan GDD §7.2, yaitu sumber CC0 yang diolah lewat `make_sfx_cc0.py`. Sumber dan lisensinya dicatat di GDD §14.
  - Suara "wus" api mungkin bisa diambil dari paket yang sudah dipakai.
  - Suara guci pecah, dengung ambien, dan tetesan air belum ada sumbernya. Rencana kerja punya langkah untuk **mengusulkan sumber CC0 ke user dan meminta izin sebelum mengunduh** *(keputusan saya)*.
- Logika (status Anglo, HP dan undian jatuhan Guci) ditulis dengan TDD. Perilaku di scene diuji dengan PlayMode test sesuai pola yang sudah ada.
- Dikerjakan di branch `feature/m2-5-isi-makam` dari `main`.

## 4. Pendekatan (dipilih user: A)

| Pendekatan | Kelebihan | Kekurangan |
|---|---|---|
| **A. Tahap kecil "Isi Makam" sekarang, sebelum M3 (dipilih)** | Rasa sepi langsung hilang dan bisa diuji di playtest berikutnya. Kit dipakai ulang oleh M4. Anglo memperkuat mekanik cahaya. | M3 mundur beberapa hari. Jadwal masih di depan target, karena M1.5 dan M2 sudah selesai. |
| B. Digabung ke M4 | Paling efisien, karena setiap benda langsung ditaruh di ruangan sungguhan | Makam tetap sepi sampai pertengahan Desember, termasuk saat presentasi progres |
| C. Tampilan dulu, interaksi di M4 | Paling murah | Hanya menjawab separuh dari "dua-duanya" |

User memilih A dengan ruang lingkup dibatasi: satu kit, tiga benda interaktif, tanpa jebakan.

## 5. Isi kit

### 5.1 Tampilan (tanpa fungsi main)

| Bagian | Isi | Catatan |
|---|---|---|
| Variasi lantai | Ubin biasa (paling sering), retak, berlumut, ada serpihan tulang, ubin rune pudar | Memakai `WeightedRandomTile` di satu tile lantai. Ubin biasa tetap dominan supaya lantai tidak ramai. *(keputusan saya: bobot sekitar 70% biasa, sisanya dibagi rata)* |
| Variasi dinding | Bata biasa, bata retak, **relung jenazah** (ceruk berisi tengkorak), dinding dengan obor padam yang tertanam | Relung dan obor tertanam hanya di dinding atas, karena dinding itulah yang terlihat dari kamera top-down |
| Dekorasi lepas | Sarang laba-laba di sudut, rantai tergantung, tulang berserakan, panji robek, lilin padam | Sprite tanpa collider, digambar di bawah karakter (sorting layer dekorasi). Tidak ada dekorasi yang bercahaya, supaya cahaya tetap milik pemain dan Anglo. |
| Debu melayang | Partikel debu kecil yang hanya terlihat di area terang | Particle System dengan material Sprite-Lit, jadi debu otomatis tidak terlihat di gelap. Jumlah partikel sedikit. |
| Suara ambien | Dengung rendah dan tetesan air sesekali, diputar terus-menerus | Satu AudioSource loop dengan volume rendah. Sumber CC0 dicatat di §14. |

### 5.2 Anglo (dapat dinyalakan)
- **Wujud:** anglo batu berisi bara padam (sprite 16x16). Ada dua keadaan: padam dan menyala, dengan api 4 frame.
- **Menyala** saat terkena serangan berelemen **api**, yaitu Bola Api dan Kobaran. Elemen lain tidak berpengaruh. Reaksi (Uap, dan nanti Ledakan Cahaya) tidak menyalakan Anglo di tahap ini *(keputusan saya: `SteamCloud` hanya mengenai layer Enemies tanpa elemen, dan mengubahnya berarti menyentuh kode reaksi)*.
- **Setelah menyala:** tetap menyala selama pemain berada di ruangan itu (tidak padam sendiri). Di arena saat ini, yang belum punya ruangan, **satu gelombang dianggap satu ruangan**: Anglo padam lagi saat gelombang baru dimulai *(keputusan saya, lihat §6)*. Ada Light2D jingga dengan radius aturan cahaya **2,5 tile** *(keputusan saya: lebih kecil dari cahaya pemain yang 3 tile, supaya tidak menerangi seluruh ruangan)*. Nyalanya berkedip memakai `EmberFlicker` yang sudah ada.
- **Aturan cahaya:** Anglo yang menyala mendaftarkan diri ke `LightField` lewat `LightSourceBehaviour`. Musuh di dekatnya jadi Tersingkap (damage +25%, bisa dikejar homing), dan musuh di sekitarnya kehilangan bonus kecepatan gelap. Inilah alasan pemain mau menyalakannya.
- **Fisik:** menahan gerak pemain dan musuh. Tembakan yang mengenainya habis (sama seperti mengenai musuh), kecuali tembakan pierce. Anglo tidak melempar bayangan, karena ia sendiri sumber cahaya.
- **Umpan balik saat menyala:** suara "wus" api, kilatan cahaya singkat (memakai `LightPulse`), dan percikan bara.
- **Jumlah:** 1–2 per ruangan, ditaruh di dekat sudut atau dinding. Di arena saat ini: 2 buah.

### 5.3 Guci Abu (dapat dihancurkan)
- **Wujud:** guci tanah liat berisi abu jenazah. Ada dua variasi bentuk.
- **HP 3:** pecah oleh satu Bola Api dasar atau satu Kobaran. Saat terkena tapi belum pecah, guci berkedip putih seperti musuh.
- **Saat pecah:** pecahan dan kepulan abu (memakai efek abu dan serpihan dari M1.5), suara pecah, lalu menyisakan sprite pecahan yang tidak menghalangi.
- **Jatuhan:** **Bara Pemulih** (pulih 1 HP, setengah hati) dengan peluang 15% *(keputusan saya)*. Bara diambil dengan cara disentuh dan hilang setelah 10 detik. Peluangnya ditulis di data supaya bisa disetel saat balancing.
- **Bara Pemulih tidak memancarkan cahaya.** Bara memakai sprite *unlit* yang berpendar pelan, jadi tetap terlihat di gelap tanpa menerangi musuh. Cara ini juga tidak membuat tampilan dan aturan cahaya berbeda, karena memang tidak ada cahaya *(keputusan saya)*.
- **Muncul lagi:** di arena saat ini, guci yang pecah muncul lagi saat gelombang baru dimulai, sesuai aturan "satu gelombang = satu ruangan" *(keputusan saya, lihat §6)*.
- **Fisik:** menahan gerak. Tembakan yang mengenainya habis, kecuali tembakan pierce. Musuh tidak bisa memecahkan guci *(keputusan saya: supaya hadiahnya milik pemain)*.
- **Bukan sasaran homing**, karena homing hanya mengejar musuh.
- **Jumlah:** 2–4 per ruangan, berkelompok di pinggir. Di arena saat ini: 3 buah.

### 5.4 Nisan dan Pilar (tempat berlindung)
- **Wujud:** pilar batu berukir (2x2 tile, menggantikan pilar tile-dinding yang sekarang), nisan tegak (1x1), dan sarkofagus (2x1).
- **Fisik:** seperti dinding. Layer `Walls`, menahan gerak dan tembakan, dan memakai `ShadowCaster2D`, jadi juga **memotong cahaya** baik di layar maupun di aturan cahaya (raycast `LightFieldHost` membaca layer `Walls`).
- Tidak bisa dihancurkan.
- **Jumlah:** di arena saat ini, dua pilar lama diganti sprite pilar baru, ditambah 2 nisan dan 1 sarkofagus.

### 5.5 Keterbacaan
- Benda yang bisa dipakai (Anglo dan Guci) punya **garis terang 1 piksel di sisi atas** sprite (rim light), dan bentuknya lebih gemuk. Dekorasi lepas digambar lebih gelap dan rata dengan lantai, tanpa rim light.
- Tidak ada warna merah terang di dekorasi atau benda, supaya mata musuh tetap menjadi satu-satunya titik merah.

## 6. Penempatan dan arsitektur
- Setiap benda interaktif adalah **prefab** dengan nilai yang bisa disetel di Inspector: radius cahaya, HP, peluang jatuhan.
- Anglo dan Guci memasang `IDamageable`, sama seperti `FrostPuddle`, sehingga Bola Api dan Kobaran otomatis bisa mengenainya tanpa mengubah kode senjata.
- Dekorasi lepas dan variasi tile disusun oleh builder editor. Untuk arena saat ini, posisinya ditulis tangan di builder, mengikuti pola `M1Builder` dan `M2ContentBuilder`. Di M4, prefab ruangan menempatkan prefab yang sama.
- Bara Pemulih memakai `Health.Heal` pada pemain.
- **Satu gelombang = satu ruangan** *(keputusan saya)*. Arena M1/M2 menjalankan gelombang tanpa henti tanpa ruangan. Supaya playtest bisa menjawab apakah pemain sengaja menyalakan Anglo dan apakah guci mengganggu, `ArenaDirector` memberi sinyal "ruangan diulang" setiap gelombang baru dimulai. Anglo padam dan Guci muncul lagi. Di M4, sinyal yang sama dikirim saat pemain masuk ruangan baru.
- **Lintasan musuh tidak diubah di tahap ini** *(keputusan saya)*. Musuh tetap mengejar lurus ke arah pemain, tanpa pathfinding. Benda yang menahan gerak ditaruh **di pinggir dan sudut**, bukan di jalur tengah, supaya musuh jarang tersangkut. Pilar lama juga tidak dikeluhkan di playtest. Kalau musuh sering tersangkut saat playtest, steering sederhana masuk M3/M4.
- Anglo dan Guci memakai collider padat di layer sendiri, **bukan** `Walls`. Jadi keduanya tidak memotong cahaya dan tidak membuat Bola Api memantul. Tembakan habis saat mengenainya, kecuali tembakan pierce.
- Penempatan detail di arena (koordinat) diserahkan ke builder, dengan syarat di §5 dan §7.

## 7. Bukti selesai
- **EditMode test:**
  - Anglo menyala hanya oleh elemen api, menyala sekali saja (terkena api lagi tidak mengubah apa pun), dan terdaftar sebagai sumber cahaya setelah menyala.
  - Guci: HP berkurang per serangan, pecah tepat saat HP habis, dan undian jatuhan menghormati peluang (diuji dengan angka acak yang dikendalikan test).
  - Sinyal "ruangan diulang": Anglo kembali padam dan Guci kembali utuh.
  - Bara Pemulih memulihkan 1 HP, tidak melewati HP maksimal, dan hilang setelah 10 detik.
- **PlayMode test:**
  - Bola Api yang mengenai Anglo membuat Anglo menyala, dan musuh di dekatnya menjadi Tersingkap menurut `LightField`.
  - Bola Api memecahkan Guci, dan tembakan habis.
  - Nisan menahan Bola Api dan menghalangi cahaya (titik di balik nisan tidak terang).
- **Visual:** screenshot arena sebelum dan sesudah, ditunjukkan ke user.
- **Playtest user (GDD §13):** apakah ruangan masih terasa sepi, apakah pemain sengaja menyalakan Anglo, apakah mata merah musuh tetap mudah terlihat, dan apakah benda yang bisa dipakai bisa dibedakan dari dekorasi.

## 8. Dampak ke dokumen lain
- **GDD:** §5.5 (Anglo sebagai sumber cahaya lingkungan), §7.1 (daftar kit dan aturan keterbacaan), §9.1 (prefab ruangan memakai kit ini), §14 (kredit SFX baru).
- **Roadmap:** sisipkan **M2.5 Isi Makam** antara M2 dan M3, dan geser M3 beberapa hari.
- **Catatan edukasi (untuk Codex nanti, bukan tahap ini):** Anglo bisa dipakai menjelaskan segitiga api (bahan bakar, oksigen, panas).

## 9. Diserahkan ke builder
- Bentuk persis sprite, selama mengikuti palet dan aturan keterbacaan di §5.5.
- Koordinat penempatan di arena.
- Detail Particle System debu (laju, ukuran, umur), selama tetap ringan.
- Nama kelas dan pembagian file, mengikuti pola yang ada.
