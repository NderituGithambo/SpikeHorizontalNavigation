using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace SpikeHorizontalNavigation.Module.BusinessObjects.SpikeHorizontalNavigation
{

    public partial class AssetType
    {
        public AssetType(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
