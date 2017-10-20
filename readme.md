例子
修改 Startup类
```c#

public void ConfigureServices(IServiceCollection services)
{
    services.AddDapperDataBase(ESqlDialect.SqlServer,() => new SqlConnection(Configuration.GetConnectionString("DefaultConnection")));
}

```


IDatabase用法使用

```c#
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
//多表关联查询
//IJoinAliasPredicate 参数的含义是，当关联表的字段名相同时，指定别名 
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

//Model类对多表，可用于分表的情况，指定tablename即可
DataUtil.Insert<TestData2>("TestData21", new TestData2() { kid = vlist.First().kid, Name = "inner name" });

var vlistJoinT = DataUtil.GetList<ViewData>(
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


foreach (var item in vlistJoinT)
{
    await context.Response.WriteAsync($"name= {item.Name} kid={item.kid} Name2={item.Name2} aid={item.aid}\n");
}


await context.Response.WriteAsync("Hello World!");

```


原始用法使用
```c#

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

```


# Introduction

Dapper Extensions is a small library that complements [Dapper](https://github.com/SamSaffron/dapper-dot-net) by adding basic CRUD operations (Get, Insert, Update, Delete) for your POCOs. For more advanced querying scenarios, Dapper Extensions provides a predicate system. The goal of this library is to keep your POCOs pure by not requiring any attributes or base class inheritance.

Customized mappings are achieved through [ClassMapper](https://github.com/tmsmith/Dapper-Extensions/wiki/AutoClassMapper). 

**Important**: This library is a separate effort from Dapper.Contrib (a sub-system of the [Dapper](https://github.com/SamSaffron/dapper-dot-net) project).

Features
--------
* Zero configuration out of the box.
* Automatic mapping of POCOs for Get, Insert, Update, and Delete operations.
* GetList, Count methods for more advanced scenarios.
* GetPage for returning paged result sets.
* Automatic support for Guid and Integer [primary keys](https://github.com/tmsmith/Dapper-Extensions/wiki/KeyTypes) (Includes manual support for other key types).
* Pure POCOs through use of [ClassMapper](https://github.com/tmsmith/Dapper-Extensions/wiki/AutoClassMapper) (_Attribute Free!_).
* Customized entity-table mapping through the use of [ClassMapper](https://github.com/tmsmith/Dapper-Extensions/wiki/AutoClassMapper).
* Composite Primary Key support.
* Singular and Pluralized table name support (Singular by default).
* Easy-to-use [Predicate System](https://github.com/tmsmith/Dapper-Extensions/wiki/Predicates) for more advanced scenarios.
* Properly escapes table/column names in generated SQL (Ex: SELECT [FirstName] FROM [Users] WHERE [Users].[UserId] = @UserId_0)
* Unit test coverage (150+ Unit Tests)

Naming Conventions
------------------
* POCO names should match the table name in the database. Pluralized table names are supported through the PlurizedAutoClassMapper.
* POCO property names should match each column name in the table.
* By convention, the primary key should be named Id. Using another name is supported through custom mappings.

# Installation

For more information, please view our [Getting Started](https://github.com/tmsmith/Dapper-Extensions/wiki/Getting-Started) guide.

http://nuget.org/List/Packages/DapperExtensions

```
PM> Install-Package DapperExtensions
```

# Examples
The following examples will use a Person POCO defined as:

```c#
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool Active { get; set; }
    public DateTime DateCreated { get; set; }
}
```


## Get Operation

```c#
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    int personId = 1;
    Person person = cn.Get<Person>(personId);	
    cn.Close();
}
```

## Simple Insert Operation

```c#
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    Person person = new Person { FirstName = "Foo", LastName = "Bar" };
    int id = cn.Insert(person);
    cn.Close();
}
```

## Advanced Insert Operation (Composite Key)

```c#
public class Car
{
    public int ModelId { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
}

...

using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    Car car = new Car { Color = "Red" };
    var multiKey = cn.Insert(car);
    cn.Close();

    int modelId = multiKey.ModelId;
    int year = multiKey.Year;
}
```

## Simple Update Operation

```c#
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    int personId = 1;
    Person person = cn.Get<Person>(personId);
    person.LastName = "Baz";
    cn.Update(person);
    cn.Close();
}
```


## Simple Delete Operation

```c#
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    Person person = cn.Get<Person>(1);
    cn.Delete(person);
    cn.Close();
}
```

## GetList Operation (with Predicates)

```c#
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
    IEnumerable<Person> list = cn.GetList<Person>(predicate);
    cn.Close();
}
```

Generated SQL

```
SELECT 
   [Person].[Id]
 , [Person].[FirstName]
 , [Person].[LastName]
 , [Person].[Active]
 , [Person].[DateCreated] 
FROM [Person] 
WHERE ([Person].[Active] = @Active_0)
```

More information on predicates can be found in [our wiki](https://github.com/tmsmith/Dapper-Extensions/wiki/Predicates).


## Count Operation (with Predicates)

```c#
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
    int count = cn.Count<Person>(predicate);
    cn.Close();
}            
```

Generated SQL

```
SELECT 
   COUNT(*) Total 
FROM [Person] 
WHERE ([Person].[DateCreated] < @DateCreated_0)
```

More information on predicates can be found in [our wiki](https://github.com/tmsmith/Dapper-Extensions/wiki/Predicates).


# License

Copyright 2011 Thad Smith, Page Brooks and contributors

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
