//using Microsoft.EntityFrameworkCore;
//using WhatsAppBulkMessaging.Domain.Entities;

//namespace WhatsAppBulkMessaging.Infrastructure.Data;

//public class AppDbContext : DbContext
//{
//    public AppDbContext(DbContextOptions<AppDbContext> options)
//        : base(options)
//    {
//    }

//    public DbSet<WhatsAppMessage> WhatsAppMessages { get; set; }
//    public DbSet<WhatsAppTemplate> WhatsAppTemplates { get; set; }

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        base.OnModelCreating(modelBuilder);

//        modelBuilder.HasDefaultSchema("erpsystem");

//        modelBuilder.Entity<WhatsAppMessage>(entity =>
//        {
//            entity.ToTable("tblwhatsappmessages");

//            entity.HasKey(x => x.Id);

//            entity.Property(x => x.PhoneNumber)
//                .HasMaxLength(20)
//                .IsRequired();

//            entity.Property(x => x.TemplateName)
//                .HasMaxLength(200)
//                .IsRequired();

//            entity.Property(x => x.MetaMessageId)
//                .HasMaxLength(200);

//            entity.Property(x => x.Status)
//                .HasMaxLength(50)
//                .IsRequired();

//            entity.Property(x => x.FailureReason)
//                .HasMaxLength(1000);
//        });

//        modelBuilder.Entity<WhatsAppTemplate>(entity =>
//        {
//            entity.ToTable("tblwhatsapptemplates");

//            entity.HasKey(x => x.Id);

//            entity.Property(x => x.TemplateName)
//                .HasMaxLength(200)
//                .IsRequired();

//            entity.Property(x => x.LanguageCode)
//                .HasMaxLength(20)
//                .IsRequired();

//            entity.Property(x => x.HeaderType)
//                .HasMaxLength(50);

//            entity.Property(x => x.HeaderMediaId)
//                .HasMaxLength(200);

//            entity.Property(x => x.IsActive)
//                .IsRequired();
//        });
//    }
//}

using Microsoft.EntityFrameworkCore;
using WhatsAppBulkMessaging.Domain.Entities;

namespace WhatsAppBulkMessaging.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<WhatsAppTemplate> WhatsAppTemplates { get; set; }
    public DbSet<WhatsAppMessage> WhatsAppMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("erpsystem");

        modelBuilder.Entity<WhatsAppTemplate>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TemplateName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.LanguageCode)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });

        modelBuilder.Entity<WhatsAppMessage>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(x => x.CandidateName)
                .HasMaxLength(200);

            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Pending");

            entity.Property(x => x.MetaMessageId)
                .HasMaxLength(200);

            entity.Property(x => x.ErrorMessage)
                .HasMaxLength(1000);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.UpdatedAt);

            entity.HasOne(x => x.WhatsAppTemplate)
            .WithMany()
            .HasForeignKey(x => x.WhatsAppTemplateId)
            .OnDelete(DeleteBehavior.Restrict);
        });
    }
}