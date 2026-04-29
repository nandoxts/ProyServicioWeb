using Microsoft.AspNetCore.Mvc;

namespace ProyMvcProyectoOnline205.ViewComponents
{
    /// <summary>
    /// Renderiza el badge del link "Notificaciones" en el sidebar admin.
    /// Lee primero la cuenta desde HttpContext.Items["AdminNotiCount"] (si la
    /// fija un middleware/filtro), si no, cae a ViewBag.NotificacionesNoLeidas
    /// usado por algunos controllers actuales. Si no hay valor o es 0, no
    /// renderiza nada (badge oculto).
    /// </summary>
    public class AdminNotificationBadgeViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            int count = 0;

            if (HttpContext.Items.TryGetValue("AdminNotiCount", out var raw) && raw is int v1)
            {
                count = v1;
            }
            else if (ViewBag is null) // ViewComponents no exponen ViewBag del controller; no-op
            {
                count = 0;
            }
            else
            {
                // Compat: algunos controllers ponen el valor en TempData["AdminNotiCount"]
                if (TempData != null && TempData.TryGetValue("AdminNotiCount", out var t) && t is int v2)
                {
                    count = v2;
                }
            }

            return View(count);
        }
    }
}
