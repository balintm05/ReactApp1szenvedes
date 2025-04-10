﻿using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Entities;
using ReactApp1.Server.Entities.Foglalas;
using ReactApp1.Server.Entities.Terem;
using ReactApp1.Server.Entities.Vetites;

namespace ReactApp1.Server.Data
{
    public class DataBaseContext(DbContextOptions<DataBaseContext> options) : DbContext(options)
    {
        public DbSet<FoglalasAdatok> FoglalasAdatok { get; set; }
        public DbSet<FoglaltSzekek> FoglaltSzekek { get; set; }
        public DbSet<Szekek> Szekek { get; set; }
        public DbSet<Terem> Terem { get; set; }
        public DbSet<Vetites> Vetites { get; set; }
        public DbSet<VetitesSzekek> VetitesSzekek { get; set; }
        public DbSet<Film> Film { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<HttpLog> HttpLogs { get; set; }
        public DbSet<Images> Images { get; set; }
        public DbSet<Email2FACodes> Email2FACodes { get; set; }
        public DbSet<JegyTipus> JegyTipus { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Szekek>(entity =>
            {
                entity.HasKey(s => new { s.Teremid, s.X, s.Y });
            });

            modelBuilder.Entity<VetitesSzekek>(entity =>
            {
                entity.HasKey(vs => new { vs.Vetitesid, vs.X, vs.Y });
                entity.HasOne(vs => vs.Vetites)
                      .WithMany(v => v.VetitesSzekek)
                      .HasForeignKey(vs => vs.Vetitesid)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.HasOne(vs => vs.FoglaltSzekek)
                      .WithOne(fs => fs.VetitesSzekek)
                      .HasForeignKey<FoglaltSzekek>(fs => new { fs.Vetitesid, fs.X, fs.Y });
            });

            modelBuilder.Entity<FoglaltSzekek>(entity =>
            {
                entity.HasKey(fs => new { fs.FoglalasAdatokid, fs.Vetitesid, fs.X, fs.Y });
                entity.HasOne(fs => fs.FoglalasAdatok)
                      .WithMany(fa => fa.FoglaltSzekek)
                      .HasForeignKey(fs => fs.FoglalasAdatokid)
                      .OnDelete(DeleteBehavior.Cascade); 
                entity.HasOne(fs => fs.VetitesSzekek)
                      .WithOne(vs => vs.FoglaltSzekek)
                      .HasForeignKey<FoglaltSzekek>(fs => new { fs.Vetitesid, fs.X, fs.Y });

                entity.HasOne(fs => fs.JegyTipus)
                      .WithMany()
                      .HasForeignKey(fs => fs.JegyTipusId)
                      .IsRequired();
            });

            modelBuilder.Entity<Vetites>(entity =>
            {
                entity.HasOne(v => v.Film)
                      .WithMany(f => f.Vetitesek)
                      .HasForeignKey(v => v.Filmid)
                      .OnDelete(DeleteBehavior.Cascade); 
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.email).IsUnique();
                entity.HasIndex(u => u.refreshToken).IsUnique();
            });

            modelBuilder.Entity<HttpLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LogTime).HasDefaultValueSql("UTC_TIMESTAMP");
            });

            modelBuilder.Entity<JegyTipus>(entity =>
            {
                entity.HasKey(jt => jt.id);
                entity.Property(jt => jt.Nev).IsRequired();
                entity.Property(jt => jt.Ar).IsRequired();
                entity.HasData(
                    new JegyTipus { id = 1, Nev = "Normál", Ar = 2000 },
                    new JegyTipus { id = 2, Nev = "Diák", Ar = 1500 },
                    new JegyTipus { id = 3, Nev = "Nyugdíjas", Ar = 1500 }
                );
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
