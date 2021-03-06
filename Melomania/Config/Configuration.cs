﻿using Optional;
using Optional.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Melomania.Config
{
    public class Configuration
    {
        private const string RootCollectionFolderKey = "rootCollectionFolder";

        public string RootConfigurationFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "melomania");

        private string ConfigFilePath => Path.Combine(RootConfigurationFolder, "config.melomania");
        public string TempFolder => Path.Combine(RootConfigurationFolder, "temp");
        public string ToolsFolder => Path.Combine(RootConfigurationFolder, "tools");

        public Dictionary<string, string> GetAllValues()
        {
            try
            {
                CheckForConfigFile();

                // The expected format for each line is 'key=value'
                var keyValuePairs = File
                    .ReadAllLines(ConfigFilePath)
                    .Select(row => row.Split('='))
                    .ToDictionary(row => row.ElementAtOrDefault(0), row => row.ElementAtOrDefault(1));

                return keyValuePairs;
            }
            catch (ArgumentException e)
            {
                throw new ConfigurationException("Duplicate configuration keys were found.", e);
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Corrupted configuration file.", e);
            }
        }

        public Option<string> GetRootCollectionFolder() =>
            GetValue(RootCollectionFolderKey);

        public Option<string> GetValue(string key) =>
            GetAllValues().GetValueOrNone(key);

        public void SetRootCollectionFolder(string value) =>
            SetValue(RootCollectionFolderKey, value);

        public void SetValue(string key, string value)
        {
            var values = GetAllValues();

            values.GetValueOrNone(key).Match(
                some: _ => values[key] = value,
                none: () => values.Add(key, value));

            var configurationPairs = values
                .Select(v => $"{v.Key}={v.Value}");

            var configurationContents = string.Join(Environment.NewLine, configurationPairs);

            // The file is very small, so we can simply rewrite it again instead of doing "in-place" editing
            File.WriteAllText(ConfigFilePath, string.Empty);
            File.WriteAllText(ConfigFilePath, configurationContents);
        }

        private void CheckForConfigFile()
        {
            if (!File.Exists(ConfigFilePath))
            {
                using (var fs = File.Create(ConfigFilePath)) { }
            }
        }
    }
}