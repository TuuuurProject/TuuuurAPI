using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;

namespace Tuuuur.Infrastructure.Data.EntityFramework;

public partial class BaseTuuuurContext : DbContext
{
    public BaseTuuuurContext(DbContextOptions options)
        : base(options)
    {
    }

    public virtual DbSet<DifficultyDft> DifficultyDft { get; set; }

    public virtual DbSet<PartyTypePtt> PartyTypePtt { get; set; }

    public virtual DbSet<ThemeThm> ThemeThm { get; set; }

    public virtual DbSet<UserAuthUat> UserAuthUat { get; set; }

    public virtual DbSet<UserUsr> UserUsr { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DifficultyDft>(entity =>
        {
            entity.ToTable("Difficulty_DFT", "ref");

            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PartyTypePtt>(entity =>
        {
            entity.ToTable("PartyType_PTT", "ref");

            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ThemeThm>(entity =>
        {
            entity.ToTable("Theme_THM");

            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserAuthUat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_USERAUTH_UAT");

            entity.ToTable("UserAuth_UAT");

            entity.HasIndex(e => new { e.UserId, e.ExpiresAt }, "IX_UserOTP_UserId");

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.ExpiresAt).HasDefaultValueSql("(dateadd(minute,(15),sysutcdatetime()))");

            entity.HasOne(d => d.User).WithMany(p => p.UserAuthUat)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAuth_User");
        });

        modelBuilder.Entity<UserUsr>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_USER_USR");

            entity.ToTable("User_USR");

            entity.HasIndex(e => e.Email, "IX_UserEmail").IsUnique();

            entity.HasIndex(e => e.NickName, "IX_UserNickName").IsUnique();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.IsNew).HasDefaultValue(true);
            entity.Property(e => e.NickName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
