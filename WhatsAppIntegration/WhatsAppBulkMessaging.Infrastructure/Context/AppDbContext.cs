using Microsoft.EntityFrameworkCore;
using WhatsAppBulkMessaging.Domain.Entities;

namespace WhatsAppBulkMessaging.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<WhatsAppMessage> WhatsAppMessages { get; set; }
    public DbSet<WhatsAppTemplate> WhatsAppTemplates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("erpsystem");

        modelBuilder.Entity<WhatsAppMessage>(entity =>
        {
            entity.ToTable("tblwhatsappmessages");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.TemplateName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.MetaMessageId)
                .HasMaxLength(200);

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.FailureReason)
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<WhatsAppTemplate>(entity =>
        {
            entity.ToTable("tblwhatsapptemplates");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.TemplateName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.LanguageCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.HeaderType)
                .HasMaxLength(50);

            entity.Property(x => x.HeaderMediaId)
                .HasMaxLength(200);

            entity.Property(x => x.IsActive)
                .IsRequired();
        });
    }
}