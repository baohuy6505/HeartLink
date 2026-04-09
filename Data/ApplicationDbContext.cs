using HeartLink.Models;
using Microsoft.EntityFrameworkCore;

namespace HeartLink.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<ProfileInterest> ProfileInterests => Set<ProfileInterest>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("ACCOUNT", t => t.UseSqlOutputClause(false));
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.PhoneNumber)
                  .IsUnique()
                  .HasFilter("[PhoneNumber] IS NOT NULL");
            entity.Property(x => x.CreatedDate).HasDefaultValueSql("GETDATE()");
            entity.Property(x => x.Status).HasDefaultValue(true);
            entity.Property(x => x.UserRole).HasDefaultValue("User");
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.ToTable("PROFILE", t => t.UseSqlOutputClause(false));
            entity.HasIndex(x => x.AccountID).IsUnique();
            entity.Property(x => x.CreatedDate).HasDefaultValueSql("GETDATE()");
            entity.Property(x => x.MinAge).HasDefaultValue(18);
            entity.Property(x => x.MaxAge).HasDefaultValue(99);
            entity.Property(x => x.Radius).HasDefaultValue(50);

            entity.HasOne(x => x.Account)
                .WithOne(x => x.Profile)
                .HasForeignKey<Profile>(x => x.AccountID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfileInterest>(entity =>
        {
            entity.ToTable("PROFILE_INTERESTS", t => t.UseSqlOutputClause(false));
            entity.HasKey(x => new { x.ProfileID, x.InterestName });

            entity.HasOne(x => x.Profile)
                .WithMany(x => x.Interests)
                .HasForeignKey(x => x.ProfileID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.ToTable("LIKES", t => t.UseSqlOutputClause(false));
            entity.HasIndex(x => new { x.SenderID, x.ReceiverID }).IsUnique();
            entity.Property(x => x.Timestamp).HasDefaultValueSql("GETDATE()");

            entity.HasOne(x => x.Sender)
                .WithMany(x => x.SentLikes)
                .HasForeignKey(x => x.SenderID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Receiver)
                .WithMany(x => x.ReceivedLikes)
                .HasForeignKey(x => x.ReceiverID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("MATCHES", t => t.UseSqlOutputClause(false));
            entity.Property(x => x.MatchedDate).HasDefaultValueSql("GETDATE()");
            entity.Property(x => x.Status).HasDefaultValue((byte)1);

            entity.Property<string>("PairKey")
                .HasMaxLength(50)
                .HasComputedColumnSql(
                    "CASE WHEN [User1ID] < [User2ID] THEN CONCAT([User1ID],N'-',[User2ID]) ELSE CONCAT([User2ID],N'-',[User1ID]) END",
                    stored: true);

            entity.HasIndex("PairKey").IsUnique();

            entity.HasOne(x => x.Like)
                .WithMany()
                .HasForeignKey(x => x.LikeID)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.User1)
                .WithMany(x => x.MatchesAsUser1)
                .HasForeignKey(x => x.User1ID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User2)
                .WithMany(x => x.MatchesAsUser2)
                .HasForeignKey(x => x.User2ID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("MESSAGE", t => t.UseSqlOutputClause(false));
            entity.Property(x => x.SentTime).HasDefaultValueSql("GETDATE()");
            entity.Property(x => x.IsRead).HasDefaultValue(false);

            entity.HasOne(x => x.Match)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.MatchID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Sender)
                .WithMany(x => x.MessagesSent)
                .HasForeignKey(x => x.SenderID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}