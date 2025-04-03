using Gamesbakery.Core.Entities;
using Microsoft.EntityFrameworkCore;

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
        // VIEWS
        public DbSet<User> UserProfiles { get; set; }
        public DbSet<Order> UserOrders { get; set; }
        public DbSet<OrderItem> UserOrderItems { get; set; }
        public DbSet<Review> UserReviews { get; set; }
        public DbSet<OrderItem> SellerOrderItems { get; set; }
        public DbSet<Seller> SellerProfiles { get; set; }


        public GamesbakeryDbContext(DbContextOptions<GamesbakeryDbContext> options)
            : base(options)
        {
            //this.ChangeTracker.LazyLoadingEnabled = false;
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
                entity.Property(u => u.Country).HasColumnName("Country").HasMaxLength(50).IsRequired();
                entity.Property(u => u.Password).HasColumnName("Password").HasMaxLength(100).IsRequired();
                entity.Property(u => u.IsBlocked).HasColumnName("IsBlocked").IsRequired();
                entity.Property(u => u.Balance).HasColumnName("Balance").HasColumnType("decimal(10,2)").IsRequired();
            });

            // UserProfile (представление)
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToView("UserProfile");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).HasColumnName("UserID");
                entity.Property(u => u.Username).HasColumnName("Name");
                entity.Property(u => u.Email).HasColumnName("Email");
                entity.Property(u => u.RegistrationDate).HasColumnName("RegistrationDate");
                entity.Property(u => u.Country).HasColumnName("Country");
                entity.Property(u => u.Password).HasColumnName("Password");
                entity.Property(u => u.IsBlocked).HasColumnName("IsBlocked");
                entity.Property(u => u.Balance).HasColumnName("Balance");
            });
            // UserOrders
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToView("UserOrders");
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).HasColumnName("OrderID");
                entity.Property(o => o.UserId).HasColumnName("UserID");
                entity.Property(o => o.OrderDate).HasColumnName("OrderDate");
                entity.Property(o => o.Price).HasColumnName("TotalPrice");
                entity.Property(o => o.IsCompleted).HasColumnName("IsCompleted");
                entity.Property(o => o.IsOverdue).HasColumnName("IsOverdue");
            });
            // UserOrderItems (представление)
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToView("UserOrderItems");
                entity.HasKey(oi => oi.Id);
                entity.Property(oi => oi.Id).HasColumnName("OrderItemID");
                entity.Property(oi => oi.OrderId).HasColumnName("OrderID");
                entity.Property(oi => oi.GameId).HasColumnName("GameID");
                entity.Property(oi => oi.SellerId).HasColumnName("SellerID");
                entity.Property(oi => oi.Key).HasColumnName("KeyText");
            });
            // UserReviews (представление)
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToView("UserReviews");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).HasColumnName("ReviewID");
                entity.Property(r => r.UserId).HasColumnName("UserID");
                entity.Property(r => r.GameId).HasColumnName("GameID");
                entity.Property(r => r.Text).HasColumnName("Comment");
                entity.Property(r => r.Rating).HasColumnName("StarRating");
                entity.Property(r => r.CreationDate).HasColumnName("CreationDate");
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
            });

            // SellerProfile (представление)
            modelBuilder.Entity<Seller>(entity =>
            {
                entity.ToView("SellerProfile");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).HasColumnName("SellerID");
                entity.Property(s => s.SellerName).HasColumnName("Name");
                entity.Property(s => s.RegistrationDate).HasColumnName("RegistrationDate");
                entity.Property(s => s.AvgRating).HasColumnName("AverageRating");
                entity.Property(s => s.Password).HasColumnName("Password");
            });

            // SellerOrderItems (представление)
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToView("SellerOrderItems");
                entity.HasKey(oi => oi.Id);
                entity.Property(oi => oi.Id).HasColumnName("OrderItemID");
                entity.Property(oi => oi.OrderId).HasColumnName("OrderID");
                entity.Property(oi => oi.GameId).HasColumnName("GameID");
                entity.Property(oi => oi.SellerId).HasColumnName("SellerID");
                entity.Property(oi => oi.Key).HasColumnName("KeyText");
            });

            // Остальные маппинги
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("CategoryID").ValueGeneratedOnAdd();
                entity.Property(c => c.GenreName).HasColumnName("Name").HasMaxLength(50).IsRequired();
                entity.Property(c => c.Description).HasColumnName("Description").IsRequired();
            });

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
                entity.HasOne<Category>().WithMany().HasForeignKey(g => g.CategoryId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).HasColumnName("OrderID").ValueGeneratedOnAdd();
                entity.Property(o => o.UserId).HasColumnName("UserID").IsRequired();
                entity.Property(o => o.OrderDate).HasColumnName("OrderDate").IsRequired();
                entity.Property(o => o.Price).HasColumnName("TotalPrice").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(o => o.IsCompleted).HasColumnName("IsCompleted").IsRequired();
                entity.Property(o => o.IsOverdue).HasColumnName("IsOverdue").IsRequired();
                entity.HasOne<User>().WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(oi => oi.Id);
                entity.Property(oi => oi.Id).HasColumnName("OrderItemID").ValueGeneratedOnAdd();
                entity.Property(oi => oi.OrderId).HasColumnName("OrderID").IsRequired();
                entity.Property(oi => oi.GameId).HasColumnName("GameID").IsRequired();
                entity.Property(oi => oi.SellerId).HasColumnName("SellerID").IsRequired();
                entity.Property(oi => oi.Key).HasColumnName("KeyText").HasMaxLength(50);
                entity.HasOne<Order>().WithMany().HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Game>().WithMany().HasForeignKey(oi => oi.GameId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Seller>().WithMany().HasForeignKey(oi => oi.SellerId).OnDelete(DeleteBehavior.Restrict);
            });

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
                entity.HasOne<User>().WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Game>().WithMany().HasForeignKey(r => r.GameId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}