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
using holonsoft.CmdLineParser.DomainModel;

namespace holonsoft.CmdLineParser
{
    public sealed partial class CommandLineParser<T>
        where T : class
    {
        private string _tokenizedArgumentName = null;
        private List<string> _tokenizedValueList = new List<string>();
        
        private void ParseArgumentList()
        {
            var tokenizer = new StringTokenizer(_argumentsFromOutside);

            _tokenizedValueList.Clear();
            _parsedArguments.Clear();

            while (true)
            {
                var token = tokenizer.Next();

                switch (token.Kind)
                {
                    case TokenKind.Unknown:
                        continue;
                    case TokenKind.Done:
                        AddArgument();
                        return;
                    case TokenKind.ArgStartMarker:
                        AddArgument();
                        continue;
                    case TokenKind.Argument:
                        _tokenizedArgumentName = token.Content;
                        continue;
                    case TokenKind.ArgContent:
                        _tokenizedValueList.Add(token.Content);
                        continue;
                    case TokenKind.QuotedArgContent:
                        _tokenizedValueList.Add(token.ContentUnquoted);
                        continue;
                    case TokenKind.Symbol:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        private void AddArgument()
        {
            if (_tokenizedArgumentName == null) return;


            if (_parsedArguments.ContainsKey(_tokenizedArgumentName))
            {
                _parsedArguments[_tokenizedArgumentName].AddRange(_tokenizedValueList.ToList());
            }
            else
            {
                _parsedArguments.Add(_tokenizedArgumentName, _tokenizedValueList.ToList());    
            }
            
            _tokenizedValueList.Clear();
            _tokenizedArgumentName = null;
        }
    }
}
