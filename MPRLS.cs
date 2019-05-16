using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace MPRLS_Driver
{
    public class MPRLS
    {
        //The MPRLS register addresses according the the datasheet: https://sensing.honeywell.com/honeywell-sensing-micropressure-board-mount-pressure-mpr-series-datasheet-32332628-d-en.pdf

        const byte address = 0x18;
        private I2cDevice sensor = null;
        private const int pinNumber = 4;
        private GpioPin eOCPin;
        private GpioPinValue pinValue;

        public async Task Initialize()
        {
            // the 'friendly name' of the i2c controller
            const string I2CControllerName = "I2C1";

            // set the i2c address of the device
            I2cConnectionSettings settings = new I2cConnectionSettings(address);

            // this device supports fast i2c communication mode
            settings.BusSpeed = I2cBusSpeed.FastMode;

            // query device for the i2c controller we need
            string query = I2cDevice.GetDeviceSelector(I2CControllerName);
            DeviceInformationCollection deviceInfo = await DeviceInformation.FindAllAsync(query);

            // using the device id and the device address, get the final device interface for read/write communication
            sensor = await I2cDevice.FromIdAsync(deviceInfo[0].Id, settings);

            // Check if device was found and throw if not
            if (sensor is null)
            {
                throw new Exception("Proximity sensor device was not found");
            }
            
            var gpio = GpioController.GetDefault();
            eOCPin = gpio.OpenPin(pinNumber);
            pinValue = GpioPinValue.Low;

        }

        public float ReadPressure()
        {
            int rawOutput = 0;
            const byte outputMeasurementCommand = 0xAA;     
            
            // Create read bytes to store read responses from sensor
            byte[] readByte1 = new byte[2];

            // Prepare command sequence to send to sensor
            byte[] writeCommand = { outputMeasurementCommand, 0x00, 0x00 };

            // Output Measurement Communication from MPR datasheet page 13
            // Step 1
            sensor.Write(writeCommand);

            // Step 2 'Option 3: Wait for the EOC indicator'
            while (pinValue != GpioPinValue.High)
            {
                pinValue = eOCPin.Read();
                Debug.WriteLine("EOC pin being evaluated...");
            }

            // Step 3
            sensor.Read(readByte1);

            rawOutput = readByte1[1];
            rawOutput <<= 16;                       

            // 'Transfer Function A' from MPR datasheet page 16
            float psi = (rawOutput - 0x19999A) * (25 - 0);
            psi /= 0xE66666 - 0x19999A;
            psi += 0;

            // Convert to mmHg
            float mmHg = psi * 760;
            mmHg /= (float)14.696;

            return mmHg;
        }
    }
}
