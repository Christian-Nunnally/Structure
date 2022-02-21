using Structure.IO;
using Structure.IO.Persistence;
using System;
using System.Collections.Generic;

namespace Structure.TaskItems
{
    public class ItemConverter<T> where T : Node
    {
        private readonly Dictionary<Type, Dictionary<Type, Func<T, T>>> _conversionMap = new Dictionary<Type, Dictionary<Type, Func<T, T>>>();

        internal bool CanConvert(Type from) => _conversionMap.ContainsKey(from);

        internal void RegisterConversion(Type from, Type to, Func<T, T> conversionFunction)
        {
            AddMapIfDoesntExist(from);
            var inner = _conversionMap[from];
            if (!inner.ContainsKey(to)) inner.Add(to, conversionFunction);
        }

        internal UserAction[] GetPossibleConversions(NodeTree<T> tree, T from)
        {
            AddMapIfDoesntExist(from.GetType());
            var possibleActions = new List<UserAction>();
            foreach (var (type, conversion) in _conversionMap[from.GetType()])
            {
                var newItem = conversion(from);
                void action() => ReplaceItem(tree, from, newItem);
                var description = $"Convert to {type.Name}";
                var userAction = new UserAction(description, action);
                possibleActions.Add(userAction);
            }
            return possibleActions.ToArray();
        }

        private void AddMapIfDoesntExist(Type from)
        {
            if (!_conversionMap.ContainsKey(from)) _conversionMap.Add(from, new Dictionary<Type, Func<T, T>>());
        }

        public void ReplaceItem(NodeTree<T> tree, T itemToReplace, T newItem)
        {
            newItem.ID = itemToReplace.ID;
            tree.Remove(itemToReplace);
            tree.Set(newItem);
        }
    }
}
