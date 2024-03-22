using System;
using System.Text;

/// <summary>
/// Line item in a point of sale order
/// </summary>
class PosOrderLineItem
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
    public readonly string ItemCategory_Name;
    public readonly string Item_Note;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="itemChargeAmount"></param>
    /// <param name="taxAmount"></param>
    /// <param name="taxName"></param>
    /// <param name="item_classId"></param>
    /// <param name="item_Name"></param>
    /// <param name="item_CategoryName"></param>
    public PosOrderLineItem(decimal itemChargeAmount, decimal taxAmount, string taxName, string item_classId, string item_Name, string item_CategoryName, string itemNote)
    {
        //Create an order item ID if necessary
        if(string.IsNullOrWhiteSpace(item_classId))
        {
            item_classId = "Generated-inline-" + Guid.NewGuid().ToString();
        }


        this.ItemChargeAmount = itemChargeAmount;
        this.TaxAmount = taxAmount;

        this.TaxName = CannonicalizeString(taxName);
        this.Item_ClassId = CannonicalizeString(item_classId);
        this.Item_Name = CannonicalizeString(item_Name);
        this.ItemCategory_Name = CannonicalizeString(item_CategoryName);
        this.Item_Note = CannonicalizeString(itemNote);
    }

    private static string CannonicalizeString(string text)
    {
        if(string.IsNullOrWhiteSpace(text))
        {
            return "";
        }
        return text;
    }

}
