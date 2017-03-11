using System;
using System.Runtime.InteropServices;

namespace C3DE.VR
{
	public class OpenHMD
	{
		private const string OpenHMDDLL = "libopenhmd";

		[DllImport(OpenHMDDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ohmd_ctx_create();

		[DllImport(OpenHMDDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ohmd_ctx_destroy(IntPtr ctxHandle);

		[DllImport(OpenHMDDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern string ohmd_ctx_get_error(IntPtr ctxHandle);

		[DllImport(OpenHMDDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ohmd_ctx_update(IntPtr ctxHandle);

		[DllImport(OpenHMDDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int ohmd_ctx_probe(IntPtr ctxHandle);

		[DllImport(OpenHMDDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ohmd_list_gets(IntPtr ctxHandle, int deviceIndex, ohmd_string_value val);

		[DllImport(OpenHMDDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ohmd_list_open_device(IntPtr ctxHandle, int deviceIndex);

		[DllImport(OpenHMDDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int ohmd_device_getf(IntPtr device, ohmd_float_value val, IntPtr out_value);

		[DllImport(OpenHMDDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int ohmd_device_setf(IntPtr device, ohmd_float_value val, IntPtr in_value);

		[DllImport(OpenHMDDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int ohmd_device_geti(IntPtr device, ohmd_int_value val, out int out_value);

		/// <summary>
		/// Return status codes, used for all functions that can return an error. 
		/// </summary>
		public enum ohmd_status
		{
			OHMD_S_OK = 0,
			OHMD_S_UNKNOWN_ERROR = -1,
			OHMD_S_INVALID_PARAMETER = -2,
			OHMD_S_USER_RESERVED = -16384,
		}

		/// <summary>
		/// A collection of string value information types, used for getting information with ohmd_list_gets().
		/// </summary>
		public enum ohmd_string_value
		{
			OHMD_VENDOR = 0,
			OHMD_PRODUCT = 1,
			OHMD_PATH = 2
		}

		/// <summary>
		/// A collection of float value information types, used for getting and setting information with ohmd_device_getf() and ohmd_device_setf().
		/// </summary>
		public enum ohmd_float_value
		{
			OHMD_ROTATION_QUAT = 1,
			OHMD_LEFT_EYE_GL_MODELVIEW_MATRIX = 2,
			OHMD_RIGHT_EYE_GL_MODELVIEW_MATRIX = 3,
			OHMD_LEFT_EYE_GL_PROJECTION_MATRIX = 4,
			OHMD_RIGHT_EYE_GL_PROJECTION_MATRIX = 5,
			OHMD_POSITION_VECTOR = 6,
			OHMD_SCREEN_HORIZONTAL_SIZE = 7,
			OHMD_SCREEN_VERTICAL_SIZE = 8,
			OHMD_LENS_HORIZONTAL_SEPARATION = 9,
			OHMD_LENS_VERTICAL_POSITION = 10,
			OHMD_LEFT_EYE_FOV = 11,
			OHMD_LEFT_EYE_ASPECT_RATIO = 12,
			OHMD_RIGHT_EYE_FOV = 13,
			OHMD_RIGHT_EYE_ASPECT_RATIO = 14,
			OHMD_EYE_IPD = 15,
			OHMD_PROJECTION_ZFAR = 16,
			OHMD_PROJECTION_ZNEAR = 17,
			OHMD_DISTORTION_K = 18
		}

		/// <summary>
		/// A collection of int value information types used for getting information with ohmd_device_geti().
		/// </summary>
		public enum ohmd_int_value
		{
			OHMD_SCREEN_HORIZONTAL_RESOLUTION = 0,
			OHMD_SCREEN_VERTICAL_RESOLUTION = 1
		}
	}
}