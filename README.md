# CBAPI-OperationalReportsClientApp

## How to instructions.

This sample application will allow you to use the Cloudbeds APIs to:
1.	Query reservations data
<br>
2. Produce a report showing check-ins/check-outs/stayovers for 60 (or more if desired) days
<br>

##To set up the application you will need to:
1.	Get Cloudbeds APIs access and access tokens.   These should be placed into a “Cloudbeds_AppConfig.xml” file (See “_ExampleSecrets” folder)
<br>
2.	Have a placeholder “Cloudbeds_UserAccessTokens.xml” file.  (This will get overwritten when you authenticate to the application)
<br>
3.	Start the application running
<br>
4.	Go to the “Setup screen” and….
<br>
4a. Set the “App Config” and “User Access Tokens” paths to point ti the XML files above
<br>
4b. Click on the “Start Auth Bootstrap” button to get a URL for application authentication.  Copy paste this into your internet browser and log in to your Cloudbeds property.  Agree to the access request.
<br>

NOTE: The browser will now be navigated to a MISSING PAGE (this is OK).  You just need the information IN the URL
<br>
4c. Copy paste the URL you were navigated into back into this application in the textbox next to the “Finish Auth Bootstrap” button.
<br>
4d. Click the “Finish Auth Bootstrap” button.
<br>

### RESULT: You will not have Cloudbeds API access tokens stored on your local machine.  
These are refreshed as needed and are stored in the “Cloudbeds_UserAccessTokens.xml” file.  

5.	Go to the “Operational Report” screen.  This will query the Cloudbeds API and download current and future reservations data and produce the report you wee
