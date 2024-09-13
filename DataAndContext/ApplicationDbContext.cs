using chattingApp.DataAndContext.Models;
using chattingApp.DataAndContext.ModelsForChattingApp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace chattingApp.DataAndContext
{
    public partial class ApplicationDbContext: IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):
            base(options)
        {
            
        }
        // adding dbsets for the new tables that we will insert in this identity context 
        public virtual DbSet<Group> Groups { get; set; } = null!;
        public virtual DbSet<GroupMember> GroupMembers { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<phoneOtp> PhoneOtps { get; set; } = null!;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // the old constructor that define the authentication Models and theire relations
            base.OnModelCreating(modelBuilder);

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

            modelBuilder.Entity<phoneOtp>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd(); // This ensures the ID is auto-incremented

                entity.Property(e => e.validTo)
                    .HasColumnType("datetime")
                    .HasColumnName("validTo");

                entity.Property(e => e.PhoneNumber)
                    .IsRequired() // Ensuring the PhoneNumber is required
                    .HasMaxLength(50)
                    .HasColumnName("phoneNumber");

                entity.Property(e => e.otp)
                    .IsRequired() // Ensuring the OTP is required
                    .HasMaxLength(50)
                    .HasColumnName("otp");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
