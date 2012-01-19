using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Azure
{
    internal class UpdateHelper
    {
        private readonly AzureTableAdapter _adapter;

        public UpdateHelper(AzureTableAdapter adapter)
        {
            _adapter = adapter;
        }

        public int Update(Table table, IDictionary<string, object> data, SimpleExpression criteria)
        {
            if (TryUpdateByPartitionKey(table, data, criteria)) return 1;

            if (TryUpdateByPartitionKeyAndRowKey(table, data, criteria)) return 1;

            int count = 0;
            foreach (var matchingRow in _adapter.Find(table.TableName, criteria))
            {
                UpdateRow(table, matchingRow, data);
                ++count;
            }

            return count;
        }

        private static bool TryUpdateByPartitionKey(Table table, IDictionary<string, object> data, SimpleExpression criteria)
        {
            var leftOperand = criteria.LeftOperand as ObjectReference;
            if ((!leftOperand.IsNull()) && leftOperand.GetName() == "PartitionKey")
            {
                table.MergeRow(criteria.RightOperand.ToString(), string.Empty, data);
                {
                    return true;
                }
            }
            return false;
        }

        private static bool TryUpdateByPartitionKeyAndRowKey(Table table, IDictionary<string, object> data, SimpleExpression criteria)
        {
            if (criteria.Type != SimpleExpressionType.And) return false;

            var dictionary = criteria.ToDictionary();

            if (dictionary == null || dictionary.Count != 2 || !(dictionary.ContainsKey("PartitionKey") && dictionary.ContainsKey("RowKey")))
                return false;

            UpdateRow(table, dictionary, data);
            return true;
        }

        private static void UpdateRow(Table table, IDictionary<string, object> keys, IDictionary<string, object> data)
        {
            table.MergeRow(keys["PartitionKey"].ToStringOrEmpty(), keys["RowKey"].ToStringOrEmpty(), data);
        }
    }

    internal static class SimpleExpressionEx
    {
        public static IDictionary<string,object> ToDictionary(this SimpleExpression expression)
        {
            try
            {
                var dictionary = new Dictionary<string, object>();
                ToDictionaryImpl(expression, dictionary);
                return dictionary;
            }
            catch (NotSimpleCriteriaException)
            {
                return null;
            }
        }

        private static void ToDictionaryImpl(SimpleExpression expression, IDictionary<string,object> dictionary)
        {
            if (expression.Type == SimpleExpressionType.And)
            {
                ToDictionaryImpl((SimpleExpression)expression.LeftOperand, dictionary);
                ToDictionaryImpl((SimpleExpression)expression.RightOperand, dictionary);
                return;
            }

            if (expression.Type == SimpleExpressionType.Equal)
            {
                var objectReference = expression.LeftOperand as ObjectReference;
                if (objectReference.IsNull()) throw new NotSimpleCriteriaException();
                dictionary.Add(objectReference.GetName(), expression.RightOperand);
                return;
            }

            throw new NotSimpleCriteriaException();
        }

        private class NotSimpleCriteriaException : Exception
        {
        }
    }

    static class ObjectEx
    {
        public static string ToStringOrEmpty(this object obj)
        {
            return ReferenceEquals(obj, null) ? string.Empty : obj.ToString();
        }
    }
}
