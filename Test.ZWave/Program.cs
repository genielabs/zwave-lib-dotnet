using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;

using NLog;
using NLog.Config;

using ZWaveLib;
using ZWaveLib.CommandClasses;

namespace Test.ZWave
{
    internal class MainClass
    {
        private static string _serialPortName = "COM3";
        private static ControllerStatus _controllerStatus = ControllerStatus.Disconnected;
        private static bool _showDebugOutput = false;
        private static readonly LoggingRule LoggingRule = LogManager.Configuration.LoggingRules[0];

        public static void Main(string[] cargs)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nZWaveLib Test Program\n");
            Console.ForegroundColor = ConsoleColor.White;

            var controller = new ZWaveController(_serialPortName);
            // Register controller event handlers
            controller.ControllerStatusChanged += Controller_ControllerStatusChanged;;
            controller.DiscoveryProgress += Controller_DiscoveryProgress;
            controller.NodeOperationProgress += Controller_NodeOperationProgress;
            controller.NodeUpdated += Controller_NodeUpdated;

            // Main program loop
            var command = "";
            while (command != "!")
            {
                ShowMenu();
                // TODO: Allow issuing CommandClass commands on nodes from the console input
                // TODO: Add "Associate node to controller" option
                // TODO: Add "Query node parameters" based on implemented classes
                command = Console.ReadLine();
                switch (command)
                {
                    case "0":
                        ToggleDebug(!_showDebugOutput);
                        break;
                    case "1":
                        ListNodes(controller);
                        break;
                    case "2":
                        StartNodeAdd(controller);
                        break;
                    case "3":
                        StopNodeAdd(controller);
                        break;
                    case "4":
                        StartNodeRemove(controller);
                        break;
                    case "5":
                        StopNodeRemove(controller);
                        break;
                    case "6":
                        HealNetwork(controller);
                        break;
                    case "7":
                        RunStressTest(controller);
                        break;
                    case "8":
                        ShowZWaveLibApi();
                        break;
                    case "9":
                        Discovery(controller);
                        break;
                    case "?":
                        SetSerialPortName(controller);
                        break;
                    case "+":
                        controller.Connect();
                        break;
                    case "~":
                        RunCommandInteractive(controller);
                        break;
                }
            }
            Console.WriteLine("\nCiao!\n");
            controller.Dispose();
        }

        private static void RunCommandInteractive(ZWaveController controller)
        {
            Console.WriteLine("Commands must be issued in format CommandClass.Command(nodeId, _additional params_).");
            Console.Write("> ");

            var command = Console.ReadLine();
            if(string.IsNullOrEmpty(command))
                return;

            var commandTerms = command.Split(new[] {'.', '(', ')', ',', ' '}, StringSplitOptions.RemoveEmptyEntries);
            if (commandTerms.Length < 3)
            {
                Console.WriteLine("Commands must be issued in format CommandClass.Command(nodeId, _additional params_).");
                return;
            }

            byte nodeId;
            if(!byte.TryParse(commandTerms[2], out nodeId))
                return;

            try
            {
                var node = controller.GetNode(nodeId);

                var ccType = Assembly.GetAssembly(typeof(ZWaveController)).GetType(string.Format("ZWaveLib.CommandClasses.{0}", commandTerms[0]), true);
                if (ccType == null)
                    return;

                // currently we try to find method using it's name and parameters count
                var methodInfos = ccType.GetMethods();
                MethodInfo methodToInvoke = null;
                foreach (var methodInfo in methodInfos)
                {
                    if (methodInfo.Name == commandTerms[1] && methodInfo.GetParameters().Length == commandTerms.Length - 2)
                        methodToInvoke = methodInfo;
                }
                if (methodToInvoke == null)
                    return;

                // prepare params
                const int additionalParamsIdx = 3;
                var invokeParams = new List<object> { node };
                var methodParams = methodToInvoke.GetParameters();
                for (var i = 0; i < commandTerms.Length - 3; i++)
                {
                    var paramType = methodParams[i + 1].ParameterType;
                    var val = TypeDescriptor.GetConverter(paramType).ConvertFromString(commandTerms[additionalParamsIdx + i]);
                    invokeParams.Add(val);
                }

                methodToInvoke.Invoke(null, invokeParams.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        private static void ShowMenu()
        {
            Console.WriteLine("\n[0] Toggle show debug (ShowDebug={0})", _showDebugOutput);
            Console.WriteLine("[1] List nodes");
            Console.WriteLine("[2] Add node start");
            Console.WriteLine("[3] Add node stop");
            Console.WriteLine("[4] Remove node start");
            Console.WriteLine("[5] Remove node stop");
            Console.WriteLine("[6] Heal Network");
            Console.WriteLine("[7] Run Node Stress Test");
            Console.WriteLine("[8] Dump available ZWaveLib API commands");
            Console.WriteLine("[9] Discovery (query all nodes data)");
            Console.WriteLine("[?] Change serial port (PortName={0})", _serialPortName);
            Console.WriteLine("[+] Connect / Reconnect (Status={0})", _controllerStatus);
            Console.WriteLine("[~] Run command");
            Console.WriteLine("[!] Exit");
            Console.WriteLine("\nEnter option and hit [enter]:");
        }

        private static void ToggleDebug(bool show = false)
        {
            LogManager.Configuration.LoggingRules.Remove(LoggingRule);
            LogManager.Configuration.Reload();
            _showDebugOutput = show;
            if (_showDebugOutput)
            {
                LogManager.Configuration.LoggingRules.Add(LoggingRule);
                LogManager.Configuration.Reload();
            }
        }

        private static void ListNodes(ZWaveController controller)
        {
            foreach (var node in controller.Nodes)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nNode {0}", node.Id);
                Console.ForegroundColor = ConsoleColor.White;
                var mspecs = node.ManufacturerSpecific;
                Console.WriteLine("    Manufacturer Specific {0}-{1}-{2}", mspecs.ManufacturerId, mspecs.TypeId, mspecs.ProductId);
                Console.WriteLine("    Basic Type {0}", (GenericType)node.ProtocolInfo.BasicType);
                Console.WriteLine("    Generic Type {0}", (GenericType)node.ProtocolInfo.GenericType);
                Console.WriteLine("    Specific Type {0}", node.ProtocolInfo.SpecificType);
                Console.WriteLine("    Secure Info Frame {0}", BitConverter.ToString(node.SecuredNodeInformationFrame));
                Console.WriteLine("    Info Frame {0}", BitConverter.ToString(node.NodeInformationFrame));
                foreach (var nodeCmdClass in node.CommandClasses)
                {
                    var versionInfo = "";
                    // TODO: GetCmdClassVersion version is not currently working
                    if (node.SupportCommandClass(CommandClass.Version))
                    {
                        versionInfo = String.Format("(version {0})", nodeCmdClass.Version);
                    }
                    if (!Enum.IsDefined(typeof(CommandClass), nodeCmdClass.Id))
                    {
                        versionInfo += " [UNSUPPORTED]";
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.WriteLine("        {0} {1}", nodeCmdClass.CommandClass, versionInfo);
                }
                Console.ForegroundColor = ConsoleColor.White;
                if (node.Version != null)
                {
                    Console.WriteLine("    Node Version info:");
                    Console.WriteLine("        LibraryType {0}", (node.Version.LibraryType));
                    Console.WriteLine("        ProtocolVersion {0}", (node.Version.ProtocolVersion));
                    Console.WriteLine("        ProtocolSubVersion {0}", (node.Version.ProtocolSubVersion));
                    Console.WriteLine("        ApplicationVersion {0}", (node.Version.ApplicationVersion));
                    Console.WriteLine("        ApplicationSubVersion {0}", (node.Version.ApplicationSubVersion));
                }
                if (node.GetData("RoutingInfo") != null)
                {
                    Console.WriteLine("    Routing Info {0}", BitConverter.ToString((byte[])node.GetData("RoutingInfo").Value));
                }
            }
            Console.WriteLine("\n");
        }

        private static void StartNodeAdd(ZWaveController controller)
        {
            ToggleDebug(true);
            controller.BeginNodeAdd();
        }

        private static void StopNodeAdd(ZWaveController controller)
        {
            ToggleDebug(true);
            controller.StopNodeAdd();
            ToggleDebug(false);
        }

        private static void StartNodeRemove(ZWaveController controller)
        {
            ToggleDebug(true);
            controller.BeginNodeRemove();
        }

        private static void StopNodeRemove(ZWaveController controller)
        {
            ToggleDebug(true);
            controller.StopNodeRemove();
            ToggleDebug(false);
        }

        private static void HealNetwork(ZWaveController controller)
        {
            ToggleDebug(true);
            foreach (var node in controller.Nodes)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nHealing Node {0}", node.Id);
                Console.ForegroundColor = ConsoleColor.White;
                controller.RequestNeighborsUpdateOptions(node.Id);
                controller.RequestNeighborsUpdate(node.Id);
                controller.GetNeighborsRoutingInfo(node.Id);
            }
            ToggleDebug(false);
        }

        private static void RunStressTest(ZWaveController controller)
        {
            ToggleDebug(true);
            // loop 10 times
            for (var x = 0; x < 10; x++)
            {
                foreach (var node in controller.Nodes)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\nNode {0} Controller.GetNodeInformationFrame", node.Id);
                    Console.ForegroundColor = ConsoleColor.White;
                    controller.GetNodeInformationFrame(node.Id);

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\nNode {0} Controller.GetNeighborsRoutingInfo", node.Id);
                    Console.ForegroundColor = ConsoleColor.White;
                    controller.GetNeighborsRoutingInfo(node.Id);

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\nNode {0} CommandClass.ManufacturerSpecific.Get", node.Id);
                    Console.ForegroundColor = ConsoleColor.White;
                    ManufacturerSpecific.Get(node);

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\nNode {0} CommandClass.Basic.Get", node.Id);
                    Console.ForegroundColor = ConsoleColor.White;
                    Basic.Get(node);
                }
                // Pause 2 secods between each test pass
                Thread.Sleep(2000);
            }
            ToggleDebug(false);
        }

        private static void Discovery(ZWaveController controller)
        {
            ToggleDebug(true);
            controller.Discovery();
            ToggleDebug(false);
        }

        private static void SetSerialPortName(ZWaveController controller)
        {
            Console.WriteLine("Enter the serial port name (eg. COM7 or /dev/ttyUSB0):");
            var port = Console.ReadLine().Trim();
            if (!String.IsNullOrWhiteSpace(port))
            {
                _serialPortName = port;
                controller.PortName = _serialPortName;
                controller.Connect();
            }
        }

        #region ZWaveController events handling

        private static void Controller_ControllerStatusChanged (object sender, ControllerStatusEventArgs args)
        {
            Console.WriteLine("ControllerStatusChange {0}", args.Status);
            ToggleDebug(true);
            var controller = (sender as ZWaveController);
            _controllerStatus = args.Status;
            switch (_controllerStatus)
            {
            case ControllerStatus.Connected:
                // Initialize the controller and get the node list
                controller.GetControllerInfo();
                controller.GetControllerCapabilities();
                controller.GetHomeId();
                controller.GetSucNodeId();
                controller.Initialize();
                break;
            case ControllerStatus.Disconnected:
                ShowMenu();
                break;
            case ControllerStatus.Initializing:
                break;
            case ControllerStatus.Ready:
                // Query all nodes (Supported Classes, Routing Info, Node Information Frame, Manufacturer Specific)
//                controller.Discovery();
                ShowMenu();
                break;
            case ControllerStatus.Error:
                Console.WriteLine("\nEnter [+] to try reconnect\n");
                ShowMenu();
                break;
            }
            ToggleDebug(false);
        }

        private static void Controller_DiscoveryProgress(object sender, DiscoveryProgressEventArgs args)
        {
            Console.WriteLine("DiscoveryProgress {0}", args.Status);
            switch (args.Status)
            {
            case DiscoveryStatus.DiscoveryStart:
                break;
            case DiscoveryStatus.DiscoveryEnd:
                break;
            }
        }

        private static void Controller_NodeOperationProgress(object sender, NodeOperationProgressEventArgs args)
        {
            Console.WriteLine("NodeOperationProgress {0} {1}", args.NodeId, args.Status);
        }

        private static void Controller_NodeUpdated(object sender, NodeUpdatedEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("NodeUpdated {0} Event Parameter {1} Value {2}", args.NodeId, args.Event.Parameter, args.Event.Value);
            Console.ForegroundColor = ConsoleColor.White;
        }

        #endregion

        #region Utility Methods

        private static void ShowZWaveLibApi()
        {
            var zwavelib = Assembly.LoadFrom("ZWaveLib.dll");
            var typelist = zwavelib.GetTypes().ToList();
            typelist.Sort(new Comparison<Type>((a, b) => String.Compare(a.Name, b.Name)));
            foreach (var typeClass in typelist)
            {
                if (typeClass.FullName.StartsWith("ZWaveLib.CommandClasses"))
                {
                    var classMethods = typeClass.GetMethods(BindingFlags.Static | BindingFlags.Public);
                    if (classMethods.Length > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\n{0}", typeClass.Name);
                        Console.ForegroundColor = ConsoleColor.White;
                        foreach (var method in classMethods)
                        {
                            var parameters = method.GetParameters();
                            var parameterDescription = string.Join
                            (", ", parameters
                                .Select(x => /*x.ParameterType + " " +*/ x.Name)
                                .ToArray());

                            Console.WriteLine("{0} {1} ({2})", "  "/*method.ReturnType*/, method.Name, parameterDescription);
                        }
                    }
                }
            }
            Console.WriteLine("\n");
        }

        #endregion

    }
}
