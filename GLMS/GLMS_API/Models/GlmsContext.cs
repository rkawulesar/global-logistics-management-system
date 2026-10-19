using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GLMS_API.Models;

public partial class GlmsContext : DbContext
{
    public GlmsContext()
    {
    }

    public GlmsContext(DbContextOptions<GlmsContext> options)
        : base(options)
    {
    }


    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clients__3214EC073723F475");

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Surname).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Region).HasMaxLength(100);
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Contract__3214EC0720223E12");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.FilePath).HasMaxLength(255);
            entity.Property(e => e.ServiceLevel).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Client).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contracts__Clien__48CFD27E");
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceR__3214EC076715960C");

            entity.Property(e => e.Cost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Contract).WithMany(p => p.ServiceRequests)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceRe__Contr__4BAC3F29");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Username)
                .HasMaxLength(100);

            entity.Property(e => e.Password)
                .HasMaxLength(100);

            entity.Property(e => e.Role)
                .HasMaxLength(50);

            entity.HasOne(d => d.Client)
                .WithMany()
                .HasForeignKey(d => d.ClientId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
