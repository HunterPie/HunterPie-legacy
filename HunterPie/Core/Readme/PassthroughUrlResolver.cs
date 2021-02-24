namespace HunterPie.Core.Readme
{
    public class PassthroughUrlResolver : IUrlResolver
    {
        public string Resolve(string url) => url;
    }
}
