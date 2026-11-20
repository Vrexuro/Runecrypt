# Desain singkat: Runecrypt 3D yang bisa dimainkan

Status: disetujui user 20 Nov 2026 ("ya mari lakukan"). Ditulis Claude dari percakapan.

## Maksud
User ingin melihat Runecrypt dalam 3D sebelum memutuskan versi mana yang dibawa ke expo. Targetnya **bisa dimainkan**, bukan setara progres 2D. Setelah jadi, kedua versi dimainkan berdampingan lalu user memutuskan.

## Tujuan
- 1 arena 3D seukuran ruangan 2D (20×11 tile), gelap, dengan kamera miring dari atas.
- Pyromancer 3D, dibuat dengan alur yang sama seperti Tulang: lembar referensi dari semua sisi lebih dulu, dicek user, baru dimodelkan di Blender.
- Tulang Penjaga 3D (model spike yang sudah ada).
- Kontrol sama dengan 2D: WASD, bidik mouse ke lantai, klik untuk menembak, gamepad twin-stick dan panah sebagai cadangan.
- Bola api membawa cahaya. Aturan cahaya GDD §5.5: terang = damage +25%, gelap = musuh 20% lebih cepat.
- Gelombang musuh, HP pemain, layar tumbang, dan tombol R untuk mengulang.

## Bukan tujuan (belum)
Relic, Kobaran, reaksi Uap, efek juice (hit-stop, angka damage, gaya mati), rig dan animasi tulang, SFX baru, generator lantai.

## Kegagalan yang harus dihindari
- Versi 2D rusak atau berubah. Semua pekerjaan 3D ada di branch dan folder worktree terpisah.
- Logika `Runecrypt.Core` disalin lalu bercabang. Logika dipakai ulang, bukan disalin.

## Pendekatan
| Pendekatan | Untung | Rugi | Keputusan |
|---|---|---|---|
| **Worktree terpisah, repo sama** | 2D dan 3D bisa dibuka bersamaan. Perbaikan `Core` bisa di-merge. Pengaturan 3D hanya ada di branch 3D. | Dua folder `Library` (ruang disk). | **Dipilih user** |
| Proyek Unity baru | Bersih | `Core`, data, dan test harus disalin, lalu bercabang | Ditolak |
| Pindah branch di folder yang sama | Satu folder | Impor ulang besar setiap pindah, 2D dan 3D tidak bisa dibuka bersamaan | Ditolak |

## Yang sudah diputuskan
- Template 2D/3D Unity bukan pengunci: renderer, Default Behavior Mode, dan fisika diatur di branch 3D saja.
- Logika di `Runecrypt.Core` dipakai ulang. Bidang permainan 2D (x, y) dipetakan ke lantai 3D (x, z).
- Model dibuat Claude di Blender lewat script (aturan art proyek: tanpa aset pihak ketiga).

## Diserahkan ke pembuat
Angka visual (tinggi kamera, intensitas lampu, bloom) disetel saat uji, lalu dicatat.
