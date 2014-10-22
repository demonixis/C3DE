using Microsoft.Kinect;

namespace C3DE.Demo.Kinect.Scripts
{
    internal static class SkeletalExtensions
    {
        public static Joint ScaleTo(this Joint joint, int width, int height, int depth, float skeletonMaxX, float skeletonMaxY, float skeletonMaxZ)
        {
            SkeletonPoint position = new SkeletonPoint();
            position.X = Scale(width, skeletonMaxX, joint.Position.X);
            position.Y = Scale(height, skeletonMaxY, -joint.Position.Y);
            position.Z = Scale(depth, skeletonMaxZ, joint.Position.Z);
            joint.Position = position;

            return joint;
        }

        public static Joint ScaleTo(this Joint joint, int width, int height, float skeletonMaxX, float skeletonMaxY)
        {
            SkeletonPoint position = new SkeletonPoint();
            position.X = Scale(width, skeletonMaxX, joint.Position.X);
            position.Y = Scale(height, skeletonMaxY, -joint.Position.Y);
            position.Z = joint.Position.Z;
            joint.Position = position;

            return joint;
        }

        public static Joint ScaleTo(this Joint joint, int width, int height)
        {
            return ScaleTo(joint, width, height, 1.0f, 1.0f);
        }

        private static float Scale(int maxPixel, float maxSkeleton, float position)
        {
            float value = ((((maxPixel / maxSkeleton) / 2) * position) + (maxPixel / 2));

            if (value > maxPixel)
                return maxPixel;

            if (value < 0)
                return 0;

            return value;
        }
    }
}
