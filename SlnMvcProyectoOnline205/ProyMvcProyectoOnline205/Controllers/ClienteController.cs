using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyMvcProyectoOnline205.Filters;
using ProyMvcProyectoOnline205.Models;
using System.Text;

namespace ProyMvcProyectoOnline205.Controllers
{
    public class ClienteController : Controller
    {
        private readonly string apiUrl = "http://localhost:5064/api/ClienteApi/";

        private List<Cliente> _clienteList = new List<Cliente>();

        // ============================
        // metodo para traer a los fakiu clientes 
        // ============================
        public async Task<List<Cliente>> TraerClientes()
        {
            using (var http = new HttpClient())
            {
                var respuesta = await http.GetAsync(apiUrl + "GetClientes");
                string json = await respuesta.Content.ReadAsStringAsync();

                _clienteList = JsonConvert.DeserializeObject<List<Cliente>>(json)!;
            }

            return _clienteList;
        }

        // ============================
        // listar atodos los pobres
        // ============================
        [RoleAuthorize(Roles.Admin, Roles.Vendedor)]
        public async Task<IActionResult> IndexCliente()
        {
            return View(await TraerClientes());
        }

        // ============================
        // detallar alos pobres
        // ============================
        [RoleAuthorize(Roles.Admin, Roles.Vendedor)]
        public async Task<IActionResult> DetailsCliente(int id)
        {
            await TraerClientes();

            var buscado = _clienteList.Find(x => x.IdCliente == id);

            return View(buscado);
        }

        // ============================
        // CREATE - GET
        // ============================
        [RoleAuthorize(Roles.Admin, Roles.Vendedor)]
        public IActionResult CreateCliente()
        {
            return View(new Cliente());
        }

        // ============================
        // CREATE - POST
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles.Admin, Roles.Vendedor)]
        public async Task<IActionResult> CreateCliente(Cliente obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var http = new HttpClient())
                    {   
                        string json = JsonConvert.SerializeObject(obj);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var respuesta = await http.PostAsync(apiUrl + "PostCliente", content);

                        string apiMessage = await respuesta.Content.ReadAsStringAsync();
                        TempData["mensaje"] = apiMessage;
                    }

                    return RedirectToAction(nameof(IndexCliente));
                }
            }
            catch
            {
            }

            return View(obj);
        }

        // ============================
        // EDIT - GET
        // ============================
        [RoleAuthorize(Roles.Admin, Roles.Vendedor)]
        public async Task<IActionResult> EditCliente(int id)
        {
            await TraerClientes();
            var buscado = _clienteList.Find(x => x.IdCliente == id);

            return View(buscado);
        }

        // ============================
        // EDIT - POST
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles.Admin, Roles.Vendedor)]
        public async Task<IActionResult> EditCliente(Cliente obj)
        {
            // 1️⃣ TRAER EL CLIENTE COMPLETO ACTUAL DESDE EL API
            Cliente? original;

            using (var http = new HttpClient())
            {
                var response = await http.GetAsync(apiUrl + "GetCliente/" + obj.IdCliente);
                string json = await response.Content.ReadAsStringAsync();

                original = JsonConvert.DeserializeObject<Cliente>(json);
            }

            if (original == null) return NotFound();

            // 2️⃣ MANTENER LOS CAMPOS QUE NO SE EDITAN
            obj.PasswordHash = original.PasswordHash;
            obj.Reestablecer = original.Reestablecer;
            obj.Activo = original.Activo;
            obj.FechaRegistro = original.FechaRegistro;

            // 3️⃣ ENVIAR EL OBJETO COMPLETO AL PUT
            using (var http = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(obj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await http.PutAsync(apiUrl + "PutCliente", content);
            }

            return RedirectToAction("IndexCliente");
        }


        // ============================
        // DELETE (LÓGICO)
        // ============================
        [RoleAuthorize(Roles.Admin, Roles.Vendedor)]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            using (var http = new HttpClient())
            {
                var respuesta = await http.DeleteAsync(apiUrl + "DeleteCliente/" + id);

                TempData["mensaje"] = await respuesta.Content.ReadAsStringAsync();
            }

            return RedirectToAction(nameof(IndexCliente));
        }
        [RoleAuthorize(Roles.Cliente)]
        public async Task<IActionResult> Perfil()
        {
            var idCliente = HttpContext.Session.GetInt32("IdCliente");
            if (idCliente == null)
                return RedirectToAction("Login", "Autenticacion");

            using var http = new HttpClient();
            var resp = await http.GetAsync(apiUrl + "GetCliente/" + idCliente);

            if (!resp.IsSuccessStatusCode)
                return RedirectToAction("Index", "Home");

            var cliente = JsonConvert.DeserializeObject<Cliente>(
                await resp.Content.ReadAsStringAsync()
            );

            return View(cliente);
        }
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles.Cliente)]
        public async Task<IActionResult> Perfil(Cliente model)
        {
            var idCliente = HttpContext.Session.GetInt32("IdCliente");
            if (idCliente == null)
                return RedirectToAction("Login", "Autenticacion");

            // 🔐 Seguridad: forzamos el ID de sesión
            model.IdCliente = idCliente.Value;

            // Traer cliente original
            Cliente original;
            using (var http = new HttpClient())
            {
                var resp = await http.GetAsync(apiUrl + "GetCliente/" + idCliente);
                original = JsonConvert.DeserializeObject<Cliente>(
                    await resp.Content.ReadAsStringAsync()
                )!;
            }

            // Campos que NO se editan
            model.PasswordHash = original.PasswordHash;
            model.Reestablecer = original.Reestablecer;
            model.Activo = original.Activo;
            model.FechaRegistro = original.FechaRegistro;

            using (var http = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await http.PutAsync(apiUrl + "PutCliente", content);
            }

            TempData["mensaje"] = "Perfil actualizado correctamente";

            return RedirectToAction(nameof(Perfil));
        }
        [RoleAuthorize(Roles.Cliente)]
        public async Task<IActionResult> EditarPerfil()
        {
            var idCliente = HttpContext.Session.GetInt32("IdCliente");
            if (idCliente == null)
                return RedirectToAction("Login", "Autenticacion");

            using var http = new HttpClient();
            var resp = await http.GetAsync(apiUrl + "GetCliente/" + idCliente);

            if (!resp.IsSuccessStatusCode)
                return RedirectToAction(nameof(Perfil));

            var cliente = JsonConvert.DeserializeObject<Cliente>(
                await resp.Content.ReadAsStringAsync()
            );

            return View(cliente);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles.Cliente)]
        public async Task<IActionResult> EditarPerfil(Cliente model)
        {
            var idCliente = HttpContext.Session.GetInt32("IdCliente");
            if (idCliente == null)
                return RedirectToAction("Login", "Autenticacion");

            model.IdCliente = idCliente.Value;

            // Traer cliente original
            Cliente original;
            using (var http = new HttpClient())
            {
                var resp = await http.GetAsync(apiUrl + "GetCliente/" + idCliente);
                original = JsonConvert.DeserializeObject<Cliente>(
                    await resp.Content.ReadAsStringAsync()
                )!;
            }

            // Mantener campos críticos
            model.PasswordHash = original.PasswordHash;
            model.Reestablecer = original.Reestablecer;
            model.Activo = original.Activo;
            model.FechaRegistro = original.FechaRegistro;

            using (var http = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await http.PutAsync(apiUrl + "PutCliente", content);
            }

            TempData["mensaje"] = "Perfil actualizado correctamente";
            return RedirectToAction(nameof(Perfil));
        }

        ///
        [RoleAuthorize(Roles.Cliente)]
        public IActionResult CambiarClave()
        {
            var idCliente = HttpContext.Session.GetInt32("IdCliente");
            if (idCliente == null)
                return RedirectToAction("Login", "Autenticacion");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles.Cliente)]
        public async Task<IActionResult> CambiarClave(CambiarClaveViewModel model)
        {
            var idCliente = HttpContext.Session.GetInt32("IdCliente");
            if (idCliente == null)
                return RedirectToAction("Login", "Autenticacion");

            if (model.NuevaClave != model.ConfirmarClave)
            {
                ViewBag.Error = "Las contraseñas no coinciden";
                return View(model);
            }

            using var http = new HttpClient();

            // 1️⃣ Traer cliente actual
            var resp = await http.GetAsync(
                $"http://localhost:5064/api/ClienteApi/GetCliente/{idCliente}"
            );

            if (!resp.IsSuccessStatusCode)
                return RedirectToAction("Perfil");

            var cliente = JsonConvert.DeserializeObject<Cliente>(
                await resp.Content.ReadAsStringAsync()
            );

            // 2️⃣ Verificar clave actual
            if (cliente!.PasswordHash != model.ClaveActual)
            {
                ViewBag.Error = "La contraseña actual es incorrecta";
                return View(model);
            }

            // 3️⃣ Enviar nueva clave al API
            var payload = new
            {
                IdCliente = idCliente,
                NuevaClave = model.NuevaClave
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await http.PostAsync(
                "http://localhost:5064/api/ClienteApi/ReestablecerClave",
                content
            );

            TempData["mensaje"] = "Contraseña actualizada correctamente.";
            return RedirectToAction(nameof(CambiarClave));
        }


    }

}
