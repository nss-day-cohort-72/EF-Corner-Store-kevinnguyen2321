namespace CornerStore.Models.DTOs;

public class OrderDTO
{
 public int Id {get;set;}
 public int CashierId {get;set;}
 public CashierDTO Cashier {get;set;}
 public DateTime? PaidOnDate {get;set;}
 public List<OrderProductDTO> OrderProducts {get;set;} = new List<OrderProductDTO>();
  public decimal Total
    {
        get
        {
            if (OrderProducts == null || OrderProducts.Count == 0)
            {
                return 0m; 
            }

            decimal total = 0m;
            foreach (OrderProductDTO op in OrderProducts)
            {
                total += op.Product.Price * op.Quantity;
            }
            return total;
        }
    }

  
}