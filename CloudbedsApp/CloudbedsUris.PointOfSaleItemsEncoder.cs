using System;
using System.Text;
using System.Web;

static partial class CloudbedsUris
{
    private static class PointOfSaleItemsEncoder
    {
        const string TemplateUrl_CustomItemToReservation_PostContents_OrderItemSegment =
        "&items[{{iwsArrayIdx}}][appItemID]={{iwsItemId}}" +
        "&items[{{iwsArrayIdx}}][itemQuantity]=1" +
        "&items[{{iwsArrayIdx}}][itemPrice]={{iwsItemPrice}}" +
        "&items[{{iwsArrayIdx}}][itemName]={{iwsItemName}}" +
        "&items[{{iwsArrayIdx}}][itemCategoryName]={{iwsCategoryName}}" +
        "&items[{{iwsArrayIdx}}][itemNote]={{iwsItemNote}}" +
        "&items[{{iwsArrayIdx}}][itemTaxes][0][taxName]={{iwsItemTaxName}}" +
        "&items[{{iwsArrayIdx}}][itemTaxes][0][taxValue]={{iwsItemTaxValue}}" +
        "";


        /// <summary>
        /// Generate the form encoding for ALL items
        /// </summary>
        /// <param name="posOrderManager"></param>
        /// <returns></returns>
        internal static string GenerateChargeItemsSegment(PosOrderManager posOrderManager)
        {
            var sb = new StringBuilder();
            int idxCount = 0;

            //==================================================================================
            //Add each of the items to be billed
            //==================================================================================
            foreach (var thisOrderItem in posOrderManager.Items)
            {
                sb.Append(helper_GenerateChargeItemsSegment_SingleItem(thisOrderItem, idxCount, posOrderManager.DefaultNoteForLineItems));
                idxCount++;
            }

            //==================================================================================
            //If there is a Gratuity, explicitly add it
            //==================================================================================
            if(posOrderManager.Gratuity > 0)
            {
                //Add the Gratuity as an explicit line item
                var gratuityLineItem = helper_GenerateLineItemForGratuity(posOrderManager);
                if(gratuityLineItem != null)
                {
                    sb.Append(helper_GenerateChargeItemsSegment_SingleItem(gratuityLineItem, idxCount, posOrderManager.DefaultNoteForLineItems));
                    idxCount++; //Increase our items count
                }
                else
                {
                    //We should never hit this...
                    IwsDiagnostics.Assert(false, "1023-755: No gratuity line item generated");
                }

            }

            return sb.ToString();
        }


        /// <summary>
        /// Generate a line item for the GRATUITY
        /// </summary>
        /// <param name="posOrderManager"></param>
        /// <returns></returns>
        private static PosOrderLineItem? helper_GenerateLineItemForGratuity(PosOrderManager posOrderManager)
        {
            var gratuityValue = posOrderManager.Gratuity;
            //No Gratuity? No line item...
            if (gratuityValue <= 0)
            {
                return null;
            }

            return new PosOrderLineItem(
                gratuityValue
                , 0 //No Tax
                , ""
                , "POS-GRATUITY"  //Class ID of this type of charge
                , "GRATUITY"  //Item name
                , "GRATUITY"  //CATEGORY NAME
                , ""    //NO NOTE
                );
        }

        /// <summary>
        /// Generate the Form encoding for a SINGLE order item
        /// </summary>
        /// <param name="posOrderItem"></param>
        /// <param name="idxCount"></param>
        /// <returns></returns>
        private static string helper_GenerateChargeItemsSegment_SingleItem(PosOrderLineItem posOrderItem, int idxCount, string itemNoteIfBlank)
        {
            string itemNote = posOrderItem.Item_Note;
            if (string.IsNullOrEmpty(itemNote))
            {
                itemNote = itemNoteIfBlank;
            }

            var sb = new StringBuilder(TemplateUrl_CustomItemToReservation_PostContents_OrderItemSegment);
            //The items are encoded in an array... write the index
            sb.Replace("{{iwsArrayIdx}}", idxCount.ToString());

            //Replace the basic fields describing the charge item
            sb.Replace("{{iwsItemId}}", helper_UrlEncode(posOrderItem.Item_ClassId));
            sb.Replace("{{iwsItemPrice}}", posOrderItem.ItemChargeAmount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Replace("{{iwsItemName}}", HttpUtility.UrlEncode(posOrderItem.Item_Name));
            sb.Replace("{{iwsCategoryName}}", helper_UrlEncode(posOrderItem.ItemCategory_Name));
            sb.Replace("{{iwsItemNote}}", helper_UrlEncode(itemNote));

            //We currently support a single kind of tax -- we can make this a dynamic array if needed
            sb.Replace("{{iwsItemTaxName}}", helper_UrlEncode(posOrderItem.TaxName));
            sb.Replace("{{iwsItemTaxValue}}", posOrderItem.TaxAmount.ToString(System.Globalization.CultureInfo.InvariantCulture));

            return sb.ToString();
        }


        /// <summary>
        /// Cannonicalize and Url Encode POST URL Encoded form values
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string helper_UrlEncode(string text)
        {
            return HttpUtility.UrlEncode(helper_CannonicalizeBlankString(text));
        }

        /// <summary>
        /// Clean up NULL/blank strings
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string helper_CannonicalizeBlankString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            return text;
        }
    }

}
