\# QuickSave



A lightweight and fast C# library for saving and loading data with \*\*MessagePack\*\* serialization, optional \*\*GZip\*\* compression, and support for custom type converters.



\[!\[.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0+-blue.svg)](https://dotnet.microsoft.com/download)

\[!\[License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)



\## Features



\- Extremely compact and fast serialization using \*\*MessagePack\*\*

\- Optional \*\*GZip\*\* compression for smaller file sizes

\- Full support for \*\*custom type converters\*\* (instructions) — perfect for complex types like `Vector3`, custom structs/classes, etc.

\- Fully \*\*asynchronous\*\* API (`SaveAsync` / `LoadAsync`)

\- Flexible configuration: file paths by keys, automatic directory creation, custom serialization options

\- Clean, simple, and strongly-typed API



## Quick Start

Here's the simplest way to get started with QuickSave:

```csharp
using QuickSave.Core;

// Basic configuration (you can store it in one place in your project)
var config = new QuickSaveConfiguration
{
    Paths = new Dictionary<string, string>
    {
        ["player"] = "saves/player.dat"
    },
    UseGzipCompression = true,           // optional: enable compression
    CreateDirectoryIfNotExist = true     // automatically create folder if needed
};
csharp```

### Working with custom types using CustomTypeConverter

QuickSave allows you to save/load any type by converting it to a simpler serializable type (string, array, number, etc.).

**Example:** Let's say you have a `Person` class, but you want to save it as a simple string "Name:Age".

#### 1. Your original class
```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class PersonToStringConverter : CustomTypeConverter<Person, string>
{
    public override string ToSerializable(Person value)
    {
        // Convert Person → string
        return $"{value.Name}:{value.Age}";
    }

    public override Person FromSerializable(string value)
    {
        // Convert string → Person
        if (string.IsNullOrEmpty(value))
            throw new InvalidOperationException("Invalid person string");

        var parts = value.Split(':');
        if (parts.Length != 2)
            throw new InvalidOperationException("Invalid format: expected Name:Age");

        if (!int.TryParse(parts[1], out int age))
            throw new InvalidOperationException("Age must be a number");

        return new Person
        {
            Name = parts[0],
            Age = age
        };
    }
}


// Register instruction (do this once, e.g. at app startup)
config.AddInstruction(new PersonToStringConverter());

// Now save/load works automatically
var person = new Person { Name = "Maria", Age = 28 };
await QuickSaveCore.SaveAsync("person", person, config);

Person loadedPerson = await QuickSaveCore.LoadAsync<Person>("person", config);

// Result: loadedPerson.Name == "Maria", loadedPerson.Age == 28