﻿/*
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

using holonsoft.CmdLineParser.DomainModel;

namespace Test.holonsoft.CmdLineParser.DomainModel
{
    public class CollectionArgs
    {
        // This default is invalid! Only for testing purpose added!
        [DefaultArgument(ArgumentTypes.MultipleUnique, HelpText = "Input files to count.")] 
        public string[] Files;

        [Argument(ArgumentTypes.AtMostOnce, ShortName = "p", HelpText = "Dummy int array")] 
        public int[] Priorities;

        [Argument(ArgumentTypes.AtMostOnce, ShortName = "pp", HelpText = "Dummy bool array")]
        public bool[] ProcessPriorities;
    }
}