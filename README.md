# hvc.DataStructures
Common Data Structures for data modelling

**hvc.DataStructures** is a .NET 6 class library used by various hvc code generators such as [hvc.DynamoDBReport](https://github.com/keturk/hvc.DynamoDBReport).

Simply, **hvc.DataStructures** allows you to create a model graph for starting from a base node to a leaf node. The model graph is a graph structure where each node is a class. The model graph is used to generate code for various purposes.

Since this library provides methods that may be useful for other projects, it is published as a separate NuGet package.

**hvc.DataStructures** uses [hvc.Extensions](https://github.com/keturk/hvc.Extensions) nuget package.