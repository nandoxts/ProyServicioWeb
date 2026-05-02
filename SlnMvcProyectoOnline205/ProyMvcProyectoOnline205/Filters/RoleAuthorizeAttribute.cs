using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProyMvcProyectoOnline205.Filters
{
    /// <summary>
    /// Roles del sistema (espejo del IdRol en BD).
    /// </summary>
    public static class Roles
    {
        public const int Admin    = 1;
        public const int Vendedor = 2;
        public const int Cliente  = 3;

        public static readonly int[] Staff       = { Admin, Vendedor };
        public static readonly int[] Todos       = { Admin, Vendedor, Cliente };
    }

    /// <summary>
    /// Restringe el acceso a una acción/controlador a los roles indicados.
    /// - Si no hay sesión → /Autenticacion/Login (con returnUrl).
    /// - Si la sesión tiene un rol no permitido → su "home" (Dashboard staff / Home cliente).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly int[] _rolesPermitidos;

        public RoleAuthorizeAttribute(params int[] roles)
        {
            _rolesPermitidos = roles ?? Array.Empty<int>();
        }

        public void OnAuthorization(AuthorizationFilterContext ctx)
        {
            var session = ctx.HttpContext.Session;
            var rol     = session.GetInt32("IdRol");

            // 1) No logueado → Login (recordando a dónde quería ir)
            if (rol == null)
            {
                var returnUrl = ctx.HttpContext.Request.Path + ctx.HttpContext.Request.QueryString;
                ctx.Result = new RedirectToActionResult(
                    "Login", "Autenticacion",
                    new { returnUrl });
                return;
            }

            // 2) Sesión corrupta (rol desconocido) → limpiar y forzar relogin
            if (!EsRolValido(rol.Value))
            {
                session.Clear();
                ctx.Result = new RedirectToActionResult("Login", "Autenticacion", null);
                return;
            }

            // 3) Logueado pero con rol incorrecto para esta acción → su home
            if (_rolesPermitidos.Length > 0 && !_rolesPermitidos.Contains(rol.Value))
            {
                ctx.Result = RedirigirAHomePorRol(rol.Value);
                return;
            }
        }

        public static bool EsRolValido(int rol) =>
            rol == Roles.Admin || rol == Roles.Vendedor || rol == Roles.Cliente;

        public static IActionResult RedirigirAHomePorRol(int rol) => rol switch
        {
            Roles.Admin    => new RedirectToActionResult("Index", "Dashboard", null),
            Roles.Vendedor => new RedirectToActionResult("Index", "Dashboard", null),
            Roles.Cliente  => new RedirectToActionResult("Index", "Home", null),
            _              => new RedirectToActionResult("Login", "Autenticacion", null)
        };
    }

    /// <summary>
    /// Permite acceso solo a usuarios SIN sesión iniciada.
    /// Útil para Login y Registro: si ya estás logueado, te manda a tu home.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireAnonymousAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext ctx)
        {
            var rol = ctx.HttpContext.Session.GetInt32("IdRol");
            if (rol == null) return; // anónimo: pasa
            if (!RoleAuthorizeAttribute.EsRolValido(rol.Value))
            {
                // Sesión basura: límpiala y deja entrar a Login.
                ctx.HttpContext.Session.Clear();
                return;
            }
            ctx.Result = RoleAuthorizeAttribute.RedirigirAHomePorRol(rol.Value);
        }
    }

    /// <summary>
    /// Bloquea explícitamente los roles indicados (los demás pasan, incluido anónimo).
    /// Útil para zonas de tienda (Carrito, Catálogo) que no son para staff.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class BlockRolesAttribute : Attribute, IAuthorizationFilter
    {
        private readonly int[] _rolesBloqueados;

        public BlockRolesAttribute(params int[] rolesBloqueados)
        {
            _rolesBloqueados = rolesBloqueados ?? Array.Empty<int>();
        }

        public void OnAuthorization(AuthorizationFilterContext ctx)
        {
            var rol = ctx.HttpContext.Session.GetInt32("IdRol");
            if (rol != null && _rolesBloqueados.Contains(rol.Value))
            {
                ctx.Result = RoleAuthorizeAttribute.RedirigirAHomePorRol(rol.Value);
            }
        }
    }
}
