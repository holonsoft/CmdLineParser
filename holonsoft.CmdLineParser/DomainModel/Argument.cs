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
using System.Reflection;
using holonsoft.CmdLineParser.DomainModel;

namespace holonsoft.DomainModel.CmdLineParser
{
    /// <summary>
    /// Contains information about field attributes
    /// </summary>
    internal class Argument
    {
        public ArgumentAttribute Attribute { get; set; }
        public FieldInfo Field { get; set; }
        public bool Seen { get; set; }

        public string FieldName => Field.Name;

        public string LongName => Attribute.LongName;
        public string ShortName => Attribute.ShortName;

        public bool IsCollection => Field.FieldType.IsArray;

        public bool VisitorWasHere { get; set; }
    }
}
