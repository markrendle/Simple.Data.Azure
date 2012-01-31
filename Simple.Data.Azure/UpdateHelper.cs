using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Azure
{
    using Simple.Azure;

    internal class UpdateHelper
    {
        private readonly AzureTableAdapter _adapter;

        public UpdateHelper(AzureTableAdapter adapter)
        {
            _adapter = adapter;
        }

        public int Update(Table table, IDictionary<string, object> data, SimpleExpression criteria)
        {
            var keys = criteria.TryGetKeyCombo();
            if (keys != KeyCombo.Empty)
            {
                UpdateRow(table, keys, data);
                return 1;
            }

            int count = 0;
            foreach (var matchingRow in _adapter.Find(table.TableName, criteria))
            {
                UpdateRow(table, KeyCombo.FromDictionary(matchingRow), data);
                ++count;
            }

            return count;
        }

        private static void UpdateRow(Table table, KeyCombo keys, IDictionary<string, object> data)
        {
            table.MergeRow(keys.PartitionKey, keys.RowKey, data);
        }
    }
}
