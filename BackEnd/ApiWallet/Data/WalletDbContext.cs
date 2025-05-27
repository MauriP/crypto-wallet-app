using System;
using System.Collections.Generic;
using ApiWallet.Models.Entyties;
using Microsoft.EntityFrameworkCore;

namespace ApiWallet.Data;

public partial class WalletDbContext : DbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CryptoPrice> CryptoPrices { get; set; }

    public virtual DbSet<Cryptocurrency> Cryptocurrencies { get; set; }

    public virtual DbSet<Exchange> Exchanges { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VTransactionHistory> VTransactionHistories { get; set; }

    public virtual DbSet<VWalletSummary> VWalletSummaries { get; set; }

    public virtual DbSet<WalletStatus> WalletStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<CryptoPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("crypto_prices");

            entity.HasIndex(e => e.CryptoId, "crypto_id");

            entity.HasIndex(e => e.ExchangeId, "exchange_id");

            entity.HasIndex(e => e.LastUpdated, "idx_crypto_prices_last_updated");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyPrice)
                .HasPrecision(18, 2)
                .HasColumnName("buy_price");
            entity.Property(e => e.CryptoId).HasColumnName("crypto_id");
            entity.Property(e => e.ExchangeId).HasColumnName("exchange_id");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("last_updated");
            entity.Property(e => e.SellPrice)
                .HasPrecision(18, 2)
                .HasColumnName("sell_price");

            entity.HasOne(d => d.Crypto).WithMany(p => p.CryptoPrices)
                .HasForeignKey(d => d.CryptoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("crypto_prices_ibfk_1");

            entity.HasOne(d => d.Exchange).WithMany(p => p.CryptoPrices)
                .HasForeignKey(d => d.ExchangeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("crypto_prices_ibfk_2");
        });

        modelBuilder.Entity<Cryptocurrency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("cryptocurrencies");

            entity.HasIndex(e => e.Code, "code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Exchange>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("exchanges");

            entity.HasIndex(e => e.Code, "code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApiUrl)
                .HasMaxLength(255)
                .HasColumnName("api_url");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.ExchangeId, "exchange_id");

            entity.HasIndex(e => e.CryptoId, "idx_transactions_crypto");

            entity.HasIndex(e => e.Datetime, "idx_transactions_datetime");

            entity.HasIndex(e => e.UserId, "idx_transactions_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasColumnType("enum('purchase','sale')")
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CryptoAmount)
                .HasPrecision(18, 8)
                .HasColumnName("crypto_amount");
            entity.Property(e => e.CryptoId).HasColumnName("crypto_id");
            entity.Property(e => e.Datetime)
                .HasColumnType("datetime")
                .HasColumnName("datetime");
            entity.Property(e => e.ExchangeId).HasColumnName("exchange_id");
            entity.Property(e => e.Money)
                .HasPrecision(18, 2)
                .HasColumnName("money");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Crypto).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CryptoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_ibfk_2");

            entity.HasOne(d => d.Exchange).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.ExchangeId)
                .HasConstraintName("transactions_ibfk_3");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<VTransactionHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_transaction_history");

            entity.Property(e => e.Action)
                .HasColumnType("enum('purchase','sale')")
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CryptoAmount)
                .HasPrecision(18, 8)
                .HasColumnName("crypto_amount");
            entity.Property(e => e.CryptoCode)
                .HasMaxLength(10)
                .HasColumnName("crypto_code");
            entity.Property(e => e.CryptoName)
                .HasMaxLength(50)
                .HasColumnName("crypto_name");
            entity.Property(e => e.Datetime)
                .HasColumnType("datetime")
                .HasColumnName("datetime");
            entity.Property(e => e.ExchangeCode)
                .HasMaxLength(20)
                .HasColumnName("exchange_code");
            entity.Property(e => e.ExchangeName)
                .HasMaxLength(50)
                .HasColumnName("exchange_name");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Money)
                .HasPrecision(18, 2)
                .HasColumnName("money");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<VWalletSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_wallet_summary");

            entity.Property(e => e.CryptoCode)
                .HasMaxLength(10)
                .HasColumnName("crypto_code");
            entity.Property(e => e.CryptoName)
                .HasMaxLength(50)
                .HasColumnName("crypto_name");
            entity.Property(e => e.CurrentPrice)
                .HasPrecision(18, 2)
                .HasColumnName("current_price");
            entity.Property(e => e.CurrentValue)
                .HasPrecision(58, 10)
                .HasColumnName("current_value");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(40, 8)
                .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<WalletStatus>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("wallet_status");

            entity.Property(e => e.CryptoCode)
                .HasMaxLength(10)
                .HasColumnName("crypto_code");
            entity.Property(e => e.CryptoName)
                .HasMaxLength(50)
                .HasColumnName("crypto_name");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(40, 8)
                .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
