using LMS.Logic;
using Microsoft.AspNetCore.Components;

namespace LMS
{
    public partial class Statistics : ComponentBase
    {
        public const string TOTAL_SCANNED_PRODUCTS = "TotalScannedProducts";
        public const string TOTAL_SCANNED_PRODUCTS_SHOPPING_LIST = "TotalScannedProductsShoppingList";
        public const string TOTAL_SCANNED_PRODUCTS_FOOD_STOCK = "TotalScannedProductsFoodStock";
        public static int TotalScannedProducts { get; set; }
        public static int TotalScannedProductsShoppingList { get; set; }
        public static int TotalScannedProductsFoodStock { get; set; }

        public static Dictionary<string, int> statisticsDic = new Dictionary<string, int>();

        internal static void IncrementTotalScannedProducts()
        {
            TotalScannedProducts++;
            statisticsDic[TOTAL_SCANNED_PRODUCTS] = TotalScannedProducts;
            FileHandler.SaveStatistics(statisticsDic);
        }

        internal static void IncrementTotalScannedProductsShoppingList()
        {
            TotalScannedProductsShoppingList++;
            statisticsDic[TOTAL_SCANNED_PRODUCTS_SHOPPING_LIST] = TotalScannedProductsShoppingList;
            FileHandler.SaveStatistics(statisticsDic);
        }

        internal static void IncrementTotalScannedProductsFoodStock()
        {
            TotalScannedProductsFoodStock++;
            statisticsDic[TOTAL_SCANNED_PRODUCTS_FOOD_STOCK] = TotalScannedProductsFoodStock;
            FileHandler.SaveStatistics(statisticsDic);
        }

        public static void LoadStatistics()
        {
            statisticsDic = FileHandler.LoadStatistics();
            if (statisticsDic == null)
            {
                statisticsDic = new Dictionary<string, int>();
            }
            TotalScannedProducts = statisticsDic.ContainsKey(TOTAL_SCANNED_PRODUCTS) ? statisticsDic[TOTAL_SCANNED_PRODUCTS] : 0;
            TotalScannedProductsShoppingList = statisticsDic.ContainsKey(TOTAL_SCANNED_PRODUCTS_SHOPPING_LIST) ? statisticsDic[TOTAL_SCANNED_PRODUCTS_SHOPPING_LIST] : 0;
            TotalScannedProductsFoodStock = statisticsDic.ContainsKey(TOTAL_SCANNED_PRODUCTS_FOOD_STOCK) ? statisticsDic[TOTAL_SCANNED_PRODUCTS_FOOD_STOCK] : 0;
        }
    }

}