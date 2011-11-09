# Mobile Battlefield Server Interface

The project is a application that you starting on a computer/server that connecting to your Battlefield 3 Server with RCON protocol, then you connecting with your smartphone browser to this application and get a mobile web app to control your server with.

It is a .Net application and i testing it on Windows but Mono could maybe work, dont know, try if you whant and report bugs i maybe can fix.

## Technical stuff about the application

The application is coded in C# and .Net 4.0 with a .Net library called BF3RCON (http://bf3rcon.codeplex.com) that fix all the stuff with the RCON protocol.

Then i using the library called NLog (http://nlog-project.org) to log to textfile console log etc.

## Setup the application

In the application folder you have a file called app.config that are a text file with some settings, open it and edit to the settings you whant, here some explaination:

* RCONServerIP = RCON server ip/host
* RCONServerPort = RCON server port, default for BF3 is 47200
* RCONServerPassword = RCON server password
* WebserviceHost = What host the webservice should listen on, default is * and listening on all ip and hosts
* WebservicePort = What port you whant the webservice to listen on, default port is 80 for web
* WebserviceAdminUsername = Username to access the webinterface
* WebserviceAdminPassword = Password to access the webinterface

You need to run the application as administrator (right click on the .exe and then "Run as Administrator") so you have right to open webservice port.

## Whant to help?

Create a fork and start adding features, i maybe missed or not done with.

## Contact me

You contact me best by e-mail me: rickardnp [ at ] gmail [ dot ] com