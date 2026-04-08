namespace GymAdmin.Infrastructure.Paths;

public interface IPathResolver
{
    string Resolve(string pathWithTokens);
}