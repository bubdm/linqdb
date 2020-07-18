﻿#if (SERVER || SOCKETS)
using LinqdbClient;
using ServerLogic;
#else
using LinqDb;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testing.tables;
using Testing.tables4;

namespace Testing.basic_tests
{
    class GetTableDefinition : ITest
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
            db.Table<Counter>().Delete(new HashSet<int>(db.Table<Counter>().Select(f => new { f.Id }).Select(f => f.Id).ToList()));
#endif
            var d = new Counter()
            {
                Id = 1,
                Name = "a",
                Value = 5
            };
            db.Table<Counter>().Save(d);

            var res = db.GetTableDefinition("Counter");
            if (res != "public class Counter { public int Id { get; set; } public int? Value { get; set; } public string Name { get; set; }  } " &&
                res != "public class Counter { public int Id { get; set; } public string Name { get; set; } public int? Value { get; set; }  } ")
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
