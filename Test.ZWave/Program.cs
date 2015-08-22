using System;
using System.Linq;
using System.Reflection;
using System.Threading;

using NLog;
using NLog.Config;

using ZWaveLib;
using ZWaveLib.CommandClasses;

namespace Test.ZWave
{
    class MainClass
    {
        private static string serialPortName = "/dev/ttyUSB0";
        private static ControllerStatus controllerStatus = ControllerStatus.Disconnected;
        private static bool showDebugOutput = false;
        private static LoggingRule loggingRule = LogManager.Configuration.LoggingRules[0];

        public static void Main(string[] cargs)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nZWaveLib Test Program\n");
            Console.ForegroundColor = ConsoleColor.White;

            var controller = new ZWaveController(serialPortName);
            // Register controller event handlers
            controller.ControllerStatusChanged += Controller_ControllerStatusChanged;;
            controller.DiscoveryProgress += Controller_DiscoveryProgress;
            controller.NodeOperationProgress += Controller_NodeOperationProgress;
            controller.NodeUpdated += Controller_NodeUpdated;

            // Main program loop
            string command = "";
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
                    ToggleDebug(!showDebugOutput);
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
                case "?":
                    SetSerialPortName(controller);
                    break;
                case "+":
                    controller.Connect();
                    break;
                }
            }

        }

        static void ShowMenu()
        {
            Console.WriteLine("\n[0] Toggle show debug (ShowDebug={0})", showDebugOutput);
            Console.WriteLine("[1] List nodes");
            Console.WriteLine("[2] Add node start");
            Console.WriteLine("[3] Add node stop");
            Console.WriteLine("[4] Remove node start");
            Console.WriteLine("[5] Remove node stop");
            Console.WriteLine("[6] Heal Network");
            Console.WriteLine("[7] Run Node Stress Test");
            Console.WriteLine("[8] Dump available ZWaveLib API commands");
            Console.WriteLine("[?] Change serial port (PortName={0})", serialPortName);
            Console.WriteLine("[+] Connect / Reconnect (Status={0})", controllerStatus);
            Console.WriteLine("[!] Exit");
            Console.WriteLine("\nEnter option and hit [enter]:");
        }

        static void ToggleDebug(bool show = false)
        {
            LogManager.Configuration.LoggingRules.Remove(loggingRule);
            LogManager.Configuration.Reload();
            showDebugOutput = show;
            if (showDebugOutput)
            {
                LogManager.Configuration.LoggingRules.Add(loggingRule);
                LogManager.Configuration.Reload();
            }
        }

        static void ListNodes(ZWaveController controller)
        {
            foreach (var node in controller.Nodes)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nNode {0}", node.Id);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("    Manufacturer Specific {0}-{1}-{2}", node.ManufacturerId, node.TypeId, node.ProductId);
                Console.WriteLine("    Basic Class {0}", (GenericType)node.BasicClass);
                Console.WriteLine("    Generic Class {0}", (GenericType)node.GenericClass);
                Console.WriteLine("    Specific Class {0}", node.SpecificClass);
                Console.WriteLine("    Secure Info {0}", BitConverter.ToString(node.SecuredNodeInformationFrame));
                Console.WriteLine("    Node Info {0}", BitConverter.ToString(node.NodeInformationFrame));
                foreach (var cclass in node.SupportedCommandClasses)
                {
                    Console.WriteLine("        {0}", cclass);
                }
                if (node.Data.ContainsKey("RoutingInfo"))
                {
                    Console.WriteLine("    Routing Info {0}", BitConverter.ToString((byte[])node.Data["RoutingInfo"]));
                }
            }
            Console.WriteLine("\n");
        }

        static void StartNodeAdd(ZWaveController controller)
        {
            ToggleDebug(true);
            controller.BeginNodeAdd();
        }

        static void StopNodeAdd(ZWaveController controller)
        {
            ToggleDebug(true);
            controller.StopNodeAdd();
            ToggleDebug(false);
        }

        static void StartNodeRemove(ZWaveController controller)
        {
            ToggleDebug(true);
            controller.BeginNodeRemove();
        }

        static void StopNodeRemove(ZWaveController controller)
        {
            ToggleDebug(true);
            controller.StopNodeRemove();
            ToggleDebug(false);
        }

        static void HealNetwork(ZWaveController controller)
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

        static void RunStressTest(ZWaveController controller)
        {
            ToggleDebug(true);
            // loop 10 times
            for (int x = 0; x < 10; x++)
            {
                foreach (var node in controller.Nodes)
                {
                    //if (node.Id == 31 || node.Id == 43 || node.Id == 44)
                    //    continue;

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

        private static void SetSerialPortName(ZWaveController controller)
        {
            Console.WriteLine("Enter the serial port name (eg. COM7 or /dev/ttyUSB0):");
            string port = Console.ReadLine().Trim();
            if (!String.IsNullOrWhiteSpace(port))
            {
                serialPortName = port;
                controller.PortName = serialPortName;
                controller.Connect();
            }
        }

        #region ZWaveController events handling

        static void Controller_ControllerStatusChanged (object sender, ControllerStatusEventArgs args)
        {
            Console.WriteLine("ControllerStatusChange {0}", args.Status);
            ToggleDebug(true);
            var controller = (sender as ZWaveController);
            controllerStatus = args.Status;
            switch (controllerStatus)
            {
            case ControllerStatus.Connected:
                // Initialize the controller and get the node list
                controller.Initialize();
                break;
            case ControllerStatus.Disconnected:
                ShowMenu();
                break;
            case ControllerStatus.Initializing:
                break;
            case ControllerStatus.Ready:
                // Query all nodes (Supported Classes, Routing Info, Node Information Frame, Manufacturer Specific)
                controller.Discovery();
                ShowMenu();
                break;
            case ControllerStatus.Error:
                Console.WriteLine("\nEnter [+] to try reconnect\n");
                ShowMenu();
                break;
            }
            ToggleDebug(false);
        }

        static void Controller_DiscoveryProgress(object sender, DiscoveryProgressEventArgs args)
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

        static void Controller_NodeOperationProgress(object sender, NodeOperationProgressEventArgs args)
        {
            Console.WriteLine("NodeOperationProgress {0} {1}", args.NodeId, args.Status);
        }

        static void Controller_NodeUpdated(object sender, NodeUpdatedEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("NodeUpdated {0} Event Parameter {1} Value {2}", args.NodeId, args.Event.Parameter, args.Event.Value);
            Console.ForegroundColor = ConsoleColor.White;
        }

        #endregion

        #region Utility Methods

        static void ShowZWaveLibApi()
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
