﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SPMeta2.Attributes;
using SPMeta2.Attributes.Regression;
using SPMeta2.Definitions;
using SPMeta2.Utils;

namespace SPMeta2.Standard.Definitions
{
    /// <summary>
    /// Allows to define and deploy SharePoint audience.
    /// </summary>
    [SPObjectType(SPObjectModelType.SSOM, "Microsoft.Office.Server.Audience.Audience", "Microsoft.Office.Server.UserProfiles")]

    [DefaultParentHost(typeof(SiteDefinition))]
    [DefaultRootHost(typeof(SiteDefinition))]

    [Serializable]
    [ExpectWithExtensionMethod]
    public class AudienceDefinition : DefinitionBase
    {
        #region properties

        [ExpectValidation]
        public string AudienceName { get; set; }

        [ExpectValidation]
        public string AudienceDescription { get; set; }

        #endregion

        #region methods

        public override string ToString()
        {
            return new ToStringResult<AudienceDefinition>(this)
                          .AddPropertyValue(p => p.AudienceName)
                          .AddPropertyValue(p => p.AudienceDescription)
                          .ToString();
        }

        #endregion
    }
}
