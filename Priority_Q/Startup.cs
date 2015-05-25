using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Priority_Q.Startup))]
namespace Priority_Q
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
