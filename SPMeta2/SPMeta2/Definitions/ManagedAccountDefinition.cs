﻿using SPMeta2.Attributes;
using SPMeta2.Attributes.Regression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPMeta2.Definitions
{
    /// <summary>
    /// Allows to define and deploy SharePoint managed account.
    /// </summary>
    /// 

    [SPObjectTypeAttribute(SPObjectModelType.SSOM, "Microsoft.SharePoint.Administration.SPManagedAccount", "Microsoft.SharePoint")]

    [DefaultRootHostAttribute(typeof(FarmDefinition))]
    [DefaultParentHostAttribute(typeof(FarmDefinition))]

    [Serializable]

    public class ManagedAccountDefinition : DefinitionBase
    {
        #region properties

        [ExpectValidation]
        public string LoginName { get; set; }

        #endregion
    }
}
