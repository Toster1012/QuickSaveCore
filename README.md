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



\## Quick Start

using QuickSave.Core;

// Настройка конфигурации (можно хранить в одном месте проекта)
var config = new QuickSaveConfiguration
{
Paths = new Dictionary<string, string>
{
\["player"] = "saves/player.dat"
},
UseGzipCompression = true,           // включить сжатие (опционально)
CreateDirectoryIfNotExist = true     // автоматически создать папку
};

// Сохранение данных
var player = new Player { Name = "Alex", Level = 42, Score = 1337 };
await QuickSaveCore.SaveAsync("player", player, config);

// Загрузка данных
Player loadedPlayer = await QuickSaveCore.LoadAsync<Player>("player", config);

