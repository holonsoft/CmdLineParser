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
using System.Text;
using holonsoft.DomainModel.CmdLineParser;

namespace holonsoft.CmdLineParser
{
    public sealed partial class CommandLineParser<T>
        where T : class
    {
        private T _parsedArgumentPoco;
        private string[] _argumentsFromOutside;

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

            _argumentsFromOutside = arguments ?? throw new ArgumentNullException("arguments", "Parameter must be provided!");

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
}
