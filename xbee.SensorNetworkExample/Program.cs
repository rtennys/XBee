using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace xbee.SensorNetworkExample
{
    internal class Program
    {
        private static readonly List<Thermometer> _thermometers = new List<Thermometer>();

        private static void Main()
        {
            using (var port = new SerialPort("COM3", 9600))
            {
                var reader = new XBeeReader(port);

                while (!Console.KeyAvailable)
                {
                    var frame = reader.ReadFrame();

                    if (frame.FrameType == XBeeFrameType.RXIOReceived)
                        ProcessTemperatureReading(new XBeeIOFrame(frame));
                    else
                        Console.WriteLine(frame);
                }
            }
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

            Console.CursorLeft = 0;
            Console.CursorTop = _thermometers.IndexOf(thermometer);
            Console.WriteLine(thermometer);

            Console.CursorTop = _thermometers.Count;
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