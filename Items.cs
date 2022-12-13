// MIT License
//
// Copyright (c) 2022 Kamil Ercan Turkarslan
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Text;

namespace hvc.DataStructures;

public class Items<T>
    where T : IKeyedObject
{
    private readonly List<T> _allItems;
    private readonly SortedDictionary<String, T> _sortedItems;

    public Items()
    {
        _allItems = new List<T>();
        _sortedItems = new SortedDictionary<String, T>();
    }

    public Items(StringComparer stringComparer)
    {
        _allItems = new List<T>();
        _sortedItems = new SortedDictionary<String, T>(stringComparer);
    }

    public Items(IEnumerable<Object> items, StringComparer stringComparer, Boolean throwIfNotFound = false)
        : this(stringComparer)
    {
        foreach (var keyedItem in items
                     .Where(item => item.GetType().AssemblyQualifiedName == typeof(T).AssemblyQualifiedName)
                     .Select(item => (T)item))
        {
            _sortedItems.Add(keyedItem.Key, keyedItem);
            _allItems.Add(keyedItem);
        }

        if(_allItems.Count == 0 && throwIfNotFound)
            throw new InvalidOperationException(nameof(_allItems));
    }

    public IEnumerable<T> AllItems => _allItems;

    public Int32 Count => _allItems.Count;

    public Boolean HasItems => _allItems.Any();

    public IEnumerable<T> OrderedItems => _sortedItems.Values;

    public T Add(T item)
    {
        _sortedItems.Add(item.Key, item);
        _allItems.Add(item);

        return item;
    }

    public T AddOrGet(T item)
    {
        if (ContainsKey(item.Key))
            return _sortedItems[item.Key];

        _sortedItems.Add(item.Key, item);
        _allItems.Add(item);

        return item;
    }

    public void AddRange(IEnumerable<IKeyedObject> allItems, Boolean ignoreIfDuplicate = false)
    {
        foreach (var item in allItems)
        {
            if (!item.GetType().IsSubclassOf(typeof(T)) && item.GetType() != typeof(T))
                continue;

            var keyedItem = (T)item;
            if (ignoreIfDuplicate && _sortedItems.ContainsKey(keyedItem.Key))
                continue;

            _sortedItems.Add(keyedItem.Key, keyedItem);
            _allItems.Add(keyedItem);
        }
    }

    public String CombineKeys(String prefix = "", String separator = "", String suffix = "")
    {
        var firstTime = true;
        var sb = new StringBuilder();

        foreach (var item in AllItems)
        {
            if (!firstTime)
                sb.Append(separator);

            firstTime = false;

            sb.Append($"{prefix}{item.Key}{suffix}");
        }

        return sb.ToString();
    }

    public Boolean Contains(T item)
    {
        return _sortedItems.ContainsKey(item.Key);
    }

    public Boolean ContainsKey(String key)
    {
        return _sortedItems.ContainsKey(key);
    }

    public void ForEach(Action<T> action)
    {
        _allItems.ForEach(action);
    }

    public void ForEach(Type itemType, Action<T> action)
    {
        foreach (var item in AllItems)
            if (item.GetType() == itemType)
                action(item);
    }

    public T Get(String key)
    {
        return GetIfExists(key) ?? throw new InvalidOperationException($"Couldn't find expected item '{key}'!");
    }

    public T? GetIfExists(String key)
    {
        return _sortedItems.ContainsKey(key) ? _sortedItems[key] : default;
    }

    public String[] GetKeys()
    {
        return AllItems.Select(item => item.Key).ToArray();
    }

    public T? ItemOf(Type typeOfItem)
    {
        var retValue = default(T);
        var itemFound = false;

        foreach (var item in _allItems.Where(item =>
                     item.GetType().IsSubclassOf(typeOfItem) || item.GetType() == typeOfItem))
        {
            if (itemFound)
                throw new InvalidOperationException(nameof(itemFound));

            retValue = item;
            itemFound = true;
        }

        return retValue;
    }

    public T[] ItemsOf(Type typeOfItem)
    {
        return _allItems.Where(item => item.GetType().IsSubclassOf(typeOfItem) || item.GetType() == typeOfItem)
            .ToArray();
    }

    public T? Single(Boolean throwIfNotFound = false)
    {
        if(throwIfNotFound && _allItems.Count == 0)
            throw new InvalidOperationException("No items were found in the collection!");
        if (_allItems.Count > 1)
            throw new InvalidOperationException("More than one item found in the collection!");

        return _allItems.FirstOrDefault();
    }
}