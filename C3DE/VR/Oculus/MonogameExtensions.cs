using System;


namespace Microsoft.Xna.Framework.Graphics
{
    public static class MonoGameExtensions
    {
        public static void GetNativeDxDeviceAndContext(this GraphicsDevice graphicsDevice, out IntPtr dxDevicePtr, out IntPtr dxContextPtr)
        {
            var graphicsDeviceType = typeof(GraphicsDevice);

            var d3dDeviceInfo  = graphicsDeviceType.GetField("_d3dDevice", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var deviceObj = d3dDeviceInfo.GetValue(graphicsDevice);
            var deviceType = deviceObj.GetType();
            var devicePtrInfo = deviceType.GetProperty("NativePointer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            dxDevicePtr =  (IntPtr)devicePtrInfo.GetValue(deviceObj);

            var d3dContextInfo = graphicsDeviceType.GetField("_d3dContext", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var contextObj = d3dContextInfo.GetValue(graphicsDevice);
            var contextType = contextObj.GetType();
            var contextPtrInfo = contextType.GetProperty("NativePointer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            dxContextPtr =  (IntPtr)contextPtrInfo.GetValue(contextObj);
        }

        public static IntPtr GetNativeDxResource(this RenderTarget2D renderTarget2D)
        {
            var renderTarget2DType = typeof(RenderTarget2D);

            var textureInfo = renderTarget2DType.GetField("_texture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var resourceObj = textureInfo.GetValue(renderTarget2D);
            var resourceType = resourceObj.GetType();
            var resourcePtrInfo = resourceType.GetProperty("NativePointer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return (IntPtr)resourcePtrInfo.GetValue(resourceObj);
        }
    }
}
