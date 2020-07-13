﻿using EDennis.AspNet.Base;
using EDennis.AspNet.Base.EntityFramework;
using Microsoft.EntityFrameworkCore;


namespace EDennis.Samples.ColorApi {
    public class ColorDbContextDesignTimeFactory : MigrationsExtensionsDbContextDesignTimeFactory<ColorContext>{ }


    public class ColorContext : DbContext  {

        public ColorContext(DbContextOptions<ColorContext> options) : base(options) { }

        public virtual DbSet<Rgb> Rgb { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder
                .ConfigureTemporalEntity<Rgb>(p => p.Id, true, true);
        }

    }
}
