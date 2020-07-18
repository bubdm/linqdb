﻿#if (SERVER || SOCKETS)
using LinqdbClient;
using ServerLogic;
#else
using LinqDb;
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testing.tables;

namespace Testing.basic_tests
{
    class LessThanDate : ITest
    {
        public void Do(Db db)
        {
            bool dispose = false; if (db == null) { db = new Db("DATA"); dispose = true; }
#if (SERVER)
            db._db_internal.CallServer = (byte[] f) => { return SocketTesting.CallServer(f); };
#endif
#if (SOCKETS)
            db._db_internal.CallServer = (byte[] f) => { return SocketTesting.CallServer(f, db); };
#endif
#if (SOCKETS || SAMEDB || INDEXES || SERVER)
            db.Table<SomeData>().Delete(new HashSet<int>(db.Table<SomeData>().Select(f => new { f.Id }).Select(f => f.Id).ToList()));
#endif
            var d = new SomeData()
            {
                Id = 1,
                Normalized = 1.2,
                PeriodId = 5,
                Date = DateTime.Now
            };
            db.Table<SomeData>().Save(d);
            d = new SomeData()
            {
                Id = 2,
                Normalized = 2.3,
                PeriodId = 10,
                Date = Convert.ToDateTime("2000-01-01", CultureInfo.InvariantCulture)
            };
            db.Table<SomeData>().Save(d);;
            d = new SomeData()
            {
                Id = 3,
                Normalized = -4.5,
                PeriodId = 15,
                Date = Convert.ToDateTime("1995-01-01", CultureInfo.InvariantCulture)
            };
            db.Table<SomeData>().Save(d);;
            var date = Convert.ToDateTime("2000-01-01");
            var res = db.Table<SomeData>()
                        .Where(f => f.Date < date)
                        .Select(f => new
                        {
                            Normalized = f.Normalized
                        });
            if (res.Count() != 1 || res[0].Normalized != -4.5)
            {
                throw new Exception("Assert failure");
            }
#if (SERVER || SOCKETS)
            if(dispose) { Logic.Dispose(); }
#else
            if (dispose) { db.Dispose(); }
#endif
#if (!SOCKETS && !SAMEDB && !INDEXES && !SERVER)
            if(dispose) { ServerSharedData.SharedUtils.DeleteFilesAndFoldersRecursively("DATA"); }
#endif
        }

        public string GetName()
        {
            return this.GetType().Name;
        }
    }
}
