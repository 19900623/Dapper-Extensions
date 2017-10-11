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
using Dapper;

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
            services.AddMigrateTable(p => p.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")), "TestData2", m =>
            {
                m.Entity<TestData>().HasIndex(p => p.kid);
                m.Entity<TestData2>().HasIndex(p => p.aid);
                m.Entity<TestData3>().HasIndex(p => p.Bid);
            });

            services.AddDapperDataBase(ESqlDialect.SqlServer, () => new SqlConnection(Configuration.GetConnectionString("DefaultConnection")), true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var MigrateTable = app.ApplicationServices.GetRequiredService<IMigrateTable>();
            MigrateTable.Migrate("TestData1", m =>
            {
                m.Entity<TestData>().ToTable("TestData1").HasIndex(p => p.kid);
                m.Entity<TestData2>().ToTable("TestData21").HasIndex(p => p.aid);
                m.Entity<TestData3>().ToTable("TestData31").HasIndex(p => p.Bid);
            });

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
                            data.Add(new TestData() { Name = $"data-{i}", CreatedTime = DateTime.Now.AddDays(-1) });
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

                        var UData = new TestData() { kid = vlist.Last().kid, Name = "Update", CreatedTime = DateTime.Now.AddDays(1) };

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
                        data.Add(new TestData() { Name = $"data-{i}", CreatedTime = DateTime.Now.AddDays(-10) });
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

                    var UData = new TestData() { kid = vlist.Last().kid, Name = "Update", CreatedTime = DateTime.Now };

                    DataUtil.Update<TestData>(UData);


                    //更新单个属性
                    DataUtil.UpdateSet<TestData>(new { kid = vlist.First().kid, CreatedTime = DateTime.Now.AddYears(10).Date });

                    //更新单个属性
                    DataUtil.UpdateSet<TestData>(new { Name = "UpdateProperty22" }, Predicates.Field<TestData>(p => p.kid, Operator.Eq, vlist.Skip(1).First().kid));
                    //更新单个属性
                    DataUtil.UpdateSet<TestData>(new Dictionary<string, object>() {
                            { "Name","UpdateProperty33" },
                            { "CreatedTime",DateTime.Now }
                        }, new { kid = vlist.Skip(2).First().kid });


                    var vData = DataUtil.Get<TestData>(UData.kid);
                    if (vData != null)
                        await context.Response.WriteAsync($"name= {vData.Name} kid={vData.kid}\n");

                    var vPage = DataUtil.GetPages<TestData>(2, 10, Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));
                    await context.Response.WriteAsync($"CurrentPage= {vPage.CurrentPage} ItemsPerPage={vPage.ItemsPerPage} TotalItems:{vPage.TotalItems} TotalPages:{vPage.TotalPages}\n");
                    foreach (var item in vPage.Items)
                    {
                        await context.Response.WriteAsync($"name= {item.Name} kid={item.kid}\n");
                    }

                    DataUtil.Insert<TestData2>(new TestData2() { kid = vlist.First().kid, Name = "inner name" });
                    //关联查询
                    var vlistJoin = DataUtil.GetList<ViewData>(
                       new List<IJoinPredicate>() {
                               Predicates.Join<TestData, TestData2>(t => t.kid, Operator.Eq, t1 => t1.kid,JoinOperator.Inner) ,
                               Predicates.Join<TestData2, TestData3>(t => t.aid, Operator.Eq, t1 => t1.aid,JoinOperator.Left)
                       },
                       new List<IJoinAliasPredicate>()
                       {
                               Predicates.JoinAlia<ViewData,TestData2>(t=>t.Name2,t1=>t1.Name)
                       }
                        , Predicates.Field<TestData>(p => p.CreatedTime, Operator.Ge, DateTime.Now.Date),
                       new List<ISort>() {
                               Predicates.Sort<TestData3>(p=>p.Name)
                       });


                    foreach (var item in vlistJoin)
                    {
                        await context.Response.WriteAsync($"name= {item.Name} kid={item.kid} Name2={item.Name2} aid={item.aid}\n");
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
                        data.Add(new TestData() { Name = $"data-{i}", CreatedTime = DateTime.Now.AddDays(1) });
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


                    var UData = new TestData() { kid = vlist.Last().kid, Name = "Update", CreatedTime = DateTime.Now.AddDays(1) };

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

            app.Map("/join", ap =>
            {
                ap.Run(async (context) =>
                {
                    var DataUtil = context.RequestServices.GetService<IDatabase>();



                    DataUtil.Delete<TestData>("TestData1", Predicates.Field<TestData>(p => p.kid, Operator.Gt, 0));
                    DataUtil.Delete<TestData2>("TestData21", Predicates.Field<TestData2>(p => p.aid, Operator.Gt, 0));
                    DataUtil.Delete<TestData3>("TestData31", Predicates.Field<TestData3>(p => p.Bid, Operator.Gt, 0));

                    var data = new List<TestData>();
                    for (int i = 0; i < 100; i++)
                    {
                        data.Add(new TestData() { Name = $"data-{i}", CreatedTime = DateTime.Now.AddDays(-10) });
                    }

                    DataUtil.RunInTransaction(() => DataUtil.Insert<TestData>("TestData1", data));

                    await context.Response.WriteAsync($"insert to {data.Count}\n");

                    var long4 = DataUtil.Count<TestData>("TestData1", Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));

                    await context.Response.WriteAsync($"like 4= {long4}\n");

                    var vlist = DataUtil.GetList<TestData>("TestData1", Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));
                    foreach (var item in vlist)
                    {
                        await context.Response.WriteAsync($"name= {item.Name} kid={item.kid}\n");
                    }

                    var UData = new TestData() { kid = vlist.Last().kid, Name = "Update", CreatedTime = DateTime.Now };

                    DataUtil.Update<TestData>("TestData1", UData);


                    DataUtil.UpdateSet<TestData>("TestData1", new { kid = vlist.First().kid, CreatedTime = DateTime.Now.AddYears(10).Date });


                    DataUtil.UpdateSet<TestData>("TestData1", new { Name = "UpdateProperty22" }, Predicates.Field<TestData>(p => p.kid, Operator.Eq, vlist.Skip(1).First().kid));

                    DataUtil.UpdateSet<TestData>("TestData1", new Dictionary<string, object>() {
                            { "Name","UpdateProperty33" },
                            { "CreatedTime",DateTime.Now }
                        }, Predicates.Field<TestData>(p => p.kid, Operator.Eq, vlist.Skip(2).First().kid));


                    var vData = DataUtil.Get<TestData>(UData.kid, "TestData1");
                    if (vData != null)
                        await context.Response.WriteAsync($"name= {vData.Name} kid={vData.kid}\n");

                    var vPage = DataUtil.GetPages<TestData>("TestData1", 2, 10, Predicates.Field<TestData>(p => p.Name, Operator.Like, "%4%"));
                    await context.Response.WriteAsync($"CurrentPage= {vPage.CurrentPage} ItemsPerPage={vPage.ItemsPerPage} TotalItems:{vPage.TotalItems} TotalPages:{vPage.TotalPages}\n");
                    foreach (var item in vPage.Items)
                    {
                        await context.Response.WriteAsync($"name= {item.Name} kid={item.kid}\n");
                    }

                    await context.Response.WriteAsync("Hello World!");

                    DataUtil.Insert<TestData2>("TestData21", new TestData2() { kid = vlist.First().kid, Name = "inner name" });

                    var vlistJoin = DataUtil.GetList<ViewData>(
                       new List<IJoinPredicate>() {
                               Predicates.Join<TestData, TestData2>(t => t.kid, Operator.Eq, t1 => t1.kid,JoinOperator.Inner,"TestData1","TestData21") ,
                               Predicates.Join<TestData2, TestData3>(t => t.aid, Operator.Eq, t1 => t1.aid,JoinOperator.Left,"TestData21","TestData31")
                       },
                       new List<IJoinAliasPredicate>()
                       {
                               Predicates.JoinAlia<ViewData,TestData2>(t=>t.Name2,t1=>t1.Name)
                       }
                        , Predicates.Field<TestData>(p => p.CreatedTime, Operator.Ge, DateTime.Now.Date, "TestData1"),
                       new List<ISort>() {
                               Predicates.Sort<TestData3>(p=>p.Name,true,"TestData31")
                       });


                    foreach (var item in vlistJoin)
                    {
                        await context.Response.WriteAsync($"name= {item.Name} kid={item.kid} Name2={item.Name2} aid={item.aid}\n");
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
