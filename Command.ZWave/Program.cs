using System;
using System.Linq;
using System.Collections.Generic;
using Mono.CSharp;

using NLog;

namespace Command.ZWave
{
    public class ForSharingWithEvaluator {
        public static ZWaveLib.ZWaveController controllerForSharingToEvaluator;
    }
    internal class MainClass
    {
        internal static Logger logger;

        private static String getConfVariable(String key, String value) {
            var fromEnv = Environment.GetEnvironmentVariable (key);
            return (fromEnv != null) ? fromEnv : value;
        }

        public static void Main (string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration ();
            var target = new NLog.Targets.ConsoleTarget ();
            target.Layout = @"${date:format=HH\:mm\:ss} ${message}";
            config.LoggingRules.Add (new NLog.Config.LoggingRule ("*", LogLevel.FromString(getConfVariable("ZWAVE_LOG_LEVEL", LogLevel.Info.Name)), target));
            LogManager.Configuration = config;
            logger = LogManager.GetCurrentClassLogger ();

            var portName = getConfVariable("ZWAVE_SERIAL_PORT", "/dev/ttyUSB0");
            logger.Info("started ZWave.Command with port {0} and command line: {1}", portName, String.Join(" ", args));
            var controller = new ZWaveLib.ZWaveController (portName);
            controller.ControllerStatusChanged += (c, ev) => {
                logger.Trace ("ControllerStatusChanged status {0}: {1}", controller.Status, ev.Status);
                switch (ev.Status) {
                case ZWaveLib.ControllerStatus.Connected:
                    controller.Initialize ();
                    break;
                case ZWaveLib.ControllerStatus.Ready:
                    controller.Discovery ();
                    logger.Info ("finished discovery with {0} nodes: {1}", controller.Nodes.ToArray ().Length, String.Join (", ", from node in controller.Nodes
                        select node.Id));

                    DiscoveryDone (controller);
                    break;
                case ZWaveLib.ControllerStatus.Error:
                    logger.Error("controller ControllerStatusChanged to Error: {0} - exiting", controller);
                    Environment.Exit(17);
                    break;
                }
            };

            controller.DiscoveryProgress += (c, ev) => {
                logger.Trace ("DiscoverProgress status {0}: {1} {2}", controller.Status, ev.GetType (), ev.Status);
            };
            controller.NodeOperationProgress += (c, ev) => {
                logger.Trace ("NodeOperationProgress status {0}: node {1} now {2}", controller.Status, ev.NodeId, ev.Status);
            };
            controller.Connect ();
        }


        private static void DiscoveryDone (ZWaveLib.ZWaveController controller)
        {
            var messages = new List<ZWaveLib.ZWaveMessage> ();

            controller.NodeUpdated += (c, ev) => {
                logger.Info ("NodeUpdated {0} {1} {2} {3}", ev.NodeId, ev.GetType(), ev.Event.Parameter, ev.Event.Value);
            };

            foreach (var node in controller.Nodes) {
                logger.Info ("node {0}: manufacturer {1} type {2} product {3} - supported command classes {4}",
                    node.Id, 
                    node.ManufacturerSpecific.ManufacturerId,
                    node.ManufacturerSpecific.TypeId,
                    node.ManufacturerSpecific.ProductId,
                    String.Join (", ", from klass in node.CommandClasses
                        select klass.CommandClass) 
                );
            }

            var settings = new CompilerSettings ();
            settings.LoadDefaultReferences = true;
            var evaluator = new Mono.CSharp.Evaluator (new CompilerContext(settings, new ConsoleReportPrinter()));
            evaluator.ReferenceAssembly (controller.GetType ().Assembly);

            evaluator.ReferenceAssembly (System.Reflection.Assembly.GetExecutingAssembly());

            foreach (var ns in new List<String> {"ZWaveLib.CommandClasses","ZWaveLib.Values","System","System.Threading","System.Linq"}) {
                evaluator.Run(String.Format("using {0};", ns));
            }

            ForSharingWithEvaluator.controllerForSharingToEvaluator = controller;
            evaluator.Run ("var controller = Command.ZWave.ForSharingWithEvaluator.controllerForSharingToEvaluator;");
            foreach (var node in controller.Nodes) {
                evaluator.Run (String.Format("var node{0} = controller.GetNode({0});",node.Id));
            }


            foreach (var arg in Environment.GetCommandLineArgs().Skip(1)) {
                try {
                    logger.Info("evaluating '{0}'", arg);
                    Object result;
                    bool resultSet;
                    evaluator.Evaluate (arg + ";", out result, out resultSet);
                    if (resultSet && result is ZWaveLib.ZWaveMessage) {
                        messages.Add((ZWaveLib.ZWaveMessage)result);
                    }
                } catch (Exception ex) {
                    logger.Error (ex, "error evaluating '{0}'", arg);
                }
            }
            foreach (var message in messages) {
                message.Wait ();
            }
            Environment.Exit (0);

        }
    }
}
