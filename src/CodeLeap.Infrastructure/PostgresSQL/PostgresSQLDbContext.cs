using CodeLeap.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeLeap.Infrastructure.PostgresSQL
{
    public class PostgresSQLDbContext:DbContext
    {
        public PostgresSQLDbContext(DbContextOptions<PostgresSQLDbContext> options) : base(options)
        {
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<ProductEntity> Products { get; set; }

        protected void onModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .IsRequired();

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.password)
                    .IsRequired()
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<ProductEntity>(entity => {
                entity.ToTable("products");

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
            });
        }
    }
}
