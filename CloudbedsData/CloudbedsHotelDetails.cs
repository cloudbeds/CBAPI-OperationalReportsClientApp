using System;
using System.Text;

/// <summary>
/// Cloudbeds Hotel Details
/// </summary>
class CloudbedsHotelDetails
{
    public readonly string Property_Id;
    public readonly string Property_Name;

    
    public CloudbedsHotelDetails(
        string propertyId,
        string propertyName 
        )
    {
        this.Property_Id = propertyId;
        this.Property_Name = propertyName;
        
    }

}
