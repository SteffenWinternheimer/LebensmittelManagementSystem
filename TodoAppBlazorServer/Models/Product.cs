using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class Product
{
    public string Product_Name { get; set; }
    public long Barcode { get; set; }
    public DateOnly ExpiryDate { get; set; }

    public Product(string product_Name)
    {
        Product_Name = product_Name;
    }

    public Product(string product_Name, long barcode, DateOnly expiryDate)
    {
        Product_Name = product_Name;
        Barcode = barcode;
        ExpiryDate = expiryDate;
    }

    public Product()
    {
    }
}
