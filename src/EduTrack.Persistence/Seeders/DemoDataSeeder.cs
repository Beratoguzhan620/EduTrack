using Core.Security;
using EduTrack.Domain.Constants;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Persistence.Seeders;

/// <summary>
/// Demo classroom for local development and the live demo: one instructor, eight students,
/// three courses with announcements, materials, assignments and graded work.
/// Runs once, only when Seed:DemoContent is enabled.
/// </summary>
public static class DemoDataSeeder
{
    public const string DemoPassword = "Demo123!";
    public const string InstructorEmail = "ogretmen@example.com";

    public static async Task SeedAsync(AppDbContext context, TimeProvider? timeProvider = null)
    {
        if (await context.Users.AnyAsync(u => u.Email == InstructorEmail))
            return;

        var now = (timeProvider ?? TimeProvider.System).GetUtcNow().UtcDateTime;
        var today = now.Date;

        var instructor = CreateUser("Ayşe", "Kaya", InstructorEmail);
        instructor.UserRoles = [new UserRole { RoleId = RoleConstants.InstructorId }];

        var students = new[]
        {
            CreateUser("Ali", "Yılmaz", "ali@example.com"),
            CreateUser("Zeynep", "Demir", "zeynep@example.com"),
            CreateUser("Mehmet", "Çelik", "mehmet@example.com"),
            CreateUser("Elif", "Şahin", "elif@example.com"),
            CreateUser("Can", "Aydın", "can@example.com"),
            CreateUser("Defne", "Öztürk", "defne@example.com"),
            CreateUser("Emir", "Arslan", "emir@example.com"),
            CreateUser("Ece", "Koç", "ece@example.com"),
        };

        context.Users.Add(instructor);
        context.Users.AddRange(students);

        var courses = new[]
        {
            new Course { Title = "5-A Matematik", Description = "Kesirler, ondalık gösterim, oran ve problem çözme.", Code = "MAT5A026", InstructorId = instructor.Id, IsActive = true, CreatedAt = today.AddDays(-30) },
            new Course { Title = "5-A Fen Bilimleri", Description = "Canlılar dünyası, kuvvetin ölçülmesi ve maddenin değişimi.", Code = "FEN5A026", InstructorId = instructor.Id, IsActive = true, CreatedAt = today.AddDays(-28) },
            new Course { Title = "Kodlama Kulübü", Description = "Scratch ile algoritma ve basit oyun tasarımı.", Code = "KOD2026K", InstructorId = instructor.Id, IsActive = true, CreatedAt = today.AddDays(-21) },
        };

        foreach (var course in courses)
            course.Id = Guid.NewGuid();

        context.Courses.AddRange(courses);

        foreach (var (course, index) in courses.Select((c, i) => (c, i)))
        {
            var members = index == 2 ? students.Take(5) : students;
            context.CourseStudent.AddRange(members.Select((s, i) => new CourseStudent
            {
                CourseId = course.Id,
                UserId = s.Id,
                EnrolledAt = course.CreatedAt.AddDays(1).AddHours(i),
            }));
        }

        var math = courses[0];
        var science = courses[1];
        var coding = courses[2];

        AddAnnouncement(context, math, instructor, today.AddDays(-12), "Hoş geldiniz! Bu dönem kesirler ve problem çözme üzerinde çalışacağız. Her cuma kısa bir değerlendirme yapacağız.");
        AddAnnouncement(context, math, instructor, today.AddDays(-2), "Perşembe günkü dersimiz kütüphanede yapılacak. Defterlerinizi getirmeyi unutmayın.");
        AddAnnouncement(context, science, instructor, today.AddDays(-5), "Gelecek hafta okul bahçesinde bitki gözlemi yapacağız. Gözlem formunu materyallerde bulabilirsiniz.");
        AddAnnouncement(context, coding, instructor, today.AddDays(-3), "Bu hafta Scratch'te ilk oyunumuzu tamamlıyoruz. Bilgisayar laboratuvarında buluşalım.");

        AddMaterial(context, math, instructor, today.AddDays(-11), "Kesirlerle toplama ve çıkarma", MaterialType.Content,
            "Paydaları eşit kesirlerde paylar toplanır, payda aynı kalır.\nPaydaları farklıysa önce ortak payda bulunur.\n\nÖrnek: 1/4 + 2/4 = 3/4", null);
        AddMaterial(context, math, instructor, today.AddDays(-6), "Ondalık gösterim alıştırmaları", MaterialType.Link,
            "Etkileşimli alıştırma sayfası.", "https://www.khanacademy.org/math/arithmetic");
        AddMaterial(context, science, instructor, today.AddDays(-5), "Bitki gözlem formu", MaterialType.Content,
            "Gözlem formunda bitkinin adı, boyu, yaprak sayısı ve rengi yer alacak. Her gün aynı saatte ölçüm yapın.", null);
        AddMaterial(context, coding, instructor, today.AddDays(-3), "Scratch başlangıç rehberi", MaterialType.Link,
            "Scratch'in resmi başlangıç eğitimleri.", "https://scratch.mit.edu/ideas");

        var fractions = AddAssignment(context, math, instructor, today.AddDays(-10), today.AddDays(-3).AddHours(20).AddMinutes(59),
            "Kesirlerle problem çözme", "Ders kitabı 4. ünite sonundaki 1-10 arası soruları çözün. Çözüm adımlarını yazmayı unutmayın.", 100);
        var decimals = AddAssignment(context, math, instructor, today.AddDays(-2), today.AddDays(5).AddHours(20).AddMinutes(59),
            "Ondalık sayılar çalışma kâğıdı", "Materyallerdeki alıştırma sayfasından 3 bölümü tamamlayın ve sonuçlarınızı yazın.", 50);
        var plants = AddAssignment(context, science, instructor, today.AddDays(-5), today.AddDays(4).AddHours(20).AddMinutes(59),
            "Bitki gözlem raporu", "Bir hafta boyunca seçtiğiniz bitkiyi gözlemleyin ve gözlem formunu doldurun.", 100);

        var grades = new int?[] { 95, 80, 72, null, 88, 64, null, 90 };
        for (var i = 0; i < students.Length; i++)
        {
            if (i == 6)
                continue; // one missing submission

            var lateBy = i == 5 ? 1 : -1;
            context.AssignmentSubmissions.Add(new AssignmentSubmission
            {
                AssignmentId = fractions.Id,
                StudentId = students[i].Id,
                Text = "1) 3/4 + 1/8 = 7/8\n2) 2/3 × 9 = 6\n3) 5/6 − 1/3 = 1/2\n…",
                SubmittedAt = fractions.DueDate.AddDays(lateBy).AddHours(-i),
                Grade = grades[i],
                Feedback = grades[i] switch
                {
                    >= 90 => "Çok düzenli bir çalışma, tebrikler.",
                    >= 75 => "İyi. Sadeleştirmeyi unutma.",
                    not null => "Payda eşitleme adımlarını tekrar edelim.",
                    _ => null,
                },
                GradedAt = grades[i] is null ? null : fractions.DueDate.AddDays(2),
            });
        }

        foreach (var student in students.Take(3))
        {
            context.AssignmentSubmissions.Add(new AssignmentSubmission
            {
                AssignmentId = decimals.Id,
                StudentId = student.Id,
                Text = "0,25 + 0,5 = 0,75\n1,2 × 3 = 3,6",
                SubmittedAt = now.AddHours(-6),
            });
        }

        context.AssignmentSubmissions.Add(new AssignmentSubmission
        {
            AssignmentId = plants.Id,
            StudentId = students[1].Id,
            Text = "Fasulye bitkisi: 1. gün 3 cm, 3. gün 5 cm, 5. gün 8 cm. Yaprak sayısı 2'den 4'e çıktı.",
            SubmittedAt = now.AddHours(-20),
        });

        await context.SaveChangesAsync();
    }

    private static User CreateUser(string firstName, string lastName, string email)
    {
        var password = HashingHelper.CreatePasswordHash(DemoPassword);
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            IsActive = true,
            EmailConfirmed = true,
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt,
            UserRoles = [],
        };
    }

    private static void AddAnnouncement(AppDbContext context, Course course, User instructor, DateTime date, string text) =>
        context.StreamPosts.Add(new StreamPost
        {
            CourseId = course.Id,
            InstructorId = instructor.Id,
            Title = $"{instructor.FirstName} {instructor.LastName}",
            Content = text,
            Type = StreamPostType.Announcement,
            Date = date.AddHours(9),
        });

    private static void AddMaterial(AppDbContext context, Course course, User instructor, DateTime date, string title, MaterialType type, string description, string? url)
    {
        var material = new CourseMaterial
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = title,
            Description = description,
            Type = type,
            Url = url,
            Date = date.AddHours(10),
        };
        context.CourseMaterials.Add(material);
        context.StreamPosts.Add(new StreamPost
        {
            CourseId = course.Id,
            InstructorId = instructor.Id,
            Title = $"{instructor.FirstName} {instructor.LastName} yeni bir materyal paylaştı",
            Content = material.Id.ToString(),
            Type = StreamPostType.Material,
            Date = material.Date,
        });
    }

    private static Assignment AddAssignment(AppDbContext context, Course course, User instructor, DateTime createdAt, DateTime dueDate, string title, string instructions, int maxPoints)
    {
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            CreatedById = instructor.Id,
            Title = title,
            Instructions = instructions,
            DueDate = dueDate,
            MaxPoints = maxPoints,
            CreatedAt = createdAt.AddHours(11),
        };
        context.Assignments.Add(assignment);
        context.StreamPosts.Add(new StreamPost
        {
            CourseId = course.Id,
            InstructorId = instructor.Id,
            Title = $"{instructor.FirstName} {instructor.LastName} yeni bir ödev paylaştı",
            Content = assignment.Id.ToString(),
            Type = StreamPostType.Assignment,
            Date = assignment.CreatedAt,
        });
        return assignment;
    }
}
