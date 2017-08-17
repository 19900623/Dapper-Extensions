using DapperExtensions.Sql;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DapperExtensions
{
    public static class DatabaseServiceCollectionExtensions
    {
        public static IServiceCollection AddDapperDataBase(this IServiceCollection services, ISqlDialect sqlDialect,Func<IDbConnection> CreateConnection)
        {
            services.AddOptions();
            services.Configure<DataBaseOptions>(opt =>
            {
                opt.DbConnection = CreateConnection;
                opt.sqlDialect = sqlDialect;
            });

            services.AddSingleton<IDapperExtensionsConfiguration, DapperExtensionsConfiguration>();
            services.AddTransient<IDatabase, Database>();

            return services;
        }
    }
}
