using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Azure
{
    using System.ComponentModel.Composition;
    using Helpers;

    [Export("Azure", typeof(Adapter))]
    public class AzureTableAdapter : Adapter
    {
        private AzureHelper _helper;

        protected override void OnSetup()
        {
            base.OnSetup();
            _helper = new AzureHelper { UrlBase = Settings.Url, SharedKey = Settings.Key, Account = Settings.Account };
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            var filter = new ExpressionFormatter().Format(criteria);
            var table = GetTable(tableName);
            return table.Query(filter);
        }

        private Table GetTable(string tableName)
        {
            return new Table(tableName, _helper);
        }

        public override IDictionary<string, object> Get(string tableName, params object[] keyValues)
        {
            if (keyValues.Length < 1 || keyValues.Length > 2)
            {
                throw new ArgumentException("AzureTableAdapter Get method requires PartitionKey and optional RowKey values.");
            }

            if (keyValues[0] == null) throw new ArgumentNullException("PartitionKey cannot be null.");

            var criteria = ObjectReference.FromStrings(tableName, "PartitionKey") == keyValues[0].ToString();

            if (keyValues.Length == 2)
            {
                if (keyValues[1] == null) throw new ArgumentNullException("RowKey cannot be null.");
                criteria = criteria && ObjectReference.FromStrings(tableName, "RowKey") == keyValues[1].ToString();
            }

            try
            {
                return Find(tableName, criteria).SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new SimpleDataException("More than one row matched Get key criteria.");
            }
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired)
        {
            var table = GetTable(tableName);
            var row = table.InsertRow(data);
            return resultRequired ? row : null;
        }

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            var table = GetTable(tableName);

            return new UpdateHelper(this).Update(table, data, criteria);

        }

        public override int Update(string tableName, IDictionary<string, object> data)
        {
            throw new NotImplementedException();
        }

        public override int Delete(string tableName, SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        public override bool IsExpressionFunction(string functionName, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
