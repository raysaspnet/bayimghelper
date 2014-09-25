using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BayImageHelper.Startup))]
namespace BayImageHelper
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
