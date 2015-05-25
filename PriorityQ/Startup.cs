using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PriorityQ.Startup))]
namespace PriorityQ
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
