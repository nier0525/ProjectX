using System;
using System.IO;
using System.Text.Json;

public static class JsonHelper
{
    public static void WriteTo<T>(T value, string fileName)
    {
        var stream = new FileStream(fileName, FileMode.Create);
        using (var writer = new StreamWriter(stream))
            writer.Write(JsonSerializer.Serialize(value));
    }

    public static T ReadFrom<T>(string fileName)
    {
        using (var reader = new StreamReader(fileName))
            return JsonSerializer.Deserialize<T>(reader.ReadToEnd());
    }
}
