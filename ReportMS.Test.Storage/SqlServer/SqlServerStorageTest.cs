﻿using System.Collections.Generic;
using System.Linq;
using Gear.Infrastructure.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReportMS.Test.Common;

namespace ReportMS.Test.Storage.SqlServer
{
    [TestClass]
    public class SqlServerStorageTest
    {
        private readonly string _connectionName = "rms";

        [TestInitialize]
        public void Initialize()
        {
            Helper.Init();
        }

        [TestMethod]
        public void QueryList_Test()
        {
            var sql = "SELECT * FROM TestTemp";
            var tests = StorageManager.CreateInstance(this._connectionName).Select<SqlServerTempTest>(sql);

            Assert.IsNotNull(tests);
            Assert.IsTrue(tests.Any());

            var detail = Utils.LookupEntityDetail(tests);
            Assert.IsNull(detail, detail);
        }

        [TestMethod]
        public void QueryListWithParams_Test()
        {
            var sql = "SELECT ID, Name, Age, Weight, Address, Enabled, CreatedDate FROM TestTemp WHERE Name = @Name AND Enabled = @Enabled";

            var param = new Dictionary<string, object>
            {
                {"Name", "Ketty"},
                {"Enabled", true}
            };

            var tests = StorageManager.CreateInstance(this._connectionName).Select<SqlServerTempTest>(sql, param);

            Assert.IsNotNull(tests);
            Assert.IsTrue(tests.Any());

            var detail = Utils.LookupEntityDetail(tests);
            Assert.IsNull(detail, detail);
        }

        [TestMethod]
        public void QueryCount_Test()
        {
            var sql = "SELECT ID, Name, Age, Weight, Address, Enabled, CreatedDate FROM TestTemp WHERE Enabled = @Enabled";

            var param = new Dictionary<string, object>
            {
                {"Enabled", true}
            };

            var tests = StorageManager.CreateInstance(this._connectionName).Select<SqlServerTempTest>(sql, 1, 2, param);

            Assert.IsNotNull(tests);
            Assert.IsTrue(tests.Any());

            var detail = Utils.LookupEntityDetail(tests);
            Assert.IsNull(detail, detail);
        }

        [TestMethod]
        public void Job_Test()
        {
            var sql = "SELECT PI_No, Bill_ID, Bill_Company, Contact, Email, RMA_NO, Modify_DT, Result, TAT FROM V_AASC_Pending_PO  WHERE (RMA_NO > @YnTRx6AWek)";
            var parms = new Dictionary<string, object>
            {
                {"@YnTRx6AWek", "A161"}
            };

            var reader = StorageManager.CreateInstance("RMA").GetDataReader(sql, parms);

            if (reader != null)
            {
                reader.Close();
            }
        }
    }
}
