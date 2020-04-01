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
using System.Reflection;

namespace holonsoft.DomainModel.CmdLineParser
{
    /// <summary>
    /// Allows control of command line parsing.
    /// Attach this attribute to instance fields of types used
    /// as the destination of command line argument parsing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ArgumentAttribute: Attribute
    {
        public ArgumentTypes ArgumentType { get; private set; }


        /// <summary>
        /// Allows control of command line parsing.
        /// </summary>
        /// <param name="argumentType"> Specifies the error checking to be done on the argument. </param>
        public ArgumentAttribute(ArgumentTypes argumentType)
        {
            ArgumentType = argumentType;
        }


        public ArgumentAttribute(ArgumentAttribute source, FieldInfo field)
        {
            
        }


        /// <summary>
        /// Returns true if the argument did not have an explicit short name specified.
        /// </summary>
        public bool HasNoDefaultShortName    { get { return null == ShortName; } }


        /// <summary>
        /// The short name of the argument.
        /// Set to null means use the default short name if it does not conflict with any other parameter name.
        /// Set to String.Empty for no short name.
        /// This property should not be set for DefaultArgumentAttributes.
        /// </summary>
        public string ShortName { get; set; }

        //{
        //    get { return this.shortName; }
        //    set { Debug.Assert(value == null || !(this is DefaultArgumentAttribute)); this.shortName = value; }
        //}

        /// <summary>
        /// Returns true if the argument did not have an explicit long name specified.
        /// </summary>
        public bool HasNoDefaultLongName     { get { return null == LongName; } }
        
        /// <summary>
        /// The long name of the argument.
        /// Set to null means use the default long name.
        /// The long name for every argument must be unique.
        /// It is an error to specify a long name of String.Empty.
        /// </summary>
        public string LongName { get; set; }
        //{
        //    get { Debug.Assert(!this.DefaultLongName); return this.longName; }
        //    set { Debug.Assert(value != ""); this.longName = value; }
        //}

        /// <summary>
        /// The default value of the argument.
        /// </summary>
        public object DefaultValue { get; set; }
        
        /// <summary>
        /// Returns true if the argument has a default value.
        /// </summary>
        public bool HasDefaultValue     { get { return null != DefaultValue; } }

        /// <summary>
        /// Returns true if the argument has help text specified.
        /// </summary>
        public bool HasHelpText         { get { return null != HelpText; } }
        
        /// <summary>
        /// The help text for the argument.
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Only for bool values valid. Sets the value to TRUE if option has been detected
        /// This allows  /install   to be set to true instead of using /install true
        /// </summary>
        public bool OccurrenceSetsBool { get; set; }
    }
}
