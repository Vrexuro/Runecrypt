# Petunjuk Anglo (perbaikan kecil setelah playtest M2.5/M2.6)

> Disetujui user 25 Nov 2026 ("lanjutkan" atas usulan di chat). Dikerjakan di `feature/m2-6-skala-dan-efek` sebelum merge.

## Masalah
Playtest 25 Nov 2026 (GDD §13): user menyalakan Anglo karena tahu Anglo ada, tetapi pemain yang tidak tahu kemungkinan tidak menemukannya. Anglo padam berada di gelap, dan Anglo terdekat dari posisi awal (0; −0,5) berjarak ±7,6 tile, jauh di luar cahaya pemain (radius 3).

## Tujuan dan bukan tujuan
- **Tujuan:** pemain baru melihat Anglo padam dalam detik-detik pertama dan cenderung menembaknya.
- **Bukan tujuan:** teks tutorial atau panah UI (ruang tutorial dikerjakan di M4); mengubah aturan cahaya.
- **Anti-tujuan:** petunjuk yang membuat area di sekitar Anglo padam tampak terang padahal dihitung gelap (GDD §5.5: tidak boleh ada keadaan "terlihat terang tapi dihitung gelap").

## Pendekatan
1. **Bara petunjuk** *(dipilih)*: sprite bara kecil berkedip di atas Anglo padam, memakai material unlit seperti mata musuh. Terlihat di gelap tanpa Light2D, jadi tidak menerangi apa pun dan aturan cahaya tidak berubah. Hilang saat Anglo menyala, muncul lagi saat ruangan dimulai ulang.
2. Pendar Light2D lemah di Anglo padam: ditolak, karena area sekitarnya akan tampak terang padahal dihitung gelap.
3. Ikon "tembak di sini" saat pemain dekat: ditolak untuk sekarang, karena terasa seperti UI tutorial dan bertentangan dengan tone gelap.

**Penempatan** *(dipilih)*: Anglo ketiga dipindah dari (4,5; 5,5) ke **(2,5; −1,5)**, ±2,9 tile dari posisi awal, sehingga sudah berada di dalam cahaya pemain saat permainan dimulai. Tembakan pertama ke arah kanan bawah kemungkinan besar mengenainya, dan efek menyala (kilatan, bara, suara) mengajarkan sisanya.

## Detail
- **Sprite:** `brazier_hint_0..3` (16x16), dibuat lewat **codeart2d** (PixelSpec di `ArtSource/codeart/brazier_hint.pixelspec.json`, digambar lewat kode tanpa model gambar). Palet api yang sudah ada: `#FF8A3D`, `#FFCC66`, `#FFE296`. Bara 1–2 piksel yang naik lalu padam; satu frame kosong supaya terasa berkedip.
- **Komponen:** anak `Hint` di prefab Anglo (SpriteRenderer unlit + SpriteFlipbook ±6 fps), sorting order di bawah mata musuh (10). `Brazier.Apply()` mengaktifkan `Hint` hanya saat padam.
- **Batas tata letak:** Anglo baru tetap ≥ 1,5 tile dari posisi awal dan dari setiap titik spawn (test `CryptKit_KeepsSpawnsAndStartClear`).

## Bukti selesai
- EditMode test: `Hint` aktif saat padam, mati saat menyala, aktif lagi setelah `RoomEvents.Started`; `Hint` tidak punya Light2D atau LightSourceBehaviour; ada Anglo dalam radius 3 tile dari posisi awal; test tata letak yang sudah ada tetap lulus.
- Screenshot Play mode di awal permainan: Anglo dan bara petunjuk terlihat.
- Playtest berikutnya (GDD §13): apakah pemain baru menyalakan Anglo tanpa diberi tahu.

## Rencana kerja
1. Tulis PixelSpec dan render lewat codeart2d (QA ketat), salin frame ke `Assets/Runecrypt/Art/Crypt/`.
2. RED: test petunjuk dan posisi Anglo.
3. GREEN: `Brazier` (+field `hint`), `M25CryptBuilder.BuildBrazier` (anak `Hint`) dan `BrazierAt`.
4. Build All, seluruh suite, screenshot, commit `feat: petunjuk bara pada Anglo padam dan Anglo dekat posisi awal`.
5. GDD §7.1 dan §13 diperbarui.
