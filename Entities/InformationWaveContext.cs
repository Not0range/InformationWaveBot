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
        /// <summary>
        /// Строка соединения
        /// </summary>
        const string CONNECTION_STRING = "User ID=postgres;Password=12345;Host=26.228.193.143;Port=5432;Database=database;";
        /// <summary>
        /// Таблица Socials
        /// </summary>
        public DbSet<Social> Socials { get; set; }

        public InformationWaveContext() { }

        /// <summary>
        /// Метод настройки соединения с БД
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseNpgsql(CONNECTION_STRING);
    }
}
