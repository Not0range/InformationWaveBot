using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace InformationWaves.Entities
{
    internal class InformationWaveContext : DbContext
    {
        const string CONNECTION_STRING = "User ID=postgres;Password=12345;Host=localhost;Port=5432;Database=database;";
        public DbSet<Social> Socials { get; set; }

        public InformationWaveContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseNpgsql(CONNECTION_STRING);
    }
}
