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

using System.Reflection;
using hvc.Extensions;

namespace hvc.DataStructures.Node;

public abstract class ModelNodeBase : IKeyedObject
{
    private static readonly Items<ModelNodeBase> EmptyChildren = new();
    private Items<ModelNodeBase> _children;
    private ModelNodeBase? _parent;

    protected ModelNodeBase(ObjectName name)
    {
        Name = name;
        _parent = null;

        // ReSharper disable once SuspiciousTypeConversion.Global
        _children = this is IModelNodeLeaf
            ? EmptyChildren
            : new Items<ModelNodeBase>(StringComparer.InvariantCultureIgnoreCase);
    }

    protected ModelNodeBase(ModelNodeBase parent, ObjectName name)
        : this(name)
    {
        Parent = parent;

        if (!Parent.CanHaveChildren)
            throw new InvalidOperationException($"{Parent.FullyQualifiedName} doesn't support children!");

        if (!Parent.Add(this))
            throw new NotSupportedException($"{FullyQualifiedName} is not a supported child for {Parent.FullyQualifiedName}");
    }

    // ReSharper disable once SuspiciousTypeConversion.Global
    public Boolean CanHaveChildren => this is not IModelNodeLeaf;

    public String FullyQualifiedName => Parent == null ? Key : $"{Parent.FullyQualifiedName}.{Key}";

    public ModelNodeBase? Parent
    {
        get => this is IModelNodeRoot ? null : _parent;
        protected set
        {
            if (this is IModelNodeRoot && value != null)
                throw new InvalidOperationException($"Parent property of {FullyQualifiedName} can't be modified!");

            _parent = value;
        }
    }

    protected Items<ModelNodeBase> Children
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        get => this is IModelNodeLeaf ? EmptyChildren : _children;
        set
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (this is IModelNodeLeaf)
                throw new InvalidOperationException($"Children property of {FullyQualifiedName} can't be modified!");

            _children = value;
        }
    }

    public ObjectName Name { get; }

    public String Key => Name.Original;

    public override String ToString()
    {
        return Name.Original;
    }

    public Boolean Add(ModelNodeBase modelNode, Boolean throwIfNotInserted = false)
    {
        if (!CanHaveChildren)
            throw new NotSupportedException($"{GetType().FullName} doesn't support Children!");

        Children.Add(modelNode);

        var isInserted = false;
        foreach (var propertyInfo in GetType()
                     .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (propertyInfo.Name == nameof(Children))
                continue;

            var ignoreAutoFill = false;
            foreach (var attr in propertyInfo.GetCustomAttributes(true))
                if (attr is IgnoreAutoFillAttribute { Value: true })
                    ignoreAutoFill = true;

            if (ignoreAutoFill)
                continue;

            if (!propertyInfo.PropertyType.IsGenericType)
                continue;

            if (propertyInfo.PropertyType.GetGenericTypeDefinition() != typeof(Items<>))
                continue;

            var genericArguments = propertyInfo.PropertyType.GetGenericArguments();
            if (genericArguments.Length != 1)
                continue;

            var genericArgument = genericArguments.First();
            if (!modelNode.GetType().CanBeTreatedAsType(genericArgument))
                continue;

            var property = propertyInfo.GetValue(this, null);

            // ReSharper disable once PossibleNullReferenceException
            var method = property?
                .GetType()
                .GetMethod(nameof(Add), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (method == null)
                continue;

            isInserted = true;
            method.Invoke(property, new Object[] { modelNode });
        }

        if (throwIfNotInserted && !isInserted)
            throw new InvalidOperationException($"'{modelNode.FullyQualifiedName}' is not inserted to any collection of '{FullyQualifiedName}'");

        return isInserted;
    }

    public NodeAttribute GetAttribute(String name)
    {
        var propertyInfos = GetType().GetProperties();

        var attribute = (from propertyInfo in propertyInfos
            where String.Equals(propertyInfo.Name, name, StringComparison.InvariantCultureIgnoreCase)
            select (NodeAttribute?)propertyInfo.GetValue(this, null)).FirstOrDefault();

        return attribute ?? throw new NotImplementedException($"Attribute '{name}' is not implemented!");
    }

    public ModelNodeBase? GetParentByType(Type typeOfParent, Boolean throwIfNotFound = false)
    {
        if(this is IModelNodeRoot)
            throw new InvalidOperationException($"{FullyQualifiedName} can not have a parent");

        for (var parent = Parent; parent != null; parent = parent.Parent)
            if (typeOfParent.IsInstanceOfType(parent))
                return parent;

        if(throwIfNotFound)
            throw new InvalidOperationException($"{FullyQualifiedName} does not have a parent with '{typeOfParent.Name}'");

        return null;
    }

    private PropertyInfo GetNodeAttribute(String name)
    {
        return GetType().GetProperties().FirstOrDefault(property => name.IsEqualTo(property.Name))
               ?? throw new InvalidOperationException("Property not found!");
    }

    private static String? GetNodeAttributeGroupName(MemberInfo propertyInfo)
    {
        var nodeAttributeGroup = propertyInfo.GetCustomAttribute<NodeAttributeGroup>();
        return nodeAttributeGroup?.Name;
    }

    private NodeAttribute GetNodeAttributeValue(PropertyInfo propertyInfo)
    {
        return propertyInfo.GetValue(this, null) as NodeAttribute
               ?? throw new InvalidOperationException("An instance of type NodeAttribute expected!");
    }

    private IEnumerable<PropertyInfo> GetGroupedAttributes()
    {
        return GetType().GetProperties()
            .Where(prop => prop.IsDefined(typeof(NodeAttributeGroup), true)).ToArray();
    }

    private Boolean IsAnySet(String name)
    {
        var propertyInfo = GetNodeAttribute(name);
        var attributeGroupName = GetNodeAttributeGroupName(propertyInfo);

        foreach (var attribute in GetGroupedAttributes())
        {
            var currentPropertyInfo = GetNodeAttribute(attribute.Name);
            String? currentGroupName = GetNodeAttributeGroupName(currentPropertyInfo);
            if (currentGroupName.IsEqualTo(attributeGroupName))
                if (GetNodeAttributeValue(currentPropertyInfo).IsSet)
                    return true;
        }

        return false;
    }

    public virtual void SetAttribute(String name, String? value)
    {
        var attribute = GetAttribute(name);

        if (IsAnySet(name))
            throw new InvalidOperationException($"An attribute in this group is already set! {name}.IsSet ");

        attribute.Set(value);
    }

    public virtual void SetAttribute(String name, String[] values)
    {
        if (!values.Any())
            throw new InvalidOperationException("Expecting at least one attribute value!");

        var attribute = GetAttribute(name);

        if (IsAnySet(name))
            throw new InvalidOperationException("An attribute in this group is already set!");

        attribute.Set(values);
    }
}