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

namespace holonsoft.CmdLineParser.DomainModel
{
    [Flags]
    public enum ArgumentTypes
    {
        /// <summary>
        /// The default type for non-collection arguments.
        /// The argument is not required, but an error will be reported if it is specified more than once.
        /// </summary>
        AtMostOnce = 0x00,

        /// <summary>
        /// Indicates that this field is required. 
        /// An error will be displayed if it is not present when parsing arguments.
        /// </summary>
        Required = 0x01,
        /// <summary>
        /// Only valid in conjunction with Multiple.
        /// Duplicate values will result in an error.
        /// </summary>
        Unique = 1 << 1,
        /// <summary>
        /// Inidicates that the argument may be specified more than once.
        /// Only valid if the argument is a collection
        /// </summary>
        Multiple = 1 << 2,

        /// <summary>
        /// Indicates that the argument must be the one and only .. if given
        /// Does not allow any other arguments within the call.
        /// </summary>
        Exclusive = 1 << 3,

        /// <summary>
        /// For non-collection arguments, when the argument is specified more than
        /// once no error is reported and the value of the argument is the last
        /// value which occurs in the argument list.
        /// </summary>
        LastOccurenceWins = Multiple,

        /// <summary>
        /// The default type for collection arguments.
        /// The argument is permitted to occur multiple times, but duplicate 
        /// values will cause an error to be reported.
        /// </summary>
        MultipleUnique = Multiple | Unique,
    }
}
