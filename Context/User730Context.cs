using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using bookshopTab.Models;

namespace bookshopTab.Context;

public partial class User730Context : DbContext
{
    public User730Context()
    {
    }

    public User730Context(DbContextOptions<User730Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Manufacturer> Manufacturers { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Productphoto> Productphotos { get; set; }

    public virtual DbSet<Productsale> Productsales { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=192.168.2.159:5432;Database=user730;Username=user730;Password=27791");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(e => e.ManufacturerId).HasName("newtable_1_pk");

            entity.ToTable("Manufacturer", "bookstore");

            entity.Property(e => e.ManufacturerId)
                .ValueGeneratedNever()
                .HasColumnName("manufacturer_id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("newtable_pk");

            entity.ToTable("Product", "bookstore");

            entity.Property(e => e.ProductId)
                .ValueGeneratedNever()
                .HasColumnName("product_id");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.Description)
                .HasColumnType("character varying")
                .HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.MainImagePath)
                .HasColumnType("character varying")
                .HasColumnName("main_image_path");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturer_id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.Products)
                .HasForeignKey(d => d.ManufacturerId)
                .HasConstraintName("product_manufacturer_fk");

            entity.HasMany(d => d.AttachedProducts).WithMany(p => p.MainProducts)
                .UsingEntity<Dictionary<string, object>>(
                    "Attachedproduct",
                    r => r.HasOne<Product>().WithMany()
                        .HasForeignKey("AttachedProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("attachedproduct_product_fk_1"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("MainProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("attachedproduct_product_fk"),
                    j =>
                    {
                        j.HasKey("MainProductId", "AttachedProductId").HasName("attachedproduct_pk");
                        j.ToTable("attachedproduct", "bookstore");
                        j.IndexerProperty<int>("MainProductId").HasColumnName("main_product_id");
                        j.IndexerProperty<int>("AttachedProductId").HasColumnName("attached_product_id");
                    });

            entity.HasMany(d => d.MainProducts).WithMany(p => p.AttachedProducts)
                .UsingEntity<Dictionary<string, object>>(
                    "Attachedproduct",
                    r => r.HasOne<Product>().WithMany()
                        .HasForeignKey("MainProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("attachedproduct_product_fk"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("AttachedProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("attachedproduct_product_fk_1"),
                    j =>
                    {
                        j.HasKey("MainProductId", "AttachedProductId").HasName("attachedproduct_pk");
                        j.ToTable("attachedproduct", "bookstore");
                        j.IndexerProperty<int>("MainProductId").HasColumnName("main_product_id");
                        j.IndexerProperty<int>("AttachedProductId").HasColumnName("attached_product_id");
                    });
        });

        modelBuilder.Entity<Productphoto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productphoto_pk");

            entity.ToTable("productphoto", "bookstore");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.PhotoPath)
                .HasColumnType("character varying")
                .HasColumnName("photo_path");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Product).WithMany(p => p.Productphotos)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("productphoto_product_fk");
        });

        modelBuilder.Entity<Productsale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productsales_pk");

            entity.ToTable("productsales", "bookstore");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Product).WithMany(p => p.Productsales)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("productsales_product_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
