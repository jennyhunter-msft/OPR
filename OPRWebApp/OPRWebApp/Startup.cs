using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OPRWebApp.Startup))]
namespace OPRWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
