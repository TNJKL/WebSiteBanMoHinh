//using Microsoft.Extensions.Localization;
//using System.Reflection;

//namespace WebSiteBanMoHinh.Resources
//{
//    public class LanguageService
//    {
//        private readonly IStringLocalizer _stringLocalizer;
//        public LanguageService(IStringLocalizer stringLocalizer , IStringLocalizerFactory factory)
//        {
//            var type = typeof(SharedResource);
//            var assmblyname = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
//            _stringLocalizer = factory.Create("SharedResource", assmblyname.Name);
//            _stringLocalizer = stringLocalizer;
//        }
//        public LocalizedString GetLocalizedHTML(string key)
//        {
//            return _stringLocalizer[key];
//        }
//    }
//}
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace WebSiteBanMoHinh.Resources
{
    public class LanguageService
    {
        private readonly IStringLocalizer _stringLocalizer;

        public LanguageService(IStringLocalizerFactory factory)
        {
            var type = typeof(SharedResource);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
            _stringLocalizer = factory.Create("SharedResource", assemblyName.Name);
        }

        public LocalizedString GetLocalizedHTML(string key)
        {
            return _stringLocalizer[key];
        }
    }
}

