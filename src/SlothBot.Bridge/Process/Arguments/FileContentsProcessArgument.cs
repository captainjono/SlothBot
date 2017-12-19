using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SlothBot.Bridge.Process.Arguments
{
    public class FileContentsProcessArgument : IProcessArgument
    {
        private readonly string _filename;
        private readonly Encoding _encoding;
        public string Name { get; }
        public string Value => GetContents(_filename);

        public FileContentsProcessArgument(string filename, string name, Encoding encoding = null)
        {
            Name = name;
            _filename = filename;
            _encoding = encoding ?? Encoding.UTF8;
        }
        private string GetContents(string filename)
        {
            return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, filename), _encoding);
        }

        public async Task Set(string value)
        {
            using (var fs = new FileStream(Path.Combine(AppContext.BaseDirectory, _filename), FileMode.Truncate, FileAccess.Write))
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                await fs.WriteAsync(bytes, 0, bytes.Length);
            }
        }
    }
}
