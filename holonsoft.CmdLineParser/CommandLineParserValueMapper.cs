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
using System.Globalization;
using System.Linq;
using System.Net;
using holonsoft.CmdLineParser.DomainModel;

namespace holonsoft.CmdLineParser
{
    public sealed partial class CommandLineParser<T>
        where T : class
    {
        private void MapArgumentListToObject()
        {
            foreach (var option in _possibleArguments.Keys)
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
                    SetValue(arg, true.ToString());
                    continue;
                }
                
                if (!_possibleArguments.ContainsKey(option))
                {
                    _errorReporter?.Invoke(ParserErrorKinds.UnknownArgument, option);
                }

            }
        }


        private void SetValue(Argument argument, string value)
        {
            argument.Seen = true;

            var fieldType = argument.Field.FieldType;
            var fieldTypeCode = Type.GetTypeCode(fieldType);

            switch (fieldTypeCode)
            {
                case TypeCode.Int16:
                    argument.Field.SetValue(_parsedArgumentPoco, Int16.Parse(value, NumberStyles.Any, argument.Attribute.CultureInfo));
                    return;
                case TypeCode.Int32:
                    if (fieldType.BaseType == typeof(Enum))
                    {
                        var fieldValue = Enum.Parse(fieldType, value);
                        argument.Field.SetValue(_parsedArgumentPoco, fieldValue);
                        return;
                    }
                    argument.Field.SetValue(_parsedArgumentPoco, int.Parse(value, NumberStyles.Any, argument.Attribute.CultureInfo));
                    return;
                case TypeCode.Int64:
                    argument.Field.SetValue(_parsedArgumentPoco, long.Parse(value, NumberStyles.Any, argument.Attribute.CultureInfo));
                    return;
                case TypeCode.UInt16:
                    argument.Field.SetValue(_parsedArgumentPoco, UInt16.Parse(value, NumberStyles.Any, argument.Attribute.CultureInfo));
                    return;
                case TypeCode.UInt32:
                    argument.Field.SetValue(_parsedArgumentPoco, UInt32.Parse(value, NumberStyles.Any, argument.Attribute.CultureInfo));
                    return;
                case TypeCode.UInt64:
                    argument.Field.SetValue(_parsedArgumentPoco, UInt64.Parse(value, NumberStyles.Any, argument.Attribute.CultureInfo));
                    return;
                case TypeCode.Decimal:
                    argument.Field.SetValue(_parsedArgumentPoco, Decimal.Parse(value, NumberStyles.Any, argument.Attribute.CultureInfo));
                    return;
                case TypeCode.Single:
                    argument.Field.SetValue(_parsedArgumentPoco, Single.Parse(value, NumberStyles.Any, argument.Attribute.CultureInfo));
                    return;
                case TypeCode.Double:
                    argument.Field.SetValue(_parsedArgumentPoco, Double.Parse(value, NumberStyles.Any, argument.Attribute.CultureInfo));
                    return;
                case TypeCode.Char:
                    argument.Field.SetValue(_parsedArgumentPoco, value[0]);
                    return;
                case TypeCode.Boolean:
                    argument.Field.SetValue(_parsedArgumentPoco, bool.Parse(value));
                    return;
                case TypeCode.DateTime:
                    argument.Field.SetValue(_parsedArgumentPoco, DateTime.Parse(value, argument.Attribute.CultureInfo));
                    return;
                case TypeCode.String:
                    argument.Field.SetValue(_parsedArgumentPoco, value);
                    return;
                case TypeCode.Byte:
                    argument.Field.SetValue(_parsedArgumentPoco, (byte)(int.Parse(value, NumberStyles.Any, argument.Attribute.CultureInfo)));
                    return;
                case TypeCode.SByte:
                    argument.Field.SetValue(_parsedArgumentPoco, (sbyte)(int.Parse(value, NumberStyles.Any, argument.Attribute.CultureInfo)));
                    return;
            }

            if (fieldType == typeof (Guid))
            {
                var fieldValue = new Guid(value);
                argument.Field.SetValue(_parsedArgumentPoco, fieldValue);
                return;
            }

            if (fieldType == typeof(IPAddress))
            {
                argument.Field.SetValue(_parsedArgumentPoco, IPAddress.Parse(value));
                return;
            }

            throw new NotSupportedException("Type " + fieldType + " is not supported by CmdLineParser");
        }


        private void SetCollectionValue(Argument argument, IEnumerable<string> values)
        {
            argument.Seen = true;

            var fieldType = argument.Field.FieldType;
            var collectionValues = new ArrayList();

            var fieldTypeCode = Type.GetTypeCode(argument.Field.FieldType.GetElementType());

            var valueList = values.ToList();
            
            if (argument.Attribute.ArgumentType == ArgumentTypes.MultipleUnique)
            {
                if (valueList.Distinct().Count() != valueList.Count())
                {
                    ReportError(ParserErrorKinds.CollectionValuesAreNotUnique, argument.FieldName);
                    return;
                }
            }


            foreach (var v in valueList)
            {
                switch (fieldTypeCode)
                {
                    case TypeCode.Int16:
                        collectionValues.Add(Int16.Parse(v));
                        continue;
                    case TypeCode.Int32:
                        collectionValues.Add( int.Parse(v));
                        continue;
                    case TypeCode.Int64:
                        collectionValues.Add( long.Parse(v));
                        continue;
                    case TypeCode.UInt16:
                        collectionValues.Add( UInt16.Parse(v));
                        continue;
                    case TypeCode.UInt32:
                        collectionValues.Add( UInt32.Parse(v));
                        continue;
                    case TypeCode.UInt64:
                        collectionValues.Add( UInt64.Parse(v));
                        continue;
                    case TypeCode.Decimal:
                        collectionValues.Add( Decimal.Parse(v));
                        continue;
                    case TypeCode.Single:
                        collectionValues.Add( Single.Parse(v));
                        continue;
                    case TypeCode.Double:
                        collectionValues.Add( Double.Parse(v));
                        continue;
                    case TypeCode.Char:
                        collectionValues.Add( v[0]);
                        continue;
                    case TypeCode.Boolean:
                        collectionValues.Add( bool.Parse(v));
                        continue;
                    case TypeCode.DateTime:
                        collectionValues.Add( DateTime.Parse(v));
                        continue;
                    case TypeCode.String:
                        collectionValues.Add(v);
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