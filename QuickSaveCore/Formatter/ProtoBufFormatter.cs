using ProtoBuf;
using System.IO.Compression;
using System.Reflection;

namespace QuickSave
{
    public class ProtoBufFormatter : IFormatter
    {
        public bool Serialize<T>(T data, QuickSaveConfiguration configuration)
        {
            return SerializeAsync(data, configuration).Result;
        }

        public T Deserialize<T>(QuickSaveConfiguration configuration)
        {
            return DeserializeAsync<T>(configuration).Result;
        }

        public async Task<bool> SerializeAsync<T>(T data, QuickSaveConfiguration configuration)
        {
            string? _directory = Path.GetDirectoryName(configuration.Path);
            if (!string.IsNullOrEmpty(_directory) && !Directory.Exists(_directory))
            {
                if (configuration.CreateDirectoryIfNotExist)
                {
                    Directory.CreateDirectory(_directory);
                }
                else
                {
                    throw new Exception("Directory not exist");
                }
            }

            try
            {
                if (!File.Exists(configuration.Path))
                    throw new Exception($"File not foun from path: {configuration.Path}");

                if (TypeIsValid<T>())
                {
                    await using var _fileStream = new FileStream(configuration.Path, FileMode.Open, FileAccess.Write);

                    if (configuration.UseGzipCompression)
                    {
                        await GZipArchiver.CompressAsync(_fileStream, data, configuration.Path);
                    }
                    else
                    {
                        Serializer.Serialize(_fileStream, data);
                        await _fileStream.FlushAsync();
                    }

                }

                return File.Exists(configuration.Path);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize data to {configuration.Path}", ex);
            }
        }

        public async Task<T> DeserializeAsync<T>(QuickSaveConfiguration configuration)
        {
            try
            {
                if (!File.Exists(configuration.Path))
                    throw new Exception($"File not foun from path: {configuration.Path}");

                if (TypeIsValid<T>())
                {
                    await using var _fileStream = new FileStream(configuration.Path, FileMode.Open, FileAccess.Read);

                    if (configuration.UseGzipCompression)
                    {
                        return await GZipArchiver.DecompressAsync<T>(_fileStream, configuration.Path);
                    }
                    else
                    {
                        T _data = Serializer.Deserialize<T>(_fileStream);
                        await _fileStream.FlushAsync();
                        return _data;
                    }
                }

                return default;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize data to {configuration.Path}", ex);
            }
        }


        private bool TypeIsValid<T>()
        {
            Type _type = typeof(T); 
            
            if (!HasProtoContractAttribute(_type))
                throw new MissingAttributeException(typeof(ProtoContractAttribute), "was not found on the file!");

            List<int> _activeTag = new List<int>();

            foreach (FieldInfo field in _type.GetFields(
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.Static))
            {
                if (HasProtoMmemberAttribute(field, out int tag))
                {
                    if (!_activeTag.Contains(tag))
                        _activeTag.Add(tag);
                    else
                        throw new IncorrectTagException(tag);
                }
                else
                {
                    throw new MissingAttributeException(typeof(ProtoMemberAttribute), "was not found on the property!");
                }
            }

            return true;
        }

        private bool HasProtoContractAttribute(Type type)
        {
            var _attributes = type.GetCustomAttributes(false);

            foreach (var attribute in _attributes)
            {
                if (attribute.GetType() == typeof(ProtoContractAttribute))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasProtoMmemberAttribute(FieldInfo type, out int tag)
        {
            var _propertyAttributes = type.GetCustomAttributes(false);

            foreach (var attribute in _propertyAttributes)
            {
                if (Attribute.IsDefined(type, typeof(ProtoMemberAttribute)))
                {
                    tag = ((ProtoMemberAttribute)attribute).Tag;
                    return true;
                }
            }

            tag = -1;
            
            return false;
        }
    }
}
