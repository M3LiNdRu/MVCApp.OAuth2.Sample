using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using MVCApp.OAuth2.Sample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace MVCApp.OAuth2.Sample.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> IndexAsync(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).
                AuthorizeAsync(cancellationToken);

            if (result.Credential != null)
            {
                //Ja estem autoritzats i podem cridar el servei de google
                var service = new Google.Apis.Oauth2.v2.Oauth2Service(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "Authentication API Sample",
                });

                UserinfoResource.GetRequest userInfo = service.Userinfo.Get();
                Google.Apis.Oauth2.v2.Data.Userinfoplus res = userInfo.Execute();

                /**
                 * A la variable res tenim tota la informació de l'usuari
                 */

                return View("UserInfo", res);
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }
        }
    }
}