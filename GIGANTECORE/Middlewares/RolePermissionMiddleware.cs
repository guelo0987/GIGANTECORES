using System.Security.Claims;
using GIGANTECORE.Context;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;

public class RolePermissionMiddleware
{
    private readonly RequestDelegate _next;

    public RolePermissionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
       

        // Excluir rutas específicas
        if (path != null && (
            path.StartsWith("/api/Auth/login") || 
            path.StartsWith("/api/Auth/register") ||
            path.StartsWith("/swagger") ||
            path.StartsWith("/api/diagnostico/get-external-ip")))
        {
            await _next(context);
            return;
        }

        var db = context.RequestServices.GetRequiredService<MyDbContext>();
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        

        if (userRole == null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Acceso denegado: Usuario no autenticado.");
            return;
        }

        // Permitir acceso completo para Administrador
        if (userRole == "Admin")
        {
            Console.WriteLine("Acceso permitido para el rol 'Admin'.");
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        var controllerName = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerName.ToLower();
        Console.WriteLine($"Controller Name: {controllerName}");

        if (controllerName == null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Error en la ruta.");
            return;
        }

        // Verificar permisos usando Include para cargar la relación Role
        var permission = await db.rolepermisos
            .Include(p => p.Role)
            .FirstOrDefaultAsync(p => p.Role.Name == userRole && p.TableName == controllerName);

        if (permission == null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync($"Acceso denegado: No se encontraron permisos para el rol '{userRole}' en la tabla '{controllerName}'.");
            return;
        }

        // Validar permisos según método HTTP
        var isAllowed = context.Request.Method switch
        {
            "GET" => permission.CanRead,
            "POST" => permission.CanCreate,
            "PUT" => permission.CanUpdate,
            "DELETE" => permission.CanDelete,
            _ => false
        };

        if (!isAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync($"Acceso denegado: El rol '{userRole}' no tiene permiso para {context.Request.Method} en la tabla '{controllerName}'.");
            return;
        }

        await _next(context);
    }
}
