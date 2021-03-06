﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPMeta2.Definitions;
using SPMeta2.Enumerations;
using SPMeta2.Regression.Tests.Base;
using SPMeta2.Regression.Tests.Impl.Scenarios.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SPMeta2.Regression.Tests.Impl.Scenarios
{
    [TestClass]
    public class ListViewScenariousTest : SPMeta2RegresionScenarioTestBase
    {
        #region internal

        [ClassInitializeAttribute]
        public static void Init(TestContext context)
        {
            InternalInit();
        }

        [ClassCleanupAttribute]
        public static void Cleanup()
        {
            InternalCleanup();
        }

        #endregion

        #region default

        [TestMethod]
        [TestCategory("Regression.Scenarios.ListsViews")]
        public void CanDeploy_ListView_AsDefaultForContentType()
        {
            TestRandomDefinition<ListViewDefinition>(def =>
            {
                def.DefaultViewForContentType = true;
            });
        }

        [TestMethod]
        [TestCategory("Regression.Scenarios.ListsViews")]
        public void CanDeploy_ListView_AsDefaultForContentType_ById()
        {
            TestRandomDefinition<ListViewDefinition>(def =>
            {
                def.DefaultViewForContentType = true;
                def.ContentTypeId = BuiltInContentTypeId.Item;
            });
        }


        [TestMethod]
        [TestCategory("Regression.Scenarios.ListsViews")]
        public void CanDeploy_ListView_AsDefaultForContentType_ByName()
        {
            TestRandomDefinition<ListViewDefinition>(def =>
            {
                def.DefaultViewForContentType = true;
                def.ContentTypeName = "Item";
            });
        }

        #endregion
    }
}
