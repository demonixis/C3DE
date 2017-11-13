using System;
using System.Collections.Generic;

namespace C3DE.VR
{
    public static class VRManager
    {
        public class VRDriver : IComparable
        {
            public VRService Service { get; set; }
            public bool Enabled { get; set; }
            public int Order { get; set; }

            public VRDriver(VRService service, bool enabled, int order)
            {
                Service = service;
                Enabled = enabled;
                Order = order;
            }

            public int CompareTo(object obj)
            {
                var driver = obj as VRDriver;

                if (driver == null || driver.Order == Order)
                    return 0;

                if (Order > driver.Order)
                    return -1;

                return 1;
            }
        }

        public static bool Enabled => ActiveService != null;

        public static VRService ActiveService { internal set; get; }

        public static List<VRDriver> AvailableDrivers = new List<VRDriver>();

        public static VRService GetAvailableService()
        {
            if (AvailableDrivers.Count == 0)
            {
#if WINDOWS
                AvailableDrivers.Add(new VRDriver(new OculusRiftService(Application.Engine), false, 2));
                AvailableDrivers.Add(new VRDriver(new OpenVRService(Application.Engine), true, 1));
                AvailableDrivers.Add(new VRDriver(new OpenHMDService(Application.Engine), true, 0));
#elif DESKTOP
                AvailableDrivers.Add(new VRDriver(new OSVRService(Application.Engine), false, 2));
                AvailableDrivers.Add(new VRDriver(new OpenVRService(Application.Engine), false, 1));
                AvailableDrivers.Add(new VRDriver(new OpenHMDService(Application.Engine), false, 0));      
#endif
            }

            AvailableDrivers.Sort();

            VRDriver driver = null;

            for (var i = 0; i < AvailableDrivers.Count; i++)
            {
                try
                {
                    driver = AvailableDrivers[i];

                    if (driver.Enabled && driver.Service.TryInitialize() == 0)
                        return AvailableDrivers[i].Service;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return null;
        }
    }
}
