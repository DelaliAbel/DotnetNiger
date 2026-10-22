using DotnetNiger.Web.Services.Ui;

namespace DotnetNiger.Tests.Web;

/// <summary>
/// Vérifie le choix d'icône d'un lien de profil : le nom retourné doit toujours être l'un
/// de ceux que <c>DnIcon</c> dessine, sinon le lien disparaîtrait silencieusement.
/// </summary>
public sealed class IconesDeLienTests
{
    [Theory]
    [InlineData("https://github.com/dotnetniger")]
    [InlineData("https://www.github.com/utilisateur")]
    [InlineData("https://gist.github.com/utilisateur")]
    [InlineData("HTTPS://GITHUB.COM/Utilisateur")]
    public void Pour_ReconnaitGithub(string url)
    {
        Assert.Equal("github", IconesDeLien.Pour(url));
    }

    [Theory]
    [InlineData("https://www.linkedin.com/in/utilisateur")]
    [InlineData("https://linkedin.com/company/dotnetniger")]
    public void Pour_ReconnaitLinkedin(string url)
    {
        Assert.Equal("linkedin", IconesDeLien.Pour(url));
    }

    [Theory]
    [InlineData("https://portfolio.exemple.test")]
    [InlineData("https://mon-blog.dev/articles")]
    [InlineData("https://exemple.test/github-clone")]
    public void Pour_RetombeSurLIconeDeSite(string url)
    {
        Assert.Equal(IconesDeLien.Site, IconesDeLien.Pour(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("pas une url")]
    [InlineData("/chemin/relatif")]
    public void Pour_RetombeSurLIconeDeSite_QuandUrlInvalide(string? url)
    {
        Assert.Equal(IconesDeLien.Site, IconesDeLien.Pour(url));
    }

    [Fact]
    public void LIconeParDefaut_EstLeGlobe()
    {
        Assert.Equal("globe", IconesDeLien.Site);
    }
}
