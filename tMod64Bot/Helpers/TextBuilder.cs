using System;
using System.Collections.Generic;

namespace tMod64Bot.Helpers
{
    public class TextBuilder
    {
        private Dictionary<String, String> _fields;
        private String _title;

        public TextBuilder() => _fields = new Dictionary<string, string>();

        /// <summary>
        /// Adds a field to the Text
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddField(string name, string value) => _fields.Add(name, value);

        /// <summary>
        /// Adds a title to the text
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public TextBuilder WithTitle(string title)
        {
            _title = title;
            return this;
        }

        /// <summary>
        /// Builds the Text
        /// </summary>
        /// <returns>formatted string with fields</returns>
        public string Build()
        {
            string fullString = $"{_title}\n\n";

            if (_fields.Count.Equals(null))
                throw new ArgumentException("Fields cant be null");

            foreach (var field in _fields)
            {
                fullString += $"{field.Key}\n{field.Value}\n\n";
            }

            return fullString;
        }
    }
}