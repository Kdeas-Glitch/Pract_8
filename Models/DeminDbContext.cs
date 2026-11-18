using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Pract_8_Demin.Models;

public partial class DeminDbContext : DbContext
{
    public DeminDbContext()
    {
    }

    public DeminDbContext(DbContextOptions<DeminDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RevokedToken> RevokedTokens { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer("Server=sql.ects;Database=DeminDB;User Id=student_00;Password=student_00;TrustServerCertificate=True;");
                //=> optionsBuilder.UseSqlServer("Server = localhost; Database = DeminDB; Trusted_Connection = True; User Id = student_04; Password = student_04;TrustServerCertificate = True; ");


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(e => e.TokenHash)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshTokens_User");
        });

        modelBuilder.Entity<RevokedToken>(entity =>
        {
            entity.Property(e => e.Jti).HasMaxLength(64);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.ToTable("Task");

            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.OwnerLogin).IsUnicode(false);
            entity.Property(e => e.Title).IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Login).IsUnicode(false);
            entity.Property(e => e.PasswordHash).IsUnicode(false);
            entity.Property(e => e.Role).IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
