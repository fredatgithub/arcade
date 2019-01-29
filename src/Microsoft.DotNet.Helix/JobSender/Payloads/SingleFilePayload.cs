using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Helix.Client
{
    internal class SingleFilePayload : IPayload
    {
        public string Name { get; }
        public byte[] Content { get; }

        public SingleFilePayload(string name, string content)
            : this(name, content, Encoding.UTF8)
        {
        }

        public SingleFilePayload(string name, string content, Encoding encoding)
            : this(name, encoding.GetBytes(content))
        {
        }

        public SingleFilePayload(string name, byte[] content)
        {
            Name = name;
            Content = content;
        }

        public async Task<string> UploadAsync(IBlobContainer payloadContainer, Action<string> log)
        {
            using (var stream = new MemoryStream())
            {
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    using (Stream entryStream = zip.CreateEntry(Name).Open())
                    {
                        await entryStream.WriteAsync(Content, 0, Content.Length);
                    }
                }
                stream.Position = 0;
                Uri zipUri = await payloadContainer.UploadFileAsync(stream, $"{Guid.NewGuid()}.zip");
                return zipUri.AbsoluteUri;
            }
        }

        public Task<Tuple<string, string>> UploadAsync(IBlobContainer payloadContainer, string destination, Action<string> log)
        {
            return Task.FromResult(new Tuple<string, string>(UploadAsync(payloadContainer, log).GetAwaiter().GetResult(), destination));
        }
    }
}
