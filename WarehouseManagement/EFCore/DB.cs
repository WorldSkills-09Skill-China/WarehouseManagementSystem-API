using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WarehouseManagement.EFCore;

public partial class DB : DbContext
{
    public DB()
    {
    }

    public DB(DbContextOptions<DB> options)
        : base(options)
    {
    }

    public virtual DbSet<AssetHistory> AssetHistories { get; set; }

    public virtual DbSet<Exception> Exceptions { get; set; }

    public virtual DbSet<ExceptionState> ExceptionStates { get; set; }

    public virtual DbSet<ExceptionType> ExceptionTypes { get; set; }

    public virtual DbSet<FixedAsset> FixedAssets { get; set; }

    public virtual DbSet<FixedAssetDetail> FixedAssetDetails { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemType> ItemTypes { get; set; }

    public virtual DbSet<Maintain> Maintains { get; set; }

    public virtual DbSet<PlaceForStorage> PlaceForStorages { get; set; }

    public virtual DbSet<PlaceForStorageDetail> PlaceForStorageDetails { get; set; }

    public virtual DbSet<RecordType> RecordTypes { get; set; }

    public virtual DbSet<ReocordState> ReocordStates { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TargetType> TargetTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WarehouseRecord> WarehouseRecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=BaseWarehouseManagement;User=sa;Password=sa2024;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssetHistory>(entity =>
        {
            entity.ToTable("AssetHistory");

            entity.Property(e => e.Note).HasMaxLength(50);
            entity.Property(e => e.OperationTime).HasColumnType("datetime");

            entity.HasOne(d => d.FixedAsset).WithMany(p => p.AssetHistories)
                .HasForeignKey(d => d.FixedAssetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AssetHistory_FixedAsset");

            entity.HasOne(d => d.PlaceForStorageDetail).WithMany(p => p.AssetHistories)
                .HasForeignKey(d => d.PlaceForStorageDetailId)
                .HasConstraintName("FK_AssetHistory_PlaceForStorageDetail");

            entity.HasOne(d => d.User).WithMany(p => p.AssetHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AssetHistory_User");
        });

        modelBuilder.Entity<Exception>(entity =>
        {
            entity.ToTable("Exception");

            entity.Property(e => e.CreateTime).HasColumnType("datetime");

            entity.HasOne(d => d.Batch).WithMany(p => p.Exceptions)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("FK_Exception_Exception");

            entity.HasOne(d => d.ExceptionState).WithMany(p => p.Exceptions)
                .HasForeignKey(d => d.ExceptionStateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exception_ExceptionState");

            entity.HasOne(d => d.ExceptionType).WithMany(p => p.Exceptions)
                .HasForeignKey(d => d.ExceptionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exception_ExceptionType");

            entity.HasOne(d => d.FixedAsset).WithMany(p => p.Exceptions)
                .HasForeignKey(d => d.FixedAssetId)
                .HasConstraintName("FK_Exception_FixedAsset");
        });

        modelBuilder.Entity<ExceptionState>(entity =>
        {
            entity.ToTable("ExceptionState");

            entity.Property(e => e.State).HasMaxLength(50);
        });

        modelBuilder.Entity<ExceptionType>(entity =>
        {
            entity.ToTable("ExceptionType");

            entity.Property(e => e.Problem).HasMaxLength(50);
        });

        modelBuilder.Entity<FixedAsset>(entity =>
        {
            entity.ToTable("FixedAsset");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.FixedAssetDetail).WithMany(p => p.FixedAssets)
                .HasForeignKey(d => d.FixedAssetDetailId)
                .HasConstraintName("FK_FixedAsset_FixedAssetDetail");

            entity.HasOne(d => d.Item).WithMany(p => p.FixedAssets)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FixedAsset_Item");

            entity.HasOne(d => d.WarehouseRcord).WithMany(p => p.FixedAssets)
                .HasForeignKey(d => d.WarehouseRcordId)
                .HasConstraintName("FK_FixedAsset_WarehouseRecords");
        });

        modelBuilder.Entity<FixedAssetDetail>(entity =>
        {
            entity.ToTable("FixedAssetDetail");

            entity.Property(e => e.Specification).HasMaxLength(50);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Item");

            entity.Property(e => e.Image).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);

            entity.HasOne(d => d.ItemType).WithMany(p => p.Items)
                .HasForeignKey(d => d.ItemTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Item_ItemType");
        });

        modelBuilder.Entity<ItemType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ItemTypeSon");

            entity.ToTable("ItemType");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Maintain>(entity =>
        {
            entity.ToTable("Maintain");

            entity.Property(e => e.Note).HasMaxLength(50);
        });

        modelBuilder.Entity<PlaceForStorage>(entity =>
        {
            entity.ToTable("PlaceForStorage");

            entity.Property(e => e.Image).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<PlaceForStorageDetail>(entity =>
        {
            entity.ToTable("PlaceForStorageDetail");

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.PlaceForStorage).WithMany(p => p.PlaceForStorageDetails)
                .HasForeignKey(d => d.PlaceForStorageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlaceForStorageDetail_PlaceForStorage");
        });

        modelBuilder.Entity<RecordType>(entity =>
        {
            entity.ToTable("RecordType");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<ReocordState>(entity =>
        {
            entity.ToTable("ReocordState");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<TargetType>(entity =>
        {
            entity.ToTable("TargetType");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        });

        modelBuilder.Entity<WarehouseRecord>(entity =>
        {
            entity.Property(e => e.CreateTime).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.FinishedTime).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(50);

            entity.HasOne(d => d.Item).WithMany(p => p.WarehouseRecords)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WarehouseRecords_Item");

            entity.HasOne(d => d.PlaceForStorageDetail).WithMany(p => p.WarehouseRecords)
                .HasForeignKey(d => d.PlaceForStorageDetailId)
                .HasConstraintName("FK_WarehouseRecords_WarehouseRecords1");

            entity.HasOne(d => d.RecordState).WithMany(p => p.WarehouseRecords)
                .HasForeignKey(d => d.RecordStateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WarehouseRecords_ReocordState");

            entity.HasOne(d => d.RecordType).WithMany(p => p.WarehouseRecords)
                .HasForeignKey(d => d.RecordTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WarehouseRecords_RecordType");

            entity.HasOne(d => d.User).WithMany(p => p.WarehouseRecords)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_WarehouseRecords_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
