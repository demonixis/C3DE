using System;
using System.Collections.Generic;

namespace C3DE.VR
{
    public static class VRManager
    {
        private static VRService currentService = null;

        public static bool Enabled => ActiveService != null;

        public static VRService ActiveService
        {
            internal set
            {
                currentService = value;
                VRServiceChanged?.Invoke(value);
            }
            get { return currentService; }
        }

        public static List<VRDriver> AvailableDrivers = new List<VRDriver>();

        public static event Action<VRService> VRServiceChanged;

        /// <summary>
        /// Gets the first available VR service.
        /// </summary>
        /// <returns>Returns a service ready to use, otherwise it returns null.</returns>
        public static VRService GetVRAvailableVRService()
        {
            if (AvailableDrivers.Count == 0)
            {
#if !ANDROID
#if WINDOWS
                AvailableDrivers.Add(new VRDriver(new OculusRiftService(Application.Engine), true, 0));
#endif
                AvailableDrivers.Add(new VRDriver(new OpenVRService(Application.Engine), true, 1));
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
