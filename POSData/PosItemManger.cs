using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
/// <summary>
/// Manages the items (e.g. menu items) in our Point of Sale
/// </summary>
class PosItemManager
{

    List<PosItem> _posItems = new List<PosItem>();

    public ICollection<PosItem> Items
    {
        get
        {
            return _posItems.AsReadOnly();
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public PosItemManager()
    {
        _posItems.Add(new PosItem((decimal) 10.20, (decimal) 0.90, "", "iws-408076e2-5317-11ed-bdc3-0242ac120002", "Pancakes", "Food"));
        _posItems.Add(new PosItem((decimal)12.00, (decimal)1.30, ""  , "iws-408076e2-5317-11ed-bdc3-0242ac120003", "French Toast", "Food"));
        _posItems.Add(new PosItem((decimal) 9.20, (decimal)0.72, ""  , "iws-720e40cc-5317-11ed-bdc3-0242ac120004", "Bacon", "Food"));
        _posItems.Add(new PosItem((decimal) 4.60, (decimal)0.52, ""  , "iws-8a105e08-5317-11ed-bdc3-0242ac120005", "Orange Juice", "Beverage (non-alcoholic)"));
        _posItems.Add(new PosItem((decimal)7.00, (decimal)0.52, ""   , "iws-8a105e08-5317-11ed-bdc3-0242ac120006", "Mimosa", "Beverage (alcoholic)"));
        _posItems.Add(new PosItem((decimal)7.00, (decimal)0.52, ""   , "iws-8a105e08-5317-11ed-bdc3-0242ac120007", "Beer (Craft)", "Beverage (alcoholic)"));
    }

}
