using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;

namespace Tuuuur.Infrastructure.Data.EntityFramework;

public partial class BaseTuuuurContext : DbContext
{
    public BaseTuuuurContext(DbContextOptions p_DbContextOptions)
        : base(p_DbContextOptions)
    {
    }

    public virtual DbSet<Theme_THM> Theme_THM { get; set; }

    public virtual DbSet<User_USR> User_USR { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Theme_THM>(entity =>
        {
            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User_USR>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_USER_USR");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NickName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .IsRequired()
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
