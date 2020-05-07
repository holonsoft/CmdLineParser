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
using System.Runtime.CompilerServices;
using System.Text;
using holonsoft.CmdLineParser.DomainModel;

namespace holonsoft.CmdLineParser
{
    public class StringTokenizer
    {
        private readonly string _content;
        private readonly Token _done = new Token(TokenKind.Done, string.Empty);
        private readonly StringBuilder _sb = new StringBuilder();
        
        private int _actPosition = -1;
        private TokenKind _lastTokenKind = TokenKind.Unknown;

        public StringTokenizer(string content)
        {
            _content = content;

            if (string.IsNullOrWhiteSpace(_content))
            {
                throw new ArgumentException("Content must not be null");
            }
        }


        public Token Next()
        {
            _actPosition++;

            if (_actPosition >= _content.Length)
            {
                return _done;
            }

            var c = _content[_actPosition];
            
            switch (c)
            {
                case '-':
                case '/':
                {
                    AppendChar();

                    if (LookAhead() == '-')
                    {
                        _actPosition++;
                        AppendChar();
                    }

                    _lastTokenKind = TokenKind.ArgStartMarker;
                    return new Token(TokenKind.ArgStartMarker, GetContent());
                }
                case ' ':
                case ':':
                    _lastTokenKind = TokenKind.Symbol;
                    return new Token(TokenKind.Symbol, " ");
                default:
                    return ReadContent();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Token ReadContent()
        {
            var c = _content[_actPosition];
            char expectedSeparator = ' ';

            if (c == '"')
            {
                expectedSeparator = c;
                _actPosition++;
            }

            while (_actPosition < _content.Length)
            {
                c = _content[_actPosition];
                if (c == expectedSeparator || ((c == ':') && (expectedSeparator != '"'))) break;

                AppendChar();

                _actPosition++;
            }

            if (_lastTokenKind == TokenKind.ArgStartMarker)
            {
                _lastTokenKind = TokenKind.Argument;
                return new Token(TokenKind.Argument, GetContent());
            }

            return new Token(expectedSeparator == ' '? TokenKind.ArgContent : TokenKind.QuotedArgContent, GetContent());
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendChar()
        {
            _sb.Append(_content[_actPosition]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetContent()
        {
            var result = _sb.ToString();
            _sb.Clear();

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char LookAhead()
        {
            var pos = _actPosition + 1;

            if (pos == _content.Length) return Char.MinValue;

            return _content[pos];
        }

    }
}