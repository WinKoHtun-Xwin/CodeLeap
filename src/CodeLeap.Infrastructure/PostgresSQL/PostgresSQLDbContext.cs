using CodeLeap.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeLeap.Infrastructure.PostgresSQL
{
    public class PostgresSQLDbContext:DbContext
    {
        public PostgresSQLDbContext(DbContextOptions<PostgresSQLDbContext> options) : base(options)
        {
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<ProductEntity> Products { get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .IsRequired();

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(500);
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
