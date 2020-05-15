/*
 * Copyright (c) by holonsoft, Christian Vogt
 * 
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 *
 * -------------------------------------------------------------------------
 *
 * Powered by holonsoft 
 * Homepage:  http://holonsoft.com    
 *            info@holonsoft.com
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using holonsoft.CmdLineParser.DomainModel;

namespace holonsoft.CmdLineParser
{
    public sealed partial class CommandLineParser<T>
        where T : class
    {
        private T _parsedArgumentPoco;
        private string[] _argumentsFromOutside;

        private static readonly string _separatorStr = "|";
        private static readonly char _separator = _separatorStr[0];

        private readonly Dictionary<string, List<string>> _parsedArguments = new Dictionary<string, List<string>>();

        private readonly Dictionary<string, Argument> _possibleArguments = new Dictionary<string, Argument>();
        private ErrorReporterDelegate _errorReporter;

        public bool HasErrors { get; private set; }

        public T Parse(string[] arguments)
        {
            return Parse(arguments, null);
        }


        public T Parse(string[] arguments, ErrorReporterDelegate errorReporter)
        {
            HasErrors = false;

            _errorReporter = errorReporter;

            _argumentsFromOutside = arguments ?? throw new ArgumentNullException(nameof(arguments), "Parameter(s) must be provided!");

            
            _parsedArgumentPoco = Activator.CreateInstance<T>();
            DetectPossibleArguments();

            ParseArgumentList();
          
            MapArgumentListToObject();
            SetDefaultValuesForUnseenFields();

            CheckForErrors();
            
            return _parsedArgumentPoco;
        }


        public IEnumerable<(string FieldName, string ShortName, string LongName, string HelpText)> GetHelpTexts()
        {
            _parsedArgumentPoco = Activator.CreateInstance<T>();
            DetectPossibleArguments();

            var list = new SortedSet<(string FieldName, string ShortName, string LongName, string HelpText)>();

            foreach (var a in _possibleArguments)
            {
                a.Value.VisitorWasHere = false;
            }

            foreach (var a in _possibleArguments)
            {
                if (a.Value.VisitorWasHere) continue;

                var v = (a.Value.FieldName, a.Value.ShortName, a.Value.LongName, a.Value.Attribute.HelpText);
                
                list.Add(v);

                a.Value.VisitorWasHere = true;
            }
            
            return list;
        }


        public string GetConsoleFormattedHelpTexts(int consoleWidth)
        {
            var sb = new StringBuilder();

            var maxLengthOfFieldName = GetHelpTexts().Select(h => h.FieldName.Length).Concat(new[] { 0 }).Max();
            maxLengthOfFieldName++;

            var maxLengthOfShortName = GetHelpTexts().Select(h => h.ShortName.Length).Concat(new[] { 0 }).Max();
            maxLengthOfShortName++;

            var maxHelpTextLength = consoleWidth - maxLengthOfShortName - maxLengthOfFieldName;


            foreach (var h in GetHelpTexts())
            {
                sb.Append(h.FieldName);

                if (h.FieldName.Length < maxLengthOfFieldName)
                {
                    sb.Append(" ".Repeat(maxLengthOfFieldName - h.FieldName.Length));
                }

                sb.Append(h.ShortName);
                if (h.ShortName.Length < maxLengthOfShortName)
                {
                    sb.Append(" ".Repeat(maxLengthOfShortName - h.ShortName.Length));
                }

                if (h.HelpText.Length <= maxHelpTextLength)
                {
                    sb.AppendLine(h.HelpText);
                }
                else
                {
                    var charCount = 0;
                    var lines = h.HelpText.Split(' ')
                        .GroupBy(w => (charCount += w.Length + 1) / maxHelpTextLength)
                        .Select(g => string.Join(" ", g));

                    var i = 0;
                    foreach (var l in lines)
                    {
                        if (i > 0)
                        {
                            var r = consoleWidth - maxHelpTextLength;
                            if (r < 0) r = 0;

                            sb.Append(" ".Repeat(r));
                        }

                        sb.AppendLine(l);
                        i++;
                    }
                }
            }

            return sb.ToString();
        }

        
        private void CheckForErrors()
        {
            foreach (var kvp in _possibleArguments
                .Where(kvp => (kvp.Value.Attribute.ArgumentType == ArgumentTypes.Required) && (!kvp.Value.Seen)))
            {
                ReportError(ParserErrorKinds.MissingArgument, kvp.Key);
            }


            foreach (var kvp in _parsedArguments
                .Where(kvp => !_possibleArguments.ContainsKey(kvp.Key)))
            {
                ReportError(ParserErrorKinds.UnknownArgument, kvp.Key);
            }
        }


        private void SetDefaultValuesForUnseenFields()
        {
            foreach (var kvp in _possibleArguments
                .Where(kvp => (kvp.Value.Attribute.HasDefaultValue) && (!kvp.Value.Seen)))
            {
                kvp.Value.Field.SetValue(_parsedArgumentPoco, kvp.Value.Attribute.DefaultValue);
            }
        }


        private void ReportError(ParserErrorKinds errorKind, string hint)
        {
            HasErrors = true;
            _errorReporter?.Invoke(errorKind, hint);
        }
        
    }


    static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Repeat(this string self, int count)
        {
            return string.Concat(Enumerable.Repeat(self, count));
        }
    }
}
