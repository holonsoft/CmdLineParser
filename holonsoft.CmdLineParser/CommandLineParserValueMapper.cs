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
using holonsoft.DomainModel.CmdLineParser;

namespace holonsoft.CmdLineParser
{
    public sealed partial class CommandLineParser<T>
        where T : class
    {
        private void MapArgumentListToObject()
        {
            foreach (string option in _possibleArguments.Keys)
            {
                var arg = (Argument)_possibleArguments[option];

                if (arg.Seen)
                {
                    continue;
                }

                if (_parsedArguments.ContainsKey(option) && _parsedArguments[option].Count > 0)
                {
                    if (arg.IsCollection)
                    {
                        SetCollectionValue(arg, _parsedArguments[option]);
                    }
                    else
                    {
                        SetValue(arg, _parsedArguments[option][0]);
                    }
                    continue;
                }


                if (_parsedArguments.ContainsKey(option) && arg.Attribute.OccurrenceSetsBool && (arg.Field.FieldType == typeof(bool)))
                {
                    SetValue(arg, "true");
                    continue;
                }



                if (arg.Attribute.HasDefaultValue)
                {
                    if (arg.IsCollection)
                    {
                        SetCollectionValue(arg, new[] { arg.Attribute.DefaultValue.ToString() });
                    }
                    else
                    {
                        SetValue(arg, arg.Attribute.DefaultValue.ToString());
                    }
                    continue;
                }

                //////////////////////
                // ERROR 
                //////////////////////
                //ReportError("");

            }
        }


        private void SetValue(Argument argument, string value)
        {
            argument.Seen = true;

            var fieldType = argument.Field.FieldType;

            if (fieldType == typeof(int))
            {
                var fieldValue = int.Parse(value);
                argument.Field.SetValue(_parsedArgumentPoco, fieldValue);
                return;
            }

            if (fieldType == typeof(string))
            {
                argument.Field.SetValue(_parsedArgumentPoco, value);
                return;
            }

            if (fieldType == typeof(bool))
            {
                var fieldValue = bool.Parse(value);
                argument.Field.SetValue(_parsedArgumentPoco, fieldValue);
                return;
            }

            if (fieldType == typeof (Guid))
            {
                var fieldValue = new Guid(value);
                argument.Field.SetValue(_parsedArgumentPoco, fieldValue);
            }

            if (fieldType.BaseType == typeof(Enum))
            {
                var fieldValue = Enum.Parse(fieldType, value);
                argument.Field.SetValue(_parsedArgumentPoco, fieldValue);
            }
        }



        private void SetCollectionValue(Argument argument, IEnumerable values)
        {
            argument.Seen = true;

            var fieldType = argument.Field.FieldType;
            var collectionValues = new ArrayList();


            foreach (var value in values)
            {
                if (fieldType == typeof(int[]))
                {
                    var fieldValue = int.Parse((string)value);
                    collectionValues.Add(fieldValue);
                    continue;
                }

                if (fieldType == typeof (string[]))
                {
                    collectionValues.Add(value);
                    continue;
                }

                if (fieldType == typeof(bool[]))
                {
                    var fieldValue = bool.Parse((string)value);
                    collectionValues.Add(fieldValue);
                    continue;
                }
            }

            if (collectionValues.Count > 0)
            {
                argument.Field.SetValue(_parsedArgumentPoco, collectionValues.ToArray(fieldType.GetElementType()));
            }

        }



 
    }
}