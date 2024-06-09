using Microsoft.Extensions.Options;
using IFileProvider = SpotMate.Application.Services.IFileProvider;

namespace SpotMate.Infrastructure.Services;

public sealed class FileProvider: IFileProvider
{
    private readonly Options.StaticFileOptions _fileOptions;

    public FileProvider(IOptions<Options.StaticFileOptions> fileOptions)
    {
        _fileOptions = fileOptions.Value;
    }

    public async Task PutStaticFileAsync(byte[] file, string filename)
    {
        Directory.CreateDirectory(_fileOptions.Path);
        var path = Path.Combine(_fileOptions.Path, filename);
        
        using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            await fileStream.WriteAsync(file);
        }    
    }

    public void DeleteStaticFileAsync(string filename)
    {
        var path = Path.Combine(_fileOptions.Path, filename);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}