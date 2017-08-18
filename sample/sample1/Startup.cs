using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Xakep.Migrate;
using sample1.Models;
using Microsoft.EntityFrameworkCore;

using DapperExtensions;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace sample1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMigrateTable<TestData>(p => p.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDapperDataBase(ESqlDialect.SqlServer, () => new SqlConnection(Configuration.GetConnectionString("DefaultConnection")), true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.Map("/dapperext", ap =>
            {
                ap.Run(async (context) =>
                {
                    var DataUtil = context.RequestServices.GetService<IOptions<DataBaseOptions>>();

                    using (var con = DataUtil.Value.DbConnection())
                    {
                        if (con.State == ConnectionState.Closed)
                            con.Open();
                        con.Delete<TestData>(Predicates.Field<TestData>(p => p.kid, Operator.Gt, 0));


                        var data = new List<TestData>();
                        for (int i = 0; i < 100; i++)
                        {
                            data.Add(new TestData() { Name = $"data-{i}" });
                        }

                        con.Insert<TestData>(data);

                        await context.Response.WriteAsync($"insert to {data.Count}\n");

                        var long4 = con.Count<TestData>(Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));

                        await context.Response.WriteAsync($"like 4= {long4}\n");

                        var vlist = con.GetList<TestData>(Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));
                        foreach (var item in vlist)
                        {
                            await context.Response.WriteAsync($"name= {item.Name} kid={item.kid}\n");
                        }

                        var UData = new TestData() { kid = vlist.Last().kid, Name = "Update" };

                        con.Update<TestData>(UData);

                        var vData = con.Get<TestData>(UData.kid);
                        if (vData != null)
                            await context.Response.WriteAsync($"name= {vData.Name} kid={vData.kid}\n");

                        var vPage = con.GetPages<TestData>(2, 10, Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));
                        await context.Response.WriteAsync($"CurrentPage= {vPage.CurrentPage} ItemsPerPage={vPage.ItemsPerPage} TotalItems:{vPage.TotalItems} TotalPages:{vPage.TotalPages}\n");
                        foreach (var item in vPage.Items)
                        {
                            await context.Response.WriteAsync($"name= {item.Name} kid={item.kid}\n");
                        }

                        await context.Response.WriteAsync("Hello World!");
                    }
                });
            });

            app.Map("/database", ap =>
            {
                ap.Run(async (context) =>
                {
                    var DataUtil = context.RequestServices.GetService<IDatabase>();
                    DataUtil.Delete<TestData>(Predicates.Field<TestData>(p => p.kid, Operator.Gt, 0));

                    var data = new List<TestData>();
                    for (int i = 0; i < 100; i++)
                    {
                        data.Add(new TestData() { Name = $"data-{i}" });
                    }

                    DataUtil.RunInTransaction(() => DataUtil.Insert<TestData>(data));

                    await context.Response.WriteAsync($"insert to {data.Count}\n");

                    var long4 = DataUtil.Count<TestData>(Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));

                    await context.Response.WriteAsync($"like 4= {long4}\n");

                    var vlist = DataUtil.GetList<TestData>(Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));
                    foreach (var item in vlist)
                    {
                        await context.Response.WriteAsync($"name= {item.Name} kid={item.kid}\n");
                    }

                    var UData = new TestData() { kid = vlist.Last().kid, Name = "Update" };

                    DataUtil.Update<TestData>(UData);

                    var vData = DataUtil.Get<TestData>(UData.kid);
                    if (vData != null)
                        await context.Response.WriteAsync($"name= {vData.Name} kid={vData.kid}\n");

                    var vPage = DataUtil.GetPages<TestData>(2, 10, Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));
                    await context.Response.WriteAsync($"CurrentPage= {vPage.CurrentPage} ItemsPerPage={vPage.ItemsPerPage} TotalItems:{vPage.TotalItems} TotalPages:{vPage.TotalPages}\n");
                    foreach (var item in vPage.Items)
                    {
                        await context.Response.WriteAsync($"name= {item.Name} kid={item.kid}\n");
                    }

                    await context.Response.WriteAsync("Hello World!");
                });
            });

            app.Map("/databaseasync", ap =>
            {
                ap.Run(async (context) =>
                {
                    var DataUtil = context.RequestServices.GetService<IDatabase>();
                    await DataUtil.DeleteAsync<TestData>(Predicates.Field<TestData>(p => p.kid, Operator.Gt, 0));


                    var data = new List<TestData>();
                    for (int i = 0; i < 100; i++)
                    {
                        data.Add(new TestData() { Name = $"data-{i}" });
                    }
                    DataUtil.RunInTransaction(() => DataUtil.Insert<TestData>(data));

                    await context.Response.WriteAsync($"insert to {data.Count}\n");

                    var long4 = await DataUtil.CountAsync<TestData>(Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));

                    await context.Response.WriteAsync($"like 4= {long4}\n");



                    var vlist = DataUtil.GetList<TestData>(Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));
                    foreach (var item in vlist)
                    {
                        await context.Response.WriteAsync($"name= {item.Name} kid={item.kid}\n");
                    }


                    var UData = new TestData() { kid = vlist.Last().kid, Name = "Update" };

                    await DataUtil.UpdateAsync<TestData>(UData);


                    var vData = await DataUtil.GetAsync<TestData>(UData.kid);
                    if (vData != null)
                        await context.Response.WriteAsync($"name= {vData.Name} kid={vData.kid}\n");

                    var vPage = await DataUtil.GetPagesAsync<TestData>(2, 10, Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));
                    await context.Response.WriteAsync($"CurrentPage= {vPage.CurrentPage} ItemsPerPage={vPage.ItemsPerPage} TotalItems:{vPage.TotalItems} TotalPages:{vPage.TotalPages}\n");
                    foreach (var item in vPage.Items)
                    {
                        await context.Response.WriteAsync($"name= {item.Name} kid={item.kid}\n");
                    }



                    await context.Response.WriteAsync("Hello World!");
                });
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
