namespace TikTakToe.Tests.BlackBox;

[CollectionDefinition(Name)]
public sealed class BlackBoxCollection : ICollectionFixture<BlackBoxComposeFixture>
{
    public const string Name = "BlackBoxCollection";
}
