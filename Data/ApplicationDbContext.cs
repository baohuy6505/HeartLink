using HeartLink.Models;
using Microsoft.EntityFrameworkCore;

namespace HeartLink.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>().ToTable("Accounts");
            modelBuilder.Entity<Profile>().ToTable("Profiles");
            modelBuilder.Entity<Like>().ToTable("Likes");
            modelBuilder.Entity<Match>().ToTable("Matches");
            modelBuilder.Entity<Message>().ToTable("Messages");

            modelBuilder.Entity<Account>().HasKey(x => x.AccountID);
            modelBuilder.Entity<Profile>().HasKey(x => x.ProfileID);
            modelBuilder.Entity<Like>().HasKey(x => x.LikeID);
            modelBuilder.Entity<Match>().HasKey(x => x.MatchID);
            modelBuilder.Entity<Message>().HasKey(x => x.MessageID);

            modelBuilder.Entity<Profile>()
                .HasOne(x => x.Account)
                .WithOne(x => x.Profile)
                .HasForeignKey<Profile>(x => x.AccountID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(x => x.Sender)
                .WithMany(x => x.SentLikes)
                .HasForeignKey(x => x.SenderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Like>()
                .HasOne(x => x.Receiver)
                .WithMany(x => x.ReceivedLikes)
                .HasForeignKey(x => x.ReceiverID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(x => x.User1)
                .WithMany(x => x.MatchesAsUser1)
                .HasForeignKey(x => x.User1ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(x => x.User2)
                .WithMany(x => x.MatchesAsUser2)
                .HasForeignKey(x => x.User2ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(x => x.Match)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.MatchID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(x => x.Sender)
                .WithMany(x => x.MessagesSent)
                .HasForeignKey(x => x.SenderID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}