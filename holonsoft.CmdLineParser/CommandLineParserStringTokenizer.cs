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
    public class StringTokenizer
    {
        private readonly string[] _content;
        private readonly Token _done = new Token(TokenKind.Done, string.Empty);
        
        private int _actPosition = -1;

        private readonly Stack<Token> _tokens = new Stack<Token>();

        public StringTokenizer(string[] content)
        {
            if (content == null || content.Length == 0)
            {
                throw new ArgumentException("Content must not be null");
            }

            var isOk = content.Aggregate(true, (current, x) => current & !(string.IsNullOrEmpty(x)));

            if (!isOk)
            {
                throw new ArgumentException("Content must not be null");
            }

            _content = content;
        }


        public Token Next()
        {
            if (_tokens.Count > 0)
            {
                return _tokens.Pop();
            }

            _actPosition++;

            if (_actPosition >= _content.Length)
            {
                return _done;
            }
            
            var arg = _content[_actPosition];

            switch (arg[0])
            {
                case '-':
                case '/':
                    _tokens.Push(new Token(TokenKind.Argument, GetArgumentWithoutMarker(arg)));
                    return new Token(TokenKind.ArgStartMarker, String.Empty);
                default:
                    return new Token(arg[0] == '"'? TokenKind.QuotedArgContent : TokenKind.ArgContent, arg);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetArgumentWithoutMarker(string arg)
        {
            if (arg.Length <= 2) return (arg.Substring(1).Trim());

            var i1 = arg.IndexOf(':');
            var i2 = arg.IndexOf('"');

            if ((i2 > i1) || (i1 > 0  && i2 < 0))
            {
                var argItself = arg.Substring(0, i1);

                var c = arg.Substring(i1 + 1);

                _tokens.Push(new Token(c[0] == '"'? TokenKind.QuotedArgContent : TokenKind.ArgContent, c));

                return argItself[1] == '-' ? argItself.Substring(2).Trim() : argItself.Substring(1).Trim();
            }
            
            return arg[1] == '-' ? arg.Substring(2).Trim() : arg.Substring(1).Trim();
        }
    }
}