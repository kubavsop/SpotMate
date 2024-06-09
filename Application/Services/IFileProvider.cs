namespace SpotMate.Application.Services;

public interface IFileProvider
{
    Task PutStaticFileAsync(byte[] file, string filename);
    void DeleteStaticFileAsync(string filename);
}