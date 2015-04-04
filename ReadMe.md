# iKettle Tinamous Sample

C# Sample application for connecting iKettle to Tinamous.

Allows the use of PC keyboard and Tinamous status posts to instruct the kettle to boil and to monitor the kettle status.

## iKettle.ConsoleHost

Console project to run the iKettle application.

## iKettle.Core

Contains the logic for discovering, connecting and communicating with the iKettle.

See: https://plus.google.com/+MarkJCox/posts/Nv6NZauosUX
and: https://github.com/lloydwatkin/ikettle.js
also: https://github.com/alistairjcbrown/node_ikettle


## iKettle.Tinamous

Tinamous client library to publish the kettle status to Tinamous.

This uses M2MQTT client library to connect to Tinamous. Note that this is done using the unsecured MQTT connection on port 1883.


### Connecting to Tinamous:


When logged into your tinamous account:
* Add a Device
* Name the device something like Kettle
* Ensure the Tinamous.xxxxxx settings are correct in the appSettings.config file for the newly created device.
* Ensure the username is username.accountname (e.g. for the device 'Kettle' on the 'demo' account it would be Kettle.demo).

Running the application:

* When running the application you will see status posts on the Tinamous timeline from your kettle.
* You can also send messages to the kettle. e.g.
  * @Kettle Boil
  * @Kettle Hello

You will need to leave the console host application running in the background to keep control.


## Configuration

Create a local appSettings.config file (It is intentionally missing, it is part of .gitignore option so is not included in the commits to prevent accidental publishing account details).

It should look like below:

```xml
<appSettings>

  <!-- Tinamous account address for MQTT Server -->
  <add key="Tinamous.Url" value="demo.Tinamous.com"/>

  <!-- Device username and password -->
  <!-- Remember for MQTT to include account as part of the username -->
  <add key="Tinamous.Username" value="Kettle.demo" />
  <add key="Tinamous.Password" value="<password goes here>" />

  <!-- Your local network base address, might be 192.168.1.{0} or similar -->
  <add key="iKettle.NetworkTemplate" value="10.0.0.{0}"/>

  <!-- If you know the ip address, or have a lot of devices at a low IP address skip over discovery of the first n IP addresses -->
  <!-- e.g. this will start the discovery process at 10.0.0.40 -->
  <add key="iKettle.StartAt" value="40"/>
  
</appSettings>
```