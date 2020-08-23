using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;

namespace WFOVFix
{
    class Program
    {
        static void Main(string[] args)
        {
            int maxRecommendedResolution = 16384;
            int GPUSpeedValue = 2000;
            decimal gpuSpeedRenderTargetScale = 1.5m;
            bool allowSupersampleFiltering = false;
            decimal supersampleScale = 1;

            try
            {
                Console.WriteLine($"{Environment.NewLine}Looking for Steam installation.");

                var steamPath =
                    Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", null)
                    ??
                    Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null);

                if (steamPath == null)
                    throw new Exception("Steam installation not found.");

                Console.WriteLine($"{Environment.NewLine}Found Steam installation in {steamPath}.");

                Console.WriteLine($"{Environment.NewLine}Looking for vrPathReg.exe in Steam installation.");

                string vrPathReg = null;

                if (System.IO.File.Exists($@"{steamPath}\steamapps\common\SteamVR\bin\win64\vrPathReg.exe"))
                {
                    vrPathReg = $@"{steamPath}\steamapps\common\SteamVR\bin\win64\vrPathReg.exe";
                }
                else if (System.IO.File.Exists($@"{steamPath}\steamapps\common\SteamVR\bin\win32\vrPathReg.exe"))
                {
                    vrPathReg = $@"{steamPath}\steamapps\common\SteamVR\bin\win32\vrPathReg.exe";
                }

                if (vrPathReg == null)
                    throw new Exception("vrPathReg.exe not found in Steam installation.");

                Console.WriteLine($"{Environment.NewLine}Found vrPathReg.exe in {vrPathReg}.");
                Console.WriteLine($"Running vrPathReg.exe to locate the configuration file.{Environment.NewLine}OK? (y/n)");
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Y)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;
                    startInfo.FileName = vrPathReg;
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.RedirectStandardOutput = true;
                    //startInfo.Arguments = "-f j -o \"" + ex1 + "\" -z 1.0 -s y " + ex2;

                    string configPath = null;

                    try
                    {
                        using (Process exeProcess = Process.Start(startInfo))
                        {
                            while (!exeProcess.StandardOutput.EndOfStream)
                            {
                                string line = exeProcess.StandardOutput.ReadLine();
                                //Console.WriteLine(line);
                                if (line.StartsWith("Config path ="))
                                {
                                    configPath = line.Substring(14);
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        throw new Exception("An error occured while trying to find the path of steamvr.vrsettings.");
                    }

                    if (configPath == null)
                        throw new Exception("The path of steamvr.vrsettings. was not found.");

                    var vrSettingsPath = $@"{configPath}\steamVr.Vrsettings";

                    if (System.IO.File.Exists(vrSettingsPath))
                    {
                        var fileinfo = new FileInfo(vrSettingsPath);

                        Console.WriteLine($"{Environment.NewLine}Found the configuration file at {vrSettingsPath}");

                        JObject json = JObject.Parse(File.ReadAllText(vrSettingsPath));

                        json["GpuSpeed"]["gpuSpeed0"] = GPUSpeedValue;
                        json["GpuSpeed"]["gpuSpeed1"] = GPUSpeedValue;
                        json["GpuSpeed"]["gpuSpeed2"] = GPUSpeedValue;
                        json["GpuSpeed"]["gpuSpeed3"] = GPUSpeedValue;
                        json["GpuSpeed"]["gpuSpeed4"] = GPUSpeedValue;
                        json["GpuSpeed"]["gpuSpeed5"] = GPUSpeedValue;
                        json["GpuSpeed"]["gpuSpeed6"] = GPUSpeedValue;
                        json["GpuSpeed"]["gpuSpeed7"] = GPUSpeedValue;
                        json["GpuSpeed"]["gpuSpeed8"] = GPUSpeedValue;
                        json["GpuSpeed"]["gpuSpeed9"] = GPUSpeedValue;
                        json["GpuSpeed"]["gpuSpeedHorsepower"] = GPUSpeedValue;
                        json["GpuSpeed"]["gpuSpeedRenderTargetScale"] = gpuSpeedRenderTargetScale;

                        json["steamvr"]["maxRecommendedResolution"] = maxRecommendedResolution;
                        json["steamvr"]["supersampleScale"] = supersampleScale;
                        json["steamvr"]["allowSupersampleFiltering"] = allowSupersampleFiltering;

                        // excluding these because i'm not entirely sure what they do and where they come from
                        //json["system.generated.reactor steamvr.exe"]["resolutionScale"] = 100;
                        //json["system.generated.reactor steamvr.exe"]["motionSmoothingOverride"] = 0;

                        Console.WriteLine($"{Environment.NewLine}The following values has been updated or added to the configuration file.");

                        Console.WriteLine($@"{Environment.NewLine}""GpuSpeed.GpuSpeed0"" = {GPUSpeedValue}");
                        Console.WriteLine($@"""GpuSpeed.GpuSpeed1"" = {GPUSpeedValue}");
                        Console.WriteLine($@"""GpuSpeed.GpuSpeed2"" = {GPUSpeedValue}");
                        Console.WriteLine($@"""GpuSpeed.GpuSpeed3"" = {GPUSpeedValue}");
                        Console.WriteLine($@"""GpuSpeed.GpuSpeed4"" = {GPUSpeedValue}");
                        Console.WriteLine($@"""GpuSpeed.GpuSpeed5"" = {GPUSpeedValue}");
                        Console.WriteLine($@"""GpuSpeed.GpuSpeed6"" = {GPUSpeedValue}");
                        Console.WriteLine($@"""GpuSpeed.GpuSpeed7"" = {GPUSpeedValue}");
                        Console.WriteLine($@"""GpuSpeed.GpuSpeed8"" = {GPUSpeedValue}");
                        Console.WriteLine($@"""GpuSpeed.GpuSpeed9"" = {GPUSpeedValue}");

                        Console.WriteLine($@"{Environment.NewLine}""GpuSpeed.gpuSpeedHorsepower"" = {GPUSpeedValue}");
                        Console.WriteLine($@"""GpuSpeed.gpuSpeedRenderTargetScale"" = {gpuSpeedRenderTargetScale}");

                        Console.WriteLine($@"{Environment.NewLine}""steamvr.maxRecommendedResolution"" = {maxRecommendedResolution}");
                        Console.WriteLine($@"""steamvr.supersampleScale"" = {supersampleScale}");
                        Console.WriteLine($@"""steamvr.allowSupersampleFiltering"" = {allowSupersampleFiltering}");

                        Console.WriteLine($"{Environment.NewLine}Would you like to save the changes? (y/n)");
                        var k = Console.ReadKey();
                        if (k.Key == ConsoleKey.Y)
                        {
                            if (fileinfo.IsReadOnly)
                            {
                                Console.WriteLine("The Configuration File is read only would you like to attempt to make it writeable?");
                                var w = Console.ReadKey();
                                if (w.Key == ConsoleKey.Y)
                                {
                                    fileinfo.IsReadOnly = false;
                                }
                                else
                                {
                                    Console.WriteLine($"{Environment.NewLine}OK. Cancelling program.");
                                }
                            }

                            System.IO.File.WriteAllText(vrSettingsPath, json.ToString());
                        }
                        else
                        {
                            Console.WriteLine($"{Environment.NewLine}OK. Cancelling program.");
                        }
                    }
                    else
                    {
                        throw new Exception("The configuration file was not found in steamVrVrSettingsPath.");
                    }
                }
                else
                {
                    Console.WriteLine($"{Environment.NewLine}OK. Cancelling program.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(Environment.NewLine + Environment.NewLine + e.Message);
            }

            Console.WriteLine($"{Environment.NewLine + Environment.NewLine}Done. Press any key to exit.");

            Console.ReadKey();
        }
    }
}
