
using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models;

public class Order
{
 public int Id {get;set;}
 [Required]
 public int CashierId {get;set;}
 public Cashier Cashier {get;set;} 
 public DateTime? PaidOnDate {get;set;}
 public List<OrderProduct> OrderProducts {get;set;} = new List<OrderProduct>();
  public decimal Total
    {
        get
        {
            if (OrderProducts == null || OrderProducts.Count == 0)
            {
                return 0m; 
            }

            decimal total = 0m;
            foreach (OrderProduct op in OrderProducts)
            {
                total += op.Product.Price * op.Quantity;
            }
            return total;
        }
    }

  
}