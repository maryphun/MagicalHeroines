using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.SimpleLocalization.Scripts
{
	/// <summary>
	/// Localization manager.
	/// </summary>
    public static class LocalizationManager
    {
		/// <summary>
		/// Fired when localization changed.
		/// </summary>
        public static event Action LocalizationChanged = () => { }; 

        public static Dictionary<string, Dictionary<string, string>> Dictionary = new Dictionary<string, Dictionary<string, string>>();
#if STEAM
        private static string _language = "Japanese_Steam";
#else
        private static string _language = "Japanese";
#endif

        /// <summary>
        /// Get or set language.
        /// </summary>
        public static string Language
        {
            get => _language;
            set { _language = value; LocalizationChanged(); }
        }

		/// <summary>
		/// Set default language.
		/// </summary>
        public static void AutoLanguage()
        {
            //　日本語をデフォルトに設定
#if STEAM
            Language = "Japanese_Steam";
#else
            Language = "Japanese";
#endif
        }

        /// <summary>
        /// Read localization spreadsheets.
        /// </summary>
        public static void Read()
        {
            if (Dictionary.Count > 0) return;

            var keys = new List<string>();

            foreach (var sheet in LocalizationSettings.Instance.Sheets)
            {
                var textAsset = sheet.TextAsset;
                var lines = GetLines(textAsset.text);
				var languages = lines[0].Split(',').Select(i => i.Trim()).ToList();

                if (languages.Count != languages.Distinct().Count())
                {
                    Debug.LogError($"Duplicated languages found in `{sheet.Name}`. This sheet is not loaded.");
                    continue;
                }
                
                for (var i = 1; i < languages.Count; i++)
                {
                    if (!Dictionary.ContainsKey(languages[i]))
                    {
                        Dictionary.Add(languages[i], new Dictionary<string, string>());
                    }
                }

                for (var i = 1; i < lines.Count; i++)
                {
                    var columns = GetColumns(lines[i]);
                    var key = columns[0];

                    if (key == "") continue;

                    if (keys.Contains(key))
                    {
                        Debug.LogError($"Duplicated key `{key}` found in `{sheet.Name}`. This key is not loaded.");
                        continue;
                    }

                    keys.Add(key);

                    for (var j = 1; j < languages.Count; j++)
                    {
                        if (Dictionary[languages[j]].ContainsKey(key))
                        {
                            Debug.LogError($"Duplicated key `{key}` in `{sheet.Name}`.");
                        }
                        else
                        {
                            Dictionary[languages[j]].Add(key, columns[j]);
                        }
                    }
                }
            }

            AutoLanguage();
        }

        /// <summary>
        /// Check if a key exists in localization.
        /// </summary>
        public static bool HasKey(string localizationKey)
        {
            return Dictionary.Count > 0 && Dictionary[Language].ContainsKey(localizationKey);
        }

        /// <summary>
        /// Get localized value by localization key.
        /// </summary>
        public static string Localize(string localizationKey)
        {
            if (Dictionary.Count == 0)
            {
                Read();
            }

            if (localizationKey == string.Empty) return localizationKey;

            if (!Dictionary.ContainsKey(Language)) throw new KeyNotFoundException("Language not found: " + Language);

            var missed = !Dictionary[Language].ContainsKey(localizationKey) || Dictionary[Language][localizationKey] == "";

            if (missed)
            {
                Debug.LogWarning($"Translation not found: {localizationKey} ({Language}).");

#if STEAM
                return Dictionary["Japanese_Steam"].ContainsKey(localizationKey) ? Dictionary["Japanese_Steam"][localizationKey] : localizationKey;
#else
                return Dictionary["Japanese"].ContainsKey(localizationKey) ? Dictionary["Japanese"][localizationKey] : localizationKey;
#endif
            }

            string manipulatedText = AddCspace(Dictionary[Language][localizationKey]);

            return manipulatedText;
        }

	    /// <summary>
	    /// Get localized value by localization key.
	    /// </summary>
		public static string Localize(string localizationKey, params object[] args)
        {
            var pattern = Localize(localizationKey);

            return string.Format(pattern, args);
        }

        public static List<string> GetLines(string text)
        {
            var _text = text.Replace("\r\n", "\n").Replace("\"\"", "[_quote_]");
            var matches = Regex.Matches(_text, "\"[\\s\\S]+?\"");

            foreach (Match match in matches)
            {
                _text = _text.Replace(match.Value, match.Value.Replace("\"", null).Replace(",", "[_comma_]").Replace("\n", "[_newline_]"));
            }

            // Making uGUI line breaks to work in asian texts.
            _text = _text.Replace("。", "。 ").Replace("、", "、 ").Replace("：", "： ").Replace("！", "！ ").Replace("（", " （").Replace("）", "） ").Trim();

            return _text.Split('\n').Where(i => i != "").ToList();
        }

        public static List<string> GetColumns(string line)
        {
            return line.Split(',').Select(j => j.Trim()).Select(j => j.Replace("[_quote_]", "\"").Replace("[_comma_]", ",").Replace("[_newline_]", "\n")).ToList();
        }

        static string AddCspace(string inputString)
        {
            string inputText = inputString;
            string pattern = "―+";
            string replacement = "<font=JP/ipaexg SDF><cspace=-0.18em>$0</cspace></font>";

            return Regex.Replace(inputString, pattern, replacement);
        }
    }
}