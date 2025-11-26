using Gamesbakery.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Gamesbakery.DataAccess
{
    public class GamesbakeryDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Gift> Gifts { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public GamesbakeryDbContext(DbContextOptions<GamesbakeryDbContext> options)
            : base(options)
        {
        }

        public decimal GetUserTotalSpent(Guid userId)
        {
            var parameter = new SqlParameter("@UserID", userId);
            return Database
                .SqlQueryRaw<decimal>("SELECT dbo.fn_GetUserTotalSpent(@UserID) AS Value", parameter)
                .AsEnumerable()
                .FirstOrDefault();
        }

        public decimal GetGameAverageRating(Guid gameId)
        {
            var parameter = new SqlParameter("@GameID", gameId);
            return Database
                .SqlQueryRaw<decimal>("SELECT dbo.fn_GetGameAverageRating(@GameID) AS Value", parameter)
                .AsEnumerable()
                .FirstOrDefault();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).HasColumnName("UserID").ValueGeneratedOnAdd();
                entity.Property(u => u.Username).HasColumnName("Name").HasMaxLength(50).IsRequired();
                entity.Property(u => u.Email).HasColumnName("Email").HasMaxLength(100).IsRequired();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.RegistrationDate).HasColumnName("RegistrationDate").IsRequired();
                entity.Property(u => u.Country).HasColumnName("Country").HasMaxLength(300).IsRequired();
                entity.Property(u => u.Password).HasColumnName("Password").HasMaxLength(100).IsRequired();
                entity.Property(u => u.IsBlocked).HasColumnName("IsBlocked").IsRequired();
                entity.Property(u => u.Balance).HasColumnName("Balance").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(u => u.TotalSpent).HasColumnName("TotalSpent").HasColumnType("decimal(10,2)").IsRequired();
                entity.HasOne(u => u.Cart)
                      .WithOne(c => c.User)
                      .HasForeignKey<Cart>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(u => u.Orders)
                      .WithOne(o => o.User)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(u => u.Reviews)
                      .WithOne(r => r.User)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(u => u.SentGifts)
                      .WithOne(g => g.Sender)
                      .HasForeignKey(g => g.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(u => u.ReceivedGifts)
                      .WithOne(g => g.Recipient)
                      .HasForeignKey(g => g.RecipientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seller
            modelBuilder.Entity<Seller>(entity =>
            {
                entity.ToTable("Sellers");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).HasColumnName("SellerID").ValueGeneratedOnAdd();
                entity.Property(s => s.SellerName).HasColumnName("Name").HasMaxLength(100).IsRequired();
                entity.Property(s => s.RegistrationDate).HasColumnName("RegistrationDate").IsRequired();
                entity.Property(s => s.AvgRating).HasColumnName("AverageRating").HasColumnType("decimal(3,2)");
                entity.Property(s => s.Password).HasColumnName("Password").HasMaxLength(100).IsRequired();
                entity.HasMany(s => s.OrderItems)
                      .WithOne(oi => oi.Seller)
                      .HasForeignKey(oi => oi.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("CategoryID").ValueGeneratedOnAdd();
                entity.Property(c => c.GenreName).HasColumnName("Name").HasMaxLength(50).IsRequired();
                entity.Property(c => c.Description).HasColumnName("Description").IsRequired();
                entity.HasMany(c => c.Games)
                      .WithOne(g => g.Category)
                      .HasForeignKey(g => g.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Game
            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Games");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Id).HasColumnName("GameID").ValueGeneratedOnAdd();
                entity.Property(g => g.CategoryId).HasColumnName("CategoryID").IsRequired();
                entity.Property(g => g.Title).HasColumnName("Title").HasMaxLength(100).IsRequired();
                entity.Property(g => g.Price).HasColumnName("Price").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(g => g.ReleaseDate).HasColumnName("ReleaseDate").IsRequired();
                entity.Property(g => g.Description).HasColumnName("Description").IsRequired();
                entity.Property(g => g.OriginalPublisher).HasColumnName("OriginalPublisher").HasMaxLength(100).IsRequired();
                entity.Property(g => g.IsForSale).HasColumnName("IsForSale").IsRequired();
                entity.HasMany(g => g.OrderItems)
                      .WithOne(oi => oi.Game)
                      .HasForeignKey(oi => oi.GameId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(g => g.Reviews)
                      .WithOne(r => r.Game)
                      .HasForeignKey(r => r.GameId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).HasColumnName("OrderID").ValueGeneratedOnAdd();
                entity.Property(o => o.UserId).HasColumnName("UserID").IsRequired();
                entity.Property(o => o.OrderDate).HasColumnName("OrderDate").IsRequired();
                entity.Property(o => o.TotalAmount).HasColumnName("TotalAmount").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(o => o.Status).HasColumnName("Status").HasMaxLength(50).IsRequired();
                entity.Property(o => o.IsCompleted).HasColumnName("IsCompleted").IsRequired();
                entity.Property(o => o.IsOverdue).HasColumnName("IsOverdue").IsRequired();
            });

            // OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(oi => oi.Id);
                entity.Property(oi => oi.Id).HasColumnName("OrderItemID").ValueGeneratedOnAdd();
                entity.Property(oi => oi.OrderId).HasColumnName("OrderID");
                entity.Property(oi => oi.GameId).HasColumnName("GameID").IsRequired();
                entity.Property(oi => oi.SellerId).HasColumnName("SellerID").IsRequired();
                entity.Property(oi => oi.Key).HasColumnName("KeyText").HasMaxLength(50);
                entity.Property(oi => oi.IsGifted).HasColumnName("IsGifted").HasDefaultValue(false);
                entity.HasMany(oi => oi.CartItems)
                      .WithOne(ci => ci.OrderItem)
                      .HasForeignKey(ci => ci.OrderItemID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Review
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("Reviews");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).HasColumnName("ReviewID").ValueGeneratedOnAdd();
                entity.Property(r => r.UserId).HasColumnName("UserID").IsRequired();
                entity.Property(r => r.GameId).HasColumnName("GameID").IsRequired();
                entity.Property(r => r.Text).HasColumnName("Comment").IsRequired();
                entity.Property(r => r.Rating).HasColumnName("StarRating").IsRequired();
                entity.Property(r => r.CreationDate).HasColumnName("CreationDate").IsRequired();
            });

            // Gift
            modelBuilder.Entity<Gift>(entity =>
            {
                entity.ToTable("Gifts");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Id).HasColumnName("GiftID").ValueGeneratedOnAdd();
                entity.Property(g => g.SenderId).HasColumnName("SenderID").IsRequired();
                entity.Property(g => g.RecipientId).HasColumnName("RecipientID").IsRequired();
                entity.Property(g => g.OrderItemId).HasColumnName("OrderItemID").IsRequired();
                entity.Property(g => g.GiftDate).HasColumnName("GiftDate").IsRequired();
                entity.Property(g => g.Type).HasColumnName("GiftType").HasConversion<int>(); // enum as int
                entity.Property(g => g.GameTitle).HasColumnName("GameTitle").HasMaxLength(100);
                entity.Property(g => g.Key).HasColumnName("KeyText").HasMaxLength(50);
            });

            // Cart
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("Carts");
                entity.HasKey(c => c.CartId);
                entity.Property(c => c.CartId).HasColumnName("CartID").ValueGeneratedOnAdd();
                entity.Property(c => c.UserId).HasColumnName("UserID").IsRequired();
                entity.HasMany(c => c.Items)
                      .WithOne(ci => ci.Cart)
                      .HasForeignKey(ci => ci.CartID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CartItem
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CartItems");
                entity.HasKey(ci => ci.CartItemID);
                entity.Property(ci => ci.CartItemID).HasColumnName("CartItemID").ValueGeneratedOnAdd();
                entity.Property(ci => ci.CartID).HasColumnName("CartID").IsRequired();
                entity.Property(ci => ci.OrderItemID).HasColumnName("OrderItemID").IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
