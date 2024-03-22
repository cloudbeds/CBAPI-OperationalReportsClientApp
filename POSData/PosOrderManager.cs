using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
/// <summary>
/// Manages the all the items in an order
/// </summary>
class PosOrderManager
{
    /// <summary>
    /// Totals for the POS order
    /// </summary>
    public struct CalculatedTotals
    {
        public readonly int NumberItems;
        public readonly decimal TotalItemsPrice;
        public readonly decimal TotalTax;
        public readonly decimal Gratuity;
        public readonly decimal GrandTotal;

        public CalculatedTotals(int count, decimal totalPrices, decimal totalTaxes, decimal gratuityAmount) : this()
        {
            this.NumberItems = count;
            this.TotalItemsPrice = totalPrices;
            this.TotalTax = totalTaxes;
            this.Gratuity = gratuityAmount;
            this.GrandTotal = totalPrices + totalTaxes + gratuityAmount;
        }
    }

    List<PosOrderLineItem> _orderLineItems= new List<PosOrderLineItem>();
    Decimal _gratuityAmount;


    private string _defaultNoteForItems = "";
    //Generte a unique ID for the order
    public readonly Guid UniqueOrderReferenceId = Guid.NewGuid();

    /// <summary>
    /// Default note for line items
    /// </summary>
    public string DefaultNoteForLineItems
    {
        get
        {
            return _defaultNoteForItems;
        }
        set
        {
            _defaultNoteForItems = CannonicalizeString(value);
        }
    }

    public ICollection<PosOrderLineItem> Items
    {
        get
        {
            return _orderLineItems.AsReadOnly();
        }
    }

    public decimal Gratuity
    {
        get
        {
            return _gratuityAmount;
        }
        set
        {
            if(value < 0)
            {
                throw new Exception("1023-720: Gratuity cannot be negative");
            }
            _gratuityAmount = value;
        }
    }


    /// <summary>
    /// Constructor
    /// </summary>
    public PosOrderManager()
    {
    }

    

    /// <summary>
    /// Adds an item into the order
    /// </summary>
    /// <param name="item"></param>
    public void AddItemToOrder(PosItem item)
    {
        //Turn a POS Item, into a specific item in this order...
        _orderLineItems.Add(new PosOrderLineItem(
            item.ItemChargeAmount,
            item.TaxAmount,
            item.TaxName,
            item.Item_ClassId,
            item.Item_Name,
            item.Item_CategoryName,
            ""));
    }

    /// <summary>
    /// Adds an item into the order
    /// </summary>
    /// <param name="item"></param>
    public void AddItemToOrder(decimal itemChargeAmount, decimal taxAmount, string taxName, string itemClassId, string itemName, string itemCategoryName)
    {
        //Turn a POS Item, into a specific item in this order...
        _orderLineItems.Add(new PosOrderLineItem(
            itemChargeAmount,
            taxAmount,
            taxName,
            itemClassId,
            itemName,
            itemCategoryName,
            ""));
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static string CannonicalizeString(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }
        return text;
    }

    /// <summary>
    /// 
    /// </summary>
    internal void ClearOrderItems()
    {
        _orderLineItems.Clear();
    }


    internal CalculatedTotals CalculateTotals()
    {
        decimal runningTotalPrices = 0;
        decimal runningTotalTaxes = 0;

        foreach(var item in _orderLineItems)
        {
            runningTotalPrices += item.ItemChargeAmount;
            runningTotalTaxes += item.TaxAmount;
        }

        var structOut = new CalculatedTotals(_orderLineItems.Count, runningTotalPrices, runningTotalTaxes, _gratuityAmount);
        return structOut;
    }
}
