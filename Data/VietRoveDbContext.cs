
using Microsoft.EntityFrameworkCore;
using VietRove.Entities;

namespace VietRove.Data
{
    public class VietRoveDbContext : DbContext
    {
        public VietRoveDbContext(DbContextOptions<VietRoveDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<LoginHistory> LoginHistories { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<GuideProfile> GuideProfiles { get; set; } = null!;
        public DbSet<GuideProvince> GuideProvinces { get; set; } = null!;
        public DbSet<GuideLanguage> GuideLanguages { get; set; } = null!;
        public DbSet<GuideExperience> GuideExperiences { get; set; } = null!;
        public DbSet<GuideDocument> GuideDocuments { get; set; } = null!;
        public DbSet<Contract> Contracts { get; set; } = null!;
        public DbSet<ViolationReport> ViolationReports { get; set; } = null!;
        public DbSet<ViolationReportEvidence> ViolationReportEvidence { get; set; } = null!;
        public DbSet<ViolationReportAction> ViolationReportActions { get; set; } = null!;
        public DbSet<Article> Articles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // role
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);


            // user
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasDefaultValue(1);

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");


            // login history
            modelBuilder.Entity<LoginHistory>()
                .Property(lh => lh.LoginTime)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<LoginHistory>()
                .HasOne(lh => lh.User)
                .WithMany(u => u.LoginHistories)
                .HasForeignKey(lh => lh.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // category
            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.CategoryType, c.CategoryName })
                .IsUnique();

            modelBuilder.Entity<Category>()
                .Property(c => c.IsActive)
                .HasDefaultValue(true);


            // guide profile
            modelBuilder.Entity<GuideProfile>()
                .HasIndex(gp => gp.UserId)
                .IsUnique();

            modelBuilder.Entity<GuideProfile>()
                .Property(gp => gp.Status)
                .HasDefaultValue(0);

            modelBuilder.Entity<GuideProfile>()
                .Property(gp => gp.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<GuideProfile>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_GuideProfiles_ExperienceYears",
                    "[ExperienceYears] IS NULL OR [ExperienceYears] >= 0"
                ));

            modelBuilder.Entity<GuideProfile>()
                .HasOne(gp => gp.User)
                .WithOne()
                .HasForeignKey<GuideProfile>(gp => gp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuideProfile>()
                .HasOne(gp => gp.CardType)
                .WithMany(c => c.CardTypeProfiles)
                .HasForeignKey(gp => gp.CardTypeId)
                .OnDelete(DeleteBehavior.Restrict);


            // guide province
            modelBuilder.Entity<GuideProvince>()
                .HasKey(gp => new { gp.GuideId, gp.ProvinceId });

            modelBuilder.Entity<GuideProvince>()
                .HasOne(gp => gp.GuideProfile)
                .WithMany(gp => gp.GuideProvinces)
                .HasForeignKey(gp => gp.GuideId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuideProvince>()
                .HasOne(gp => gp.Province)
                .WithMany(c => c.GuideProvinces)
                .HasForeignKey(gp => gp.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);


            // guide language
            modelBuilder.Entity<GuideLanguage>()
                .HasIndex(gl => new { gl.GuideId, gl.LanguageId })
                .IsUnique();

            modelBuilder.Entity<GuideLanguage>()
                .Property(gl => gl.VerificationStatus)
                .HasDefaultValue(0);

            modelBuilder.Entity<GuideLanguage>()
                .Property(gl => gl.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<GuideLanguage>()
                .HasOne(gl => gl.GuideProfile)
                .WithMany(gp => gp.GuideLanguages)
                .HasForeignKey(gl => gl.GuideId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuideLanguage>()
                .HasOne(gl => gl.Language)
                .WithMany(c => c.GuideLanguages)
                .HasForeignKey(gl => gl.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GuideLanguage>()
                .HasOne(gl => gl.Verifier)
                .WithMany(u => u.VerifiedLanguages)
                .HasForeignKey(gl => gl.VerifiedBy)
                .OnDelete(DeleteBehavior.Restrict);


            // guide experience
            modelBuilder.Entity<GuideExperience>()
                .Property(ge => ge.VerificationStatus)
                .HasDefaultValue(0);

            modelBuilder.Entity<GuideExperience>()
                .Property(ge => ge.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<GuideExperience>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_GuideExperiences_DateRange",
                    "[StartDate] IS NULL OR [EndDate] IS NULL OR [EndDate] >= [StartDate]"
                ));

            modelBuilder.Entity<GuideExperience>()
                .HasOne(ge => ge.GuideProfile)
                .WithMany(gp => gp.GuideExperiences)
                .HasForeignKey(ge => ge.GuideId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuideExperience>()
                .HasOne(ge => ge.Verifier)
                .WithMany(u => u.VerifiedExperiences)
                .HasForeignKey(ge => ge.VerifiedBy)
                .OnDelete(DeleteBehavior.Restrict);


            // guide document
            modelBuilder.Entity<GuideDocument>()
                .Property(gd => gd.VerificationStatus)
                .HasDefaultValue(0);

            modelBuilder.Entity<GuideDocument>()
                .Property(gd => gd.UploadedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<GuideDocument>()
                .HasOne(gd => gd.GuideProfile)
                .WithMany(gp => gp.GuideDocuments)
                .HasForeignKey(gd => gd.GuideId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuideDocument>()
                .HasOne(gd => gd.Verifier)
                .WithMany(u => u.VerifiedDocuments)
                .HasForeignKey(gd => gd.VerifiedBy)
                .OnDelete(DeleteBehavior.Restrict);


            // contract
            modelBuilder.Entity<Contract>()
                .Property(c => c.Fee)
                .HasDefaultValue(0);

            modelBuilder.Entity<Contract>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Contract>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Contracts_DateRange",
                        "[EndDate] >= [StartDate]"
                    );

                    t.HasCheckConstraint(
                        "CK_Contracts_Fee",
                        "[Fee] >= 0"
                    );
                });

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.GuideProfile)
                .WithMany(gp => gp.Contracts)
                .HasForeignKey(c => c.GuideId)
                .OnDelete(DeleteBehavior.Restrict);


            // violation report
            modelBuilder.Entity<ViolationReport>()
                .Property(vr => vr.Status)
                .HasDefaultValue(0);

            modelBuilder.Entity<ViolationReport>()
                .Property(vr => vr.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<ViolationReport>()
                .HasOne(vr => vr.GuideProfile)
                .WithMany(gp => gp.ViolationReports)
                .HasForeignKey(vr => vr.GuideId)
                .OnDelete(DeleteBehavior.Restrict);


            // violation report evidence
            modelBuilder.Entity<ViolationReportEvidence>()
                .Property(e => e.UploadedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<ViolationReportEvidence>()
                .HasOne(e => e.Report)
                .WithMany(r => r.Evidence)
                .HasForeignKey(e => e.ReportId)
                .OnDelete(DeleteBehavior.Cascade);


            // violation report action
            modelBuilder.Entity<ViolationReportAction>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<ViolationReportAction>()
                .HasOne(a => a.Report)
                .WithMany(r => r.Actions)
                .HasForeignKey(a => a.ReportId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ViolationReportAction>()
                .HasOne(a => a.Admin)
                .WithMany(u => u.ViolationReportActions)
                .HasForeignKey(a => a.AdminId)
                .OnDelete(DeleteBehavior.Restrict);


            // article
            modelBuilder.Entity<Article>()
                .Property(a => a.ArticleType)
                .HasDefaultValue(0);

            modelBuilder.Entity<Article>()
                .Property(a => a.Status)
                .HasDefaultValue(0);

            modelBuilder.Entity<Article>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Article>()
                .HasOne(a => a.Author)
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);


            // date columns
            modelBuilder.Entity<GuideProfile>()
                .Property(gp => gp.DateOfBirth)
                .HasColumnType("date");

            modelBuilder.Entity<GuideProfile>()
                .Property(gp => gp.IssueDate)
                .HasColumnType("date");

            modelBuilder.Entity<GuideProfile>()
                .Property(gp => gp.ExpiryDate)
                .HasColumnType("date");

            modelBuilder.Entity<GuideExperience>()
                .Property(ge => ge.StartDate)
                .HasColumnType("date");

            modelBuilder.Entity<GuideExperience>()
                .Property(ge => ge.EndDate)
                .HasColumnType("date");

            modelBuilder.Entity<Contract>()
                .Property(c => c.StartDate)
                .HasColumnType("date");

            modelBuilder.Entity<Contract>()
                .Property(c => c.EndDate)
                .HasColumnType("date");


            // login history index
            modelBuilder.Entity<LoginHistory>()
                .HasIndex(lh => new { lh.UserId, lh.LoginTime })
                .IsDescending(false, true)
                .HasDatabaseName("IX_LoginHistory_UserId_LoginTime");


            // guide profile indexes
            modelBuilder.Entity<GuideProfile>()
                .HasIndex(gp => gp.Status)
                .HasDatabaseName("IX_GuideProfiles_Status");

            modelBuilder.Entity<GuideProfile>()
                .HasIndex(gp => gp.FullName)
                .HasDatabaseName("IX_GuideProfiles_FullName");

            modelBuilder.Entity<GuideProfile>()
                .HasIndex(gp => gp.ContactPhone)
                .IsUnique()
                .HasFilter("[ContactPhone] IS NOT NULL")
                .HasDatabaseName("UX_GuideProfiles_ContactPhone");

            modelBuilder.Entity<GuideProfile>()
                .HasIndex(gp => gp.IdentityNumber)
                .IsUnique()
                .HasFilter("[IdentityNumber] IS NOT NULL")
                .HasDatabaseName("UX_GuideProfiles_IdentityNumber");

            modelBuilder.Entity<GuideProfile>()
                .HasIndex(gp => gp.CardNumber)
                .IsUnique()
                .HasFilter("[CardNumber] IS NOT NULL")
                .HasDatabaseName("UX_GuideProfiles_CardNumber");


            // guide experience index
            modelBuilder.Entity<GuideExperience>()
                .HasIndex(ge => ge.GuideId)
                .HasDatabaseName("IX_GuideExperiences_GuideId");


            // guide document index
            modelBuilder.Entity<GuideDocument>()
                .HasIndex(gd => gd.GuideId)
                .HasDatabaseName("IX_GuideDocuments_GuideId");


            // contract index
            modelBuilder.Entity<Contract>()
                .HasIndex(c => c.GuideId)
                .HasDatabaseName("IX_Contracts_GuideId");


            // violation report index
            modelBuilder.Entity<ViolationReport>()
                .HasIndex(vr => vr.Status)
                .HasDatabaseName("IX_ViolationReports_Status");


            // article index
            modelBuilder.Entity<Article>()
                .HasIndex(a => new { a.Status, a.CreatedAt })
                .IsDescending(false, true)
                .HasDatabaseName("IX_Articles_Status_CreatedAt");
        }
    }
}
