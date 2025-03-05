namespace GIGANTECORE.DTO;

public class ProductoUploadDTO
{
    public ProductoDTO Producto { get; set; }
    public IFormFile ImageFile { get; set; }
}