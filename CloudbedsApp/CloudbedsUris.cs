
using System;
using System.Text;
using System.Web;

static partial class CloudbedsUris
{
    //===========================================================================================
    //OAUTH requests....
    //===========================================================================================
    const string TemplateUrl_RequestOAuthAccess =
        "{{iwsServerUrl}}/api/v1.1/oauth?client_id={{iwsClientId}}&redirect_uri={{iwsOAuthResponseUri}}&response_type=code&"
        + "scope="
        + "read:dashboard"
        + "%20read:hotel"
        + "%20read:guest"
        + "%20read:reservation"
        ;
    const string TemplateUrl_RequestOAuthRefreshToken_PostContents =
        "grant_type=refresh_token&client_id={{iwsClientId}}&client_secret={{iwsClientSecret}}&refresh_token={{iwsOAuthRefreshToken}}";


    const string TemplateUrl_RequestOAuthAccessToken =
        "{{iwsServerUrl}}/api/v1.1/access_token";

    const string TemplateUrl_RequestOAuthAccessToken_PostContents =
       "grant_type=authorization_code&client_id={{iwsClientId}}&client_secret={{iwsClientSecret}}&redirect_uri={{iwsOAuthResponseUri}}&code={{iwsOAuthCode}}";

    const string TemplateUrl_RequestOAuthRefreshToken =
        "{{iwsServerUrl}}/api/v1.1/access_token";




    //===========================================================================================
    //API requests
    //===========================================================================================
    const string TemplateUrl_BrowserGuestDetailsUrl = "{{iwsServerUrl}}/connect/{{iwsPropertyId}}#/guests/{{iwsGuestId}}/details";

    const string TemplateUrl_CustomItemToReservation =
        "{{iwsServerUrl}}/api/v1.1/postCustomItem?{{iwsPropertyIdSegment}}";

    const string TemplateUrl_CustomItemToReservation_PostContents =
        "reservationID={{iwsReservationId}}" +
        "&guestID={{iwsGuestId}}" +
        "&referenceID={{iwsReferenceId}}" +
        "{{iwsOrderItems}}" +
        "";

    const string TemplateUrl_PostAdjustmentToReservation =
        "{{iwsServerUrl}}/api/v1.1/postAdjustment?{{iwsPropertyIdSegment}}";

    const string Template_PostAdjustmentToReservation_PostContents =
        "reservationID={{iwsReservationId}}&type={{iwsAdjustmentType}}&amount={{iwsAdjustmentAmount}}&notes={{iwsNotes}}";


    const string TemplateUrl_HotelDashboard = "{{iwsServerUrl}}/api/v1.1/getDashboard?{{iwsPropertyIdSegment}}";
    const string TemplateUrl_HotelDetails= "{{iwsServerUrl}}/api/v1.1/getHotelDetails?{{iwsPropertyIdSegment}}";
    const string TemplateUrl_RequestAuthUserInfo = "{{iwsServerUrl}}/api/v1.1/userinfo?{{iwsPropertyIdSegment}}";
    const string TemplateUrl_GetCurrentGuestsList = "{{iwsServerUrl}}/api/v1.1/getGuestsByStatus?{{iwsPropertyIdSegment}}status=in_house&pageNumber={{iwsPageNumber}}&pageSize={{iwsPageSize}}";
    const string TemplateUrl_GetCheckedInReservationsList = "{{iwsServerUrl}}/api/v1.1/getReservations?{{iwsPropertyIdSegment}}status=checked_in&pageNumber={{iwsPageNumber}}&pageSize={{iwsPageSize}}";
    const string TemplateUrl_GetDateWindowReservationsList_FilterCheckInStatus = "{{iwsServerUrl}}/api/v1.1/getReservations?{{iwsPropertyIdSegment}}status={{iwsReservationStatus}}&checkInFrom={{iwsCheckInFrom}}&checkInTo={{iwsCheckInTo}}&pageNumber={{iwsPageNumber}}&pageSize={{iwsPageSize}}";
    const string TemplateUrl_GetDateWindowReservationsList = "{{iwsServerUrl}}/api/v1.1/getReservations?{{iwsPropertyIdSegment}}checkInFrom={{iwsCheckInFrom}}&checkInTo={{iwsCheckInTo}}&pageNumber={{iwsPageNumber}}&pageSize={{iwsPageSize}}";
    
    
    const string TemplateUrl_ReservationsWithRates_GetDateWindow =
        "{{iwsServerUrl}}/api/v1.1/getReservationsWithRateDetails?{{iwsPropertyIdSegment}}reservationCheckOutFrom={{iwsCheckOutFrom}}&reservationCheckOutTo={{iwsCheckOutTo}}&pageNumber={{iwsPageNumber}}&pageSize={{iwsPageSize}}";


    const string TemplateUrl_ReservationsWithRooms_GetDateWindow =
        "{{iwsServerUrl}}/api/v1.2/getReservations?{{iwsPropertyIdSegment}}checkOutFrom={{iwsCheckOutFrom}}&checkOutTo={{iwsCheckOutTo}}&includeAllRooms=true&pageNumber={{iwsPageNumber}}&pageSize={{iwsPageSize}}";

    /// <summary>
    /// 2023-02-06: Interestingly the URL for showing a reservations details DOES NOT use the ReservationId that is returned in JSON,
    ///             instead it looks up some other (internal?) ID.  We should see if we can get a URL endpoint created that taked in 
    ///             the JSON returned ReservationId and shows the web page
    /// </summary>
    const string TemplateUrl_BrowserReservationDetailsUrl = "{{iwsServerUrl}}/connect/{{iwsPropertyId}}#/reservations";

    //===========================================================================================
    //URL segment that contians the property id (which is often optional)
    //===========================================================================================
    const string Template_UrlSegment_PropertyId = "propertyID={{iwsPropertyId}}&";

    /// <summary>
    /// Generate the PropertyId segment
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <returns></returns>
    private static string UrlSegment_PropertyIdOrBlank(ICloudbedsServerInfo cbServerInfo)
    {
        string propertyId = cbServerInfo.PropertyIdOrNull;

        //No property ID, then the segment shoudl be blank
        if (string.IsNullOrEmpty(propertyId))
        {
            return "";
        }

        StringBuilder sb;
        sb = new StringBuilder(Template_UrlSegment_PropertyId);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsPropertyId}}", propertyId);
        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;

    }

    //    const string TemplateUrl_BrowserReservationDetailsUrl = "{{iwsServerUrl}}/connect/{{iwsPropertyId}}#/reservations/{{iwsReservationId}}";


    /// <summary>
    /// Get the current set of reservations
    /// https://hotels.cloudbeds.com/api/docs/#api-Reservation-getReservations
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <returns></returns>
    internal static string GetRoomReservationsWithRates(
        ICloudbedsServerInfo cbServerInfo,
        //string reservationStatus,
        DateTime checkOutStartWindow,
        DateTime checkOutEndWindow,
        int pageNumber,
        int pageSize)
    {
        StringBuilder sb;
        sb = new StringBuilder(TemplateUrl_ReservationsWithRates_GetDateWindow);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);
        sb.Replace("{{iwsCheckOutFrom}}", helper_UrlDateText(checkOutStartWindow));
        sb.Replace("{{iwsCheckOutTo}}", helper_UrlDateText(checkOutEndWindow));

        sb.Replace("{{iwsPageNumber}}", pageNumber.ToString());
        sb.Replace("{{iwsPageSize}}", pageSize.ToString());

        //===================================================================================
        //If there is a Property ID we explicitly have, put it in...
        //===================================================================================
        sb.Replace("{{iwsPropertyIdSegment}}",
            UrlSegment_PropertyIdOrBlank(cbServerInfo));


        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Get the current set of reservations
    /// https://hotels.cloudbeds.com/api/docs/#api-Reservation-getReservations
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <returns></returns>
    internal static string GetRoomReservationsWithRooms(
        ICloudbedsServerInfo cbServerInfo,
        //string reservationStatus,
        DateTime checkOutStartWindow,
        DateTime checkOutEndWindow,
        int pageNumber,
        int pageSize)
    {
        StringBuilder sb;
        sb = new StringBuilder(TemplateUrl_ReservationsWithRooms_GetDateWindow);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);
        sb.Replace("{{iwsCheckOutFrom}}", helper_UrlDateText(checkOutStartWindow));
        sb.Replace("{{iwsCheckOutTo}}", helper_UrlDateText(checkOutEndWindow));

        sb.Replace("{{iwsPageNumber}}", pageNumber.ToString());
        sb.Replace("{{iwsPageSize}}", pageSize.ToString());

        //===================================================================================
        //If there is a Property ID we explicitly have, put it in...
        //===================================================================================
        sb.Replace("{{iwsPropertyIdSegment}}",
            UrlSegment_PropertyIdOrBlank(cbServerInfo));


        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }


    /// <summary>
    /// Called to broker an access token
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <param name="oauthCode"></param>
    /// <returns></returns>
    public static string UriGenerate_BrowserGuestDetailsUrl(ICloudbedsServerInfo serverInfo, CloudbedsHotelDetails hotelDetails, CloudbedsGuest guest)
    {
        var sb = new StringBuilder(TemplateUrl_BrowserGuestDetailsUrl);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", serverInfo.ServerUrl);
        sb.Replace("{{iwsPropertyId}}", hotelDetails.Property_Id);

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Called to broker an access token
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <param name="oauthCode"></param>
    /// <returns></returns>
    public static string UriGenerate_BrowserReservationDetailsUrl(ICloudbedsServerInfo serverInfo, CloudbedsHotelDetails hotelDetails, CloudbedsReservation_v1 reservation)
    {
        var sb = new StringBuilder(TemplateUrl_BrowserReservationDetailsUrl);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", serverInfo.ServerUrl);
        sb.Replace("{{iwsPropertyId}}", hotelDetails.Property_Id);
        sb.Replace("{{iwsReservationId}}", reservation.Reservation_Id);
        sb.Replace("{{iwsGuestId}}", reservation.Guest_Id);

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Get the Dashboard for the hotel
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <returns></returns>
    internal static string UriGenerate_RequestDashboard(ICloudbedsServerInfo cbServerInfo)
    {
        var sb = new StringBuilder(TemplateUrl_HotelDashboard);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);

        //===================================================================================
        //If there is a Property ID we explicitly have, put it in...
        //===================================================================================
        sb.Replace("{{iwsPropertyIdSegment}}",
            UrlSegment_PropertyIdOrBlank(cbServerInfo));

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Get the Dashboard for the hotel
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <returns></returns>
    internal static string UriGenerate_RequestHotelDetails(ICloudbedsServerInfo cbServerInfo)
    {
        var sb = new StringBuilder(TemplateUrl_HotelDetails);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);

        //===================================================================================
        //If there is a Property ID we explicitly have, put it in...
        //===================================================================================
        sb.Replace("{{iwsPropertyIdSegment}}",
            UrlSegment_PropertyIdOrBlank(cbServerInfo));


        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Get the current set of guests
    /// https://hotels.cloudbeds.com/api/docs/#api-Guest-getGuestsByStatus
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <returns></returns>
    internal static string UriGenerate_GetCurrentGuestsList(
        ICloudbedsServerInfo cbServerInfo,
        int pageNumber,
        int pageSize)
    {
        var sb = new StringBuilder(TemplateUrl_GetCurrentGuestsList);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);
        sb.Replace("{{iwsPageNumber}}", pageNumber.ToString());
        sb.Replace("{{iwsPageSize}}", pageSize.ToString());


        //===================================================================================
        //If there is a Property ID we explicitly have, put it in...
        //===================================================================================
        sb.Replace("{{iwsPropertyIdSegment}}",
            UrlSegment_PropertyIdOrBlank(cbServerInfo));

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Get the current set of reservations
    /// https://hotels.cloudbeds.com/api/docs/#api-Reservation-getReservations
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <returns></returns>
    internal static string UriGenerate_GetCheckedInReservationsList(
        ICloudbedsServerInfo cbServerInfo,
        int pageNumber,
        int pageSize)
    {
        var sb = new StringBuilder(TemplateUrl_GetCheckedInReservationsList);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);
        sb.Replace("{{iwsPageNumber}}", pageNumber.ToString());
        sb.Replace("{{iwsPageSize}}", pageSize.ToString());

        //===================================================================================
        //If there is a Property ID we explicitly have, put it in...
        //===================================================================================
        sb.Replace("{{iwsPropertyIdSegment}}",
            UrlSegment_PropertyIdOrBlank(cbServerInfo));

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }


    /// <summary>
    /// Date we can use in a URL
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    private static string helper_UrlDateText(DateTime date)
    {
        return date.Year.ToString() + "-" + date.Month.ToString("00") + "-" + date.Day.ToString("00");
    }



    /// <summary>
    /// Get the current set of reservations
    /// https://hotels.cloudbeds.com/api/docs/#api-Reservation-getReservations
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <returns></returns>
    internal static string GetCheckedInDateWindowReservationsList(
        ICloudbedsServerInfo cbServerInfo,
        string reservationStatus,
        DateTime checkInStartWindow,
        DateTime checkInEndWindow,
        int pageNumber,
        int pageSize)
    {
        StringBuilder sb;

        //Choose whether or not we are filtering by check in status
        if(string.IsNullOrWhiteSpace(reservationStatus))
        {
            sb = new StringBuilder(TemplateUrl_GetDateWindowReservationsList);
        }
        else
        {
            sb = new StringBuilder(TemplateUrl_GetDateWindowReservationsList_FilterCheckInStatus);
            sb.Replace("{{iwsReservationStatus}}", reservationStatus);
        }

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);
        sb.Replace("{{iwsCheckInFrom}}", helper_UrlDateText(checkInStartWindow));
        sb.Replace("{{iwsCheckInTo}}", helper_UrlDateText(checkInEndWindow));

        sb.Replace("{{iwsPageNumber}}", pageNumber.ToString());
        sb.Replace("{{iwsPageSize}}", pageSize.ToString());


        //===================================================================================
        //If there is a Property ID we explicitly have, put it in...
        //===================================================================================
        sb.Replace("{{iwsPropertyIdSegment}}",
            UrlSegment_PropertyIdOrBlank(cbServerInfo));

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Post a custom item to a reservation
    /// https://hotels.cloudbeds.com/api/docs/#api-Item-postCustomItem
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <returns></returns>
    internal static string UriGenerate_PostCustomItemToReservation(
        ICloudbedsServerInfo cbServerInfo)
    {
        var sb = new StringBuilder(TemplateUrl_CustomItemToReservation);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);

        //===================================================================================
        //If there is a Property ID we explicitly have, put it in...
        //===================================================================================
        sb.Replace("{{iwsPropertyIdSegment}}",
            UrlSegment_PropertyIdOrBlank(cbServerInfo));

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Called to generate the post contents for a reservation adjustment (i.e. add fee to a reservation)
    /// https://hotels.cloudbeds.com/api/docs/#api-Item-postCustomItem    
    /// </summary>
    /// <returns></returns>
    public static string UriGenerate_PostCustomItemToReservation_PostContents(
        CloudbedsGuest guest, PosOrderManager posOrderManager)
    {
        var sb = new StringBuilder(TemplateUrl_CustomItemToReservation_PostContents);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsReservationId}}", HttpUtility.UrlEncode(guest.Reservation_Id));
        sb.Replace("{{iwsGuestId}}", HttpUtility.UrlEncode(guest.Guest_Id));
        //Store a unique reference ID; so if the order is submitted multiple times it is not duplicated
        sb.Replace("{{iwsReferenceId}}", HttpUtility.UrlEncode(posOrderManager.UniqueOrderReferenceId.ToString()));

        sb.Replace("{{iwsOrderItems}}", PointOfSaleItemsEncoder.GenerateChargeItemsSegment(posOrderManager));

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Post an adjustment to a reservation
    /// https://hotels.cloudbeds.com/api/docs/#api-Adjustment-postAdjustment
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <returns></returns>
    internal static string UriGenerate_PostAdjustmentToReservation(
        ICloudbedsServerInfo cbServerInfo)
    {
        var sb = new StringBuilder(TemplateUrl_PostAdjustmentToReservation);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);


        //===================================================================================
        //If there is a Property ID we explicitly have, put it in...
        //===================================================================================
        sb.Replace("{{iwsPropertyIdSegment}}",
            UrlSegment_PropertyIdOrBlank(cbServerInfo));

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Called to generate the post contents for a reservation adjustment (i.e. add fee to a reservation)
    /// https://hotels.cloudbeds.com/api/docs/#api-Adjustment-postAdjustment
    /// </summary>
    /// <returns></returns>
    public static string UriGenerate_PostAdjustmentToReservation_PostContents(
        string reservationId, CloudbedsAdjustmentType adjustmentType, decimal amount, string notes)
    {
        //Cannonicalize
        if (string.IsNullOrWhiteSpace(notes))
        {
            notes = "";
        }
        var sb = new StringBuilder(Template_PostAdjustmentToReservation_PostContents);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsReservationId}}", HttpUtility.UrlEncode(reservationId));
        sb.Replace("{{iwsAdjustmentType}}", HttpUtility.UrlEncode(Helper_GenerateCloudbedsAdjustmentType(adjustmentType)));
        sb.Replace("{{iwsAdjustmentAmount}}", amount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        sb.Replace("{{iwsNotes}}", HttpUtility.UrlEncode(notes));

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }



    /// <summary>
    /// Get the user info for the user who has authenticated us
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <returns></returns>
    internal static string UriGenerate_RequestAuthUserInfo(ICloudbedsServerInfo cbServerInfo)
    {
        var sb = new StringBuilder(TemplateUrl_RequestAuthUserInfo);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);

        //===================================================================================
        //If there is a Property ID we explicitly have, put it in...
        //===================================================================================
        sb.Replace("{{iwsPropertyIdSegment}}",
            UrlSegment_PropertyIdOrBlank(cbServerInfo));


        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;

    }


    /// <summary>
    /// Called to broker an access token
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <param name="oauthCode"></param>
    /// <returns></returns>
    public static string UriGenerate_RequestOAuthRefreshToken(CloudbedsAppConfig cbAppConfig)
    {
        var sb = new StringBuilder(TemplateUrl_RequestOAuthRefreshToken);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbAppConfig.CloudbedsServerUrl);

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Called to broker an access token
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <param name="oauthCode"></param>
    /// <returns></returns>
    public static string UriGenerate_RequestOAuthRefreshToken_PostContents(CloudbedsAppConfig cbAppConfig, OAuth_RefreshToken oauthRefreshToken)
    {
        var sb = new StringBuilder(TemplateUrl_RequestOAuthRefreshToken_PostContents);

        //===================================================================
        //Perform the replacements
        //===================================================================
        //        sb.Replace("{{iwsServerUrl}}", cbAppConfig.CloudbedsServerUrl);
        sb.Replace("{{iwsClientId}}", cbAppConfig.CloudbedsAppClientId);
        sb.Replace("{{iwsClientSecret}}", cbAppConfig.CloudbedsAppClientSecret);
        sb.Replace("{{iwsOAuthRefreshToken}}", oauthRefreshToken.TokenValue);

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }



    /// <summary>
    /// Turn the enumeration into text
    /// </summary>
    /// <param name="adjustment"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static string Helper_GenerateCloudbedsAdjustmentType(CloudbedsAdjustmentType adjustment)
    {
        switch (adjustment)
        {
            case CloudbedsAdjustmentType.Rate:
                return "rate";
            case CloudbedsAdjustmentType.Fee:
                return "fee";
            case CloudbedsAdjustmentType.Product:
                return "product";
            case CloudbedsAdjustmentType.Tax:
                return "tax";
            default:
                IwsDiagnostics.Assert(false, "1023-1202: Unknown Cloudbeds Adjustment type");
                throw new Exception("1023-1202: Unknown Cloudbeds Adjustment type");
        }
    }


    /// <summary>
    /// Called to broker an access token
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <param name="oauthCode"></param>
    /// <returns></returns>
    public static string UriGenerate_RequestOAuthAccessToken(CloudbedsAppConfig cbAppConfig)
    {
        var sb = new StringBuilder(TemplateUrl_RequestOAuthAccessToken);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbAppConfig.CloudbedsServerUrl);

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Called to broker an access token
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <param name="oauthCode"></param>
    /// <returns></returns>
    public static string UriGenerate_RequestOAuthAccessToken_PostContents(CloudbedsAppConfig cbAppConfig, OAuth_BootstrapCode oAuthBootstrapCode)
    {
        var sb = new StringBuilder(TemplateUrl_RequestOAuthAccessToken_PostContents);

        //===================================================================
        //Perform the replacements
        //===================================================================
        //        sb.Replace("{{iwsServerUrl}}", cbAppConfig.CloudbedsServerUrl);
        sb.Replace("{{iwsClientId}}", cbAppConfig.CloudbedsAppClientId);
        sb.Replace("{{iwsClientSecret}}", cbAppConfig.CloudbedsAppClientSecret);
        sb.Replace(
            "{{iwsOAuthResponseUri}}",
            HttpUtility.UrlEncode(cbAppConfig.CloudbedsAppOAuthRedirectUri));
        sb.Replace("{{iwsOAuthCode}}", oAuthBootstrapCode.TokenValue);

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }


    /// <summary>
    /// URI to use for requesting an application key
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <returns></returns>
    public static string UriGenerate_RequestOAuthAccess(CloudbedsAppConfig cbAppConfig)
    {
        var sb = new StringBuilder(TemplateUrl_RequestOAuthAccess);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbAppConfig.CloudbedsServerUrl);
        sb.Replace("{{iwsClientId}}", cbAppConfig.CloudbedsAppClientId);
        sb.Replace(
            "{{iwsOAuthResponseUri}}",
            HttpUtility.UrlEncode(cbAppConfig.CloudbedsAppOAuthRedirectUri));

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }


    /// <summary>
    /// Double check that we have taken care of the tokens
    /// </summary>
    /// <param name="text"></param>
    private static void AssertTemplateCompleted(string text)
    {
        if (text.Contains("{{"))
        {
            IwsDiagnostics.Assert(false, "722-204: Template still contains tokens");
        }
    }
}
