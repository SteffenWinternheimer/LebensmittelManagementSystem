namespace LMS.Services;

public interface IProductService
{
    public void Add(Product item);
    public IEnumerable<Product> GetAll();
    public IEnumerable<Product> GetAllItems();
    public void Delete(Product item);
}