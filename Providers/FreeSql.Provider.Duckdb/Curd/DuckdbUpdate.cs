﻿using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Duckdb.Curd
{

    class DuckdbUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1>
    {

        public DuckdbUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        internal string InternalTableAlias { get; set; }
        internal StringBuilder InternalSbSet => _set;
        internal StringBuilder InternalSbSetIncr => _setIncr;
        internal Dictionary<string, bool> InternalIgnore => _ignore;
        internal void InternalResetSource(List<T1> source) => _source = source;
        internal string InternalWhereCaseSource(string CsName, Func<string, string> thenValue) => WhereCaseSource(CsName, thenValue);
        internal void InternalToSqlCaseWhenEnd(StringBuilder sb, ColumnInfo col) => ToSqlCaseWhenEnd(sb, col);

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        protected override List<TReturn> ExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns) => base.SplitExecuteUpdated<TReturn>(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, columns);
        protected override List<TReturn> RawExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns) => throw new NotImplementedException($"FreeSql.Provider.Duckdb {CoreErrorStrings.S_Not_Implemented_Feature}");

        protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys)
        {
            if (primarys.Length == 1)
            {
                var pk = primarys.First();
                if (string.IsNullOrEmpty(InternalTableAlias) == false) caseWhen.Append(InternalTableAlias).Append(".");
                caseWhen.Append(_commonUtils.RereadColumn(pk, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
                return;
            }
            caseWhen.Append("(");
            var pkidx = 0;
            foreach (var pk in primarys)
            {
                if (pkidx > 0) caseWhen.Append(" || '+' || ");
                if (string.IsNullOrEmpty(InternalTableAlias) == false) caseWhen.Append(InternalTableAlias).Append(".");
                caseWhen.Append(_commonUtils.RereadColumn(pk, _commonUtils.QuoteSqlName(pk.Attribute.Name))).Append("::text");
                ++pkidx;
            }
            caseWhen.Append(")");
        }

        protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d)
        {
            if (primarys.Length == 1)
            {
                sb.Append(_commonUtils.FormatSql("{0}", primarys[0].GetDbValue(d)));
                return;
            }
            sb.Append("(");
            var pkidx = 0;
            foreach (var pk in primarys)
            {
                if (pkidx > 0) sb.Append(" || '+' || ");
                sb.Append(_commonUtils.FormatSql("{0}", pk.GetDbValue(d))).Append("::text");
                ++pkidx;
            }
            sb.Append(")");
        }

        protected override void ToSqlCaseWhenEnd(StringBuilder sb, ColumnInfo col)
        {
            if (_noneParameter == false) return;
            if (col.Attribute.MapType == typeof(string))
            {
                sb.Append("::text");
                return;
            }
            var dbtype = _commonUtils.CodeFirst.GetDbInfo(col.Attribute.MapType)?.dbtype;
            if (dbtype == null) return;

            sb.Append("::").Append(dbtype);
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => base.SplitExecuteAffrowsAsync(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
        protected override Task<List<TReturn>> ExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default) => base.SplitExecuteUpdatedAsync<TReturn>(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, columns, cancellationToken);
        protected override Task<List<TReturn>> RawExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default) => throw new NotImplementedException($"FreeSql.Provider.Duckdb {CoreErrorStrings.S_Not_Implemented_Feature}");

#endif
    }
}
