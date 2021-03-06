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
    /// Allows to define and deploy computed field.
    /// </summary>
    /// 
    [SPObjectType(SPObjectModelType.SSOM, "Microsoft.SharePoint.SPFieldComputed", "Microsoft.SharePoint")]
    [SPObjectTypeAttribute(SPObjectModelType.CSOM, "Microsoft.SharePoint.Client.FieldComputed", "Microsoft.SharePoint.Client")]

    [DefaultParentHost(typeof(SiteDefinition))]
    [DefaultRootHost(typeof(SiteDefinition))]

    [Serializable]
    [ExpectArrayExtensionMethod]

    public class ComputedFieldDefinition : FieldDefinition
    {
        #region constructors

        public ComputedFieldDefinition()
        {
            FieldType = BuiltInFieldTypes.Computed;
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
        [ExpectUpdate]

        public bool? EnableLookup { get; set; }

        #endregion

        #region methods

        public override string ToString()
        {
            return new ToStringResult<ComputedFieldDefinition>(this, base.ToString())
                          .ToString();
        }

        #endregion
    }
}
