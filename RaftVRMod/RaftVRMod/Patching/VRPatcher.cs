using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace RaftVR.Patching
{
    public static class VRPatcher
    {
        private static string DataPath => Application.dataPath;
        private static string PluginsPath => Path.Combine(DataPath, "Plugins");
        private static string SteamVRPath => Path.Combine(DataPath, "StreamingAssets", "SteamVR");

        public static PatchErrorCode PatchVR()
        {
            PatchErrorCode patchResult = PatchGGM(Path.Combine(DataPath, "globalgamemanagers"));

            if (patchResult != PatchErrorCode.Failed)
            {
                PatchErrorCode copyResult = CopyPlugins();
                if (patchResult == PatchErrorCode.Success) return PatchErrorCode.Success;
                return copyResult;
            }

            return PatchErrorCode.Failed;
        }

        private static PatchErrorCode CopyPlugins()
        {
            Debug.Log("[RaftVR] Checking for VR plugins...");

            PatchErrorCode result = PatchErrorCode.Failed;

            Dictionary<string, byte[]> plugins = new Dictionary<string, byte[]>()
            {
                { "AudioPluginOculusSpatializer.dll", Properties.Resources.AudioPluginOculusSpatializer },
                { "openvr_api.dll", Properties.Resources.openvr_api },
                { "OVRPlugin.dll", Properties.Resources.OVRPlugin }
            };

            try
            {
                if (CopyFiles(PluginsPath, plugins))
                {
                    Debug.Log("[RaftVR] Successfully copied VR plugins!");
                    result = PatchErrorCode.Success;
                }
                else
                {
                    Debug.Log("[RaftVR] VR plugins already present");
                    result = PatchErrorCode.AlreadyPatched;
                }
            }
            catch(Exception e)
            {
                Debug.LogError("[RaftVR] Error while copying VR plugins");
                Debug.LogException(e);

                return PatchErrorCode.Failed;
            }
            
            Debug.Log("[RaftVR] Checking for binding files...");


            if (!Directory.Exists(SteamVRPath))
            {
                try
                {
                    Directory.CreateDirectory(SteamVRPath);
                }
                catch (Exception e)
                {
                    Debug.LogError("[RaftVR] Could not create SteamVR folder");
                    Debug.LogException(e);
                    return PatchErrorCode.Failed;
                }
            }

            Dictionary<string, byte[]> bindingFiles = new Dictionary<string, byte[]>()
            {
                { "actions.json", Properties.Resources.actions },
                { "binding_holographic_hmd.json", Properties.Resources.binding_holographic_hmd },
                { "binding_index_hmd.json", Properties.Resources.binding_index_hmd },
                { "binding_rift.json", Properties.Resources.binding_rift },
                { "binding_vive.json", Properties.Resources.binding_vive },
                { "binding_vive_cosmos.json", Properties.Resources.binding_vive_cosmos },
                { "binding_vive_pro.json", Properties.Resources.binding_vive_pro },
                { "binding_vive_tracker_camera.json", Properties.Resources.binding_vive_tracker_camera },
                { "bindings_holographic_controller.json", Properties.Resources.bindings_holographic_controller },
                { "bindings_knuckles.json", Properties.Resources.bindings_knuckles },
                { "bindings_logitech_stylus.json", Properties.Resources.bindings_logitech_stylus },
                { "bindings_oculus_touch.json", Properties.Resources.bindings_oculus_touch },
                { "bindings_vive_controller.json", Properties.Resources.bindings_vive_controller },
                { "bindings_vive_cosmos_controller.json", Properties.Resources.bindings_vive_cosmos_controller }
            };

            try
            {
                bool flag = CopyFiles(SteamVRPath, bindingFiles, true);

                if (flag)
                {
                    Debug.Log("[RaftVR] Successfully copied binding files!");
                    result = PatchErrorCode.Success;
                }
                else
                    Debug.Log("[RaftVR] Binding files already present");
            }
            catch (Exception e)
            {
                Debug.LogError("[RaftVR] Error while copying binding files");
                Debug.LogException(e);

                return PatchErrorCode.Failed;
            }
            
            return result;
        }

        private static bool CopyFiles(string destinationPath, Dictionary<string, byte[]> filesToCopy, bool replaceIfDifferent = false)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(destinationPath);
            FileInfo[] files = directoryInfo.GetFiles();
            bool flag = false;
            foreach (var file in filesToCopy)
            {
                string fileName = file.Key;
                if (!Array.Exists(files, (FileInfo fileInfo) => fileName == fileInfo.Name))
                {
                    flag = true;
                    using (MemoryStream manifestResourceStream = new MemoryStream(file.Value))
                    {
                        using (FileStream fileStream = new FileStream(Path.Combine(directoryInfo.FullName, fileName), FileMode.Create, FileAccess.ReadWrite, FileShare.Delete))
                        {
                            Debug.Log("[RaftVR] Copying " + fileName);
                            manifestResourceStream.CopyTo(fileStream);
                        }
                    }
                }
                else if (replaceIfDifferent)
                {
                    string resourceFileContent;
                    using (MemoryStream manifestResourceStream = new MemoryStream(file.Value))
                    {
                        using (StreamReader reader = new StreamReader(manifestResourceStream))
                        {
                            resourceFileContent = reader.ReadToEnd();
                        }
                    }

                    FileInfo installedFile = files.First(fileInfo => fileInfo.Name == fileName);
                    string installedFileContent = File.ReadAllText(@installedFile.FullName);

                    if (resourceFileContent != installedFileContent)
                    {
                        flag = true;
                        Debug.Log("Overwriting " + fileName);
                        File.WriteAllText(installedFile.FullName, resourceFileContent);
                    }
                }
            }
            return flag;
        }

        private static PatchErrorCode PatchGGM(string path)
        {
            if (XRSettings.supportedDevices.Length == 3)
            {
                Debug.Log("[RaftVR] GGM patch not necessary. Supported devices count is 3 as it should be.");
                return PatchErrorCode.AlreadyPatched;
            }

            Debug.Log("[RaftVR] Patching GGM...");

            AssetsManager assetsManager = new AssetsManager();

            Debug.Log("[RaftVR] Loading GGM from path " + path);
            AssetsFileInstance assetsFileInstance = assetsManager.LoadAssetsFile(path, false);

            using (MemoryStream cldbStream = new MemoryStream(Properties.Resources.cldb))
            {
                assetsManager.LoadClassDatabase(cldbStream);
            }

            Debug.Log("[RaftVR] Starting patch...");

            int num = 0;
            while ((long)num < (long)((ulong)assetsFileInstance.table.assetFileInfoCount))
            {
                bool foundArray = false;
                try
                {
                    AssetFileInfoEx assetInfo = assetsFileInstance.table.GetAssetInfo((long)num);
                    AssetTypeInstance ati = assetsManager.GetATI(assetsFileInstance.file, assetInfo, false);
                    AssetTypeValueField globalField = (ati != null) ? ati.GetBaseField(0) : null;
                    AssetTypeValueField vrDevicesField = (globalField != null) ? globalField.Get("enabledVRDevices") : null;
                    if (vrDevicesField != null && vrDevicesField.childrenCount != -1)
                    {
                        Debug.Log("[RaftVR] Found VR devices field! Attempting patch...");

                        AssetTypeValueField devicesArray = vrDevicesField.Get("Array");

                        if (devicesArray != null)
                        {
                            foundArray = true;

                            bool wasPatched = devicesArray.GetChildrenCount() == 3;

                            if (wasPatched)
                            {
                                Debug.Log("[RaftVR] GGM already patched.");

                                return PatchErrorCode.AlreadyPatched;
                            }
                            else
                            {
                                AssetTypeValueField noneValueField = ValueBuilder.DefaultValueFieldFromArrayTemplate(devicesArray);
                                noneValueField.GetValue().Set("None");
                                AssetTypeValueField openVRValuefield = ValueBuilder.DefaultValueFieldFromArrayTemplate(devicesArray);
                                openVRValuefield.GetValue().Set("OpenVR");
                                AssetTypeValueField oculusValueField = ValueBuilder.DefaultValueFieldFromArrayTemplate(devicesArray);
                                oculusValueField.GetValue().Set("Oculus");
                                devicesArray.SetChildrenList(new AssetTypeValueField[]
                                {
                                noneValueField,
                                openVRValuefield,
                                oculusValueField
                                });
                                byte[] array;
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    using (AssetsFileWriter assetsFileWriter = new AssetsFileWriter(memoryStream))
                                    {
                                        assetsFileWriter.bigEndian = false;
                                        AssetWriters.Write(globalField, assetsFileWriter, 0);
                                        array = memoryStream.ToArray();
                                    }
                                }
                                List<AssetsReplacer> list = new List<AssetsReplacer>
                                {
                                    new AssetsReplacerFromMemory(0, (long)num, (int)assetInfo.curFileType, ushort.MaxValue, array)
                                };
                                using (MemoryStream memoryStream2 = new MemoryStream())
                                {
                                    using (AssetsFileWriter assetsFileWriter2 = new AssetsFileWriter(memoryStream2))
                                    {
                                        assetsFileInstance.file.Write(assetsFileWriter2, 0UL, list, 0U, null);
                                        assetsFileInstance.stream.Close();
                                        File.WriteAllBytes(path, memoryStream2.ToArray());
                                    }
                                }
                                Debug.Log("[RaftVR] Successfully patched GGM!");
                                return PatchErrorCode.Success;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (foundArray)
                    {
                        Debug.LogException(e);
                        foundArray = false;
                    }
                }
                num++;
            }
            Debug.LogError("[RaftVR] VR devices field could not be found! The GGM patch has failed. Contact DrBibop#7000 in the RaftModding or Flat2VR Discord server.");

            return PatchErrorCode.Failed;
        }

        public enum PatchErrorCode
        {
            Success,
            AlreadyPatched,
            Failed
        }
    }
}
