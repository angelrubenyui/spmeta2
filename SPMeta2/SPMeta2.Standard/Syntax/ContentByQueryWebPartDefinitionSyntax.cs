﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SPMeta2.Models;
using SPMeta2.Standard.Definitions.Webparts;
using SPMeta2.Syntax.Default.Extensions;

namespace SPMeta2.Standard.Syntax
{
    public static class ContentByQueryWebPartDefinitionSyntax
    {
        #region publishing page

        public static ModelNode AddContentByQueryWebPart(this ModelNode model, ContentByQueryWebPartDefinition definition)
        {
            return AddContentByQueryWebPart(model, definition, null);
        }

        public static ModelNode AddContentByQueryWebPart(this ModelNode model, ContentByQueryWebPartDefinition definition, Action<ModelNode> action)
        {
            return model.AddDefinitionNode(definition, action);
        }

        #endregion

        #region array overload

        public static ModelNode AddContentByQueryWebParts(this ModelNode model, IEnumerable<ContentByQueryWebPartDefinition> definitions)
        {
            foreach (var definition in definitions)
                model.AddDefinitionNode(definition);

            return model;
        }

        #endregion
    }
}
