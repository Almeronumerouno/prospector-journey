# Product Requirements Document (PRD)
## Prospector Journey

| | |
|---|---|
| **Versi Dokumen** | 0.2 |
| **Status** | Draft, mayoritas keputusan sudah dikonfirmasi |
| **Engine** | Unity 3D |
| **Genre** | VR Simulation / Resource Gathering |
| **Konteks** | Proyek tugas akhir semester (final project mata kuliah) |

---

## 1. Ringkasan & Visi Produk

**Prospector Journey** adalah game VR simulasi bertema *gold panning* (menyaring emas dari pasir sungai) menggunakan alat sederhana: **shovel** dan **saringan (pan)**. Pemain memulai sebagai penambang pemula di kawasan perbukitan yang dialiri sungai, membeli peralatan dasar dari pedagang lokal, lalu menggali dan menyaring material di tepi sungai untuk menemukan logam berharga (emas, perak, nikel, tembaga). Hasil temuan dijual untuk membeli alat yang lebih baik.

Proyek ini dikerjakan sebagai **tugas akhir semester** yang akan dipresentasikan ke dosen penguji. Karena itu, prioritas utama bukan fitur sebanyak-banyaknya, melainkan **core loop yang solid, stabil, dan bebas dari error/crash saat demo**.

## 2. Keputusan yang Sudah Dikonfirmasi

| # | Topik | Keputusan |
|---|---|---|
| 1 | Skala proyek | Tugas akhir semester kuliah, untuk dipresentasikan |
| 2 | Prioritas kualitas | Stabilitas di atas segalanya, tidak boleh ada error saat presentasi |
| 3 | Gaya visual | Low-medium poly |
| 4 | Model bisnis | Free to play |
| 5 | Multiplayer | Tidak untuk versi ini, direncanakan sebagai pengembangan masa depan pasca-semester |
| 6 | Jumlah map/area | Hanya 2 map: Map 1 (area awal/hub) dan Map 2 (area kedua) |
| 7 | Nama game | Prospector Journey |
| 8 | Gestur Panning Manual | Gerakan goyang horizontal (kanan-kiri) controller saat pan berada di air |
| 9 | Peralatan Sluicing Lanjut | Akan ada alat sluicing asli (sluice box / alat berat mekanis) sebagai progression/upgrade |

## 3. Pertanyaan yang Masih Terbuka

| # | Topik | Catatan |
|---|---|---|
| 1 | Target platform VR | Belum dikonfirmasi headset yang dipakai untuk build & demo. Asumsi sementara di dokumen ini: **Meta Quest standalone**, karena paling praktis untuk dibawa dan didemokan langsung tanpa gaming PC. Jika kampus/lab menyediakan PCVR (SteamVR/Vive/Index), beri tahu agar requirement teknis disesuaikan. |
| 2 | Cara transisi Map 1 ke Map 2 | Perlu didetailkan: apakah lewat titik/portal tertentu di dunia game, menu pilih level, atau cutscene singkat. |

## 4. Target Platform & Perangkat

- **Engine**: Unity 3D (versi LTS terbaru)
- **XR Framework**: Unity XR Interaction Toolkit (XRI) + OpenXR
- **Target Device (asumsi)**: Meta Quest (standalone) — dipilih karena praktis untuk demo tugas akhir tanpa perlu PC tambahan
- **Input**: Motion controllers (grip untuk memegang shovel/pan, trigger untuk aksi menggali/menggoyang)

## 5. Target Audiens

- Audiens utama: dosen penguji/pembimbing saat sesi presentasi tugas akhir
- Audiens sekunder (jika dirilis lebih luas nanti): penggemar game VR simulasi/santai, mirip *Job Simulator* atau *Vacation Simulator*

## 6. Core Gameplay Loop

```
Spawn di Map 1 (hub area: bukit + sungai)
        |
Beli/ambil shovel & pan dari toko pemula
        |
Gali pasir/material di tepi sungai dengan shovel -> isi ke pan
        |
Bawa pan ke tepi air -> goyangkan (sluicing) hingga pasir tersapu
        |
Material tersisa terlihat (gold / silver / nickel / copper / kosong)
        |
Material otomatis masuk ke Backpack (inventory)
        |
Kembali ke toko -> jual material -> dapat uang
        |
Beli shovel/pan tier lebih tinggi
        |
(Opsional) Pindah ke Map 2 untuk material yang lebih baik
        |
Ulangi loop
```

## 7. Mekanik Gameplay Detail

### 7.1 Menggali (Shoveling)
- Pemain memegang shovel di satu tangan, melakukan gerakan menyendok ke tumpukan pasir/gravel di tepi sungai.
- Setiap "sendokan" yang berhasil mengisi sebagian kapasitas pan.
- Shovel tier tinggi membawa lebih banyak pasir sekaligus (mengurangi jumlah trip).
- Feedback: partikel pasir, suara gesekan.

### 7.2 Menyaring (Panning / Sluicing Manual & Lanjutan)
- **Manual Gold Pan**: Setelah pan terisi penuh, pemain mendekat ke tepi sungai dan mencelupkan pan ke air.
- Pemain menggoyangkan pan dengan gerakan **kanan-kiri (horizontal shake/oscillation)**. Logika script mendeteksi pergantian arah/kecepatan controller di sumbu lateral saat pan berada di volume air (Water Trigger).
- Pasir dan gravel ringan tersapu air secara bertahap seiring goyangan, material berat (logam) tetap tertinggal di dasar pan.
- Proses selesai ketika pasir habis, menyisakan 0 atau beberapa material kecil di dasar pan.
- **Alat Sluicing Lanjut (Heavy Equipment / Sluice Box)**: Sebagai upgrade/progression di tahap lanjut (atau Map 2), disediakan alat sluicing sungguhan (sluice box / wash plant mekanis) di mana pemain dapat memasukkan material dalam volume lebih besar dan aliran air sungai menyaring material secara semi-otomatis.

### 7.3 Sistem Material & Rarity

| Material | Rarity | Nilai Jual (relatif) |
|---|---|---|
| Kosong (tidak dapat apa-apa) | Sangat umum | - |
| Copper (tembaga) | Umum | Rendah |
| Nickel | Umum-menengah | Rendah-menengah |
| Silver (perak) | Jarang | Menengah |
| Gold (emas) | Sangat jarang | Tinggi |

Probabilitas temuan dipengaruhi oleh: map/lokasi menggali, dan tier shovel/pan yang digunakan.

### 7.4 Inventory / Backpack
- Backpack menyimpan seluruh material yang terkumpul, bisa dibuka pemain kapan saja (gesture buka tas atau menu wrist-mounted).
- Menampilkan jumlah dan jenis material yang dimiliki.

### 7.5 Ekonomi & Toko Upgrade
- Toko berada di perumahan kecil dekat titik spawn (Map 1).
- Pemain menjual material untuk mendapatkan mata uang dalam game.
- Uang digunakan untuk membeli shovel/pan tier lebih tinggi (kapasitas, kecepatan, rate temuan material langka lebih baik).

## 8. World & Level Design

Scope dibatasi pada **2 map** untuk memastikan proyek selesai tepat waktu dan stabil:

### 8.1 Map 1 — Hub / Area Awal
- Kawasan perbukitan dengan sungai kecil mengalir di tengahnya.
- Beberapa bangunan kecil (kios/rumah) berfungsi sebagai toko alat dan tempat jual material.
- Area digging tersebar di sepanjang tepi sungai.
- Titik transisi menuju Map 2 (detail mekanisme transisi: lihat Bagian 3).

### 8.2 Map 2 — Area Kedua
- Area dengan potensi material lebih baik (lebih tinggi kemungkinan silver/gold), sebagai "reward" progres dari Map 1.
- Bisa memiliki variasi kondisi sungai (arus lebih deras, pasir lebih padat) untuk sedikit variasi tantangan, tanpa menambah kompleksitas mekanik baru.

*(Catatan: sistem multi-area yang lebih luas / dunia terbuka progresif tidak masuk scope versi ini, disimpan sebagai ide pengembangan pasca-semester bersamaan dengan multiplayer.)*

## 9. UI/UX Requirements (VR-Specific)

- Minim UI 2D di layar (diegetic UI diutamakan).
- Info kapasitas pan/shovel ditampilkan langsung di objeknya.
- Menu toko dan backpack berupa panel 3D saat berinteraksi dengan NPC/toko atau gesture tertentu.
- Tutorial awal berbentuk *guided interaction* (NPC penjual menjelaskan cara pakai shovel & pan).

## 10. Kenyamanan & Aksesibilitas VR (Comfort)

- Opsi locomotion: teleport (default, aman dari motion sickness).
- Vignette saat bergerak untuk mengurangi motion sickness.
- Opsi duduk/berdiri (seated/standing play).

## 11. Technical Requirements

- Unity 3D dengan Universal Render Pipeline (URP) untuk performa optimal di perangkat standalone.
- Unity XR Interaction Toolkit untuk grab/interaksi objek (shovel, pan).
- Simulasi partikel pasir & air: pendekatan visual sederhana (particle system + shader) alih-alih physics simulation penuh — lebih ringan dan lebih mudah dijaga stabil, penting mengingat prioritas "bebas error" untuk presentasi.
- Physics grab untuk shovel & pan (Rigidbody + XR Grab Interactable).
- **Rekomendasi khusus untuk konteks tugas akhir**: sisakan waktu khusus di akhir jadwal pengerjaan untuk QA/testing menyeluruh sebelum presentasi (lihat Bagian 14).

## 12. Art & Audio Direction

- Palet warna hangat (coklat pasir, hijau perbukitan, biru air sungai).
- Model low-medium poly untuk semua aset (sesuai keputusan gaya visual), sekaligus membantu performa dan mempercepat produksi aset dalam waktu satu semester.
- Audio penting untuk *immersion*: suara gesekan shovel, cipratan air, chime saat menemukan material langka.

## 13. Scope & Roadmap (Disesuaikan untuk Proyek Semester)

| Fase | Fokus | Output |
|---|---|---|
| **Fase 1 – Prototype Core Mechanic** | Mekanik gali + saring berfungsi di Map 1, minimal 1 material bisa ditemukan | Vertical slice yang bisa dites (feel gerakan tangan) |
| **Fase 2 – Core Loop Lengkap** | Rarity system lengkap, backpack, toko, ekonomi jual-beli, upgrade alat | Loop lengkap: gali -> jual -> upgrade |
| **Fase 3 – Map 2 & Polish** | Bangun Map 2, transisi antar-map, polish art & audio | Build hampir final, siap diuji coba |
| **Fase 4 – QA & Presentasi** | Testing menyeluruh, bug fixing, buffer waktu khusus tanpa fitur baru | Build final bebas-crash, siap presentasi |

## 14. Kriteria Sukses untuk Presentasi (Definition of Done)

Karena prioritas utama adalah presentasi tugas akhir tanpa error, berikut kriteria minimal yang disarankan sebelum dianggap "siap":

- Core loop (gali -> saring -> temukan material -> jual -> upgrade) bisa dijalankan end-to-end tanpa crash.
- Sudah diuji coba minimal untuk sesi demo sepanjang durasi presentasi (misalnya 10-15 menit berturut-turut) tanpa error fatal.
- Transisi Map 1 ke Map 2 berjalan mulus, tanpa softlock (pemain tidak bisa terjebak/stuck).
- Tidak ada bug yang menghentikan progres pemain (misalnya item hilang, toko tidak bisa diakses, dsb).
- Ada rencana cadangan (misalnya video rekaman gameplay) jika terjadi kendala teknis di hari-H (headset bermasalah, dsb).

## 15. Risiko & Mitigasi

| Risiko | Mitigasi |
|---|---|
| Waktu satu semester terbatas untuk fitur + polish + testing | Scope dijaga ketat di 2 map dan fitur inti saja (lihat Bagian 8 & 13), sisakan buffer waktu QA di akhir (Fase 4) |
| Simulasi pasir/air realistis terlalu berat/berisiko bug | Gunakan pendekatan visual "fake" (particle + animasi terarah) alih-alih physics simulation penuh |
| Gerakan menggoyang pan sulit dideteksi secara natural | Prototipe cepat dengan beberapa metode deteksi gestur sebelum difinalisasi (lihat Bagian 3) |
| Bug muncul tepat saat sesi presentasi | Testing berulang sebelum hari-H, siapkan build cadangan/rekaman video sebagai fallback |

## 16. Langkah Selanjutnya / Diskusi Lanjutan

1. Konfirmasi target platform VR final (Quest standalone, atau ada opsi PCVR dari kampus)
2. Detail teknis mekanik sluicing (bagaimana persisnya gestur terdeteksi sebagai "berhasil menyaring")
3. Mekanisme transisi Map 1 ke Map 2 (portal, menu, atau cutscene)
4. Timeline/deadline konkret (kapan presentasi, agar Fase 1-4 di atas bisa diberi target tanggal)

---
*Dokumen ini adalah draft v0.2, terbuka untuk direvisi sesuai hasil diskusi lebih lanjut.*
