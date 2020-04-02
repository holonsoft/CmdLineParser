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
using System.Linq;
using holonsoft.CmdLineParser.DomainModel;
using holonsoft.DomainModel.CmdLineParser;

namespace holonsoft.CmdLineParser
{
    public sealed partial class CommandLineParser<T>
        where T : class
    {
        private void DetectPossibleArguments()
        {
            if (_possibleArguments.Count > 0) return;
            

            foreach (var argument in from field in _parsedArgumentPoco.GetType().GetFields()
                                     where !field.IsInitOnly && !field.IsLiteral && !field.IsStatic
                                     let attributes = field.GetCustomAttributes(typeof (ArgumentAttribute), false)
                                     let attribute = (attributes.Length == 1) ? (ArgumentAttribute) attributes[0] : null
                                     where attribute != null
                                     select new Argument() {Attribute = attribute, Field = field, Seen = false})
            {
                try
                {
                    // Register argument with fieldname, longname and shortname, if applicable
                    _possibleArguments[argument.FieldName] = argument;

                    if (!string.IsNullOrWhiteSpace(argument.ShortName))
                    {
                        _possibleArguments[argument.ShortName] = argument;
                    }

                    if (!string.IsNullOrWhiteSpace(argument.LongName))
                    {
                        _possibleArguments[argument.LongName] = argument;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("LongNames and ShortNames must be unique! Have a problem with " + argument.FieldName, ex);
                }
            }
        }
    }
}
