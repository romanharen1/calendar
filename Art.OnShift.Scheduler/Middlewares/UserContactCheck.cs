using Art.OnShift.Shared.Interfaces;
using Art.OnShift.Shared.Models;
using System.Security.Claims;


namespace Art.OnShift.Scheduler.Middlewares
{
    public class UserContactCheck
    {
        private readonly RequestDelegate _next;

        public UserContactCheck(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true) // Fix for CS8602  
            {
                // Obtenha o ID do usuário dos claims (como string)  
                var userIdString = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userIdString))
                {
                    // Resolve o serviço de usuário  
                    var userService = context.RequestServices.GetRequiredService<IUserService>();

                    // Tenta buscar o usuário no banco de dados  
                    var user = await userService.GetUserByIdAsync(userIdString);

                    // Se o usuário não existir, registre um novo  
                    if (user == null)
                    {
                        user = new UserModel
                        {
                            Id = userIdString, // Agora usamos o ID como string  
                            Name = context.User?.Claims.FirstOrDefault(c => c.Type == "name")?.Value, // Obtenha o nome dos claims  
                            Email = context.User?.Identity?.Name // Obtenha o e-mail dos claims  
                        };

                        // Salva o novo usuário no banco de dados  
                        await userService.CreateUserAsync(user);
                    }
                }
                else
                {
                    // Lida com o caso de ID inválido  
                    context.Response.Redirect("/Error");
                    return;
                }
            }
            await _next(context);
        }
    }
}
