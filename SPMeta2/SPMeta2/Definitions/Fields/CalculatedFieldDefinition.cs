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
    /// Allows to define and deploy calculated field.
    /// </summary>
    /// 
    [SPObjectType(SPObjectModelType.SSOM, "Microsoft.SharePoint.SPFieldCalculated", "Microsoft.SharePoint")]
    [SPObjectTypeAttribute(SPObjectModelType.CSOM, "Microsoft.SharePoint.Client.FieldCalculated", "Microsoft.SharePoint.Client")]

    [DefaultParentHost(typeof(SiteDefinition))]
    [DefaultRootHost(typeof(SiteDefinition))]

    [Serializable]
    [ExpectArrayExtensionMethod]

    public class CalculatedFieldDefinition : FieldDefinition
    {
        #region constructors

        public CalculatedFieldDefinition()
        {
            FieldType = BuiltInFieldTypes.Calculated;
            FieldReferences = new Collection<string>();

            DateFormat = BuiltInDateTimeFieldFormatType.DateOnly;
            CurrencyLocaleId = 1033;
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
        public int? CurrencyLocaleId { get; set; }

        [ExpectValidation]
        public string DateFormat { get; set; }

        [ExpectValidation]
        public string DisplayFormat { get; set; }

        [ExpectValidation]
        [ExpectUpdateAsCalculatedFieldFormula]
        public string Formula { get; set; }

        [ExpectValidation]
        [ExpectUpdateAssCalculatedFieldOutputType]
        public string OutputType { get; set; }

        [ExpectValidation]
        //[ExpectUpdate]
        public bool? ShowAsPercentage { get; set; }

        [ExpectValidation]
        //[ExpectUpdateAssCalculatedFieldReferences]
        public Collection<string> FieldReferences { get; set; }

        [ExpectValidation]
        public override string DefaultValue
        {
            get
            {
                // #SPBUG
                // Calculated field MUST return string.Empty to avoid setting DefaultValue for field.
                // SharePoint drive crazy if calculated field has default value. FieldRefs would be NULL, tons of failures would be there.
                return string.Empty;
            }
            set { }
        }

        #endregion

        #region methods

        public override string ToString()
        {
            return new ToStringResult<CalculatedFieldDefinition>(this, base.ToString())
                          .ToString();
        }

        #endregion
    }
}
