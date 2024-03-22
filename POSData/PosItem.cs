using System;
using System.Text;

/// <summary>
/// Line item in a point of sale order
/// </summary>
class PosItem
{
    /// <summary>
    /// How much does it cost?
    /// </summary>
    public readonly decimal ItemChargeAmount;

    /// <summary>
    /// Taxes?
    /// </summary>
    public readonly decimal TaxAmount;
    public readonly string TaxName;
    public readonly string Item_ClassId;
    public readonly string Item_Name;
    public readonly string Item_CategoryName;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="itemChargeAmount"></param>
    /// <param name="taxAmount"></param>
    /// <param name="taxName"></param>
    /// <param name="item_classId"></param>
    /// <param name="item_Name"></param>
    /// <param name="item_CategoryName"></param>
    public PosItem(decimal itemChargeAmount, decimal taxAmount, string taxName, string item_classId, string item_Name, string item_CategoryName)
    {
        //Create an order item ID if necessary
        if(string.IsNullOrWhiteSpace(item_classId))
        {
            item_classId = "Generated-inline-" + Guid.NewGuid().ToString();
        }

        //Cannonicalize
        if(string.IsNullOrWhiteSpace(taxName))
        {
            taxName = "Generic Tax";
        }

        //Cannonicalize
        if (string.IsNullOrWhiteSpace(item_CategoryName))
        {
            item_CategoryName = "No Category";
        }

        this.ItemChargeAmount = itemChargeAmount;
        this.TaxAmount = taxAmount;
        this.TaxName = taxName;
        this.Item_ClassId = item_classId;
        this.Item_Name = item_Name;
        this.Item_CategoryName = item_CategoryName;
    }

}
