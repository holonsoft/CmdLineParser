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
using System.Collections.Generic;

namespace holonsoft.CmdLineParser
{
    public sealed partial class CommandLineParser<T>
        where T : class
    {
        private void ParseArgumentList()
        {
            List<string> valueList = null;
            string argumentName = null;

            foreach (var argument in _argumentsFromOutside)
            {
                switch (argument[0])
                {
                    case '-':
                    case '/':
                        if (argumentName != null)
                        {
                            _parsedArguments.Add(argumentName, valueList);
                            argumentName = null;
                        }

                        var offset = ((argument.Length > 1) && (argument[1] == '-')) ? 1 : 0;

                        var endIndex = argument.IndexOfAny(new char[] { ':', '+', '-', '"' }, 1 + offset);
                        
                        var option = argument.Substring(1 + offset, endIndex == -1 ? argument.Length - 1 - offset: endIndex - 1 - offset);
                        argumentName = option;

                        string optionArgument = null;

                        if (option.Length + 1 + offset == argument.Length)
                        {
                            optionArgument = null;
                        }
                        else
                        if (argument.Length > 1 + option.Length && argument[1 + option.Length] == ':')
                        {
                            optionArgument = argument.Substring(option.Length + 2);
                        }
                        else
                        {
                            optionArgument = argument.Substring(option.Length + 1);
                        }

                        valueList = new List<string>();

                        if (optionArgument != null)
                        {
                            valueList.Add(optionArgument);
                        }
                        break;
                    case '@':
                        break;
                    default:
                        valueList?.Add(argument);
                        break;
                }                                    
            }

            if (argumentName != null)
            {
                _parsedArguments.Add(argumentName, valueList);
            }
        }

    }

}
