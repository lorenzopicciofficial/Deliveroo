using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DeliverooApp.Filters;

public class OnlyAdminAttribute:ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ISession session=context.HttpContext.Session;
        string utente=session.GetString("user");
        if (utente == "admin")
        {
            //ok
            //la richiesta viene passata al controller
        }
        else
        {
            context.Result = new BadRequestObjectResult("Questa pagina è riservata all'amministratore");
        }
    }
}