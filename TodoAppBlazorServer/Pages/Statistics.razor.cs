using Microsoft.AspNetCore.Components;

namespace LMS
{
    public partial class Statistics : ComponentBase
    {
        public static int TotalScannedProducts { get; set; }
        public static int TotalScannedProductsShoppingList { get; set; }
        public static int TotalScannedProductsFoodStock { get; set; }

        
    }

}