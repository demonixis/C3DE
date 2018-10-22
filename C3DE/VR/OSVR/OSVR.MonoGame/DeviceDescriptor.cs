/// OSVR-Unity Connection
///
/// http://sensics.com/osvr
///
/// <copyright>
/// Copyright 2014 Sensics, Inc.
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///     http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
/// </copyright>

using System.Collections;
using System.Text;

//a class for storing information about a device based on it's json descriptor file
using Newtonsoft.Json;
using System.IO;
using System;
using System.Diagnostics;


public class DeviceDescriptor
{

    //filename
    private string _fileName = "";
    public string FileName
    {
        get { return _fileName; }
        set { _fileName = value; }
    }
    //hmd
    private string _vendor = "";
    public string Vendor
    {
        get { return _vendor; }
        set { _vendor = value; }
    }
    private string _model = "";
    public string Model
    {
        get { return _model; }
        set { _model = value; }
    }
    private string _version;
    public string Version
    {
        get { return _version; }
        set { _version = value; }
    }
    private string _note = "";
    public string Note
    {
        get { return _note; }
        set { _note = value; }
    }
    //field of view
    private float _monocularHorizontal = 60f;
    public float MonocularHorizontal
    {
        get { return _monocularHorizontal; }
        set { _monocularHorizontal = value; }
    }
    private float _monocularVertical = 60f;
    public float MonocularVertical
    {
        get { return _monocularVertical; }
        set { _monocularVertical = value; }
    }
    private float _overlapPercent = 100f;
    public float OverlapPercent
    {
        get { return _overlapPercent; }
        set { _overlapPercent = value; }
    }
    private float _pitchTilt = 0;
    public float PitchTilt
    {
        get { return _pitchTilt; }
        set { _pitchTilt = value; }
    }
    //resolutions
    private int _width = 1920;
    public int Width
    {
        get { return _width; }
        set { _width = value; }
    }
    private int _height = 1080;
    public int Height
    {
        get { return _height; }
        set { _height = value; }
    }
    private int _numDisplays = 1;
    public int NumDisplays
    {
        get { return _numDisplays; }
        set { _numDisplays = value; }
    }
    private int _videoInputs = 1;
    public int VideoInputs
    {
        get { return _videoInputs; }
        set { _videoInputs = value; }
    }
    private string _displayMode = "horz_side_by_side";
    public string DisplayMode
    {
        get { return _displayMode; }
        set { _displayMode = value; }
    }
    //distortion
    private float _k1Red = 0;
    public float K1Red
    {
        get { return _k1Red; }
        set { _k1Red = value; }
    }
    private float _k1Green = 0;
    public float K1Green
    {
        get { return _k1Green; }
        set { _k1Green = value; }
    }
    private float _k1Blue = 0;
    public float K1Blue
    {
        get { return _k1Blue; }
        set { _k1Blue = value; }
    }
    //rendering
    private float _leftRoll = 0;
    public float LeftRoll
    {
        get { return _leftRoll; }
        set { _leftRoll = value; }
    }
    private float _rightRoll = 0;
    public float RightRoll
    {
        get { return _rightRoll; }
        set { _rightRoll = value; }
    }
    //eyes
    private float _centerProjX = 0.5f;
    public float CenterProjX
    {
        get { return _centerProjX; }
        set { _centerProjX = value; }
    }
    private float _centerProjY = 0.5f;
    public float CenterProjY
    {
        get { return _centerProjY; }
        set { _centerProjY = value; }
    }
    private int _rotate180 = 0;
    public int Rotate180
    {
        get { return _rotate180; }
        set { _rotate180 = value; }
    }

    //constructors
    public DeviceDescriptor() { }
    public DeviceDescriptor(string fileName, string vendor, string model, string version, int numDisplays, string note, float monocularHorizontal,
        float monocularVertical, float overlapPercent, float pitchTilt, int width, int height, int videoInputs, string displayMode, float k1Red,
        float k1Green, float k1Blue, float leftRoll, float rightRoll, float centerProjX, float centerProjY, int rotate180)
    {
        this._fileName = fileName;
        this._vendor = vendor;
        this._model = model;
        this._version = version;
        this._numDisplays = numDisplays;
        this._note = note;
        this._monocularHorizontal = monocularHorizontal;
        this._monocularVertical = monocularVertical;
        this._overlapPercent = overlapPercent;
        this._pitchTilt = pitchTilt;
        this._width = width;
        this._height = height;
        this._videoInputs = videoInputs;
        this._displayMode = displayMode;
        this._k1Red = k1Red;
        this._k1Green = k1Green;
        this._k1Blue = k1Blue;
        this._leftRoll = leftRoll;
        this._rightRoll = rightRoll;
        this._centerProjX = centerProjX;
        this._centerProjY = centerProjY;
        this._rotate180 = rotate180;
    }

    public override string ToString()
    {

        if (FileName.Equals(""))
        {
            return "No json file has been provided.";
        }
        //print
        StringBuilder jsonPrinter = new StringBuilder(64);
        jsonPrinter.AppendLine("Json File Device Description:")
        .Append("filename = ").AppendLine(FileName)
        .Append("HMD\n")
        .Append("vendor = ").AppendLine(Vendor)
        .Append("model = ").AppendLine(Model)
        .Append("version = ").AppendLine(Version)
        .Append("num_displays = ").AppendLine(NumDisplays.ToString())
        .Append("notes = ").AppendLine(Note)
        .Append("\nFIELD OF VIEW\n")
        .Append("monocular_horizontal = ").AppendLine(MonocularHorizontal.ToString())
        .Append("monocular_vertical = ").AppendLine(MonocularVertical.ToString())
        .Append("overlap_percent = ").AppendLine(OverlapPercent.ToString())
        .Append("pitch_tilt = ").AppendLine(PitchTilt.ToString())
        .Append("\nRESOLUTION\n")
        .Append("width = ").AppendLine(Width.ToString())
        .Append("height = ").AppendLine(Height.ToString())
        .Append("video_inputs = ").AppendLine(VideoInputs.ToString())
        .Append("display_mode = ").AppendLine(DisplayMode)
        .Append("\nDISTORTION\n")
        .Append("k1_red = ").AppendLine(K1Red.ToString())
        .Append("k1_green = ").AppendLine(K1Green.ToString())
        .Append("k1_blue = ").AppendLine(K1Blue.ToString())
        .Append("\nRENDERING\n")
        .Append("left_roll = ").AppendLine(LeftRoll.ToString())
        .Append("right_roll = ").AppendLine(RightRoll.ToString())
        .Append("\nEYES\n")
        .Append("center_proj_x = ").AppendLine(CenterProjX.ToString())
        .Append("center_proj_y = ").AppendLine(CenterProjY.ToString())
        .Append("rotate_180 = ").AppendLine(Rotate180.ToString());
        return jsonPrinter.ToString();
    }

    /// <summary>
    /// This function will parse the device parameters from a device descriptor json file using Newstonsoft
    /// Returns a DeviceDescriptor object containing stored json values.
    /// </summary>
    public static DeviceDescriptor Parse(string deviceDescriptorJson)
    {
        var reader = new JsonTextReader(new StringReader(deviceDescriptorJson));
        if (reader == null)
            return null;

        var deviceDescriptor = new DeviceDescriptor();

        while (reader.Read())
        {
            if (reader.Value != null && reader.ValueType == typeof(String))
            {
                var parsedJson = reader.Value.ToString().ToLower();

                try
                {
                    switch (parsedJson)
                    {
                        case "vendor":
                            deviceDescriptor.Vendor = reader.ReadAsString();
                            break;
                        case "model":
                            deviceDescriptor.Model = reader.ReadAsString();
                            break;
                        case "version":
                            deviceDescriptor.Version = reader.ReadAsString();
                            break;
                        case "num_displays":
                            deviceDescriptor.NumDisplays = int.Parse(reader.ReadAsString());
                            break;
                        case "note":
                            deviceDescriptor.Note = reader.ReadAsString();
                            break;
                        case "monocular_horizontal":
                            deviceDescriptor.MonocularHorizontal = float.Parse(reader.ReadAsString());
                            break;
                        case "monocular_vertical":
                            deviceDescriptor.MonocularVertical = float.Parse(reader.ReadAsString());
                            break;
                        case "overlap_percent":
                            deviceDescriptor.OverlapPercent = float.Parse(reader.ReadAsString());
                            break;
                        case "pitch_tilt":
                            deviceDescriptor.PitchTilt = float.Parse(reader.ReadAsString());
                            break;
                        case "width":
                            deviceDescriptor.Width = int.Parse(reader.ReadAsString());
                            break;
                        case "height":
                            deviceDescriptor.Height = int.Parse(reader.ReadAsString());
                            break;
                        case "video_inputs":
                            deviceDescriptor.VideoInputs = int.Parse(reader.ReadAsString());
                            break;
                        case "display_mode":
                            deviceDescriptor.DisplayMode = reader.ReadAsString();
                            break;
                        case "k1_red":
                            deviceDescriptor.K1Red = float.Parse(reader.ReadAsString());
                            break;
                        case "k1_green":
                            deviceDescriptor.K1Green = float.Parse(reader.ReadAsString());
                            break;
                        case "k1_blue":
                            deviceDescriptor.K1Blue = float.Parse(reader.ReadAsString());
                            break;
                        case "right_roll":
                            deviceDescriptor.RightRoll = float.Parse(reader.ReadAsString());
                            break;
                        case "left_roll":
                            deviceDescriptor.LeftRoll = float.Parse(reader.ReadAsString());
                            break;
                        case "center_proj_x":
                            deviceDescriptor.CenterProjX = float.Parse(reader.ReadAsString());
                            break;
                        case "center_proj_y":
                            deviceDescriptor.CenterProjY = float.Parse(reader.ReadAsString());
                            break;
                        case "rotate_180":
                            deviceDescriptor.Rotate180 = int.Parse(reader.ReadAsString());
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        return deviceDescriptor;
    }
}
