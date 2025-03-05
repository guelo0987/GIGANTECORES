namespace GIGANTECORE.DTO;

public class RolePermissionDTO
{
    public string Role { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public bool CanCreate { get; set; }
    public bool CanRead { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
}