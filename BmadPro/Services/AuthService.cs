namespace BmadPro.Services;

public class AuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool ValidateCredentials(string username, string password)
    {
        var validUsername = _configuration["DemoData:LoginCredentials:Username"];
        var validPassword = _configuration["DemoData:LoginCredentials:Password"];
        return username == validUsername && password == validPassword;
    }
}
