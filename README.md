# CBAPI-OperationalReportsClientApp

## How to instructions.

This sample application will allow you to use the Cloudbeds APIs to:
<br>
1. Query reservations data
2. Produce a report showing check-ins/check-outs/stayovers for 60 (or more if desired) days
<br>

## Option 1: Set up the application using API Access tokens (preferred)
<br>
1. Follow the instructions to create an API Access token: https://integrations.cloudbeds.com/hc/en-us/articles/18746883407387-Quickstart-Guide-API-Authentication-for-property-level-users
<br>
2. Copy/paste that token into an Cloudbeds_AppConfig.xml file  
(See example: "_ExampleSecrets\Example_CloudbedsApiKey_AppConfig.xml")
<br>
3. Start the application running
<br>
4. Go to the “Setup screen” and set the “App Config” path to point to the XML file above
<br>
5. Go to the “Operational Report” screen.  This will query the Cloudbeds API and download current and future reservations data and produce the report you see

## Option 2: Set up the application using OAUTH Token API access
<br>
1.	Get Cloudbeds APIs access and access tokens.   These should be placed into a “Cloudbeds_AppConfig.xml” file (See example: “_ExampleSecrets\Example_CloudbedsOAuth_AppConfig.xml”)
<br>
2.	Have a placeholder “Cloudbeds_UserAccessTokens.xml” file.  (This will get overwritten when you authenticate to the application.  See example: “_ExampleSecrets\Example_CloudbedsOAuth_TransientTokensStorage.xml” )
<br>
3.	Start the application running
<br>
4.	Go to the “Setup screen” and….
<br>
4a. Set the “App Config” and “User Access Tokens” paths to point to the XML files above
<br>
4b. Click on the “Start Auth Bootstrap” button to get a URL for application authentication.  Copy paste this into your internet browser and log in to your Cloudbeds property.  Agree to the access request.
<br>

NOTE: The browser will now be navigated to a MISSING PAGE (this is OK).  You just need the information IN the URL
<br>
4c. Copy paste the URL you were navigated into back into this application in the textbox next to the “Finish Auth Bootstrap” button.
<br>
4d. Click the “Finish Auth Bootstrap” button.
<br>

RESULT: You will now have Cloudbeds API access tokens stored on your local machine.  
These are refreshed as needed and are stored in the “Cloudbeds_UserAccessTokens.xml” file.  
<br>
5. Go to the “Operational Report” screen.  This will query the Cloudbeds API and download current and future reservations data and produce the report you see
