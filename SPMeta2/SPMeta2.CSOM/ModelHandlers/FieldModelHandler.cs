﻿using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.SharePoint.Client;
using SPMeta2.Common;
using SPMeta2.CSOM.Extensions;
using SPMeta2.CSOM.ModelHosts;
using SPMeta2.Definitions;
using SPMeta2.Enumerations;
using SPMeta2.Exceptions;
using SPMeta2.ModelHandlers;
using SPMeta2.Services;
using SPMeta2.Utils;

namespace SPMeta2.CSOM.ModelHandlers
{
    public class FieldModelHandler : CSOMModelHandlerBase
    {
        #region properties

        public override Type TargetType
        {
            get { return typeof(FieldDefinition); }
        }

        protected SiteModelHost CurrentSiteModelHost { get; set; }

        #endregion

        #region methods

        protected static XElement GetNewMinimalSPFieldTemplate()
        {
            return new XElement("Field",
                new XAttribute(BuiltInFieldAttributes.ID, String.Empty),
                new XAttribute(BuiltInFieldAttributes.StaticName, String.Empty),
                new XAttribute(BuiltInFieldAttributes.DisplayName, String.Empty),
                new XAttribute(BuiltInFieldAttributes.Title, String.Empty),
                new XAttribute(BuiltInFieldAttributes.Name, String.Empty),
                new XAttribute(BuiltInFieldAttributes.Type, String.Empty),
                new XAttribute(BuiltInFieldAttributes.Group, String.Empty));
        }

        #endregion

        #region methods


        protected List ExtractListFromHost(object modelHost)
        {
            if (modelHost is ListModelHost)
                return (modelHost as ListModelHost).HostList;

            return null;
        }

        protected Field FindField(object modelHost, FieldDefinition definition)
        {
            if (modelHost is SiteModelHost)
                return FindExistingSiteField(modelHost as SiteModelHost, definition);

            if (modelHost is ListModelHost)
                return FindExistingListField((modelHost as ListModelHost).HostList, definition);

            TraceService.ErrorFormat((int)LogEventId.ModelProvisionCoreCall, "FindField() does not support modelHost of type: [{0}]. Throwing SPMeta2NotSupportedException", modelHost);

            throw new SPMeta2NotSupportedException(
                string.Format("Validation for artifact of type [{0}] under model host [{1}] is not supported.",
                    definition.GetType(),
                    modelHost.GetType()));
        }

        protected virtual Type GetTargetFieldType(FieldDefinition model)
        {
            return typeof(Field);
        }


        protected Site ExtractSiteFromHost(object modelHost)
        {
            if (modelHost is SiteModelHost)
                return (modelHost as SiteModelHost).HostSite;

            if (modelHost is ListModelHost)
                return (modelHost as ListModelHost).HostSite;

            return null;
        }

        private Web ExtractWebFromHost(object modelHost)
        {
            if (modelHost is SiteModelHost)
                return (modelHost as SiteModelHost).HostWeb;

            if (modelHost is ListModelHost)
                return (modelHost as ListModelHost).HostWeb;

            return null;
        }

        protected Site HostSite { get; set; }
        protected Web HostWeb { get; set; }

        public override void DeployModel(object modelHost, DefinitionBase model)
        {
            if (!(modelHost is SiteModelHost || modelHost is ListModelHost))
                throw new ArgumentException("modelHost needs to be SiteModelHost/ListModelHost instance.");

            CurrentSiteModelHost = modelHost as SiteModelHost;

            HostSite = ExtractSiteFromHost(modelHost);
            HostWeb = ExtractWebFromHost(modelHost);

            TraceService.Verbose((int)LogEventId.ModelProvisionCoreCall, "Casting field model definition");
            var fieldModel = model.WithAssertAndCast<FieldDefinition>("model", value => value.RequireNotNull());

            Field currentField = null;
            ClientRuntimeContext context = null;

            InvokeOnModelEvent(this, new ModelEventArgs
            {
                CurrentModelNode = null,
                Model = null,
                EventType = ModelEventType.OnProvisioning,
                Object = null,
                ObjectType = GetTargetFieldType(fieldModel),
                ObjectDefinition = model,
                ModelHost = modelHost
            });
            InvokeOnModelEvent<FieldDefinition, Field>(currentField, ModelEventType.OnUpdating);

            if (modelHost is SiteModelHost)
            {
                var siteHost = modelHost as SiteModelHost;
                context = siteHost.HostSite.Context;

                currentField = DeploySiteField(siteHost as SiteModelHost, fieldModel);
            }
            else if (modelHost is ListModelHost)
            {
                var listHost = modelHost as ListModelHost;
                context = listHost.HostList.Context;

                currentField = DeployListField(modelHost as ListModelHost, fieldModel);
            }

            object typedField = null;

            // emulate context.CastTo<>() call for typed field type
            if (GetTargetFieldType(fieldModel) != currentField.GetType())
            {
                var targetFieldType = GetTargetFieldType(fieldModel);

                TraceService.VerboseFormat((int)LogEventId.ModelProvisionCoreCall, "Calling context.CastTo() to field type: [{0}]", targetFieldType);

                var method = context.GetType().GetMethod("CastTo");
                var generic = method.MakeGenericMethod(targetFieldType);

                typedField = generic.Invoke(context, new object[] { currentField });
            }

            InvokeOnModelEvent(this, new ModelEventArgs
            {
                CurrentModelNode = null,
                Model = null,
                EventType = ModelEventType.OnProvisioned,
                Object = typedField ?? currentField,
                ObjectType = GetTargetFieldType(fieldModel),
                ObjectDefinition = model,
                ModelHost = modelHost
            });
            InvokeOnModelEvent<FieldDefinition, Field>(currentField, ModelEventType.OnUpdated);

            TraceService.Verbose((int)LogEventId.ModelProvisionCoreCall, "UpdateAndPushChanges(true)");
            currentField.UpdateAndPushChanges(true);

            TraceService.Verbose((int)LogEventId.ModelProvisionCoreCall, "ExecuteQuery()");
            context.ExecuteQueryWithTrace();
        }



        private Field DeployListField(ListModelHost modelHost, FieldDefinition fieldModel)
        {
            TraceService.Verbose((int)LogEventId.ModelProvisionCoreCall, "Deploying list field");

            var list = modelHost.HostList;
            var context = list.Context;

            var scope = new ExceptionHandlingScope(context);

            Field field;

            using (scope.StartScope())
            {
                using (scope.StartTry())
                {
                    field = list.Fields.GetById(fieldModel.Id);
                    context.Load(field);
                }

                using (scope.StartCatch())
                {

                }
            }

            TraceService.Verbose((int)LogEventId.ModelProvisionCoreCall, "ExecuteQuery()");
            context.ExecuteQueryWithTrace();

            if (!scope.HasException)
            {
                field = list.Fields.GetById(fieldModel.Id);
                context.Load(field);

                TraceService.VerboseFormat((int)LogEventId.ModelProvisionCoreCall, "Found site list with Id: [{0}]", fieldModel.Id);
                TraceService.Verbose((int)LogEventId.ModelProvisionCoreCall, "ExecuteQuery()");

                context.ExecuteQueryWithTrace();

                return EnsureField(context, field, list.Fields, fieldModel);
            }

            TraceService.VerboseFormat((int)LogEventId.ModelProvisionCoreCall, "Cannot find list field with Id: [{0}]", fieldModel.Id);
            return EnsureField(context, null, list.Fields, fieldModel);
        }

        protected Field FindExistingSiteField(SiteModelHost siteHost, FieldDefinition fieldDefinition)
        {
            var id = fieldDefinition.Id;
            var rootWeb = siteHost.HostSite.RootWeb;

            TraceService.VerboseFormat((int)LogEventId.ModelProvisionCoreCall, "FindExistingSiteField with Id: [{0}]", id);

            var context = rootWeb.Context;
            var scope = new ExceptionHandlingScope(context);

            Field field = null;

            using (scope.StartScope())
            {
                using (scope.StartTry())
                {
                    rootWeb.Fields.GetById(id);
                }

                using (scope.StartCatch())
                {

                }
            }

            TraceService.Verbose((int)LogEventId.ModelProvisionCoreCall, "ExecuteQuery()");
            context.ExecuteQueryWithTrace();

            if (!scope.HasException)
            {
                field = rootWeb.Fields.GetById(id);
                context.Load(field);

                TraceService.VerboseFormat((int)LogEventId.ModelProvisionCoreCall, "Found site field with Id: [{0}]", id);
                TraceService.Verbose((int)LogEventId.ModelProvisionCoreCall, "ExecuteQuery()");

                context.ExecuteQueryWithTrace();
            }
            else
            {
                TraceService.VerboseFormat((int)LogEventId.ModelProvisionCoreCall, "Cannot find site field with Id: [{0}]", id);
            }

            return field;
        }

        private Field DeploySiteField(SiteModelHost siteModelHost, FieldDefinition fieldModel)
        {
            TraceService.Verbose((int)LogEventId.ModelProvisionCoreCall, "Deploying site field");

            var site = siteModelHost.HostSite;
            var context = site.Context;

            var field = FindExistingSiteField(siteModelHost, fieldModel);

            return EnsureField(context, field, site.RootWeb.Fields, fieldModel);
        }

        protected virtual void ProcessFieldProperties(Field field, FieldDefinition definition)
        {
            field.Title = definition.Title;

            field.Description = definition.Description ?? string.Empty;
            field.Group = definition.Group ?? string.Empty;

            field.Required = definition.Required;

            if (!string.IsNullOrEmpty(definition.ValidationMessage))
                field.ValidationMessage = definition.ValidationMessage;

            if (!string.IsNullOrEmpty(definition.ValidationFormula))
                field.ValidationFormula = definition.ValidationFormula;

            if (!string.IsNullOrEmpty(definition.DefaultValue))
                field.DefaultValue = definition.DefaultValue;

            field.Indexed = definition.Indexed;

#if !NET35
            field.JSLink = definition.JSLink;
#endif

            // missed in CSOM

            //if (definition.AllowDeletion.HasValue)
            //    field.AllowDeletion = definition.AllowDeletion.Value;

            //if (definition.ShowInEditForm.HasValue)
            //    field.ShowInEditForm = definition.ShowInEditForm.Value;

            //if (definition.ShowInDisplayForm.HasValue)
            //    field.ShowInDisplayForm = definition.ShowInDisplayForm.Value;

            //if (definition.ShowInListSettings.HasValue)
            //    field.ShowInListSettings = definition.ShowInListSettings.Value;

            //if (definition.ShowInViewForms.HasValue)
            //    field.ShowInViewForms = definition.ShowInViewForms.Value;

            //if (definition.ShowInNewForm.HasValue)
            //    field.ShowInNewForm = definition.ShowInNewForm.Value;

            //if (definition.ShowInVersionHistory.HasValue)
            //    field.ShowInVersionHistory = definition.ShowInVersionHistory.Value;
        }

        private Field EnsureField(ClientRuntimeContext context, Field currentField, FieldCollection fieldCollection,
            FieldDefinition fieldModel)
        {
            TraceService.Verbose((int)LogEventId.ModelProvisionCoreCall, "EnsureField()");

            if (currentField == null)
            {
                TraceService.Verbose((int)LogEventId.ModelProvisionProcessingNewObject, "Current field is NULL. Creating new");

                var fieldDef = GetTargetSPFieldXmlDefinition(fieldModel);

                var addFieldOptions = (AddFieldOptions)(int)fieldModel.AddFieldOptions;
                var resultField = fieldCollection.AddFieldAsXml(fieldDef, fieldModel.AddToDefaultView, addFieldOptions);

                ProcessFieldProperties(resultField, fieldModel);

                return resultField;
            }
            else
            {
                TraceService.Verbose((int)LogEventId.ModelProvisionProcessingExistingObject, "Processing existing field");

                ProcessFieldProperties(currentField, fieldModel);

                return currentField;
            }
        }

        protected Field FindExistingListField(List list, FieldDefinition fieldModel)
        {
            TraceService.VerboseFormat((int)LogEventId.ModelProvisionCoreCall, "FindListField with Id: [{0}]", fieldModel.Id);

            var context = list.Context;

            Field field;

            var scope = new ExceptionHandlingScope(context);

            using (scope.StartScope())
            {
                using (scope.StartTry())
                {
                    field = list.Fields.GetById(fieldModel.Id);
                    context.Load(field);
                }
                using (scope.StartCatch())
                {
                    field = null;
                    //context.Load(field);
                }
            }

            context.ExecuteQueryWithTrace();

            if (!scope.HasException)
            {
                field = list.Fields.GetById(fieldModel.Id);
                context.Load(field);

                TraceService.VerboseFormat((int)LogEventId.ModelProvisionCoreCall, "Found list field with Id: [{0}]", fieldModel.Id);
                TraceService.Verbose((int)LogEventId.ModelProvisionCoreCall, "ExecuteQuery()");

                context.ExecuteQueryWithTrace();
            }
            else
            {
                TraceService.VerboseFormat((int)LogEventId.ModelProvisionCoreCall, "Cannot find list field with Id: [{0}]", fieldModel.Id);
            }

            return field;
        }

        protected virtual void ProcessSPFieldXElement(XElement fieldTemplate, FieldDefinition fieldModel)
        {
            // minimal set
            fieldTemplate
              .SetAttribute(BuiltInFieldAttributes.ID, fieldModel.Id.ToString("B"))
              .SetAttribute(BuiltInFieldAttributes.StaticName, fieldModel.InternalName)
              .SetAttribute(BuiltInFieldAttributes.DisplayName, fieldModel.Title)
              .SetAttribute(BuiltInFieldAttributes.Title, fieldModel.Title)
              .SetAttribute(BuiltInFieldAttributes.Name, fieldModel.InternalName)
              .SetAttribute(BuiltInFieldAttributes.Type, fieldModel.FieldType)
              .SetAttribute(BuiltInFieldAttributes.Group, fieldModel.Group ?? string.Empty);

            // additions
            if (!String.IsNullOrEmpty(fieldModel.JSLink))
                fieldTemplate.SetAttribute(BuiltInFieldAttributes.JSLink, fieldModel.JSLink);

            if (!string.IsNullOrEmpty(fieldModel.DefaultValue))
                fieldTemplate.SetSubNode("Default", fieldModel.DefaultValue);

            fieldTemplate.SetAttribute(BuiltInFieldAttributes.Hidden, fieldModel.Hidden.ToString().ToUpper());

            // ShowIn* settings
            if (fieldModel.ShowInDisplayForm.HasValue)
                fieldTemplate.SetAttribute(BuiltInFieldAttributes.ShowInDisplayForm, fieldModel.ShowInDisplayForm.Value.ToString().ToUpper());

            if (fieldModel.ShowInEditForm.HasValue)
                fieldTemplate.SetAttribute(BuiltInFieldAttributes.ShowInEditForm, fieldModel.ShowInEditForm.Value.ToString().ToUpper());

            if (fieldModel.ShowInListSettings.HasValue)
                fieldTemplate.SetAttribute(BuiltInFieldAttributes.ShowInListSettings, fieldModel.ShowInListSettings.Value.ToString().ToUpper());

            if (fieldModel.ShowInNewForm.HasValue)
                fieldTemplate.SetAttribute(BuiltInFieldAttributes.ShowInNewForm, fieldModel.ShowInNewForm.Value.ToString().ToUpper());

            if (fieldModel.ShowInVersionHistory.HasValue)
                fieldTemplate.SetAttribute(BuiltInFieldAttributes.ShowInVersionHistory, fieldModel.ShowInVersionHistory.Value.ToString().ToUpper());

            if (fieldModel.ShowInViewForms.HasValue)
                fieldTemplate.SetAttribute(BuiltInFieldAttributes.ShowInViewForms, fieldModel.ShowInViewForms.Value.ToString().ToUpper());

            // misc
            if (fieldModel.AllowDeletion.HasValue)
                fieldTemplate.SetAttribute(BuiltInFieldAttributes.AllowDeletion, fieldModel.AllowDeletion.Value.ToString().ToUpper());

            fieldTemplate.SetAttribute(BuiltInFieldAttributes.Indexed, fieldModel.Indexed.ToString().ToUpper());

        }

        protected virtual string GetTargetSPFieldXmlDefinition(FieldDefinition fieldModel)
        {
            var fieldTemplate = GetNewMinimalSPFieldTemplate();

            if (!string.IsNullOrEmpty(fieldModel.RawXml))
                fieldTemplate = XDocument.Parse(fieldModel.RawXml).Root;

            ProcessSPFieldXElement(fieldTemplate, fieldModel);

            // add up additional attributes
            if (fieldModel.AdditionalAttributes.Any())
                foreach (var fieldAttr in fieldModel.AdditionalAttributes)
                    fieldTemplate.SetAttribute(fieldAttr.Name, fieldAttr.Value);

            return fieldTemplate.ToString();
        }

        #endregion
        
    }
}
