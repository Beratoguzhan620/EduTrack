# EduTrack — Yol Haritası

Öğretmen ve öğrencilerin ders, duyuru ve materyal paylaştığı sınıf yönetim platformu (Google Classroom benzeri).

- **Önerilen repo adı:** `edutrack`
- **Teknoloji:** ASP.NET Core MVC (.NET 10), EF Core, SQL Server, Bootstrap 5 + özel SCSS, xUnit
- **Kurallar:** `CONTRIBUTING.md` · **Tasarım:** `docs/DESIGN.md`

Her aşama ayrı bir branch ve PR olarak yapılır (`stage/<no>-<kisa-ad>`). Bir aşama ancak tüm maddeleri bitince, build/testler geçince ve ekran görüntüleri eklenince tamamlanmış sayılır. Biten maddeler `[x]` yapılır.

## Aşama 1 — Temizlik ve tasarım sistemi
- [x] `.gitignore` kontrolü; `appsettings*.json` içinde gerçek değer yok, `.env.example` güncel
- [x] `docs/DESIGN.md`'deki token'larla ortak SCSS/CSS dosyası (`wwwroot/css/design.css`): renkler, tipografi (IBM Plex Sans), radius, gölge
- [x] `_Layout` baştan: üst bar + sol menü (öğretmen/öğrenci rolüne göre), responsive, aktif menü durumu
- [x] Ortak bileşenler (partial): sayfa başlığı, boş durum, onay modalı, toast bildirimi, sayfalama
- [x] Login / Register sayfaları yeni tasarımla
- [x] CI'ın `main` ve `dev` için çalıştığını doğrula
- [x] Deploy ayarları (`deploy.yml`, `docker-compose.yml`) bu repoya ait registry'yi kullanır; deploy yalnızca elle tetiklenir
- [x] Ekran görüntüleri için "UI preview" workflow'u (SQL Server + Playwright)

## Aşama 2 — Eksik özellikler
- [x] E-posta doğrulama (kayıt sonrası kod/link; `IsActive=false` ile başla) — SMTP ayarları user-secrets / env üzerinden
- [x] `S3FileService` ya tamamlanır ya da kaldırılıp dosyalar FileAPI üzerinden çalışır; `NotImplementedException` kalmaz
- [x] Ödevler (Assignments): oluşturma, teslim etme, teslimleri listeleme, puanlama
- [x] Kullanıcı listesinde dinamik arama/filtre (TODO: dynamic query)
- [x] Deprecated `AddFluentValidationClientsideAdapters` yerine güncel yaklaşım

## Aşama 3 — Sayfaların cilası + test
- [ ] Kurs listesi (kart grid), kurs detayı (sekme: Akış / Materyaller / Ödevler / Öğrenciler) yeni tasarımla
- [ ] Tüm formlarda doğrulama mesajları, yükleniyor durumları, boş durumlar
- [ ] Demo seed verisi: 1 öğretmen, 8 öğrenci, 3 kurs, duyurular, materyaller (demo hesapları README'de)
- [ ] Unit test kapsamını iş kurallarına (BusinessRules) genişlet; tüm testler yeşil
- [ ] 404 / 500 hata sayfaları

## Aşama 4 — Yayın ve README
- [ ] `docker-compose up` ile tek komutta çalışır (Web + FileAPI + SQL Server)
- [ ] Canlı demo (Azure App Service / Render / Fly.io) — link README'ye
- [ ] `docs/screenshots/` ekran görüntüleri + kısa GIF
- [ ] README (TR + EN) — `CONTRIBUTING.md`'deki şablona göre, mimari şeması (Mermaid) dahil
- [ ] README'de "Proje geçmişi / Background" bölümü — TR: "Bu proje bir bootcamp projesi olarak başladı, sonra baştan geliştirildi." EN: "This project started as a bootcamp project and was later rebuilt from the ground up." Altına neyin yeniden yapıldığı kısaca maddelenir (tasarım, özellikler, testler, CI).
