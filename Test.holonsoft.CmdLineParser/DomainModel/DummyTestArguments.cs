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
using holonsoft.DomainModel.CmdLineParser;

namespace Test.holonsoft.CmdLineParser.DomainModel
{
    public class DummyTestArguments
    {
        [Argument(ArgumentTypes.Required, ShortName = "", HelpText = "Starting number of connections.")]
        public int StartConnections;
        
        [Argument(ArgumentTypes.Required, HelpText = "Maximum number of connections.")]
        public int MaxConnections;

        [Argument(ArgumentTypes.Unique, HelpText = "Force internal connection pool to be used")]
        public bool ForcePoolUsing;

        [Argument(ArgumentTypes.Required, ShortName = "inc", HelpText = "Number of connections to increment, if needed.")]
        public int IncrementOfConnections;
        
        [DefaultArgument(ArgumentTypes.AtMostOnce, DefaultValue = @"{AppPath}\myconfig.xml", HelpText = "Path with params to config file.")]
        public string Configpath;

        [Argument(ArgumentTypes.AtMostOnce, DefaultValue = true, HelpText = "Count number of lines in the input text.")] 
        public bool Lines;

        [Argument(ArgumentTypes.AtMostOnce, LongName = "WordsToBeCounted", HelpText = "Count number of words in the input text.")] 
        public bool Words;

        [Argument(ArgumentTypes.AtMostOnce, HelpText = "Count number of chars in the input text.")] 
        public bool Chars;

        [Argument(ArgumentTypes.AtMostOnce, DefaultValue = 17, HelpText = "Count errors before function breaks")]
        public int MaxErrorsBeforeStop;

        [Argument(ArgumentTypes.AtMostOnce, HelpText = "Count errors before function breaks")]
        public Guid ProgramId;

        // This default is invalid! Only for testing purpose added!
        [DefaultArgument(ArgumentTypes.MultipleUnique, HelpText = "Input files to count.")] 
        public string[] Files;

        [Argument(ArgumentTypes.AtMostOnce, ShortName = "p", HelpText = "Dummy int array")] 
        public int[] Priorities;

        [Argument(ArgumentTypes.AtMostOnce, ShortName = "pp", HelpText = "Dummy bool array")]
        public bool[] ProcessPriorities;

        [Argument(ArgumentTypes.AtMostOnce, OccurrenceSetsBool = true)]
        public bool FlagWhenFound;

        public string ThisIsNotAnArgumentButShoudNotCreateAnError;

        private bool PrivateDummyField;
    }
}