using CodeLeap.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodeLeap.Infrastructure.PostgresSQL
{
    public class PostgresSqlDbContext : IdentityDbContext<UserEntity>
    {
        public PostgresSqlDbContext(DbContextOptions<PostgresSqlDbContext> options) : base(options)
        {
        }

        // Users DbSet is inherited from IdentityDbContext
        public DbSet<ProductEntity> Products { get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPORTANT: Call base first for Identity configuration
            base.OnModelCreating(modelBuilder);

            // Custom configuration for UserEntity (beyond Identity defaults)
            modelBuilder.Entity<UserEntity>(entity =>
            {
                // Identity already configures the primary key (Id) and core properties
                // We only need to configure our custom properties
                entity.Property(e => e.CreatedBy)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<ProductEntity>(entity => {
                entity.ToTable("Products");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .IsRequired();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Stock)
                    .IsRequired();

                entity.Property(e => e.CreatedBy)
                    .IsRequired();

                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne<UserEntity>()                      // Product has one User
                     .WithMany(u => u.Products)                 // User has many Products
                     .HasForeignKey(p => p.CreatedBy)           // FK in Product
                     .HasPrincipalKey(u => u.Id)                // PK in User
                     .OnDelete(DeleteBehavior.Restrict);        // Prevent cascade delete
             });

            modelBuilder.Entity<RefreshTokenEntity>(entity =>
            {
                entity.ToTable("RefreshTokens");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .IsRequired();

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.ExpiresAt)
                    .IsRequired();

                entity.Property(e => e.IsRevoked)
                    .IsRequired()
                    .HasDefaultValue(false);
            });
        }
    }
}
