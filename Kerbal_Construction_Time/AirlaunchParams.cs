using System;

namespace KerbalConstructionTime
{
    public class AirlaunchParams
    {
        public Guid VesselId { get; set; }
        public double Altitude { get; set; }
        public double KscAzimuth { get; set; }
        public double KscDistance { get; set; }
        public double LaunchAzimuth { get; set; }
        public double Velocity { get; set; }

        public bool Validate(out string errorMsg)
        {
            //TODO: tech-based progression
            // 7600 m, 150 m/s, 7500 kg for B-29 with X-1A
            // 9100 m, 175 m/s, 11300 kg for B-50 with X-2
            // 13500 m, 220 m/s, 14200 kg for B-52 with X-15 (no drop tanks)
            double minAlt = 5000;
            double maxAlt = 13500;
            double minVelocity = 125;
            double maxVelocity = 220;
            double minKscDist = 0;
            double maxKscDist = 1.5e6;

            if (KscAzimuth >= 360 || KscAzimuth < 0)
            {
                errorMsg = "Invalid KSC azimuth";
                return false;
            }

            if (LaunchAzimuth >= 360 || LaunchAzimuth < 0)
            {
                errorMsg = "Invalid KSC azimuth";
                return false;
            }

            if (Altitude > maxAlt || Altitude < minAlt)
            {
                errorMsg = $"Altitude needs to be between {minAlt} and {maxAlt} m";
                return false;
            }

            if (Velocity > maxVelocity || Velocity < minVelocity)
            {
                errorMsg = $"Velocity needs to be between {minVelocity} and {maxVelocity} m/s";
                return false;
            }

            if (KscDistance > maxKscDist || KscDistance < minKscDist)
            {
                errorMsg = $"Distance from Space Center needs to be between {minKscDist / 1000:0.#} and {maxKscDist / 1000:0.#} km";
                return false;
            }

            errorMsg = null;
            return true;
        }
    }
}