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

        public GamesbakeryDbContext(DbContextOptions<GamesbakeryDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // -------------------------
            // User -> [Users]
            // -------------------------
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.Id)
                      .HasColumnName("UserID")
                      .ValueGeneratedOnAdd();

                entity.Property(u => u.Username)
                      .HasColumnName("Name")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(u => u.Email)
                      .HasColumnName("Email")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(u => u.RegistrationDate)
                      .HasColumnName("RegistrationDate")
                      .HasColumnType("date")
                      .IsRequired();

                entity.Property(u => u.Country)
                      .HasColumnName("Country")
                      .HasMaxLength(50);

                entity.Property(u => u.Password)
                      .HasColumnName("Password")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(u => u.IsBlocked)
                      .HasColumnName("IsBlocked")
                      .IsRequired();

                entity.Property(u => u.Balance)
                      .HasColumnName("Balance")
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                entity.HasIndex(u => u.Email)
                      .IsUnique();
            });

            // -------------------------
            // Seller -> [Sellers]
            // -------------------------
            modelBuilder.Entity<Seller>(entity =>
            {
                entity.ToTable("Sellers");

                entity.HasKey(s => s.Id);

                entity.Property(s => s.Id)
                      .HasColumnName("SellerID")
                      .ValueGeneratedOnAdd();

                entity.Property(s => s.SellerName)
                      .HasColumnName("Name")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(s => s.RegistrationDate)
                      .HasColumnName("RegistrationDate")
                      .IsRequired();

                // В БД: DECIMAL(3,2).
                entity.Property(s => s.AvgRating)
                      .HasColumnName("AverageRating")
                      .HasColumnType("decimal(3,2)")
                      .IsRequired();
            });

            // -------------------------
            // Category -> [Categories]
            // -------------------------
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");

                entity.HasKey(c => c.Id);

                entity.Property(c => c.Id)
                      .HasColumnName("CategoryID")
                      .ValueGeneratedOnAdd();

                entity.Property(c => c.GenreName)
                      .HasColumnName("Name")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(c => c.Description)
                      .HasColumnName("Description")
                      .HasMaxLength(255);
            });

            // -------------------------
            // Game -> [Games]
            // -------------------------
            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Games");

                entity.HasKey(g => g.Id);

                entity.Property(g => g.Id)
                      .HasColumnName("GameID")
                      .ValueGeneratedOnAdd();

                entity.Property(g => g.CategoryId)
                      .HasColumnName("CategoryID")
                      .IsRequired();

                entity.Property(g => g.Title)
                      .HasColumnName("Title")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(g => g.Price)
                      .HasColumnName("Price")
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                entity.Property(g => g.ReleaseDate)
                      .HasColumnName("ReleaseDate")
                      .IsRequired();

                entity.Property(g => g.Description)
                      .HasColumnName("Description")
                      .IsRequired();

                entity.Property(g => g.OriginalPublisher)
                      .HasColumnName("OriginalPublisher")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(g => g.IsForSale)
                      .HasColumnName("IsForSale")
                      .IsRequired();

                // Связь: Game -> Category
                entity.HasOne<Category>()
                      .WithMany()
                      .HasForeignKey(g => g.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------
            // Order -> [Orders]
            // -------------------------
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");

                entity.HasKey(o => o.Id);

                entity.Property(o => o.Id)
                      .HasColumnName("OrderID")
                      .ValueGeneratedOnAdd();

                entity.Property(o => o.UserId)
                      .HasColumnName("UserID")
                      .IsRequired();

                entity.Property(o => o.OrderDate)
                      .HasColumnName("OrderDate")
                      .IsRequired();

                // Price -> TotalPrice
                entity.Property(o => o.Price)
                      .HasColumnName("TotalPrice")
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                entity.Property(o => o.IsCompleted)
                      .HasColumnName("IsCompleted")
                      .IsRequired();

                entity.Property(o => o.IsOverdue)
                      .HasColumnName("IsOverdue")
                      .IsRequired();

                // Связь: Orders -> Users
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------
            // OrderItem -> [OrderItems]
            // -------------------------
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");

                entity.HasKey(oi => oi.Id);

                entity.Property(oi => oi.Id)
                      .HasColumnName("OrderItemID")
                      .ValueGeneratedOnAdd();

                entity.Property(oi => oi.OrderId)
                      .HasColumnName("OrderID")
                      .IsRequired();

                entity.Property(oi => oi.GameId)
                      .HasColumnName("GameID")
                      .IsRequired();

                entity.Property(oi => oi.SellerId)
                      .HasColumnName("SellerID")
                      .IsRequired();

                entity.Property(oi => oi.Key)
                      .HasColumnName("KeyText")
                      .HasMaxLength(50);

                // Связи:
                entity.HasOne<Order>()
                      .WithMany()
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Game>()
                      .WithMany()
                      .HasForeignKey(oi => oi.GameId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Seller>()
                      .WithMany()
                      .HasForeignKey(oi => oi.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Уникальный индекс с фильтром
                entity.HasIndex(oi => oi.Key)
                      .IsUnique()
                      .HasFilter("[KeyText] IS NOT NULL");
            });

            // -------------------------
            // Review -> [Reviews]
            // -------------------------
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("Reviews");

                entity.HasKey(r => r.Id);

                entity.Property(r => r.Id)
                      .HasColumnName("ReviewID")
                      .ValueGeneratedOnAdd();

                entity.Property(r => r.UserId)
                      .HasColumnName("UserID")
                      .IsRequired();

                entity.Property(r => r.GameId)
                      .HasColumnName("GameID")
                      .IsRequired();

                entity.Property(r => r.Text)
                      .HasColumnName("Comment")
                      .IsRequired();

                entity.Property(r => r.Rating)
                      .HasColumnName("StarRating")
                      .IsRequired();

                entity.Property(r => r.CreationDate)
                      .HasColumnName("CreationDate")
                      .IsRequired();

                // Связи:
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Game>()
                      .WithMany()
                      .HasForeignKey(r => r.GameId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
