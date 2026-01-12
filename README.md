# QuickSave

A lightweight and fast C# library for saving and loading data with serialization, optional **GZip** compression, and support for custom type converters.

[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0+-blue.svg)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- Optional **GZip** compression for smaller file sizes
- Full support for **custom type converters** (instructions) - custom structs/classes, etc.
- Fully **asynchronous** API (`SaveAsync` / `LoadAsync`)
- Flexible configuration: file paths by keys, automatic directory creation, custom serialization options
- Clean, simple, and strongly-typed API

## Quick Start

Basic usage:

````csharp
using QS.Core;

// Basic configuration (you can store it in one place in your project)
Configuration _config = new Configuration
{
    UseGzipCompression = true,           // optional: enable compression
    CreateDirectoryIfNotExist = true     // automatically create folder if needed
};

_config.AddPath("person", "D:\\Project\\QuickSaveExample\\QuickSaveExample\\player.bin");

// Save data
Person _person = new Person { Name = "Alex", Age = 42 };
await QuickSave.SaveAsync("person", _person, _config);

// Load data
Person _loadedPerson = await QuickSave.LoadAsync<Person>("person", _config);
Console.WriteLine($"Name: {_loadedPerson.Name}, Age: {_loadedPerson.Age}");
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
Configuration _config = new Configuration
{
    UseGzipCompression = true,           // optional: enable compression
    CreateDirectoryIfNotExist = true     // automatically create folder if needed
};

SerializeOption _option = new SerializeOption()
{
    Formatter = new BinaryFormatter()
};

// Register converter once (e.g. at app startup)
_option.AddCustomConverter(new PersonToStringConverter());

_config.AddPath("person", "saves/player.dat");

QuickSave.Option = _option;

// Save & Load work automatically
Person _person = new Person { Name = "Maria", Age = 28 };
await QuickSave.SaveAsync("person", _person, _config);

Person _loadedPerson = await QuickSave.LoadAsync<Person>("person", _config);
// Result: loadedPerson.Name == "Maria", loadedPerson.Age == 28
Console.WriteLine($"Name: {_loadedPerson.Name}, Age: {_loadedPerson.Age}");
````
