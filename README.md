# Z-Wave Home Automation library for .NET

## Features

- Works with most Z-Wave serial controllers
- Event driven
- Hot plug
- Automatically restabilish connection on error/disconnect
- 100% managed code implementation
- Compatible with Mono

## NuGet Package

ZWaveLib  is available as a [NuGet package](https://www.nuget.org/packages/ZWaveLib).

Run `Install-Package ZWaveLib` in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console) or search for “ZWaveLib” in your IDE’s package management plug-in.

## Example usage
```csharp

// Initialize the ZWaveController

var controller = new ZWaveController(serialPortName);

// Register the Controller event handlers (see methods example below)

controller.ControllerStatusChanged += Controller_ControllerStatusChanged;;
controller.DiscoveryProgress += Controller_DiscoveryProgress;
controller.NodeOperationProgress += Controller_NodeOperationProgress;
controller.NodeUpdated += Controller_NodeUpdated;

// Issue some commands on a dimmer and a thermostat node

var dimmer = controller.GetNode(4);
// Set dimmer level to 50
SwitchMultilevel.Set(dimmer, 50);

var thermostat = controller.GetNode(10);
// Configure the Set Point
ThermostatSetPoint.Set(thermostat, ThermostatSetPoint.Value.Heating, 21);
// Set the Thermostat mode to Heat
ThermostatMode.Set(thermostat, ThermostatMode.Value.Heat);
// Or turn it off
ThermostatMode.Set(thermostat, ThermostatMode.Value.Off);

// Controller event handlers

void Controller_ControllerStatusChanged (object sender, ControllerStatusEventArgs args)
{
    Console.WriteLine("ControllerStatusChange {0}", args.Status);
    var controller = (sender as ZWaveController);
    switch (args.Status)
    {
    case ControllerStatus.Connected:
        // Initialize the controller and get the node list
        controller.Initialize();
        break;
    case ControllerStatus.Disconnected:
        break;
    case ControllerStatus.Initializing:
        break;
    case ControllerStatus.Ready:
        // Query all nodes (Supported Classes, Routing Info, Node Information Frame, Manufacturer Specific)
        controller.Discovery();
        break;
    case ControllerStatus.Error:
        break;
    }
}

void Controller_DiscoveryProgress(object sender, DiscoveryProgressEventArgs args)
{
    Console.WriteLine("DiscoveryProgress {0}", args.Status);
    var controller = (sender as ZWaveController);
    switch (args.Status)
    {
    case DiscoveryStatus.DiscoveryStart:
        break;
    case DiscoveryStatus.DiscoveryEnd:
        break;
    }
}

void Controller_NodeOperationProgress(object sender, NodeOperationProgressEventArgs args)
{
    // this will fire on a node operation such as Add, Remove, Updating Routing, etc..
    Console.WriteLine("NodeOperationProgress {0} {1}", args.NodeId, args.Status);
}

void Controller_NodeUpdated(object sender, NodeUpdatedEventArgs args)
{
    // this will fire when new data is received from a node such as level, temperature, humidity, etc...
    Console.WriteLine("NodeUpdated {0} Event Parameter {1} Value {2}", args.NodeId, args.Event.Parameter, args.Event.Value);
}

```

A test program is also shipped with the ZWaveLib source code:
https://github.com/genielabs/zwave-lib-dotnet/blob/master/Test.ZWave/Program.cs
