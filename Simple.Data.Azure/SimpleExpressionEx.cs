namespace Simple.Data.Azure
{
    using System;
    using System.Collections.Generic;

    internal static class SimpleExpressionEx
    {
        public static IDictionary<string, object> ToDictionary(this SimpleExpression expression)
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

        public static string TryGetValue(this SimpleExpression criteria, string name)
        {
            if (criteria == null) return null;

            string value = null;
            var leftOperand = criteria.LeftOperand as ObjectReference;
            if ((!leftOperand.IsNull()) && leftOperand.GetName() == name)
            {
                if (criteria.RightOperand != null)
                    value = criteria.RightOperand.ToString();
            }
            return value;
        }

        public static KeyCombo TryGetKeyCombo(this SimpleExpression criteria)
        {
            if (criteria.Type != SimpleExpressionType.And) return new KeyCombo(TryGetValue(criteria, "PartitionKey"));

            return new KeyCombo(TryGetValue(criteria.LeftOperand as SimpleExpression, "PartitionKey"), TryGetValue(criteria.RightOperand as SimpleExpression, "RowKey"));
        }

        private static void ToDictionaryImpl(SimpleExpression expression, IDictionary<string, object> dictionary)
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
}