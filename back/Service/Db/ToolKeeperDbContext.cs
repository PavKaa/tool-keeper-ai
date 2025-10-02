using Microsoft.EntityFrameworkCore;
using Service.Model;

namespace Service.Db
{
	public class ToolKeeperDbContext : DbContext
	{
        public DbSet<Tool> Tools { get; set; }

        public DbSet<ToolKit> ToolKits { get; set; }

        public DbSet<Inventory> Inventory { get; set; }
        
        public DbSet<Employee> Employees { get; set; }
        
        public DbSet<ToolUsage> ToolUsages { get; set; }

        public ToolKeeperDbContext(DbContextOptions<ToolKeeperDbContext> options) : base(options)
        {

        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //    => optionsBuilder.UseNpgsql(@"Host=host.docker.internal;Port=5432;Database=ToolKeeperAI;User Id=postgres;Password=123456");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
            modelBuilder.Entity<ToolKit>(builder =>
            {
                builder.HasKey(toolKit => toolKit.Id);
                builder.HasMany(toolKit => toolKit.Tools).WithOne(tool => tool.ToolKit).HasForeignKey(tool => tool.ToolKitId).IsRequired();
            });

            modelBuilder.Entity<Tool>(builder =>
            {
                builder.HasKey(tool => tool.Id);
                builder.HasOne(tool => tool.ToolKit).WithMany(toolKit => toolKit.Tools);
                builder.HasOne<Inventory>().WithOne(inventory => inventory.Tool).HasForeignKey<Inventory>(inventory => inventory.ToolId).IsRequired();
                builder.HasMany(tool => tool.ToolUsages).WithOne(toolUsage => toolUsage.Tool).HasForeignKey(toolUsage => toolUsage.ToolId).IsRequired();
            });

            modelBuilder.Entity<Employee>(builder =>
            {
                builder.HasKey(employee => employee.Id);
                builder.HasMany(employee => employee.ToolUsages).WithOne(toolUsage => toolUsage.Employee).HasForeignKey(toolUsage => toolUsage.EmployeeId).IsRequired();
            });

            modelBuilder.Entity<Inventory>(builder =>
            {
                builder.HasKey(inventory => inventory.Id);
                builder.HasOne(inventory => inventory.Tool).WithOne();
            });

            modelBuilder.Entity<ToolUsage>(builder =>
            {
                builder.HasKey(toolUsage => toolUsage.Id);
                builder.HasOne(toolUsage => toolUsage.Tool).WithMany(tool => tool.ToolUsages);
                builder.HasOne(toolUsage => toolUsage.Employee).WithMany(employee => employee.ToolUsages);
            });

            base.OnModelCreating(modelBuilder);
		}
	}
}
