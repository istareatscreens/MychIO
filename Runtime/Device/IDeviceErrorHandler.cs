using MychIO.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MychIO.Device
{
    public interface IDeviceErrorHandler
    {
        void Handle(IOEventType eventType, DeviceClassification deviceType, string msg);
    }
}
