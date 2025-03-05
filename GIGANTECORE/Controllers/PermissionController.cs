using GIGANTECORE.Context;
using GIGANTECORE.DTO;
using GIGANTECORE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GIGANTECORE.Controllers;



[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireAdministratorRole")]
public class PermissionController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly ILogger<PermissionController> _logger;

    public PermissionController(ILogger<PermissionController> logger, MyDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    
    [HttpGet]
    public IActionResult GetPermissions()
    {
        var permissions = _db.RolePermisos
            .Include(p => p.Role)
            .ToList();
        return Ok(permissions);
    }
    
    [HttpPost]
    public IActionResult AddOrUpdatePermission([FromBody] RolePermissionDTO permission)
    {
        var role = _db.Roles.FirstOrDefault(r => r.Name == permission.Role);
        if (role == null)
        {
            return NotFound($"Role '{permission.Role}' not found");
        }

        var existingPermission = _db.RolePermisos
            .Include(p => p.Role)
            .FirstOrDefault(p => p.Role.Name == permission.Role && p.TableName == permission.TableName);

        if (existingPermission != null)
        {
            existingPermission.CanCreate = permission.CanCreate;
            existingPermission.CanRead = permission.CanRead;
            existingPermission.CanUpdate = permission.CanUpdate;
            existingPermission.CanDelete = permission.CanDelete;
        }
        else
        {
            var newPermission = new RolePermiso
            {
                RoleId = role.IdRol,
                TableName = permission.TableName,
                CanCreate = permission.CanCreate,
                CanDelete = permission.CanDelete,
                CanRead = permission.CanRead,
                CanUpdate = permission.CanUpdate
            };
            
            _db.RolePermisos.Add(newPermission);
        }

        _db.SaveChanges();
        return Ok("Permisos actualizados correctamente.");
    }
    
    
    
    
    
    
    
    
    
    
    
}