﻿using System;
using System.Collections.Generic;
using Gear.Infrastructure.Algorithms.Cryptography;
using Gear.Infrastructure.Storage.Config;
using Gear.Utility.IO.Excels;
using ReportMS.DataTransferObjects.Dtos;
using ReportMS.Reports.Dao;
using ReportMS.Reports.ReadModel;
using ReportMS.Reports.DataTables;
using ReportMS.ServiceContracts;

namespace ReportMS.Reports.Managers
{
    /// <summary>
    /// 报表读取管理
    /// </summary>
    public class ReportReadManager : IReportRead
    {
        private readonly ReportSqlWrapper sqlWrapper = new ReportSqlWrapper();
        private readonly IDataDao dataDao;
        private readonly IReportQueryService _reportQueryServiceContract;
        private ReportDto report;

        #region Ctor

        /// <summary>
        /// 初始化<c>ReportReadManager</c>
        /// </summary>
        /// <param name="dataDao">报表数据访问对象</param>
        /// <param name="reportQueryServiceContract">报表查询服务</param>
        public ReportReadManager(IDataDao dataDao, IReportQueryService reportQueryServiceContract)
        {
            this.dataDao = dataDao;
            this._reportQueryServiceContract = reportQueryServiceContract;

            var tableId = this.sqlWrapper.GetReportTableOrViewId();
            if (tableId.HasValue)
                this.GetTableOrViewName(tableId.Value);
        }

        #endregion

        #region Public Methods

        public IEnumerable<ReportData> ExecuteSqlQuery()
        {
            var sqlQueryAndParms = this.GetSqlQueryAndParms(SelectClauseBuildMode.StringWithAlias);

            var connectionOpt = this.GetConnectionOption();
            return this.dataDao.Query<ReportData>(connectionOpt.Item1, connectionOpt.Item2, sqlQueryAndParms.Item1, sqlQueryAndParms.Item2);
        }

        public object ExecuteDataTablesQuery()
        {
            var sqlQueryAndParms = this.GetSqlQueryAndParms(SelectClauseBuildMode.StringWithAlias);

            var sqlQuery = sqlQueryAndParms.Item1;
            var sqlWhere = sqlQueryAndParms.Item2;
            var start = DataTablesOption.Start + 1; // 分页中 start 为起始行，不是要跳过的行值
            var length = DataTablesOption.Length;

            var connectionOpt = this.GetConnectionOption();
            var itemCount = this.dataDao.QueryCount(connectionOpt.Item1, connectionOpt.Item2, sqlQuery, sqlWhere);
            var items = this.dataDao.Query<ReportData>(connectionOpt.Item1, connectionOpt.Item2, sqlQuery, sqlWhere, start, length);

            var datatable = new DataTables<ReportData>(items, itemCount);
            return datatable.WrapDataTablesObject();
        }

        public byte[] ExecuteExcelExport(string sheetName)
        {
            var connectionOpt = this.GetConnectionOption();
            var sqlQueryAndParms = this.GetSqlQueryAndParms(SelectClauseBuildMode.Raw);
            var reader = DatabaseReader.Create(connectionOpt.Item1, connectionOpt.Item2)
                .Reader.GetDataReader(sqlQueryAndParms.Item1, sqlQueryAndParms.Item2);

            var excel = ExcelFactory.Create(sheetName, reader);
            return excel.SaveAsBytes();
        }

        public Guid ReportId
        {
            get { return this.report.ID; }
        }

        public string TableOrViewName
        {
            get { return this.report.ReportName; }
        }

        public Tuple<string, IDictionary<string, object>> GetSqlQueryAndParameters()
        {
            return this.GetSqlQueryAndParms(SelectClauseBuildMode.Raw);
        }

        #endregion

        #region Private Methods

        private void GetTableOrViewName(Guid tableOrViewId)
        {
            this.report = this._reportQueryServiceContract.GetReport(tableOrViewId, false);
            if (report == null)
                throw new Exception(String.Format("The table collection is not exists the table id [{0}].",
                    tableOrViewId.ToString()));
        }

        private Tuple<string, IDictionary<string, object>> GetSqlQueryAndParms(SelectClauseBuildMode mode)
        {
            this.sqlWrapper.ExecuteSqlBagBuilder(this.TableOrViewName, mode);

            var sqlQuery = this.sqlWrapper.GetSqlClause();
            var parameters = this.sqlWrapper.GetSqlParameters();

            return Tuple.Create(sqlQuery, parameters);
        }

        private Tuple<ConnectionOptions, string> GetConnectionOption()
        {
            var rdbms = this.report.Rdbms;
            var connectionOpt = new ConnectionOptions
            {
                DataSource = rdbms.Server,
                InitialCatalog = rdbms.Catalog,
                UserId = rdbms.UserId,
                Password = rdbms.Password,
                ReadOnly = rdbms.ReadOnly
            };

            return Tuple.Create(connectionOpt, rdbms.Provider);
        }
        #endregion
    }
}
