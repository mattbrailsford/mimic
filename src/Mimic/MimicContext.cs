using Mimic.Services;
using Newtonsoft.Json.Linq;

namespace Mimic
{
    public class MimicContext
    {
        public static MimicContext Current { get; internal set;  }

        internal MimicContext()
        { }

        public MimicServicesContext Services { get; internal set; }

        public string BaseUrl { get; internal set; }

        public string BasePath { get; internal set; }

        public JObject Sitemap { get; internal set; }

        public JObject CurrentPage { get; internal set; }

    }

    public class MimicServicesContext
    {
        internal MimicServicesContext()
        { }

        public MimicService MimicService { get; internal set; }
    }
}
