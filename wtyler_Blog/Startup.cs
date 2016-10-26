using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(wtyler_Blog.Startup))]
namespace wtyler_Blog
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
