using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VMart.Models;

namespace VMart.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> o) : base(o) { }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<Contact> Contact { get; set; }

        public DbSet<Logs> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Home & Garden", DisplayOrder = 2 },
                new Category { Id = 3, Name = "Furniture", DisplayOrder = 3 },
                new Category { Id = 4, Name = "Music & Audio", DisplayOrder = 4 }
            );

            // Seed Products with unique IDs
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Title = "Wireless Bluetooth Headphones",
                    Description = "Premium over-ear wireless headphones with active noise cancellation and 30-hour battery life.",
                    Price = 299.99m,
                    CategoryId = 1,
                    ImageUrl = "https://res.cloudinary.com/dzf9kamfw/image/upload/v1751917316/51CnDMbXZzL._SL1200__dtit2u.jpg",
                    Quantity = 50,
                    Ratings = 5,
                },
                new Product
                {
                    Id = 2,
                    Title = "Samsung 108 cm (43 inches) Crystal 4K Vista Pro Ultra HD Smart LED TV",
                    Description = "Ultra HD 4K Smart TV with HDR support, built-in streaming apps, and voice control.",
                    Price = 799.99m,
                    CategoryId = 1,
                    ImageUrl = "https://res.cloudinary.com/dzf9kamfw/image/upload/v1751917383/81_muTwo6yL._SL1500__f2q85g.jpg",
                    Quantity = 25,
                    Ratings = 5,
                },
                new Product
                {
                    Id = 3,
                    Title = "Apple iPhone 15 (128 GB) - Pink",
                    Description = "Latest flagship smartphone with triple camera system, 5G connectivity, and fast charging.",
                    Price = 899.99m,
                    CategoryId = 1,
                    ImageUrl = "https://res.cloudinary.com/dzf9kamfw/image/upload/v1751917450/71v2jVh6nIL._SL1500__xiwhp5.jpg",
                    Quantity = 40,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 4,
                    Title = "Apple 2024 MacBook Pro Laptop with M4 Pro chip",
                    Description = "Built for Apple Intelligence, 14.2″ Liquid Retina XDR Display, 24GB Unified Memory, 512GB SSD Storage in Space Black",
                    Price = 1499.99m,
                    CategoryId = 1,
                    ImageUrl = "https://res.cloudinary.com/dzf9kamfw/image/upload/v1751917584/61eA9PkZ07L._SL1500__sz893u.jpg",
                    Quantity = 20,
                    Ratings = 5,
                },
                new Product
                {
                    Id = 5,
                    Title = "Ambrane 15W Qi-2 MagSafe Wireless Charger",
                    Description = "15W Qi-2 MagSafe Wireless Charger with 60W Type C Cable Magnetic for iPhone 16/15/14/13/12 series",
                    Price = 49.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/71nVnTAnYDL._SL1500_.jpg",
                    Quantity = 100,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 6,
                    Title = "HyperX Pulsefire Haste 2 Wireless Gaming Mouse",
                    Description = "Ultra Lightweight 61g gaming mouse with 100 Hour Battery Life, 2.4Ghz Wireless, Up to 26000 DPI in White",
                    Price = 89.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/51x7IElGq+L._SL1500_.jpg",
                    Quantity = 75,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 7,
                    Title = "Kreo Swarm 75% Wireless Mechanical Keyboard",
                    Description = "Wireless Gaming Keyboard with Milky Purple Pre-lubed Switches, 5-pin Hot Swap PCB and 5 Layers Sound Absorption",
                    Price = 159.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/61JosiSIzQL._SL1080_.jpg",
                    Quantity = 60,
                    Ratings = 5,
                },
                new Product
                {
                    Id = 8,
                    Title = "Samsung T7 Shield Portable SSD 1TB",
                    Description = "USB 3.2 Gen2 External SSD, Up to 1,050MB/s, Rugged, IP65 Water & Dust Resistant for photographers and gaming",
                    Price = 199.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/714cd9DfoeL._SL1500_.jpg",
                    Quantity = 80,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 9,
                    Title = "Logitech MX Brio Ultra HD 4K Webcam",
                    Description = "4K Streaming Webcam, 1080p at 60 FPS, USB-C, Works with Microsoft Teams, Zoom, Google Meet in Graphite",
                    Price = 129.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/61WQr6U3mwL._SY450_.jpg",
                    Quantity = 45,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 10,
                    Title = "Bluetooth Speaker Waterproof",
                    Description = "Portable waterproof Bluetooth speaker with 360-degree sound and 20-hour battery life.",
                    Price = 79.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/71o6CU8MqVL._SL1500_.jpg",
                    Quantity = 90,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 11,
                    Title = "Apple Watch Series 9",
                    Description = "Apple Watch Series 9 GPS 41mm Smartwatch with RED Aluminum Case. Fitness Tracker with Blood Oxygen & ECG Apps",
                    Price = 399.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/71TMn2dnDyL._SL1500_.jpg",
                    Quantity = 35,
                    Ratings = 5,
                },
                new Product
                {
                    Id = 12,
                    Title = "Samsung Galaxy Tab S10 FE",
                    Description = "Samsung Galaxy Tab S10 FE with S Pen, 27.7 cm LCD Display, 8 GB RAM, 128 GB Storage, Wi-Fi Tablet in Gray",
                    Price = 649.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/61XbdrL5XgL._SY450_.jpg",
                    Quantity = 30,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 13,
                    Title = "PEATOP 7 in 1 USB Hub 3.0",
                    Description = "USB-C Hub with 4K HDMI, TF/SD Card Reader, Compatible with Type-C devices, Laptop, iPad, iPhone, MacBook",
                    Price = 69.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/61664MZGUcL._SL1500_.jpg",
                    Quantity = 85,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 14,
                    Title = "Ambrane 20000mAh Power Bank",
                    Description = "33W Fast Charging Power Bank with Super Fast PD 3.0, Type-C Input & Output, Triple Output Ports in Golden",
                    Price = 59.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/61Fj6lur8UL._SL1500_.jpg",
                    Quantity = 100,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 15,
                    Title = "AONEPTER GPS Drone with 4K Camera",
                    Description = "RC Quadcopter with Auto Return, Follow Me, Gesture Control, Waypoints, Headless Mode, Compatible with VR Glasses",
                    Price = 899.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/61uYF+asvfL._SL1350_.jpg",
                    Quantity = 20,
                    Ratings = 5,
                },
                new Product
                {
                    Id = 16,
                    Title = "GTPLAYER Gaming Chair RGB",
                    Description = "Multi-Functional Ergonomic Gaming Chair with Massage Cushion, Premium PU Leather, Adjustable Neck & Lumbar Support",
                    Price = 299.99m,
                    CategoryId = 3,
                    ImageUrl = "https://m.media-amazon.com/images/I/51nlgm4v3wL._SL1080_.jpg",
                    Quantity = 25,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 17,
                    Title = "LG 43\" Ultrafine Smart Monitor",
                    Description = "UHD 4K IPS Display, HDR 10, Wireless Connectivity, webOS, AirPlay 2, USB Type-C with 65W PD in White",
                    Price = 549.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/610Cgk4Wl3L._SL1500_.jpg",
                    Quantity = 40,
                    Ratings = 5,
                },
                new Product
                {
                    Id = 18,
                    Title = "ASUS TUF Gaming GeForce RTX 4080",
                    Description = "16GB GDDR6X OC Edition Graphics Card for gaming and content creation in Black",
                    Price = 1199.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/71dfNA+N52L._SL1500_.jpg",
                    Quantity = 15,
                    Ratings = 5,
                },
                new Product
                {
                    Id = 19,
                    Title = "Meta Quest 3 VR Headset",
                    Description = "512GB Breakthrough Mixed Reality headset with Powerful Performance, includes Asgard's Wrath 2 bundle",
                    Price = 699.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/61nkctF66PL._SL1500_.jpg",
                    Quantity = 30,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 20,
                    Title = "MIOOX Electric Scooter",
                    Description = "Sporty Design Electric Scooter in Black and Grey with LED Headlight, 48V 30AH Battery, 60-70km Range",
                    Price = 799.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/61R+BP9ROKL._SL1080_.jpg",
                    Quantity = 25,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 21,
                    Title = "Dekorly Artificial Potted Plants",
                    Description = "8 Pack Artificial Plastic Eucalyptus Plants, Small Indoor Potted Houseplants for Home Decor, Bathroom, Office, Farmhouse",
                    Price = 39.99m,
                    CategoryId = 2,
                    ImageUrl = "https://m.media-amazon.com/images/I/8139T8YbdkL._SL1500_.jpg",
                    Quantity = 120,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 22,
                    Title = "SAF Flower Pot Wall Painting",
                    Description = "Set of 3 3D Modern Art Painting for Living Room, Large Size with Frames for Home Decoration, Hotel, Office",
                    Price = 89.99m,
                    CategoryId = 2,
                    ImageUrl = "https://m.media-amazon.com/images/I/71YOoiF35BL._SL1500_.jpg",
                    Quantity = 65,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 23,
                    Title = "Artvibes Buddha Wooden Wall Hanging",
                    Description = "Meditating Gautam Buddha Wooden Wall Hanging for Home Decor, Decoration Items for Living Room, Office Art",
                    Price = 129.99m,
                    CategoryId = 2,
                    ImageUrl = "https://m.media-amazon.com/images/I/71WkX7VmVpL._SL1500_.jpg",
                    Quantity = 45,
                    Ratings = 5,
                },
                new Product
                {
                    Id = 24,
                    Title = "Ryme Mini Fake Artificial Plant",
                    Description = "Small Mini Fake Wild Artificial Plant with Pot/Vase for Home Office Decor, Dimensions: 5 cm X 13 cm",
                    Price = 19.99m,
                    CategoryId = 2,
                    ImageUrl = "https://m.media-amazon.com/images/I/71d9f-yjdRL._SL1200_.jpg",
                    Quantity = 150,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 25,
                    Title = "Rellon Industries Study Table",
                    Description = "Foldable Laptop Table for Students, Portable & Lightweight Mini Table, Bed Reading Table, Laptop Desk",
                    Price = 79.99m,
                    CategoryId = 3,
                    ImageUrl = "https://m.media-amazon.com/images/I/61okssN3Y6L._SL1080_.jpg",
                    Quantity = 85,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 26,
                    Title = "CELLBELL Desire C104 Mesh Chair",
                    Description = "Ergonomic Office Study Chair, Revolving Computer Chair for Work from Home, Heavy Duty Metal Base in Black",
                    Price = 199.99m,
                    CategoryId = 3,
                    ImageUrl = "https://m.media-amazon.com/images/I/51A5WZTAYyL._SL1000_.jpg",
                    Quantity = 55,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 27,
                    Title = "Nikon D850 Digital SLR Camera",
                    Description = "45.7MP Digital SLR Camera in Black with AF-S Nikkor 24-120mm F/4G ED VR Lens and 64GB Memory Card",
                    Price = 2499.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/81WtQ64-SOL._SL1500_.jpg",
                    Quantity = 15,
                    Ratings = 5,
                },
                new Product
                {
                    Id = 28,
                    Title = "Yamaha F280 Acoustic Guitar",
                    Description = "Acoustic Rosewood Guitar in Natural finish, perfect for beginners and intermediate players",
                    Price = 349.99m,
                    CategoryId = 4,
                    ImageUrl = "https://m.media-amazon.com/images/I/61b6DTHgNWL._SL1500_.jpg",
                    Quantity = 40,
                    Ratings = 5,
                },
                new Product
                {
                    Id = 29,
                    Title = "MAONO PD200X USB/XLR Dynamic Mic",
                    Description = "Professional Microphone for Singing, PC, YouTube Recording with Programmable Software and 0-Latency monitoring",
                    Price = 189.99m,
                    CategoryId = 4,
                    ImageUrl = "https://m.media-amazon.com/images/I/616cSpmg0hL._SL1500_.jpg",
                    Quantity = 70,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 30,
                    Title = "Alesis Nitro Max Electronic Drum Kit",
                    Description = "Eight Piece Electronic Drum Kit with Mesh Heads and Bluetooth connectivity for modern drumming",
                    Price = 899.99m,
                    CategoryId = 4,
                    ImageUrl = "https://m.media-amazon.com/images/I/814UlK7arzL._SX569_.jpg",
                    Quantity = 25,
                    Ratings = 5,
                },
                new Product
                {
                    Id = 31,
                    Title = "boAt Rockerz 255 Pro+ Neckband",
                    Description = "60HRS Battery, Fast Charge, IPX7, Dual Pairing, Low Latency, Magnetic Earbuds Bluetooth Neckband in Active Black",
                    Price = 79.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/61+SW9nDTEL._SL1500_.jpg",
                    Quantity = 100,
                    Ratings = 4,
                },
                new Product
                {
                    Id = 32,
                    Title = "boAt Lunar Discovery Smart Watch",
                    Description = "1.39\" HD Display, Turn-by-Turn Navigation, DIY Watch Face Studio, Bluetooth Calling, Emergency SOS in Active Black",
                    Price = 149.99m,
                    CategoryId = 1,
                    ImageUrl = "https://m.media-amazon.com/images/I/71Iit7U1S+L._SL1500_.jpg",
                    Quantity = 80,
                    Ratings = 4,
                }
            );
        }
    }
}