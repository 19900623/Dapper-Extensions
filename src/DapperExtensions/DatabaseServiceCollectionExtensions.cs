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

        /// <summary>
        /// 设置SqlDialect,使用默认用法
        /// </summary>
        /// <param name="services"></param>
        /// <param name="SqlDialect"></param>
        /// <example>
        /// using(var con=new SqlConnection(Configuration.GetConnectionString("DefaultConnection"))
        /// {
        ///     con.Insert<TestData>(data);
        /// }
        /// </example>
        /// <returns></returns>
        public static IServiceCollection AddDapper(this IServiceCollection services, ESqlDialect SqlDialect)
        {
            DapperExtensions.Configure(SqlDialectUtil.ConvertESqlDialect(SqlDialect));

            return services;
        }

        /// <summary>
        /// 使用IDatabase用法
        /// </summary>
        /// <param name="services"></param>
        /// <param name="SqlDialect"></param>
        /// <param name="CreateConnection"></param>
        /// <param name="UseExtension">是否同时使用扩展方法</param>
        /// <returns></returns>
        public static IServiceCollection AddDapperDataBase(this IServiceCollection services, ESqlDialect sqlDialect, Func<IDbConnection> CreateConnection, bool UseExtension = false)
        {
            var SqlDialect = SqlDialectUtil.ConvertESqlDialect(sqlDialect);
            services.AddOptions();
            services.Configure<DataBaseOptions>(opt =>
            {
                opt.DbConnection = CreateConnection;
                opt.sqlDialect = SqlDialectUtil.ConvertESqlDialect(sqlDialect);
            });

            if (UseExtension)
            {
                var Configuration = new DapperExtensionsConfiguration(SqlDialect);
                DapperExtensions.Configure(Configuration);
                services.AddSingleton<IDapperExtensionsConfiguration>(Configuration);
            }
            else
                services.AddSingleton<IDapperExtensionsConfiguration, DapperExtensionsConfiguration>();

            services.AddTransient<IDatabase, Database>();
            return services;
        }
    }
}
