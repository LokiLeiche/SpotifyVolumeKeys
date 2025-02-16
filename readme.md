# Spotify Volume Keys

## What is this?
This is a simple console application written in C# allowing you to control your spotify volume with your keyboard. I made this because I wanted the volume know on my keyboard to change the Spotify volume instead of the windows volume. This currently only works for windows, not tested with wine.

## How does it work?
This program utilizes the spotify API to read and change your playback volume. This means it only works when connected to the internet. The spotify docs can be found [here](https://developer.spotify.com/documentation/web-api/reference/set-volume-for-users-playback). You need Spotify Premium for this to work! This app currently uses .NET 8.0 but could probably be compiled for a bunch of other versions.

## Disclaimer
This is my first time doing anything with C#, so the code is probably terrible and not the best. Feel free to PR improvements.


# Installation

## 1. Create a Spotify app
First you'll need to create your own spotify application. Go to the [Spotify developer dashboard](https://developer.spotify.com/dashboard) and click "Create app". You can choose anything for name and description, website can be left empty. In Redirect URIs, you'll need a domain to where you'll be redirected after authorization and can copy a code, more on that later. Ideally you should use a domain owned by you for privacy/security reasons, but you could use any URI. I used my own shop, you can use it too if you want https://www.lokiscripts.com. For the APIs/SDKs select Web API and check the terms and guidelines box at the bottom and then click save. This should now open your apps dashboard. Click on settings in the top right corner. You'll see your Client ID at the top, we need that later. You can also click "View client secret" to see the client secret, we'll need that later as well, so keep this site open.

## 2. Authorize your app
To authorize your app, you need to add all relevant information to this link: https://accounts.spotify.com/en/authorize.  In the web we use ? after the link to add parameters in a style like this: link?parameter1=value1&parameter2=value2. We apply the same scheme here. Spotify needs 4 parameters here, the client ID, the redirect URI, a response type and a scope. With these values we'll build our link. The order doesn't matter but I will start with the reponse type. In our case this is just "code" because we want to receive a code for authentication. Next we'll need the client id, this is added as client_id. Insert the client ID you copied from your app's settings here. The redirect URI is added as recirect_uri, this has to be the same URI you added in your app's settings. Lastly we'll need the scope, this is the permissions of our app. They are documented [here](https://developer.spotify.com/documentation/web-api/concepts/scopes). In this case we need two scopes, user-read-playback-state to read the current volume and user-modify-playback-state to set the current volume. Scopes are separated by spaces, because we are in a link we replace the space with %20. Your final link should look something like this: https://accounts.spotify.com/en/authorize?response_type=code&scope=user-modify-playback-state%20user-read-playback-state&client_id=e7e3....996494&redirect_uri=https://www.lokiscripts.com/. You can also just use this link from me and replace the client_id. Now open this link in your browser. You'll be prompted by spotify to login if you aren't already and then you can click authorize for the app. After that you'll be redirected to the previously entered redicrect URI. On that site, click on the adress bar in your browser. It will look something like this: https://www.lokiscripts.com/?code=1234...1234. Copy the part after the code=, so in this case 1234...1234 and save it for later. This code is needed to create an access token that the API uses for authorization.

## 3. Launch the program
Now you can either download the zip file in this repos build directory and unzip that, or build it yourself from source. Put the files in any folder and launch the spotify_volume.exe file. A new console window will open, follow the instructions the app gives you. You need to enter your client_id, client_secret, redirect_uri and the code you got from step 2. As soon as you did that the program requested a refresh token and an access token from the spotify API, the access token is temporary and expires after one hour, the refresh token is given to create new access tokens. A new access token is automatically generated every 50 minutes or at every launch. The refresh token, client id and client secret are now saved in a newly generated config.ini file in the same directory as your .exe file.

## 4. Configuring
You can now open that .ini file. The tokens and ids are stored at the bottom, usually don't touch these. At the top you have some configuration options. default_volume is the volume in % that is used as starting value if your current volume couldn't be retrieved through the API. volume_up_button and volume_down_button are the keys you use to increase and decrease the volume, you can find a list of all keys and IDs in the [microsoft documentations](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?view=windowsdesktop-9.0). By default these are the volume up and volume down keys on your keyboard, if you have those, you but can change it to anything else. Lastly there is volume_increment, this is just how much the volume changes on each button press in %.


## 5. Changing your keyboard keys (OPTIONAL)
If you use the volume up and volume down keys on your keyboard, they still also control the windows volume. I found no way to disable that in windows. But some keyboard allow you to change the keys, corsair for example gives you the option to remap each key in the ICUE software, other keyboards come with open source QMK firmware. You'll have to figure that out depending on your keyboard. I chose to remap my volume keys to F13 and F14 since they are pretty much never used anymore.


# Thanks for reading
If you have any questions/issues, feel free to open an issue on the github page and I'll take a look.