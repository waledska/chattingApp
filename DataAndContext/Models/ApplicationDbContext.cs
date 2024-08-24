using System;
using System.Collections.Generic;
using chattingApp.DataAndContext.ModelsForChattingApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace chattingApp.DataAndContext.Models
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Group> Groups { get; set; } = null!;
        public virtual DbSet<GroupMember> GroupMembers { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=SAKSOOOK;Database=chattingApp;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>(entity =>
            {
                entity.ToTable("group");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("createdAt");

                entity.Property(e => e.CreatedById)
                    .HasMaxLength(450)
                    .HasColumnName("createdBy_id");

                entity.Property(e => e.GroupName)
                    .HasMaxLength(150)
                    .HasColumnName("groupName");
            });

            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.ToTable("group_member");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.GroupId).HasColumnName("group_id");

                entity.Property(e => e.UserId)
                    .HasMaxLength(450)
                    .HasColumnName("user_id");

                entity.Property(e => e.UserJoinedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("userJoinedAt");

                entity.Property(e => e.UserRemovedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("userRemovedAt");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.GroupMembers)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_group_member_group");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("messages");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.GroupId).HasColumnName("group_id");

                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");

                entity.Property(e => e.IsModified).HasColumnName("isModified");

                entity.Property(e => e.MessageStatus)
                    .HasMaxLength(100)
                    .HasColumnName("messageStatus");

                entity.Property(e => e.MessageText).HasColumnName("messageText");

                entity.Property(e => e.RecieverId)
                    .HasMaxLength(450)
                    .HasColumnName("reciever_id");

                entity.Property(e => e.SenderId)
                    .HasMaxLength(450)
                    .HasColumnName("sender_id");

                entity.Property(e => e.TimeOfSend)
                    .HasColumnType("datetime")
                    .HasColumnName("timeOfSend");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
