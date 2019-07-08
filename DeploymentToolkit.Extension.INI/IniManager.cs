﻿using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace DeploymentToolkit.Extension.INI
{
    internal static class IniManager
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static FileIniDataParser _parser = new FileIniDataParser();

        private static Dictionary<string, IniData> _iniCache = new Dictionary<string, IniData>();

        private static IniData GetIniFile(string path)
        {
            try
            {
                if (_iniCache.ContainsKey(path))
                {
                    return _iniCache[path];
                }

                if (!File.Exists(path))
                {
                    return new IniData();
                }

                var iniFile = _parser.ReadFile(path);
                _iniCache.Add(path, iniFile);
                return iniFile;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to read ini file");
                return new IniData();
            }
        }

        private static bool SaveIniFile(string path, IniData iniFile)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                _parser.WriteFile(path, iniFile);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save ini file");
                return false;
            }
        }

        internal static bool SetData(string path, string section, string key, string data)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            if (string.IsNullOrEmpty(section))
                return false;
            if (string.IsNullOrEmpty(key))
                return false;

            var iniFile = GetIniFile(path);
            if (!SetData(iniFile, section, key, data))
            {
                _logger.Warn("Failed to set data");
                return false;
            }

            return SaveIniFile(path, iniFile);
        }

        private static bool SetData(IniData iniFile, string section, string key, string data)
        {
            if (!iniFile.Sections.ContainsSection(section))
                return false;
            if (!iniFile[section].ContainsKey(key))
                return false;

            iniFile[section][key] = data;
            return true;
        }
    }
}