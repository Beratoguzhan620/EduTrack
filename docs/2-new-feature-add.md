# Course Material Degisikligi

Bu dokuman, yeni bir ozelligin projeye eklenmesi icin yapilan degisiklikleri ve yeni bir benzer degisiklikte izlenecek adimlari ozetler.

## Yapilan Degisiklikler

### 1. Domain Entity

Course material modeli domain katmanina eklendi.

- `EduTrack.Domain/Entities/CourseMaterial.cs`

### 2. Persistence Configuration

Entity icin Entity Framework configuration sinifi eklendi.

- `EduTrack.Persistence/Configurations/CourseMaterialConfiguration.cs`

### 3. AppDbContext DbSet

`AppDbContext` icerisine `CourseMaterial` icin `DbSet` tanimi eklendi.

- `EduTrack.Persistence`

### 4. Migration

Course material tablosu ve iliskileri icin migration olusturuldu.

Migration ve database update islemleri manuel yonetilmelidir.

### 5. Repository

Course material icin repository abstraction ve persistence implementasyonu olusturuldu.

- `EduTrack.Application/Repositories/ICourseMaterialRepository.cs`
- `EduTrack.Persistence/Repositories/CourseMaterialRepository.cs`

### 6. Repository DI

Repository implementasyonu dependency injection container'a kaydedildi.

- `EduTrack.Persistence/PersistenceExtensions.cs`

### 7. DTO

Course material islemlerinde kullanilacak DTO olusturuldu.

- `EduTrack.Application/DTOs/File/CourseMaterialDto.cs`

### 8. Service Abstraction ve Implementation

Course material servis interface'i ve concrete servis sinifi olusturuldu.

- `EduTrack.Application/Services/Abstract/ICourseMaterialService.cs`
- `EduTrack.Application/Services/Concrete/CourseMaterialService.cs`

### 9. Service DI

Course material servisi application katmanindaki DI kayitlarina eklendi.

- `EduTrack.Application/ApplicationExtensions.cs`

### 10. Business Rules

Course material islemleri icin business rule sinifi olusturuldu.

- `EduTrack.Application/BusinessRules/CourseMaterialBusinessRules.cs`

### 11. Service Implementation

Course material servis metotlari business rules ve repository kullanacak sekilde implemente edildi.

- `EduTrack.Application/Services/Concrete/CourseMaterialService.cs`

### 12. MVC Baglantisi

Ozellik MVC projesine controller ve view uzerinden baglandi.

- `EduTrack.Web/Controllers/MaterialsController.cs`
- `EduTrack.Web/Views/Materials/Add.cshtml`

## Yeni Bir Degisiklik Icin Kontrol Listesi

Benzer bir entity veya modul eklenirken asagidaki sira takip edilebilir:

1. Domain entity sinifini olustur.
2. Persistence configuration sinifini ekle.
3. `AppDbContext` icerisine ilgili `DbSet` tanimini ekle.
4. Gerekli migration'i manuel olarak olustur.
5. Application repository interface'ini olustur.
6. Persistence repository implementasyonunu olustur.
7. Repository DI kaydini `PersistenceExtensions` icine ekle.
8. Gerekli DTO siniflarini olustur.
9. Service interface ve concrete service sinifini olustur.
10. Service DI kaydini `ApplicationExtensions` icine ekle.
11. Business rules sinifini olustur.
12. Service metotlarini repository ve business rules ile implemente et.
13. Web katmaninda controller ve view baglantilarini yap.
14. Build alarak katmanlar arasi referanslari ve DI kayitlarini kontrol et.

