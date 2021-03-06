﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using SPMeta2.Attributes;
using SPMeta2.Attributes.Regression;
using SPMeta2.Enumerations;
using SPMeta2.Utils;

namespace SPMeta2.Definitions.Fields
{
    /// <summary>
    /// Allows to define and deploy URL field.
    /// </summary>
    /// 
    [SPObjectType(SPObjectModelType.SSOM, "Microsoft.SharePoint.SPFieldUrl", "Microsoft.SharePoint")]
    [SPObjectTypeAttribute(SPObjectModelType.CSOM, "Microsoft.SharePoint.Client.FieldUrl", "Microsoft.SharePoint.Client")]

    [DefaultParentHost(typeof(SiteDefinition))]
    [DefaultRootHost(typeof(SiteDefinition))]

    [Serializable]
    [ExpectArrayExtensionMethod]

    public class URLFieldDefinition : FieldDefinition
    {
        #region constructors

        public URLFieldDefinition()
        {
            FieldType = BuiltInFieldTypes.URL;
            DisplayFormat = BuiltInUrlFieldFormatType.Hyperlink;
        }

        #endregion

        #region properties

        [ExpectValidation]
        public override string ValidationMessage
        {
            get { return string.Empty; }
            set { }
        }

        [ExpectValidation]
        public override string ValidationFormula
        {
            get { return string.Empty; }
            set { }
        }

        [ExpectValidation]
        [ExpectUpdateAsUrlFieldFormat]
        public string DisplayFormat { get; set; }

        #endregion

        #region methods

        public override string ToString()
        {
            return new ToStringResult<URLFieldDefinition>(this, base.ToString())
                          .AddPropertyValue(p => p.DisplayFormat)
                          .ToString();
        }

        #endregion
    }
}
