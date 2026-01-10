# QuickSave

A lightweight and fast C# library for saving and loading data with **MessagePack** serialization, optional **GZip** compression, and support for custom type converters.

[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0+-blue.svg)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- Extremely compact and fast serialization using **MessagePack**
- Optional **GZip** compression for smaller file sizes
- Full support for **custom type converters** (instructions) — perfect for complex types like `Vector3`, custom structs/classes, etc.
- Fully **asynchronous** API (`SaveAsync` / `LoadAsync`)
- Flexible configuration: file paths by keys, automatic directory creation, custom serialization options
- Clean, simple, and strongly-typed API

## Quick Start

Basic usage:

````csharp
using QS.Core;

// Basic configuration (you can store it in one place in your project)
var config = new Configuration
{
    UseGzipCompression = true,           // optional: enable compression
    CreateDirectoryIfNotExist = true     // automatically create folder if needed
};

config.AddPath("person", "saves/player.dat");

// Save data
var person = new Person { Name = "Alex", Age = 42 };
await QuickSave.SaveAsync("person", person, config);

// Load data
Person loadedPlayer = await QuickSave.LoadAsync<Person>("person", config);
````
Working with custom types using CustomTypeConverter
QuickSave allows you to save/load any type by converting it to a simpler serializable type (string, array, number, etc.).

Converting a Person Class to a String
Class Definition

````csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}
````

Converter Definition

````csharp
public class PersonToStringConverter : CustomTypeConverter<Person, string>
{
    public override Person ReadObject(string existingValue)
    {
        // Convert string → Person
        if (string.IsNullOrEmpty(existingValue))
            throw new InvalidOperationException("Invalid person string");

        var parts = existingValue.Split(':');
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

    public override string WriteObject(Person? value)
    {
        return $"{value.Name}:{value.Age}";
    }
}
````

Usage

````csharp
using QS.Convert;
using QS.Core;
using QS.Serialization;


// Basic configuration (you can store it in one place in your project)
var config = new Configuration
{
    UseGzipCompression = true,           // optional: enable compression
    CreateDirectoryIfNotExist = true     // automatically create folder if needed
};

var options = new SerializeOption()
{
    Formatter = new MessagePackFormatter()
};

// Register converter once (e.g. at app startup)
options.AddCustomConverter(new PersonToStringConverter());

config.AddPath("person", "saves/player.dat");

QuickSave.Option = options;

// Save & Load work automatically
var person = new Person { Name = "Maria", Age = 28 };
await QuickSave.SaveAsync("person", person, config);

Person loadedPerson = await QuickSave.LoadAsync<Person>("person", config);
// Result: loadedPerson.Name == "Maria", loadedPerson.Age == 28
````
