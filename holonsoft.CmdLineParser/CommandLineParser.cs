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
using System.Collections;
using System.Collections.Generic;
using holonsoft.DomainModel.CmdLineParser;

namespace holonsoft.CmdLineParser
{
    public sealed partial class CommandLineParser<T>
        where T : class
    {
        private T _parsedArgumentPoco;
        private string[] _argumentsFromOutside;

        private readonly Dictionary<string, List<string>> _parsedArguments = new Dictionary<string, List<string>>();

        private readonly Hashtable _possibleArguments = new Hashtable();
        private ErrorReporterDelegate _errorReporter;
        
        public T Parse(string[] arguments)
        {
            return Parse(arguments, null);
        }


        public T Parse(string[] arguments, ErrorReporterDelegate errorReporter)
        {
            _errorReporter = errorReporter;

            _parsedArgumentPoco = Activator.CreateInstance<T>();

            if (arguments == null)
            {
                throw new ArgumentNullException("arguments", "Parameter must be provided!");
            }

            _argumentsFromOutside = arguments;

            DetectPossibleArguments();
            ParseArgumentList();

            if (HasErrors())
            {
                //////////////////
                // Error
                ////////////////
                return _parsedArgumentPoco;
            }

            MapArgumentListToObject();

            return _parsedArgumentPoco;
        }


        private bool HasErrors()
        {
            var result = false;



            return result;
        }


        private void ReportError(ParserErrorKinds errorKind, string hint)
        {
            if (_errorReporter == null) return;

            _errorReporter(errorKind, hint);
        }
    }
}
