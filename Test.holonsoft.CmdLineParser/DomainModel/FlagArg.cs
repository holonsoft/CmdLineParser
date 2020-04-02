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
using holonsoft.CmdLineParser.DomainModel;
using holonsoft.DomainModel.CmdLineParser;

namespace Test.holonsoft.CmdLineParser.DomainModel
{
    public class FlagArg
    {
        [Argument(ArgumentTypes.AtMostOnce, OccurrenceSetsBool = true)]
        public bool FlagWhenFound;

        public string ThisIsNotAnArgumentButShouldNotCreateAnError;

#pragma warning disable 169
        private bool PrivateDummyField;
#pragma warning restore 169
    }
}