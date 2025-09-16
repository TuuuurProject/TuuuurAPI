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

    public virtual DbSet<Difficulty_DFT> Difficulty_DFT { get; set; }

    public virtual DbSet<PartyType_PTT> PartyType_PTT { get; set; }

    public virtual DbSet<Theme_THM> Theme_THM { get; set; }

    public virtual DbSet<UserAuth_UAT> UserAuth_UAT { get; set; }

    public virtual DbSet<User_USR> User_USR { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Difficulty_DFT>(entity =>
        {
            entity.ToTable("Difficulty_DFT", "ref");

            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PartyType_PTT>(entity =>
        {
            entity.ToTable("PartyType_PTT", "ref");

            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Theme_THM>(entity =>
        {
            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserAuth_UAT>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_USERAUTH_UAT");

            entity.HasIndex(e => new { e.UserId, e.ExpiresAt }, "IX_UserOTP_UserId");

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.ExpiresAt).HasDefaultValueSql("(dateadd(minute,(15),sysutcdatetime()))");

            entity.HasOne(d => d.User).WithMany(p => p.UserAuth_UAT)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAuth_User");
        });

        modelBuilder.Entity<User_USR>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_USER_USR");

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
