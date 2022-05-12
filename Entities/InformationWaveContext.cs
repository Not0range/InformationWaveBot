using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InformationWaves.Entities
{
    internal class InformationWaveContext : DbContext
    {
        /// <summary>
        /// Строка соединения по умолчанию
        /// </summary>
        const string DEFAULT_CONNECTION_STRING = "Data Source=database.db;";
        /// <summary>
        /// Ядро СУБД по умолчанию
        /// </summary>
        const string DEFAULT_DATABASE_CORE = "SQLITE";

        /// <summary>
        /// Строка подлючения
        /// </summary>
        string connectionString;
        /// <summary>
        /// Ядро СУБД
        /// </summary>
        string databaseCore;

        /// <summary>
        /// Таблица Socials
        /// </summary>
        public DbSet<Social> Socials { get; set; }

        public InformationWaveContext() 
        {
            connectionString = Environment.GetEnvironmentVariable("connection_string");
            if (string.IsNullOrEmpty(connectionString))
                connectionString = DEFAULT_CONNECTION_STRING;
            databaseCore = Environment.GetEnvironmentVariable("database_core");
            if (string.IsNullOrEmpty(databaseCore))
                databaseCore = DEFAULT_DATABASE_CORE;
        }

        /// <summary>
        /// Метод настройки соединения с БД
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            switch (databaseCore.ToUpper())
            {
                case "SQLITE":
                    options.UseSqlite(connectionString);
                    break;
                case "POSTGRESQL":
                    options.UseNpgsql(connectionString);
                    break;
                case "MYSQL":
                    options.UseMySql(ServerVersion.AutoDetect(connectionString));
                    break;
                case "MSSQL":
                    options.UseSqlServer(connectionString);
                    break;
                default:
                    throw new ArgumentNullException(null, "В переменных окружения database_core неверно указано используемое ядро СУБД. " +
                        "Возможные варианты: SQLITE, POSTGRESQL, MYSQL, MSSQL");
            }
        }
    }
}
