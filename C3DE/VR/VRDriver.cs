using System;

namespace C3DE.VR
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
}
