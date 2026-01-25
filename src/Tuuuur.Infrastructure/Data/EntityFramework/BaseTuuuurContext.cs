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

    public virtual DbSet<AnswerAns> AnswerAns { get; set; }

    public virtual DbSet<DifficultyDft> DifficultyDft { get; set; }

    public virtual DbSet<EloElo> EloElo { get; set; }

    public virtual DbSet<PartyDifficultyPdf> PartyDifficultyPdf { get; set; }

    public virtual DbSet<PartyPty> PartyPty { get; set; }

    public virtual DbSet<PartyQuestionPqt> PartyQuestionPqt { get; set; }

    public virtual DbSet<PartyThemePth> PartyThemePth { get; set; }

    public virtual DbSet<PartyTypePty> PartyTypePty { get; set; }

    public virtual DbSet<PartyUserPus> PartyUserPus { get; set; }

    public virtual DbSet<QuestionQst> QuestionQst { get; set; }

    public virtual DbSet<QuestionThemeQth> QuestionThemeQth { get; set; }

    public virtual DbSet<RefreshTokenRtk> RefreshTokenRtk { get; set; }

    public virtual DbSet<ThemeThm> ThemeThm { get; set; }

    public virtual DbSet<UserAuthUat> UserAuthUat { get; set; }

    public virtual DbSet<UserPartyQuestionUpq> UserPartyQuestionUpq { get; set; }

    public virtual DbSet<UserUsr> UserUsr { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnswerAns>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ANSWER_ANS");

            entity.ToTable("Answer_ANS");

            entity.HasIndex(e => e.IdQuestion, "IX_Answer_Question");

            entity.Property(e => e.IdQuestion).HasColumnName("Id_Question");
            entity.Property(e => e.Value)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.IdQuestionNavigation).WithMany(p => p.AnswerAns)
                .HasForeignKey(d => d.IdQuestion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Answer_Question");
        });

        modelBuilder.Entity<DifficultyDft>(entity =>
        {
            entity.ToTable("Difficulty_DFT", "ref");

            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EloElo>(entity =>
        {
            entity.HasKey(e => new { e.IdUser, e.IdTheme }).HasName("PK_ELO_ELO");

            entity.ToTable("Elo_ELO");

            entity.Property(e => e.IdUser).HasColumnName("Id_User");
            entity.Property(e => e.IdTheme).HasColumnName("Id_Theme");
            entity.Property(e => e.Value).HasDefaultValue(1000);

            entity.HasOne(d => d.IdThemeNavigation).WithMany(p => p.EloElo)
                .HasForeignKey(d => d.IdTheme)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Elo_Theme");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.EloElo)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Elo_User");
        });

        modelBuilder.Entity<PartyDifficultyPdf>(entity =>
        {
            entity.ToTable("PartyDifficulty_PDF");

            entity.Property(e => e.IdDifficulty).HasColumnName("Id_Difficulty");
            entity.Property(e => e.IdParty).HasColumnName("Id_Party");

            entity.HasOne(d => d.IdDifficultyNavigation).WithMany(p => p.PartyDifficultyPdf)
                .HasForeignKey(d => d.IdDifficulty)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartyDifficulty_Theme");

            entity.HasOne(d => d.IdPartyNavigation).WithMany(p => p.PartyDifficultyPdf)
                .HasForeignKey(d => d.IdParty)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartyDifficulty_Party");
        });

        modelBuilder.Entity<PartyPty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PARTY_PTY");

            entity.ToTable("Party_PTY");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Dt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IdPartyType).HasColumnName("Id_Party_Type");
            entity.Property(e => e.IdUserHost).HasColumnName("Id_User_Host");

            entity.HasOne(d => d.IdPartyTypeNavigation).WithMany(p => p.PartyPty)
                .HasForeignKey(d => d.IdPartyType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Party_Type");

            entity.HasOne(d => d.IdUserHostNavigation).WithMany(p => p.PartyPty)
                .HasForeignKey(d => d.IdUserHost)
                .HasConstraintName("FK_Party_HostUser");
        });

        modelBuilder.Entity<PartyQuestionPqt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PARTYQUESTION_PQT");

            entity.ToTable("PartyQuestion_PQT");

            entity.HasIndex(e => new { e.IdParty, e.Order }, "IX_PartyQuestion_Party");

            entity.Property(e => e.IdParty).HasColumnName("Id_Party");
            entity.Property(e => e.IdQuestion).HasColumnName("Id_Question");

            entity.HasOne(d => d.IdPartyNavigation).WithMany(p => p.PartyQuestionPqt)
                .HasForeignKey(d => d.IdParty)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartyQuestion_Party");

            entity.HasOne(d => d.IdQuestionNavigation).WithMany(p => p.PartyQuestionPqt)
                .HasForeignKey(d => d.IdQuestion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartyQuestion_Question");
        });

        modelBuilder.Entity<PartyThemePth>(entity =>
        {
            entity.ToTable("PartyTheme_PTH");

            entity.Property(e => e.IdParty).HasColumnName("Id_Party");
            entity.Property(e => e.IdTheme).HasColumnName("Id_Theme");

            entity.HasOne(d => d.IdPartyNavigation).WithMany(p => p.PartyThemePth)
                .HasForeignKey(d => d.IdParty)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartyTheme_Party");

            entity.HasOne(d => d.IdThemeNavigation).WithMany(p => p.PartyThemePth)
                .HasForeignKey(d => d.IdTheme)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartyTheme_Theme");
        });

        modelBuilder.Entity<PartyTypePty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PartyType_PTT");

            entity.ToTable("PartyType_PTY", "ref");

            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PartyUserPus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PARTYUSER_PUS");

            entity.ToTable("PartyUser_PUS");

            entity.HasIndex(e => new { e.IdUser, e.IdParty }, "IX_PartyUserUniq").IsUnique();

            entity.Property(e => e.IdParty).HasColumnName("Id_Party");
            entity.Property(e => e.IdUser).HasColumnName("Id_User");

            entity.HasOne(d => d.IdPartyNavigation).WithMany(p => p.PartyUserPus)
                .HasForeignKey(d => d.IdParty)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartyUser_Party");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.PartyUserPus)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartyUser_User");
        });

        modelBuilder.Entity<QuestionQst>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_QUESTION_QST");

            entity.ToTable("Question_QST");

            entity.Property(e => e.IdDifficulty).HasColumnName("Id_Difficulty");
            entity.Property(e => e.Question)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.IdDifficultyNavigation).WithMany(p => p.QuestionQst)
                .HasForeignKey(d => d.IdDifficulty)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Question_Difficulty");
        });

        modelBuilder.Entity<QuestionThemeQth>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_QUESTION_THEME_QTH");

            entity.ToTable("QuestionTheme_QTH");

            entity.HasIndex(e => new { e.IdQuestion, e.IdTheme }, "IX_QuestionTheme").IsUnique();

            entity.Property(e => e.IdQuestion).HasColumnName("Id_Question");
            entity.Property(e => e.IdTheme).HasColumnName("Id_Theme");

            entity.HasOne(d => d.IdQuestionNavigation).WithMany(p => p.QuestionThemeQth)
                .HasForeignKey(d => d.IdQuestion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuestionTheme_Question");

            entity.HasOne(d => d.IdThemeNavigation).WithMany(p => p.QuestionThemeQth)
                .HasForeignKey(d => d.IdTheme)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuestionTheme_Theme");
        });

        modelBuilder.Entity<RefreshTokenRtk>(entity =>
        {
            entity.ToTable("RefreshToken_RTK");

            entity.HasIndex(e => e.Token, "IX_RefreshToken_Token");

            entity.HasIndex(e => e.UserId, "IX_RefreshToken_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokenRtk)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshToken_User");
        });

        modelBuilder.Entity<ThemeThm>(entity =>
        {
            entity.ToTable("Theme_THM");

            entity.Property(e => e.Icon)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
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

        modelBuilder.Entity<UserPartyQuestionUpq>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_USERPARTYQUESTION_UPQ");

            entity.ToTable("UserPartyQuestion_UPQ");

            entity.HasIndex(e => new { e.IdUser, e.IdPartyQuestion }, "IX_UserPartyQuestion_User");

            entity.Property(e => e.AnswersOrder).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IdAnswer).HasColumnName("Id_Answer");
            entity.Property(e => e.IdPartyQuestion).HasColumnName("Id_Party_Question");
            entity.Property(e => e.IdUser).HasColumnName("Id_User");

            entity.HasOne(d => d.IdAnswerNavigation).WithMany(p => p.UserPartyQuestionUpq)
                .HasForeignKey(d => d.IdAnswer)
                .HasConstraintName("FK_UserPartyQuestion_Answer");

            entity.HasOne(d => d.IdPartyQuestionNavigation).WithMany(p => p.UserPartyQuestionUpq)
                .HasForeignKey(d => d.IdPartyQuestion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserPartyQuestion_PartyQuestion");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.UserPartyQuestionUpq)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserPartyQuestion_User");
        });

        modelBuilder.Entity<UserUsr>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_USER_USR");

            entity.ToTable("User_USR", tb => tb.HasTrigger("TR_User_DeleteCascade"));

            entity.HasIndex(e => new { e.Email, e.IsGoogleUser }, "IX_UserEmail").IsUnique();

            entity.HasIndex(e => e.NickName, "IX_UserNickName").IsUnique();

            entity.Property(e => e.Avatar).IsUnicode(false);
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
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
