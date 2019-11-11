using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HR_Leave.Startup))]
namespace HR_Leave
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
