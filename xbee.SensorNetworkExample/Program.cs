using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.IO.Ports;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;

namespace xbee.SensorNetworkExample
{
    internal class Program
    {
        private static readonly List<Thermometer> _thermometers = new List<Thermometer>();
        private static XBeeReader _xbee;

        private static void Main()
        {
            using (var port = new SerialPort("COM3", 9600))
            {
                _xbee = new XBeeReader(port);

                CreatePipeListener();

                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        var c = char.ToUpper(Console.ReadKey(true).KeyChar);
                        if (c == 'X') break;
                        if (c == ' ') TriggerGarageDoor(_xbee);
                    }
                    else if (_xbee.FrameAvailable)
                    {
                        var frame = _xbee.ReadFrame();

                        if (frame.FrameType == XBeeFrameType.RXIOReceived)
                            ProcessTemperatureReading(new XBeeIOFrame(frame));
                        else
                            Console.WriteLine(frame);
                    }

                    Thread.Sleep(500);
                }
            }
        }

        private static void CreatePipeListener()
        {
            var pipe = new NamedPipeServerStream("XBEE SAMPLE", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            //var security = pipe.GetAccessControl();
            //security.AddAccessRule(new PipeAccessRule("rob-pc\\rob", PipeAccessRights.FullControl, AccessControlType.Allow));
            //pipe.SetAccessControl(security);
            pipe.BeginWaitForConnection(Pipe_Connected, pipe);
        }

        private static void Pipe_Connected(IAsyncResult ar)
        {
            var pipe = ar.AsyncState as NamedPipeServerStream;
            pipe.EndWaitForConnection(ar);

            CreatePipeListener();

            using (var sr = new StreamReader(pipe))
            {
                var message = sr.ReadLine();
                if (message == "Garage Door")
                    TriggerGarageDoor(_xbee);
            }

            pipe.Close();
        }

        private static void ProcessTemperatureReading(XBeeIOFrame frame)
        {
            var thermometer = _thermometers.SingleOrDefault(x => x.SourceAddress == frame.SourceAddress);
            if (thermometer == null)
            {
                _thermometers.Add(thermometer = new Thermometer(frame.SourceAddress));
                _thermometers.Sort((a, b) => a.SourceAddress.CompareTo(b.SourceAddress));
            }

            thermometer.AddSample(frame.Analog0Sample);

            var top = Console.CursorTop;

            Console.CursorLeft = 0;
            Console.CursorTop = _thermometers.IndexOf(thermometer);
            Console.WriteLine(thermometer);

            Console.CursorTop = Math.Max(top, _thermometers.Count);
        }

        private static void TriggerGarageDoor(XBeeReader reader)
        {
            var atFrame = new XBeeATCommandFrame(0x0013A200408697CB, "D1", 5);
            reader.WriteFrame(atFrame.GetFrame());

            Thread.Sleep(100);

            atFrame.Parameter = 0;
            reader.WriteFrame(atFrame.GetFrame());
        }

        public class Thermometer
        {
            public const int NUM_SAMPLES_T0_AVERAGE = 10;

            public Thermometer(ulong sourceAddress)
            {
                SourceAddress = sourceAddress;
                _samples = new Queue<int>(NUM_SAMPLES_T0_AVERAGE);
            }

            private readonly Queue<int> _samples;

            public ulong SourceAddress { get; private set; }
            public double AverageSample { get; private set; }
            public double AverageKelvin { get; private set; }
            public double AverageCelsius { get; private set; }
            public double AverageFahrenheit { get; private set; }

            public void AddSample(ushort sample)
            {
                if (_samples.Count == NUM_SAMPLES_T0_AVERAGE) _samples.Dequeue();
                _samples.Enqueue(sample);

                AverageSample = (double)_samples.Sum() / _samples.Count;
                AverageKelvin = AverageSample / 1023 * 1.2 * 3 * 100;
                AverageCelsius = AverageKelvin - 273.15;
                AverageFahrenheit = AverageKelvin * 9 / 5 - 459.67;
            }

            public override string ToString()
            {
                return string.Format("0x{0:X}: {1:##0.0} °F ({2:##0.0} °C)", SourceAddress, AverageFahrenheit, AverageCelsius);
            }
        }
    }
}