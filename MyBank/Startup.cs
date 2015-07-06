using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MyBank.Startup))]
namespace MyBank
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
