using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Prakt15.Models;

public partial class YourDbContext : DbContext
{
    public YourDbContext()
    {
    }

    public YourDbContext(DbContextOptions<YourDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<CategoriesBackup> CategoriesBackups { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductTag> ProductTags { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=ECHIPSCOMPUTER\\SQLEXPRESS;Database=ElectronicsShop;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("brands$");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CategoriesBackup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("categories_backup");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories$");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products$");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Stock).HasColumnName("stock");

            entity.HasOne(d => d.Brand)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK_products$_brands$");

            entity.HasOne(d => d.Category)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_products$_categories$");
        });

        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.ToTable("product_tags$");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.HasKey(e => new { e.ProductId, e.TagId });

            entity.HasOne(d => d.Product)
                .WithMany(p => p.ProductTags)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_product_tags$_products$");

            entity.HasOne(d => d.Tag)
                .WithMany(t => t.ProductTags)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_product_tags$_tags$");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags$");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

