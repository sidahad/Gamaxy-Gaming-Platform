using GamaxyWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace GamaxyWebApplication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatFile> ChatFiles { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MatchAssign> MatchAssignToUsers { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Registration> Registrationinfo { get; set; }
        public DbSet<Contact> Contactinfo { get; set; }
        public DbSet<Favourite> Favourites { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<User> Usersinfo { get; set; }
        public DbSet<Gamer> Gamers { get; set; }
        public DbSet<FreeGameRating> FreeGameRatings { get; set; }
        public DbSet<PremiumGameRating> PremiumGameRatings { get; set; }
        public DbSet<LeaderboardPlayer> LeaderboardPlayers { get; set; }
        public DbSet<GamerPlayinghistory> Playinghistorygamer { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Payment> Payments { get; set; } // Added Payment DbSet

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LeaderboardPlayer>()
                .ToTable("LeaderboardPlayers");

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryId).HasColumnName("CategoryId");
                entity.Property(e => e.CategoryName).HasColumnName("CategoryName");
            });

            // Registration table name mapping and unique constraints
            modelBuilder.Entity<Registration>().ToTable("Registrationinfo");
            modelBuilder.Entity<Registration>()
                .HasIndex(r => r.Name)
                .IsUnique()
                .HasFilter("[Name] IS NOT NULL");
            modelBuilder.Entity<Registration>()
                .HasIndex(r => r.GamerCode)
                .IsUnique()
                .HasFilter("[GamerCode] IS NOT NULL");

            // Favourite relationship
            modelBuilder.Entity<Favourite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // FreeGameRating relationship
            modelBuilder.Entity<FreeGameRating>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // PremiumGameRating relationship
            modelBuilder.Entity<PremiumGameRating>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PremiumGameRating>()
                .HasOne(r => r.Game)
                .WithMany()
                .HasForeignKey(r => r.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Gamer)
                .WithMany()
                .HasForeignKey(p => p.GamerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Game)
                .WithMany()
                .HasForeignKey(p => p.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

//using GamaxyWebApplication.Models;
//using Microsoft.EntityFrameworkCore;
//namespace GamaxyWebApplication.Data
//{
//    public class ApplicationDbContext : DbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//            : base(options) { }

//        public DbSet<ChatMessage> ChatMessages { get; set; }
//        public DbSet<ChatFile> ChatFiles { get; set; }
//        public DbSet<UploadedFile> UploadedFiles { get; set; }
//        public DbSet<User> Users { get; set; }
//        public DbSet<Category> Categories { get; set; }
//        public DbSet<MatchAssign> MatchAssignToUsers { get; set; }
//        public DbSet<Match> Matches { get; set; }
//        public DbSet<Registration> Registrationinfo { get; set; }
//        public DbSet<Contact> Contactinfo { get; set; }
//        public DbSet<Favourite> Favourites { get; set; }
//        public DbSet<Game> Games { get; set; }
//        public DbSet<User> Usersinfo { get; set; }
//        public DbSet<Gamer> Gamers { get; set; }
//        public DbSet<FreeGameRating> FreeGameRatings { get; set; }
//        public DbSet<PremiumGameRating> PremiumGameRatings { get; set; }
//        public DbSet<LeaderboardPlayer> LeaderboardPlayers { get; set; }
//        public DbSet<GamerPlayinghistory> Playinghistorygamer { get; set; }
//        public DbSet<Feedback> Feedbacks { get; set; }
//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            modelBuilder.Entity<LeaderboardPlayer>()
//                .ToTable("LeaderboardPlayers"); // Ensure correct table name

//            // Category configuration
//            modelBuilder.Entity<Category>(entity =>
//            {
//                entity.ToTable("Categories");
//                entity.HasKey(e => e.CategoryId);
//                entity.Property(e => e.CategoryId).HasColumnName("CategoryId");
//                entity.Property(e => e.CategoryName).HasColumnName("CategoryName");
//            });

//            // Registration table name mapping and unique constraints
//            modelBuilder.Entity<Registration>().ToTable("Registrationinfo");
//            modelBuilder.Entity<Registration>()
//                .HasIndex(r => r.Name)
//                .IsUnique()
//                .HasFilter("[Name] IS NOT NULL"); // Ensure Name is unique when not null
//            modelBuilder.Entity<Registration>()
//                .HasIndex(r => r.GamerCode)
//                .IsUnique()
//                .HasFilter("[GamerCode] IS NOT NULL"); // Ensure GamerCode is unique when not null

//            // Favourite relationship
//            modelBuilder.Entity<Favourite>()
//                .HasOne(f => f.User)
//                .WithMany()
//                .HasForeignKey(f => f.UserId)
//                .OnDelete(DeleteBehavior.Cascade);

//            // FreeGameRating relationship
//            modelBuilder.Entity<FreeGameRating>()
//                .HasOne(r => r.User)
//                .WithMany()
//                .HasForeignKey(r => r.UserId)
//                .OnDelete(DeleteBehavior.Cascade);

//            // PremiumGameRating relationship
//            modelBuilder.Entity<PremiumGameRating>()
//                .HasOne(r => r.User)
//                .WithMany()
//                .HasForeignKey(r => r.UserId)
//                .OnDelete(DeleteBehavior.Cascade);

//            modelBuilder.Entity<PremiumGameRating>()
//                .HasOne(r => r.Game)
//                .WithMany()
//                .HasForeignKey(r => r.GameId)
//                .OnDelete(DeleteBehavior.Cascade);
//        }

//    }
//}
